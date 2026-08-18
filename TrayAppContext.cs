namespace mouse_nudge;

sealed class TrayAppContext : ApplicationContext
{
    const string IdleTooltip = "Mouse Nudge — stopped";
    const string RunningTooltip = "Mouse Nudge — running";

    readonly NotifyIcon notifyIcon;
    readonly ToolStripMenuItem toggleItem;
    readonly Icon icon;

    bool isRunning;
    OptionsForm? optionsForm;

    public TrayAppContext()
    {
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

        notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
        notifyIcon.BalloonTipTitle = "Mouse Nudge";
        notifyIcon.BalloonTipText = "Running in the system tray — right-click the icon for options.";
        notifyIcon.ShowBalloonTip(3000);
    }

    void OnToggleClicked(object? sender, EventArgs e)
    {
        isRunning = !isRunning;
        toggleItem.Text = isRunning ? "Stop nudging" : "Start nudging";
        notifyIcon.Text = isRunning ? RunningTooltip : IdleTooltip;
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

        optionsForm = new OptionsForm();
        optionsForm.FormClosed += (_, _) => optionsForm = null;
        optionsForm.Show();
        optionsForm.Activate();
    }

    void OnExitClicked(object? sender, EventArgs e)
    {
        notifyIcon.Visible = false;
        ExitThread();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            icon.Dispose();
            optionsForm?.Dispose();
        }

        base.Dispose(disposing);
    }
}
