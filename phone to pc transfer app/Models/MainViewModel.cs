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
            
        }

        public void Initialize()
        {

        }

        private void OnDevicesChanged(object? sender, EventArgs e)
        {

        }

        private void OnFileReceived(object? sender, FileReceivedEventArgs e)
        {

        }

        private void OnTextReceived(object? sender, TextReceivedEventArgs e)
        {

        }

        private void OnTransferFailed(object? sender, Exception ex)
        {

        }
    }
}
