namespace mouse_nudge;

sealed record NudgeOptions(int Distance, bool RandomDirection, bool ReturnToOrigin, bool Smooth, int EdgePadding);

sealed class MouseNudger
{
    const int SmoothDurationMs = 250;
    const int SmoothStepMs = 15;
    const int ReturnPauseMs = 300;

    public async Task NudgeAsync(NudgeOptions options, CancellationToken cancellationToken = default)
    {
        Point origin = Cursor.Position;
        Point target = CalculateTarget(origin, options);

        await MoveAsync(origin, target, options.Smooth, cancellationToken);

        if (options.ReturnToOrigin)
        {
            await Task.Delay(ReturnPauseMs, cancellationToken);
            await MoveAsync(target, origin, options.Smooth, cancellationToken);
        }
    }

    static Point CalculateTarget(Point origin, NudgeOptions options)
    {
        double angle = options.RandomDirection ? Random.Shared.NextDouble() * Math.Tau : 0;
        int x = (int)Math.Round(origin.X + (Math.Cos(angle) * options.Distance));
        int y = (int)Math.Round(origin.Y + (Math.Sin(angle) * options.Distance));

        return ClampToScreen(new Point(x, y), Screen.FromPoint(origin).Bounds, options.EdgePadding);
    }

    static Point ClampToScreen(Point point, Rectangle bounds, int padding)
    {
        int paddingX = Math.Min(padding, Math.Max(0, (bounds.Width - 1) / 2));
        int paddingY = Math.Min(padding, Math.Max(0, (bounds.Height - 1) / 2));

        return new Point(
            Math.Clamp(point.X, bounds.Left + paddingX, bounds.Right - 1 - paddingX),
            Math.Clamp(point.Y, bounds.Top + paddingY, bounds.Bottom - 1 - paddingY));
    }

    static async Task MoveAsync(Point from, Point to, bool smooth, CancellationToken cancellationToken)
    {
        if (!smooth)
        {
            SetCursorPosition(to);
            return;
        }

        int steps = Math.Max(1, SmoothDurationMs / SmoothStepMs);

        for (int step = 1; step <= steps; step++)
        {
            double progress = EaseInOutCubic((double)step / steps);
            SetCursorPosition(new Point(
                (int)Math.Round(from.X + ((to.X - from.X) * progress)),
                (int)Math.Round(from.Y + ((to.Y - from.Y) * progress))));

            await Task.Delay(SmoothStepMs, cancellationToken);
        }
    }

    static double EaseInOutCubic(double t) =>
        t < 0.5 ? 4 * t * t * t : 1 - (Math.Pow((-2 * t) + 2, 3) / 2);

    static void SetCursorPosition(Point position) => Cursor.Position = position;
}
