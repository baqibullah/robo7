using Microsoft.Extensions.Logging;
using RoboLink.Core;
using RoboLink.Infrastructure;

var password = Environment.GetEnvironmentVariable("ROBO_PASSWORD")
    ?? throw new InvalidOperationException("Set ROBO_PASSWORD first.");

var folder = new SmbRoboFolder(new SmbOptions
{
    Host = "192.168.0.150",
    User = "robo7user",
    Password = password,
});
using var loggerFactory = LoggerFactory.Create(b => b.AddSimpleConsole());
var handshake = new RoboHandshake(folder, new HandshakeOptions(), loggerFactory.CreateLogger<RoboHandshake>());

// Leftover files would print on the next Initialize, so stop if the folder isn't empty.
var leftovers = await folder.ListAsync();
if (leftovers.Count > 0)
{
    Console.WriteLine("Folder is not empty, clean it by hand first:");
    foreach (var n in leftovers) Console.WriteLine($"  {n}");
    return;
}

var now = DateTime.Now;
var today = DateOnly.FromDateTime(now);
var trayNo = now.Hour * 100 + now.Minute; // hhmm, so it's different every minute

Tube MakeTube(int seq, bool last, string barcode) => new(
    new TubeOrder(
        HostTerminal: 5, RoboTerminal: 1, Date: today, TrayNo: trayNo,
        IsLastTube: last, SeqNo: seq, QueueNo: 1234,
        PatientId: "0123456789", PatientName: "Tiger Woods", Container: "HbA1c",
        Volume: 1.0m, Department: "Pediatrics", LabelCount: 2, StockerCode: 99),
    "Pediatrics ward", barcode);

var tray = new List<Tube>
{
    MakeTube(1, false, "A12345678901A"),
    MakeTube(2, true, "A12345678902A"),
};

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

var errors = await handshake.SendAsync(tray);
if (errors.Count > 0)
{
    foreach (var e in errors) Console.WriteLine($"{e.Field}: {e.Message}");
    return;
}

Console.WriteLine($"Sent tray {trayNo}. Press Initialize in the ROBO app now. Ctrl+C cancels.");

var id = ControlFileName.From(tray[0].TubeOrder);
var started = DateTime.Now;

try
{
    RoboReply reply;
    while ((reply = await handshake.CheckReplyAsync(id, cts.Token)) == RoboReply.Pending)
    {
        Console.Write($"\rWaiting {(DateTime.Now - started).ToString(@"mm\:ss")} ...");
        await Task.Delay(2000, cts.Token);
    }

    Console.WriteLine($"\nMachine says: {reply}");
    Console.WriteLine($"DEF still there: {await folder.ExistsAsync(id.BuildDef())} (should be False)");

    await handshake.CleanupAsync(tray);
}
catch (OperationCanceledException)
{
    Console.WriteLine("\nCancelling ...");
    await handshake.CancelAsync(tray);
}

Console.WriteLine("Files left in the folder:");
foreach (var n in await folder.ListAsync()) Console.WriteLine($"  {n}");
Console.WriteLine("Done.");