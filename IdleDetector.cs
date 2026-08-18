using System.Runtime.InteropServices;

namespace mouse_nudge;

static class IdleDetector
{
    const uint FallbackIdleMs = 3600000;

    public static uint GetLastInputTick()
    {
        LastInputInfo info = new() { Size = (uint)Marshal.SizeOf<LastInputInfo>() };

        unchecked
        {
            return NativeMethods.GetLastInputInfo(ref info) ? info.Time : (uint)Environment.TickCount - FallbackIdleMs;
        }
    }

    public static TimeSpan GetIdleTime()
    {
        LastInputInfo info = new() { Size = (uint)Marshal.SizeOf<LastInputInfo>() };

        if (!NativeMethods.GetLastInputInfo(ref info))
        {
            return TimeSpan.Zero;
        }

        unchecked
        {
            uint elapsed = (uint)Environment.TickCount - info.Time;
            return TimeSpan.FromMilliseconds(elapsed);
        }
    }
}
