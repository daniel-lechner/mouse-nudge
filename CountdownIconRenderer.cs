using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace mouse_nudge;

sealed class CountdownIconRenderer : IDisposable
{
    const int IconSize = 32;

    static readonly Color BackgroundColor = Color.FromArgb(0x2D, 0x2D, 0x30);

    Icon? currentIcon;
    nint currentHandle;

    public void Apply(NotifyIcon notifyIcon, int secondsRemaining)
    {
        string text = FormatRemaining(secondsRemaining);

        using Bitmap bitmap = new(IconSize, IconSize, PixelFormat.Format32bppArgb);

        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            graphics.Clear(Color.Transparent);

            using SolidBrush background = new(BackgroundColor);
            graphics.FillEllipse(background, 0, 0, IconSize - 1, IconSize - 1);

            using Font font = new("Segoe UI", GetFontSize(text.Length), FontStyle.Bold, GraphicsUnit.Point);
            using StringFormat format = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            graphics.DrawString(text, font, Brushes.White, new RectangleF(0, 0, IconSize, IconSize), format);
        }

        nint handle = bitmap.GetHicon();
        Icon icon = Icon.FromHandle(handle);

        notifyIcon.Icon = icon;

        ReleaseCurrent();

        currentIcon = icon;
        currentHandle = handle;
    }

    public void Release() => ReleaseCurrent();

    static string FormatRemaining(int secondsRemaining)
    {
        int seconds = Math.Max(0, secondsRemaining);

        return seconds < 100 ? seconds.ToString() : $"{seconds / 60}m";
    }

    static float GetFontSize(int length) => length switch
    {
        <= 1 => 18f,
        2 => 14f,
        _ => 10f
    };

    void ReleaseCurrent()
    {
        if (currentIcon is not null)
        {
            currentIcon.Dispose();
            currentIcon = null;
        }

        if (currentHandle != 0)
        {
            NativeMethods.DestroyIcon(currentHandle);
            currentHandle = 0;
        }
    }

    public void Dispose() => ReleaseCurrent();
}
