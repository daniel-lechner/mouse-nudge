namespace mouse_nudge;

sealed class AppSettings
{
    public int IntervalSeconds { get; set; } = 30;
    public int IntervalJitterPercent { get; set; } = 10;
    public int DistancePixels { get; set; } = 10;
    public bool RandomDirection { get; set; } = true;
    public bool ReturnToOrigin { get; set; }
    public bool SmoothMovement { get; set; } = true;
    public int EdgePaddingPixels { get; set; } = 50;
    public bool KeepAwakeOnly { get; set; }
    public bool NudgeOnlyWhenIdle { get; set; }
    public int IdleThresholdSeconds { get; set; } = 60;
    public bool StartOnLaunch { get; set; }
    public bool ShowTrayNotifications { get; set; } = true;
}
