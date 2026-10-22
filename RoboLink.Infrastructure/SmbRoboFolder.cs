using System.Net;
using RoboLink.Core;
using SMBLibrary;
using SMBLibrary.Client;
using SmbFileAttributes = SMBLibrary.FileAttributes;

namespace RoboLink.Infrastructure;

public class SmbRoboFolder(SmbOptions options) : IRoboFolder
{
    public Task<IReadOnlyList<string>> ListAsync(CancellationToken ct = default) =>
        Task.Run(() => Run(List), ct);

    public Task<bool> ExistsAsync(string name, CancellationToken ct = default) =>
        Task.Run(() => Run(store => List(store).Contains(name, StringComparer.OrdinalIgnoreCase)), ct);

    public Task WriteAsync(string name, byte[] content, CancellationToken ct = default) =>
        Task.Run(() => RunVoid(store => Write(store, name, content)), ct);

    public Task DeleteAsync(string name, CancellationToken ct = default) =>
        Task.Run(() => RunVoid(store => Delete(store, name)), ct);

    // Connects, logs in, opens the share, runs the action, then closes everything.
    // Func<ISMBFileStore, T> means "a method that takes the file store and returns a T".
    private T Run<T>(Func<ISMBFileStore, T> action)
    {
        var client = new SMB2Client();
        try
        {
            if (!client.Connect(IPAddress.Parse(options.Host), SMBTransportType.DirectTCPTransport))
                throw new IOException($"Could not connect to {options.Host}.");

            var status = client.Login(options.Domain, options.User, options.Password);
            if (status != NTStatus.STATUS_SUCCESS)
                throw new IOException($"SMB login failed: {status}.");

            // "out status" fills in status as a second return value.
            var store = client.TreeConnect(options.Share, out status);
            if (status != NTStatus.STATUS_SUCCESS)
                throw new IOException($"Could not open share '{options.Share}': {status}.");

            try
            {
                return action(store);
            }
            finally
            {
                store.Disconnect();
            }
        }
        finally
        {
            client.Logoff();
            client.Disconnect();
        }
    }

    private void RunVoid(Action<ISMBFileStore> action) => Run(store => { action(store); return true; });

    // Same rule as LocalRoboFolder: file names only, no folders.
    private string PathFor(string name)
    {
        if (name.Contains('\\') || name.Contains('/'))
            throw new ArgumentException("Name must be a file name only, no folders.", nameof(name));

        return $@"{options.Folder}\{name}";
    }

    // Writes to name.tmp, then renames over the final name, so the machine never sees a half-written file.
    private void Write(ISMBFileStore store, string name, byte[] content)
    {
        var finalPath = PathFor(name);
        var tmpPath = finalPath + ".tmp";

        var status = store.CreateFile(out var handle, out _, tmpPath,
            AccessMask.GENERIC_WRITE | AccessMask.DELETE | AccessMask.SYNCHRONIZE,
            SmbFileAttributes.Normal, ShareAccess.None, CreateDisposition.FILE_OVERWRITE_IF,
            CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null);
        if (status != NTStatus.STATUS_SUCCESS)
            throw new IOException($"Could not create '{tmpPath}': {status}.");

        try
        {
            // Our files are tiny, but write in 32 KB chunks anyway. An empty file skips the loop.
            const int chunk = 32 * 1024;
            for (var offset = 0; offset < content.Length; offset += chunk)
            {
                var part = content[offset..Math.Min(offset + chunk, content.Length)];
                status = store.WriteFile(out _, handle, offset, part);
                if (status != NTStatus.STATUS_SUCCESS)
                    throw new IOException($"Could not write '{tmpPath}': {status}.");
            }

            status = store.SetFileInformation(handle,
                new FileRenameInformationType2 { ReplaceIfExists = true, FileName = finalPath });
            if (status != NTStatus.STATUS_SUCCESS)
                throw new IOException($"Could not rename '{tmpPath}' to '{finalPath}': {status}.");
        }
        finally
        {
            store.CloseFile(handle);
        }
    }

    // A missing file is not an error, so cancel can be retried safely.
    private void Delete(ISMBFileStore store, string name)
    {
        var path = PathFor(name);

        var status = store.CreateFile(out var handle, out _, path,
            AccessMask.DELETE | AccessMask.SYNCHRONIZE, SmbFileAttributes.Normal, ShareAccess.None,
            CreateDisposition.FILE_OPEN,
            CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null);
        if (status == NTStatus.STATUS_OBJECT_NAME_NOT_FOUND)
            return;
        if (status != NTStatus.STATUS_SUCCESS)
            throw new IOException($"Could not open '{path}' for delete: {status}.");

        try
        {
            status = store.SetFileInformation(handle, new FileDispositionInformation { DeletePending = true });
            if (status != NTStatus.STATUS_SUCCESS)
                throw new IOException($"Could not delete '{path}': {status}.");
        }
        finally
        {
            store.CloseFile(handle);
        }
    }

    // File names in the folder. Skips subfolders, "." and "..", and our own .tmp files.
    private IReadOnlyList<string> List(ISMBFileStore store)
    {
        var status = store.CreateFile(out var handle, out _, options.Folder,
            AccessMask.GENERIC_READ, SmbFileAttributes.Directory,
            ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN,
            CreateOptions.FILE_DIRECTORY_FILE, null);
        if (status != NTStatus.STATUS_SUCCESS)
            throw new IOException($"Could not open folder '{options.Folder}': {status}.");

        try
        {
            status = store.QueryDirectory(out var entries, handle, "*", FileInformationClass.FileDirectoryInformation);
            if (status != NTStatus.STATUS_SUCCESS && status != NTStatus.STATUS_NO_MORE_FILES)
                throw new IOException($"Could not list folder '{options.Folder}': {status}.");

            return [.. entries
                .OfType<FileDirectoryInformation>()
                .Where(f => (f.FileAttributes & SmbFileAttributes.Directory) == 0)
                .Select(f => f.FileName)
                .Where(n => !n.EndsWith(".tmp"))];
        }
        finally
        {
            store.CloseFile(handle);
        }
    }
}
