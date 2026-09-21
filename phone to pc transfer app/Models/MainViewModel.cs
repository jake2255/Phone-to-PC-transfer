using phone_to_pc_transfer_app.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


namespace phone_to_pc_transfer_app.Models
{
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        private readonly DeviceListener _listener;
        private readonly DeviceRegistry _registry;
        private readonly DeviceBeacon _beacon;
        private readonly TransferServer _server;
        private readonly TransferClient _client;

        [ObservableProperty]
        private ObservableCollection<DiscoveredDevice> devices = new();

        [ObservableProperty]
        private DiscoveredDevice? selectedDevice;

        [ObservableProperty]
        private string textToSend = string.Empty;

        [ObservableProperty]
        private string statusMessage = "Ready";

        [ObservableProperty]
        private bool isBusy;

        public MainViewModel()
        {
            var deviceId = AppSettings.DeviceId;

            _listener = new DeviceListener(deviceId);
            _registry = new DeviceRegistry(_listener);
            _beacon = new DeviceBeacon(deviceId, AppSettings.DeviceName, AppSettings.Platform, AppSettings.TransferPort);
            _server = new TransferServer(AppSettings.TransferPort, AppSettings.SaveFolder);
            _client = new TransferClient();

            _registry.DevicesChanged += OnDevicesChanged;
            _server.FileReceived += OnFileReceived;
            _server.TextReceived += OnTextReceived;
            _server.TransferFailed += OnTransferFailed;
        }

        public void Initialize()
        {
            _listener.Start();
            _beacon.Start();
            _server.Start();
        }

        private void OnDevicesChanged(object? sender, EventArgs e)
        {
            var current = _registry.GetDevices();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Devices.Clear();
                foreach (var device in current)
                {
                    Devices.Add(device);
                }
            });
        }

        private void OnFileReceived(object? sender, FileReceivedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusMessage = $"Received '{e.FileName}' from {e.SenderIpAddress}.";
            });
        }

        private void OnTextReceived(object? sender, TextReceivedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusMessage = $"Text from {e.SenderIpAddress}: {e.Text}.";
            });
        }

        private void OnTransferFailed(object? sender, Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusMessage = $"Transfer failed: {ex.Message}";
            });
        }

        [RelayCommand]
        private async Task SendFileAsync()
        {
            if (SelectedDevice is null)
            {
                StatusMessage = "Select a device first.";
                return;
            }

            var pickResult = await FilePicker.Default.PickAsync();
            if (pickResult is null)
                return;

            IsBusy = true;
            StatusMessage = $"Sending '{pickResult.FileName}' to {SelectedDevice.Name}...";

            try
            {
                await _client.SendFileAsync(SelectedDevice.IpAddress, SelectedDevice.TransferPort, pickResult.FullPath);
                StatusMessage = $"Sent '{pickResult.FileName}' to {SelectedDevice.Name}.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Send failed: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SendTextAsync()
        {
            if (SelectedDevice is null)
            {
                StatusMessage = "Select a device first.";
                return;
            }

            if (string.IsNullOrWhiteSpace(TextToSend))
                return;

            IsBusy = true;

            try
            {
                await _client.SendTextAsync(SelectedDevice.IpAddress, SelectedDevice.TransferPort, TextToSend);
                StatusMessage = $"Sent text to {SelectedDevice.Name}.";
                TextToSend = string.Empty;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Send failed: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void Dispose()
        {
            _registry.DevicesChanged -= OnDevicesChanged;
            _server.FileReceived -= OnFileReceived;
            _server.TextReceived -= OnTextReceived;
            _server.TransferFailed -= OnTransferFailed;

            _beacon.Dispose();
            _listener.Dispose();
            _registry.Dispose();
            _server.Dispose();
        }
    }
}
