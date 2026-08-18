using System.Runtime.InteropServices;

namespace mouse_nudge;

static class IdleDetector
{
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
