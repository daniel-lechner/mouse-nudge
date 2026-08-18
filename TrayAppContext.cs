namespace mouse_nudge;

sealed class TrayAppContext : ApplicationContext
{
    const string StoppedTooltip = "Mouse Nudge — stopped";
    const string PausedTooltip = "Mouse Nudge — paused (you're active)";

    readonly NotifyIcon notifyIcon;
    readonly ToolStripMenuItem toggleItem;
    readonly Icon icon;
    readonly AppSettings settings;
    readonly NudgeScheduler scheduler;
    readonly CountdownIconRenderer countdownRenderer = new();

    OptionsForm? optionsForm;

    public TrayAppContext()
    {
        settings = SettingsStore.Load();
        icon = AppIcon.Load();

        toggleItem = new ToolStripMenuItem("Start nudging");
        toggleItem.Click += OnToggleClicked;

        ToolStripMenuItem optionsItem = new("Options…");
        optionsItem.Click += OnOptionsClicked;

        ToolStripMenuItem exitItem = new("Exit");
        exitItem.Click += OnExitClicked;

        ContextMenuStrip menu = new();
        menu.Items.Add(toggleItem);
        menu.Items.Add(optionsItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);

        notifyIcon = new NotifyIcon
        {
            Icon = icon,
            Text = "Mouse Nudge",
            Visible = true,
            ContextMenuStrip = menu
        };
        notifyIcon.DoubleClick += OnOptionsClicked;

        scheduler = new NudgeScheduler(settings);
        scheduler.StateChanged += OnSchedulerStateChanged;
        scheduler.CountdownChanged += OnCountdownChanged;
        scheduler.PauseChanged += OnSchedulerPauseChanged;

        ShowBalloon("Mouse Nudge", "Running in the system tray — right-click the icon for options.");

        if (settings.StartOnLaunch)
        {
            scheduler.Start();
        }
    }

    void OnToggleClicked(object? sender, EventArgs e)
    {
        if (scheduler.IsRunning)
        {
            scheduler.Stop();
            return;
        }

        scheduler.Start();
    }

    void OnSchedulerStateChanged(bool isRunning)
    {
        toggleItem.Text = isRunning ? "Stop nudging" : "Start nudging";

        if (!isRunning)
        {
            notifyIcon.Icon = icon;
            notifyIcon.Text = StoppedTooltip;
            countdownRenderer.Release();
        }

        if (settings.ShowTrayNotifications)
        {
            ShowBalloon("Mouse Nudge", isRunning ? "Nudging started" : "Nudging stopped");
        }
    }

    void OnSchedulerPauseChanged(bool isPaused)
    {
        if (!isPaused)
        {
            return;
        }

        countdownRenderer.ApplyPaused(notifyIcon);
        notifyIcon.Text = PausedTooltip;
    }

    void OnCountdownChanged(int secondsRemaining)
    {
        if (scheduler.IsPaused)
        {
            return;
        }

        countdownRenderer.Apply(notifyIcon, secondsRemaining);
        notifyIcon.Text = $"Mouse Nudge — next nudge in {Math.Max(0, secondsRemaining)}s";
    }

    void ShowBalloon(string title, string text)
    {
        notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
        notifyIcon.BalloonTipTitle = title;
        notifyIcon.BalloonTipText = text;
        notifyIcon.ShowBalloonTip(3000);
    }

    void OnOptionsClicked(object? sender, EventArgs e)
    {
        if (optionsForm is not null && !optionsForm.IsDisposed)
        {
            if (optionsForm.WindowState == FormWindowState.Minimized)
            {
                optionsForm.WindowState = FormWindowState.Normal;
            }

            optionsForm.Activate();
            optionsForm.BringToFront();
            return;
        }

        optionsForm = new OptionsForm(settings);
        optionsForm.FormClosed += (_, _) => optionsForm = null;
        optionsForm.Show();
        optionsForm.Activate();
    }

    void OnExitClicked(object? sender, EventArgs e)
    {
        scheduler.Stop();
        notifyIcon.Visible = false;
        ExitThread();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            scheduler.Dispose();
            notifyIcon.Visible = false;
            notifyIcon.Icon = null;
            notifyIcon.Dispose();
            countdownRenderer.Dispose();
            icon.Dispose();
            optionsForm?.Dispose();
        }

        base.Dispose(disposing);
    }
}
