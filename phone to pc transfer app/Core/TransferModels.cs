using System;
using System.Collections.Generic;
using System.Text;

namespace phone_to_pc_transfer_app.Core
{
    public class FileReceivedEventArgs : EventArgs
    {
        public string FileName { get; init; } = string.Empty;
        public string SavedPath { get; init; } = string.Empty;
        public long SizeBytes { get; init; }
        public string SenderIpAddress { get; init; } = string.Empty;
    }

    public class TextReceivedEventArgs : EventArgs
    {
        public string Text { get; init; } = string.Empty;
        public string SenderIpAddress { get; init; } = string.Empty;
    }
}
