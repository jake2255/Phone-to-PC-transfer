using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace phone_to_pc_transfer_app.Core
{
    public class TransferServer : IDisposable
    {
        private readonly int _port;
        private readonly string _saveFolder;
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private Task? _acceptLoop;

        public event EventHandler<FileReceivedEventArgs>? FileReceived;

        public event EventHandler<TextReceivedEventArgs>? TextReceived;

    }
}
