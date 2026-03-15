using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SurfaceInputResetApp;

public sealed class DeviceService
{
    private static readonly string[] CandidateClasses = ["Keyboard", "Mouse", "HIDClass"];
    private static readonly Regex SurfaceRegex = new("Surface", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex TypeCoverRegex = new("Type Cover", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex TouchpadRegex = new("Touchpad|Trackpad", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex MicrosoftRegex = new("Microsoft", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex InstanceRegex = new("MSHW|MSMN|HID\\\\VID_045E", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex GenericInputRegex = new(
        "HID Keyboard Device|HID-compliant mouse|HID-compliant touch pad",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public IReadOnlyList<DeviceCandidate> GetCandidateDevices()
    {
        var devices = new Dictionary<string, DeviceCandidate>(StringComparer.OrdinalIgnoreCase);
        var json = RunPowerShell(
            "$ErrorActionPreference = 'Stop'; " +
            "$devices = foreach ($className in 'Keyboard','Mouse','HIDClass') { " +
            "Get-PnpDevice -Class $className -PresentOnly -ErrorAction SilentlyContinue | " +
            "Select-Object FriendlyName, Class, Manufacturer, Status, InstanceId }; " +
            "$devices | ConvertTo-Json -Depth 3");

        var results = JsonSerializer.Deserialize<List<PowerShellDevice>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? [];

        foreach (var result in results)
        {
            var className = result.Class?.Trim() ?? string.Empty;
            var instanceId = result.InstanceId?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(className) || string.IsNullOrWhiteSpace(instanceId))
            {
                continue;
            }

            if (!CandidateClasses.Contains(className, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            var friendlyName = string.IsNullOrWhiteSpace(result.FriendlyName) ? "(no friendly name)" : result.FriendlyName.Trim();
            var manufacturer = string.IsNullOrWhiteSpace(result.Manufacturer) ? "(unknown)" : result.Manufacturer.Trim();
            var status = string.IsNullOrWhiteSpace(result.Status) ? "Unknown" : result.Status.Trim();
            var score = ScoreDevice(friendlyName, manufacturer, instanceId);

            devices[instanceId] = new DeviceCandidate
            {
                FriendlyName = friendlyName,
                ClassName = className,
                Manufacturer = manufacturer,
                Status = status,
                InstanceId = instanceId,
                Score = score
            };
        }

        var preferred = devices.Values
            .Where(device => device.Score > 0)
            .OrderByDescending(device => device.Score)
            .ThenBy(device => device.FriendlyName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (preferred.Count > 0)
        {
            return preferred;
        }

        return devices.Values
            .OrderBy(device => device.FriendlyName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task RestartDeviceAsync(string instanceId, CancellationToken cancellationToken)
    {
        await RunPnPUtilAsync($"/disable-device \"{instanceId}\" /force", cancellationToken);
        await Task.Delay(1500, cancellationToken);
        await RunPnPUtilAsync($"/enable-device \"{instanceId}\"", cancellationToken);
    }

    private static string RunPowerShell(string script)
    {
        var escapedScript = script.Replace("\"", "`\"");
        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{escapedScript}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var stdOut = process.StandardOutput.ReadToEnd();
        var stdErr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"PowerShell device query failed with code {process.ExitCode}.{(string.IsNullOrWhiteSpace(stdErr) ? string.Empty : $"{Environment.NewLine}{stdErr.Trim()}")}");
        }

        return stdOut;
    }

    private static int ScoreDevice(string friendlyName, string manufacturer, string instanceId)
    {
        var score = 0;

        if (SurfaceRegex.IsMatch(friendlyName))
        {
            score += 5;
        }

        if (TypeCoverRegex.IsMatch(friendlyName))
        {
            score += 4;
        }

        if (TouchpadRegex.IsMatch(friendlyName))
        {
            score += 4;
        }

        if (MicrosoftRegex.IsMatch(manufacturer))
        {
            score += 2;
        }

        if (InstanceRegex.IsMatch(instanceId))
        {
            score += 2;
        }

        if (GenericInputRegex.IsMatch(friendlyName))
        {
            score += 1;
        }

        return score;
    }

    private static async Task RunPnPUtilAsync(string arguments, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "pnputil.exe",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var stdOutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stdErrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var stdOut = await stdOutTask;
        var stdErr = await stdErrTask;

        if (process.ExitCode != 0)
        {
            var details = string.Join(
                Environment.NewLine,
                new[] { stdOut.Trim(), stdErr.Trim() }.Where(text => !string.IsNullOrWhiteSpace(text)));

            throw new InvalidOperationException(
                $"pnputil exited with code {process.ExitCode}.{(details.Length > 0 ? $"{Environment.NewLine}{details}" : string.Empty)}");
        }
    }

    private sealed class PowerShellDevice
    {
        public string? FriendlyName { get; init; }

        public string? Class { get; init; }

        public string? Manufacturer { get; init; }

        public string? Status { get; init; }

        public string? InstanceId { get; init; }
    }
}
