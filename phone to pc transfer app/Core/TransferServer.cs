using System;
using System.Collections.Generic;
using System.Text;

namespace phone_to_pc_transfer_app.Core
{
    public class TransferServer : IAsyncDisposable
    {
        private readonly int _port;
        private readonly string _saveFolder;
        private WebApplication? _app;

        public event EventHandler<FileReceivedEventArgs>? FileReceived;

        public event EventHandler<TextReceivedEventArgs>? TextReceived;

    }
}
