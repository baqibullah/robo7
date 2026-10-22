using RoboLink.Infrastructure;

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

var testName = "smoke-test.txt";
Console.WriteLine($"Writing {testName} ...");
await folder.WriteAsync(testName, "hello"u8.ToArray());
Console.WriteLine($"Exists: {await folder.ExistsAsync(testName)}");
foreach (var name in await folder.ListAsync())
    Console.WriteLine($"  {name}");

Console.WriteLine("Deleting it twice (second one must not fail) ...");
await folder.DeleteAsync(testName);
await folder.DeleteAsync(testName);
Console.WriteLine($"Exists: {await folder.ExistsAsync(testName)}");
Console.WriteLine("Done.");