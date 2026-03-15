namespace SurfaceInputResetApp;

public sealed class DeviceCandidate
{
    public required string FriendlyName { get; init; }

    public required string ClassName { get; init; }

    public required string Manufacturer { get; init; }

    public required string Status { get; init; }

    public required string InstanceId { get; init; }

    public required int Score { get; init; }
}
