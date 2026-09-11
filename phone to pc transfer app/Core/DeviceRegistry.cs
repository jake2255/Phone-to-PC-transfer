using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace phone_to_pc_transfer_app.Core
{
    public class DeviceRegistry : IDisposable
    {
        private readonly ConcurrentDictionary<string, DiscoveredDevice> _devices = new();
        private readonly TimeSpan _expiry;
        private readonly Timer _expiryTimer;
        private readonly DeviceListener _listener;

        public event EventHandler? DevicesChanged;

        public DeviceRegistry(DeviceListener listener, TimeSpan? expiry = null)
        {
            _listener = listener;
            _expiry = expiry ?? TimeSpan.FromSeconds(10);
            _listener.DeviceAnnounced += OnDeviceAnnounced;

            _expiryTimer = new Timer(_ => SweepExpired(), null, _expiry, _expiry);
        }

        private void OnDeviceAnnounced(object? sender, DiscoveredDevice device)
        {
            _devices.AddOrUpdate(device.DeviceId, device, (_, _) => device);
            DevicesChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SweepExpired()
        {
            var cutoff = DateTime.UtcNow - _expiry;
            var staleIds = _devices.Where(kv => kv.Value.lastSeen < cutoff).Select(kv => kv.Key).ToList();

            if (staleIds.Count == 0)
                return;

            foreach (var id in staleIds)
                _devices.TryRemove(id, out _);

            DevicesChanged?.Invoke(this, EventArgs.Empty);
        }

        public IReadOnlyList<DiscoveredDevice> GetDevices() => _devices.Values.ToList();

        public void Dispose()
        {
            _listener.DeviceAnnounced -= OnDeviceAnnounced;
            _expiryTimer.Dispose();
        }

    }
}
