namespace mouse_nudge;

sealed class NudgeScheduler : IDisposable
{
    readonly AppSettings settings;
    readonly MouseNudger nudger = new();
    readonly System.Windows.Forms.Timer timer;

    int secondsRemaining;
    bool isNudging;

    public event Action<int>? CountdownChanged;
    public event Action<bool>? StateChanged;

    public NudgeScheduler(AppSettings settings)
    {
        this.settings = settings;

        timer = new System.Windows.Forms.Timer { Interval = 1000 };
        timer.Tick += OnTick;
    }

    public bool IsRunning => timer.Enabled;

    public void Start()
    {
        if (IsRunning)
        {
            return;
        }

        ScheduleNext();
        ApplyExecutionState();
        timer.Start();

        StateChanged?.Invoke(true);
        CountdownChanged?.Invoke(secondsRemaining);
    }

    public void Stop()
    {
        if (!IsRunning)
        {
            return;
        }

        timer.Stop();
        ClearExecutionState();

        StateChanged?.Invoke(false);
    }

    void OnTick(object? sender, EventArgs e)
    {
        ApplyExecutionState();

        secondsRemaining--;

        if (secondsRemaining > 0)
        {
            CountdownChanged?.Invoke(secondsRemaining);
            return;
        }

        Fire();
        ScheduleNext();
        CountdownChanged?.Invoke(secondsRemaining);
    }

    void Fire()
    {
        if (settings.NudgeOnlyWhenIdle && IdleDetector.GetIdleTime() < TimeSpan.FromSeconds(settings.IdleThresholdSeconds))
        {
            return;
        }

        if (settings.KeepAwakeOnly || isNudging)
        {
            return;
        }

        RunNudge();
    }

    async void RunNudge()
    {
        isNudging = true;

        try
        {
            NudgeOptions options = new(
                settings.DistancePixels,
                settings.RandomDirection,
                settings.ReturnToOrigin,
                settings.SmoothMovement,
                settings.EdgePaddingPixels);

            await nudger.NudgeAsync(options);
        }
        catch (Exception)
        {
        }
        finally
        {
            isNudging = false;
        }
    }

    void ScheduleNext()
    {
        double jitter = settings.IntervalJitterPercent / 100.0;
        double factor = 1 + (((Random.Shared.NextDouble() * 2) - 1) * jitter);

        secondsRemaining = Math.Max(1, (int)Math.Round(settings.IntervalSeconds * factor));
    }

    void ApplyExecutionState() => NativeMethods.SetThreadExecutionState(settings.KeepAwakeOnly
        ? NativeMethods.EsContinuous | NativeMethods.EsSystemRequired | NativeMethods.EsDisplayRequired
        : NativeMethods.EsContinuous);

    static void ClearExecutionState() => NativeMethods.SetThreadExecutionState(NativeMethods.EsContinuous);

    public void Dispose()
    {
        timer.Stop();
        timer.Dispose();
        ClearExecutionState();
    }
}
