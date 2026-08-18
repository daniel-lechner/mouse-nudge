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

    GroupBox BuildBehaviorGroup()
    {
        TableLayoutPanel grid = CreateGrid(8);

        grid.Controls.Add(CreateLabel("Interval"), 0, 0);
        grid.Controls.Add(intervalUpDown, 1, 0);
        grid.Controls.Add(CreateLabel("seconds"), 2, 0);

        grid.Controls.Add(CreateLabel("Interval jitter"), 0, 1);
        grid.Controls.Add(jitterTrackBar, 1, 1);
        grid.Controls.Add(jitterValueLabel, 2, 1);

        grid.Controls.Add(CreateLabel("Distance"), 0, 2);
        grid.Controls.Add(distanceTrackBar, 1, 2);
        grid.Controls.Add(distanceValueLabel, 2, 2);

        AddFullWidth(grid, randomDirectionCheckBox, 3);
        AddFullWidth(grid, returnToOriginCheckBox, 4);
        AddFullWidth(grid, smoothMovementCheckBox, 5);

        grid.Controls.Add(CreateLabel("Screen edge padding"), 0, 6);
        grid.Controls.Add(edgePaddingUpDown, 1, 6);
        grid.Controls.Add(CreateLabel("px"), 2, 6);

        AddFullWidth(grid, previewButton, 7);

        GroupBox group = CreateGroupBox("Nudge behavior");
        group.Controls.Add(grid);
        return group;
    }

    GroupBox BuildModeGroup()
    {
        TableLayoutPanel grid = CreateGrid(4);

        AddFullWidth(grid, nudgeRadio, 0);
        AddFullWidth(grid, awakeRadio, 1);
        AddFullWidth(grid, idleOnlyCheckBox, 2);

        Label idleLabel = CreateLabel("Idle threshold");
        idleLabel.Margin = new Padding(18, 6, 8, 6);
        grid.Controls.Add(idleLabel, 0, 3);
        grid.Controls.Add(idleThresholdUpDown, 1, 3);
        grid.Controls.Add(CreateLabel("seconds"), 2, 3);

        GroupBox group = CreateGroupBox("Mode");
        group.Controls.Add(grid);
        return group;
    }

    GroupBox BuildApplicationGroup()
    {
        TableLayoutPanel grid = CreateGrid(2);
        AddFullWidth(grid, startOnLaunchCheckBox, 0);
        AddFullWidth(grid, showNotificationsCheckBox, 1);

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
        startOnLaunchCheckBox.Checked = settings.StartOnLaunch;
        showNotificationsCheckBox.Checked = settings.ShowTrayNotifications;

        UpdateJitterLabel();
        UpdateDistanceLabel();
        UpdateIdleThresholdEnabled();
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

    void UpdateJitterLabel() => jitterValueLabel.Text = $"±{jitterTrackBar.Value} %";

    void UpdateDistanceLabel() => distanceValueLabel.Text = $"{distanceTrackBar.Value} px";

    void UpdateIdleThresholdEnabled() => idleThresholdUpDown.Enabled = idleOnlyCheckBox.Checked;
}
