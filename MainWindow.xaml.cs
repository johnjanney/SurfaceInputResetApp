using System.Collections.ObjectModel;
using System.Windows;

namespace SurfaceInputResetApp;

public partial class MainWindow : Window
{
    private readonly DeviceService _deviceService = new();
    private readonly ObservableCollection<DeviceCandidate> _devices = [];

    public MainWindow()
    {
        InitializeComponent();
        DeviceGrid.ItemsSource = _devices;
        Loaded += async (_, _) => await LoadDevicesAsync();
    }

    private async Task LoadDevicesAsync()
    {
        try
        {
            ToggleUi(false);
            WriteLog("Refreshing device list.");

            var devices = await Task.Run(_deviceService.GetCandidateDevices);
            _devices.Clear();

            foreach (var device in devices)
            {
                _devices.Add(device);
            }

            DeviceGrid.UnselectAll();
            foreach (var device in devices.Where(device => device.Score >= 6))
            {
                DeviceGrid.SelectedItems.Add(device);
            }

            WriteLog($"Loaded {_devices.Count} device candidates.");
        }
        catch (Exception ex)
        {
            WriteLog($"Refresh failed: {ex.Message}");
        }
        finally
        {
            ToggleUi(true);
        }
    }

    private void ToggleUi(bool isEnabled)
    {
        RefreshButton.IsEnabled = isEnabled;
        SelectSuggestedButton.IsEnabled = isEnabled;
        RestartSelectedButton.IsEnabled = isEnabled;
        CopyIdsButton.IsEnabled = isEnabled;
        DeviceGrid.IsEnabled = isEnabled;
    }

    private void WriteLog(string message)
    {
        LogBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        LogBox.ScrollToEnd();
    }

    private async void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        await LoadDevicesAsync();
    }

    private void SelectSuggestedButton_OnClick(object sender, RoutedEventArgs e)
    {
        DeviceGrid.UnselectAll();

        foreach (var device in _devices.Where(device => device.Score >= 6))
        {
            DeviceGrid.SelectedItems.Add(device);
        }

        WriteLog("Selected the highest-confidence Surface-related devices.");
    }

    private void CopyIdsButton_OnClick(object sender, RoutedEventArgs e)
    {
        var selected = DeviceGrid.SelectedItems.Cast<DeviceCandidate>().ToList();
        if (selected.Count == 0)
        {
            WriteLog("No devices selected to copy.");
            return;
        }

        Clipboard.SetText(string.Join(Environment.NewLine, selected.Select(device => device.InstanceId)));
        WriteLog($"Copied {selected.Count} instance ID(s) to the clipboard.");
    }

    private async void RestartSelectedButton_OnClick(object sender, RoutedEventArgs e)
    {
        var selected = DeviceGrid.SelectedItems.Cast<DeviceCandidate>().ToList();
        if (selected.Count == 0)
        {
            MessageBox.Show(
                "Select at least one device first.",
                "Surface Input Reset",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        try
        {
            ToggleUi(false);

            foreach (var device in selected)
            {
                WriteLog($"Restarting {device.FriendlyName} [{device.ClassName}].");
                await _deviceService.RestartDeviceAsync(device.InstanceId, CancellationToken.None);
                WriteLog($"Restarted {device.InstanceId}.");
            }
        }
        catch (Exception ex)
        {
            WriteLog($"Restart failed: {ex.Message}");
        }
        finally
        {
            ToggleUi(true);
        }
    }
}
