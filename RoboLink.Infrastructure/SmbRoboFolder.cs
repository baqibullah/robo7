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
        throw new NotImplementedException();

    public Task DeleteAsync(string name, CancellationToken ct = default) =>
        throw new NotImplementedException();

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
