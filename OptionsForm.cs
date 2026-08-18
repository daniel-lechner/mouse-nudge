namespace mouse_nudge;

sealed class OptionsForm : Form
{
    readonly NumericUpDown intervalUpDown = CreateUpDown(1, 3600, 30);
    readonly TrackBar jitterTrackBar = CreateTrackBar(0, 50, 0);
    readonly Label jitterValueLabel = CreateValueLabel();
    readonly TrackBar distanceTrackBar = CreateTrackBar(1, 100, 10);
    readonly Label distanceValueLabel = CreateValueLabel();
    readonly CheckBox randomDirectionCheckBox = CreateCheckBox("Random direction", true);
    readonly CheckBox returnToOriginCheckBox = CreateCheckBox("Return cursor to original position", true);
    readonly CheckBox smoothMovementCheckBox = CreateCheckBox("Smooth movement", true);
    readonly NumericUpDown edgePaddingUpDown = CreateUpDown(0, 500, 50);
    readonly RadioButton nudgeRadio = CreateRadioButton("Move cursor (nudge)", true);
    readonly RadioButton awakeRadio = CreateRadioButton("Keep awake only (no cursor movement)", false);
    readonly CheckBox idleOnlyCheckBox = CreateCheckBox("Only nudge when user is idle", true);
    readonly NumericUpDown idleThresholdUpDown = CreateUpDown(5, 600, 60);
    readonly CheckBox pauseWhileActiveCheckBox = CreateCheckBox("Pause countdown while user is active", true);
    readonly NumericUpDown resumeDelayUpDown = CreateUpDown(1, 600, 10);
    readonly CheckBox startOnLaunchCheckBox = CreateCheckBox("Start nudging when app launches", false);
    readonly CheckBox showNotificationsCheckBox = CreateCheckBox("Show tray notification on start/stop", true);
    readonly Button previewButton = new()
    {
        Text = "Preview nudge",
        AutoSize = true,
        MinimumSize = new Size(120, 28),
        Anchor = AnchorStyles.Left,
        Margin = new Padding(0, 8, 8, 2)
    };
    readonly ToolTip toolTip = new()
    {
        AutoPopDelay = 15000,
        InitialDelay = 400,
        ReshowDelay = 200
    };
    readonly MouseNudger nudger = new();
    readonly AppSettings settings;

    bool isLoading = true;

    public OptionsForm(AppSettings settings)
    {
        this.settings = settings;

        Text = "Mouse Nudge — Options";
        Icon = AppIcon.Load();
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = true;
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 4
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        root.Controls.Add(BuildBehaviorGroup(), 0, 0);
        root.Controls.Add(BuildModeGroup(), 0, 1);
        root.Controls.Add(BuildApplicationGroup(), 0, 2);
        root.Controls.Add(BuildButtons(), 0, 3);

        Controls.Add(root);

        ApplySettingsToControls();
        WireEvents();

        isLoading = false;
    }

    static GroupBox CreateGroupBox(string text) => new()
    {
        Text = text,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        Dock = DockStyle.Fill,
        Padding = new Padding(10, 6, 10, 10),
        Margin = new Padding(0, 0, 0, 10)
    };

    static TableLayoutPanel CreateGrid(int rowCount)
    {
        TableLayoutPanel grid = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 3,
            RowCount = rowCount
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        return grid;
    }

    static Label CreateLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        Margin = new Padding(0, 6, 8, 6)
    };

    static Label CreateValueLabel() => new()
    {
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        MinimumSize = new Size(60, 0),
        Margin = new Padding(0, 8, 8, 6)
    };

    static NumericUpDown CreateUpDown(int minimum, int maximum, int value) => new()
    {
        Minimum = minimum,
        Maximum = maximum,
        Value = value,
        Width = 90,
        Anchor = AnchorStyles.Left,
        Margin = new Padding(0, 3, 8, 3)
    };

    static TrackBar CreateTrackBar(int minimum, int maximum, int value) => new()
    {
        Minimum = minimum,
        Maximum = maximum,
        Value = value,
        Width = 220,
        TickStyle = TickStyle.None,
        AutoSize = false,
        Height = 30,
        Anchor = AnchorStyles.Left,
        Margin = new Padding(0, 3, 8, 3)
    };

    static CheckBox CreateCheckBox(string text, bool isChecked) => new()
    {
        Text = text,
        Checked = isChecked,
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        Margin = new Padding(0, 6, 8, 6)
    };

    static RadioButton CreateRadioButton(string text, bool isChecked) => new()
    {
        Text = text,
        Checked = isChecked,
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        Margin = new Padding(0, 6, 8, 6)
    };

    static void AddFullWidth(TableLayoutPanel grid, Control control, int row)
    {
        grid.Controls.Add(control, 0, row);
        grid.SetColumnSpan(control, 3);
    }

    void SetTip(Control control, string text) => toolTip.SetToolTip(control, text);

    void AddRow(TableLayoutPanel grid, int row, string labelText, Control input, Control trailing, string tip, int indent = 0)
    {
        Label label = CreateLabel(labelText);
        label.Margin = new Padding(indent, 6, 8, 6);

        grid.Controls.Add(label, 0, row);
        grid.Controls.Add(input, 1, row);
        grid.Controls.Add(trailing, 2, row);

        SetTip(label, tip);
        SetTip(input, tip);
    }

    void AddRow(TableLayoutPanel grid, int row, string labelText, Control input, string unit, string tip, int indent = 0) =>
        AddRow(grid, row, labelText, input, CreateLabel(unit), tip, indent);

    void AddCheckRow(TableLayoutPanel grid, Control control, int row, string tip)
    {
        AddFullWidth(grid, control, row);
        SetTip(control, tip);
    }

    GroupBox BuildBehaviorGroup()
    {
        TableLayoutPanel grid = CreateGrid(8);

        AddRow(grid, 0, "Interval", intervalUpDown, "seconds", "How often the mouse is nudged. The countdown in the tray icon shows the time until the next nudge.");
        AddRow(grid, 1, "Interval jitter", jitterTrackBar, jitterValueLabel, "Randomly varies each interval by up to this percentage so the nudges look less mechanical.");
        AddRow(grid, 2, "Distance", distanceTrackBar, distanceValueLabel, "How far the cursor moves during a nudge, in pixels.");

        AddCheckRow(grid, randomDirectionCheckBox, 3, "Move the cursor in a random direction each time instead of always to the right.");
        AddCheckRow(grid, returnToOriginCheckBox, 4, "After nudging, move the cursor back to where it was.");
        AddCheckRow(grid, smoothMovementCheckBox, 5, "Glide the cursor to its target instead of jumping instantly.");

        AddRow(grid, 6, "Screen edge padding", edgePaddingUpDown, "px", "Keeps nudges away from the screen edges by this many pixels so nothing at the border gets clicked or triggered.");

        AddCheckRow(grid, previewButton, 7, "Performs one nudge right now with the current settings.");

        GroupBox group = CreateGroupBox("Nudge behavior");
        group.Controls.Add(grid);
        return group;
    }

    GroupBox BuildModeGroup()
    {
        TableLayoutPanel grid = CreateGrid(6);

        AddCheckRow(grid, nudgeRadio, 0, "Physically moves the mouse cursor at each interval.");
        AddCheckRow(grid, awakeRadio, 1, "Doesn't move the cursor; instead tells Windows to stay awake and keep the display on.");
        AddCheckRow(grid, idleOnlyCheckBox, 2, "Skips a nudge if you used mouse or keyboard within the idle threshold. The countdown keeps running; only the nudge itself is skipped.");

        AddRow(grid, 3, "Idle threshold", idleThresholdUpDown, "seconds", "How long you must be inactive before a due nudge is actually performed.", 18);

        AddCheckRow(grid, pauseWhileActiveCheckBox, 4, "Freezes the countdown while you're using mouse or keyboard. A pause symbol is shown in the tray icon.");

        AddRow(grid, 5, "Resume after", resumeDelayUpDown, "seconds of inactivity", "How long you must be inactive before the paused countdown continues.", 18);

        GroupBox group = CreateGroupBox("Mode");
        group.Controls.Add(grid);
        return group;
    }

    GroupBox BuildApplicationGroup()
    {
        TableLayoutPanel grid = CreateGrid(2);
        AddCheckRow(grid, startOnLaunchCheckBox, 0, "Begin nudging automatically as soon as the app starts.");
        AddCheckRow(grid, showNotificationsCheckBox, 1, "Show a small balloon notification when nudging starts or stops.");

        GroupBox group = CreateGroupBox("Application");
        group.Controls.Add(grid);
        return group;
    }

    FlowLayoutPanel BuildButtons()
    {
        Button closeButton = new()
        {
            Text = "Close",
            AutoSize = true,
            MinimumSize = new Size(90, 28),
            Margin = new Padding(8, 0, 0, 0)
        };
        closeButton.Click += (_, _) => Close();

        AcceptButton = closeButton;
        CancelButton = closeButton;

        FlowLayoutPanel panel = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 4, 0, 0)
        };
        panel.Controls.Add(closeButton);
        return panel;
    }

    void ApplySettingsToControls()
    {
        intervalUpDown.Value = settings.IntervalSeconds;
        jitterTrackBar.Value = settings.IntervalJitterPercent;
        distanceTrackBar.Value = settings.DistancePixels;
        randomDirectionCheckBox.Checked = settings.RandomDirection;
        returnToOriginCheckBox.Checked = settings.ReturnToOrigin;
        smoothMovementCheckBox.Checked = settings.SmoothMovement;
        edgePaddingUpDown.Value = settings.EdgePaddingPixels;
        awakeRadio.Checked = settings.KeepAwakeOnly;
        nudgeRadio.Checked = !settings.KeepAwakeOnly;
        idleOnlyCheckBox.Checked = settings.NudgeOnlyWhenIdle;
        idleThresholdUpDown.Value = settings.IdleThresholdSeconds;
        pauseWhileActiveCheckBox.Checked = settings.PauseWhileActive;
        resumeDelayUpDown.Value = settings.ResumeDelaySeconds;
        startOnLaunchCheckBox.Checked = settings.StartOnLaunch;
        showNotificationsCheckBox.Checked = settings.ShowTrayNotifications;

        UpdateJitterLabel();
        UpdateDistanceLabel();
        UpdateIdleThresholdEnabled();
        UpdateResumeDelayEnabled();
    }

    void WireEvents()
    {
        intervalUpDown.ValueChanged += (_, _) => Persist(() => settings.IntervalSeconds = (int)intervalUpDown.Value);

        jitterTrackBar.ValueChanged += (_, _) =>
        {
            UpdateJitterLabel();
            Persist(() => settings.IntervalJitterPercent = jitterTrackBar.Value);
        };

        distanceTrackBar.ValueChanged += (_, _) =>
        {
            UpdateDistanceLabel();
            Persist(() => settings.DistancePixels = distanceTrackBar.Value);
        };

        randomDirectionCheckBox.CheckedChanged += (_, _) => Persist(() => settings.RandomDirection = randomDirectionCheckBox.Checked);
        returnToOriginCheckBox.CheckedChanged += (_, _) => Persist(() => settings.ReturnToOrigin = returnToOriginCheckBox.Checked);
        smoothMovementCheckBox.CheckedChanged += (_, _) => Persist(() => settings.SmoothMovement = smoothMovementCheckBox.Checked);
        edgePaddingUpDown.ValueChanged += (_, _) => Persist(() => settings.EdgePaddingPixels = (int)edgePaddingUpDown.Value);
        awakeRadio.CheckedChanged += (_, _) => Persist(() => settings.KeepAwakeOnly = awakeRadio.Checked);

        idleOnlyCheckBox.CheckedChanged += (_, _) =>
        {
            UpdateIdleThresholdEnabled();
            Persist(() => settings.NudgeOnlyWhenIdle = idleOnlyCheckBox.Checked);
        };

        idleThresholdUpDown.ValueChanged += (_, _) => Persist(() => settings.IdleThresholdSeconds = (int)idleThresholdUpDown.Value);

        pauseWhileActiveCheckBox.CheckedChanged += (_, _) =>
        {
            UpdateResumeDelayEnabled();
            Persist(() => settings.PauseWhileActive = pauseWhileActiveCheckBox.Checked);
        };

        resumeDelayUpDown.ValueChanged += (_, _) => Persist(() => settings.ResumeDelaySeconds = (int)resumeDelayUpDown.Value);
        startOnLaunchCheckBox.CheckedChanged += (_, _) => Persist(() => settings.StartOnLaunch = startOnLaunchCheckBox.Checked);
        showNotificationsCheckBox.CheckedChanged += (_, _) => Persist(() => settings.ShowTrayNotifications = showNotificationsCheckBox.Checked);

        previewButton.Click += OnPreviewClicked;
    }

    void Persist(Action apply)
    {
        if (isLoading)
        {
            return;
        }

        apply();
        SettingsStore.Save(settings);
    }

    async void OnPreviewClicked(object? sender, EventArgs e)
    {
        previewButton.Enabled = false;

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
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Preview nudge failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            if (!IsDisposed && !previewButton.IsDisposed)
            {
                previewButton.Enabled = true;
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            toolTip.Dispose();
        }

        base.Dispose(disposing);
    }

    void UpdateJitterLabel() => jitterValueLabel.Text = $"±{jitterTrackBar.Value} %";

    void UpdateDistanceLabel() => distanceValueLabel.Text = $"{distanceTrackBar.Value} px";

    void UpdateIdleThresholdEnabled() => idleThresholdUpDown.Enabled = idleOnlyCheckBox.Checked;

    void UpdateResumeDelayEnabled() => resumeDelayUpDown.Enabled = pauseWhileActiveCheckBox.Checked;
}
