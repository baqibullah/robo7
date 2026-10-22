var password = Environment.GetEnvironmentVariable("ROBO_PASSWORD")
    ?? throw new InvalidOperationException("Set ROBO_PASSWORD first.");

var folder = new SmbRoboFolder(new SmbOptions
{
    Host = "192.168.0.150",
    User = "robo7user",
    Password = password,
});

Console.WriteLine(@"Listing PRINT\DATA ...");
foreach (var name in await folder.ListAsync())
    Console.WriteLine($"  {name}");
Console.WriteLine("Done.");