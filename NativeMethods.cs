using System.Runtime.InteropServices;

namespace mouse_nudge;

[StructLayout(LayoutKind.Sequential)]
struct MouseInput
{
    public int Dx;
    public int Dy;
    public uint MouseData;
    public uint Flags;
    public uint Time;
    public nint ExtraInfo;
}

[StructLayout(LayoutKind.Sequential)]
struct Input
{
    public uint Type;
    public MouseInput Mouse;
}

[StructLayout(LayoutKind.Sequential)]
struct LastInputInfo
{
    public uint Size;
    public uint Time;
}

static class NativeMethods
{
    public const uint InputMouse = 0;
    public const uint MouseEventMove = 0x0001;
    public const uint MouseEventAbsolute = 0x8000;
    public const uint MouseEventVirtualDesk = 0x4000;

    public const int SmXVirtualScreen = 76;
    public const int SmYVirtualScreen = 77;
    public const int SmCxVirtualScreen = 78;
    public const int SmCyVirtualScreen = 79;

    public const uint EsContinuous = 0x80000000;
    public const uint EsSystemRequired = 0x00000001;
    public const uint EsDisplayRequired = 0x00000002;

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint SendInput(uint inputCount, ref Input inputs, int size);

    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(int index);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetLastInputInfo(ref LastInputInfo info);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyIcon(nint handle);

    [DllImport("kernel32.dll")]
    public static extern uint SetThreadExecutionState(uint flags);
}
