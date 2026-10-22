namespace RoboLink.Infrastructure;

public record SmbOptions
{
    public required string Host { get; init; } // machine IP, e.g. 192.168.0.150
    public string Share { get; init; } = "robo7_data";
    public string Folder { get; init; } = @"PRINT\DATA"; // path inside the share
    public required string User { get; init; } // robo7user
    public required string Password { get; init; }
    public string Domain { get; init; } = ""; // empty for a local account
}
