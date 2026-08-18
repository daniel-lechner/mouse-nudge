namespace mouse_nudge;

sealed class NudgeScheduler : IDisposable
{
    const int SelfInputToleranceMs = 500;

    readonly AppSettings settings;
    readonly MouseNudger nudger = new();
    readonly System.Windows.Forms.Timer timer;

    int secondsRemaining;
    bool isNudging;
    bool isPaused;
    bool hasSelfInput;
    uint lastSelfInputTick;

    public event Action<int>? CountdownChanged;
    public event Action<bool>? StateChanged;
    public event Action<bool>? PauseChanged;

    public NudgeScheduler(AppSettings settings)
    {
        this.settings = settings;

        timer = new System.Windows.Forms.Timer { Interval = 1000 };
        timer.Tick += OnTick;
    }

    public bool IsRunning => timer.Enabled;

    public bool IsPaused => isPaused;

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
        isPaused = false;

        StateChanged?.Invoke(false);
    }

    void OnTick(object? sender, EventArgs e)
    {
        ApplyExecutionState();

        bool shouldPause = settings.PauseWhileActive && !isNudging && IsUserActive();

        if (shouldPause != isPaused)
        {
            isPaused = shouldPause;
            PauseChanged?.Invoke(isPaused);

            if (!isPaused)
            {
                CountdownChanged?.Invoke(secondsRemaining);
            }
        }

        if (isPaused)
        {
            return;
        }

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
            lastSelfInputTick = (uint)Environment.TickCount;
            hasSelfInput = true;
            isNudging = false;
        }
    }

    bool IsUserActive()
    {
        uint lastInput = IdleDetector.GetLastInputTick();
        uint now = (uint)Environment.TickCount;
        uint idleMs = unchecked(now - lastInput);

        if (idleMs >= (uint)settings.ResumeDelaySeconds * 1000)
        {
            return false;
        }

        if (hasSelfInput && unchecked((int)(lastInput - lastSelfInputTick)) <= SelfInputToleranceMs)
        {
            return false;
        }

        return true;
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
