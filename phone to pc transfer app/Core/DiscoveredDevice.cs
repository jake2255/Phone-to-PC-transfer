using System;
using System.Collections.Generic;
using System.Text;

namespace phone_to_pc_transfer_app.Core
{
    public class DiscoveredDevice
    {
        public string DeviceId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Platform { get; set; } = string.Empty;

        public string IpAddress { get; set; } = string.Empty;

        public int TransferPort { get; set; }

        public DateTime lastSeen { get; set; }

        public override string ToString() => $"{Name} ({Platform}) @ {IpAddress}:{TransferPort}";
    }

    public class BeaconPayload
    {
        public string DeviceId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public int TransferPort { get; set; }

    }
}
