namespace mouse_nudge;

sealed class OptionsForm : Form
{
    readonly TrackBar jitterTrackBar = CreateTrackBar(0, 50, 0);
    readonly Label jitterValueLabel = CreateValueLabel();
    readonly TrackBar distanceTrackBar = CreateTrackBar(1, 100, 10);
    readonly Label distanceValueLabel = CreateValueLabel();
    readonly CheckBox idleOnlyCheckBox = CreateCheckBox("Only nudge when user is idle", true);
    readonly NumericUpDown idleThresholdUpDown = CreateUpDown(5, 600, 60);
    readonly CheckBox randomDirectionCheckBox = CreateCheckBox("Random direction", true);
    readonly CheckBox returnToOriginCheckBox = CreateCheckBox("Return cursor to original position", true);
    readonly CheckBox smoothMovementCheckBox = CreateCheckBox("Smooth movement", true);
    readonly NumericUpDown edgePaddingUpDown = CreateUpDown(0, 500, 50);
    readonly Button previewButton = new()
    {
        Text = "Preview nudge",
        AutoSize = true,
        MinimumSize = new Size(120, 28),
        Anchor = AnchorStyles.Left,
        Margin = new Padding(0, 8, 8, 2)
    };
    readonly MouseNudger nudger = new();

    public OptionsForm()
    {
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

    static void AddFullWidth(TableLayoutPanel grid, Control control, int row)
    {
        grid.Controls.Add(control, 0, row);
        grid.SetColumnSpan(control, 3);
    }

    GroupBox BuildBehaviorGroup()
    {
        TableLayoutPanel grid = CreateGrid(8);

        NumericUpDown intervalUpDown = CreateUpDown(1, 3600, 30);
        grid.Controls.Add(CreateLabel("Interval"), 0, 0);
        grid.Controls.Add(intervalUpDown, 1, 0);
        grid.Controls.Add(CreateLabel("seconds"), 2, 0);

        jitterTrackBar.Scroll += (_, _) => UpdateJitterLabel();
        UpdateJitterLabel();
        grid.Controls.Add(CreateLabel("Interval jitter"), 0, 1);
        grid.Controls.Add(jitterTrackBar, 1, 1);
        grid.Controls.Add(jitterValueLabel, 2, 1);

        distanceTrackBar.Scroll += (_, _) => UpdateDistanceLabel();
        UpdateDistanceLabel();
        grid.Controls.Add(CreateLabel("Distance"), 0, 2);
        grid.Controls.Add(distanceTrackBar, 1, 2);
        grid.Controls.Add(distanceValueLabel, 2, 2);

        AddFullWidth(grid, randomDirectionCheckBox, 3);
        AddFullWidth(grid, returnToOriginCheckBox, 4);
        AddFullWidth(grid, smoothMovementCheckBox, 5);

        grid.Controls.Add(CreateLabel("Screen edge padding"), 0, 6);
        grid.Controls.Add(edgePaddingUpDown, 1, 6);
        grid.Controls.Add(CreateLabel("px"), 2, 6);

        previewButton.Click += OnPreviewClicked;
        AddFullWidth(grid, previewButton, 7);

        GroupBox group = CreateGroupBox("Nudge behavior");
        group.Controls.Add(grid);
        return group;
    }

    GroupBox BuildModeGroup()
    {
        TableLayoutPanel grid = CreateGrid(4);

        RadioButton nudgeRadio = new()
        {
            Text = "Move cursor (nudge)",
            Checked = true,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 6, 8, 6)
        };
        RadioButton awakeRadio = new()
        {
            Text = "Keep awake only (no cursor movement)",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 6, 8, 6)
        };
        AddFullWidth(grid, nudgeRadio, 0);
        AddFullWidth(grid, awakeRadio, 1);

        idleOnlyCheckBox.CheckedChanged += (_, _) => UpdateIdleThresholdEnabled();
        AddFullWidth(grid, idleOnlyCheckBox, 2);

        UpdateIdleThresholdEnabled();

        Label idleLabel = CreateLabel("Idle threshold");
        idleLabel.Margin = new Padding(18, 6, 8, 6);
        grid.Controls.Add(idleLabel, 0, 3);
        grid.Controls.Add(idleThresholdUpDown, 1, 3);
        grid.Controls.Add(CreateLabel("seconds"), 2, 3);

        GroupBox group = CreateGroupBox("Mode");
        group.Controls.Add(grid);
        return group;
    }

    static GroupBox BuildApplicationGroup()
    {
        TableLayoutPanel grid = CreateGrid(2);
        AddFullWidth(grid, CreateCheckBox("Start nudging when app launches", false), 0);
        AddFullWidth(grid, CreateCheckBox("Show tray notification on start/stop", true), 1);

        GroupBox group = CreateGroupBox("Application");
        group.Controls.Add(grid);
        return group;
    }

    FlowLayoutPanel BuildButtons()
    {
        Button okButton = new()
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            AutoSize = true,
            MinimumSize = new Size(90, 28),
            Margin = new Padding(8, 0, 0, 0)
        };
        okButton.Click += (_, _) => Close();

        Button cancelButton = new()
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            AutoSize = true,
            MinimumSize = new Size(90, 28),
            Margin = new Padding(8, 0, 0, 0)
        };
        cancelButton.Click += (_, _) => Close();

        AcceptButton = okButton;
        CancelButton = cancelButton;

        FlowLayoutPanel panel = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 4, 0, 0)
        };
        panel.Controls.Add(cancelButton);
        panel.Controls.Add(okButton);
        return panel;
    }

    async void OnPreviewClicked(object? sender, EventArgs e)
    {
        previewButton.Enabled = false;

        try
        {
            NudgeOptions options = new(
                distanceTrackBar.Value,
                randomDirectionCheckBox.Checked,
                returnToOriginCheckBox.Checked,
                smoothMovementCheckBox.Checked,
                (int)edgePaddingUpDown.Value);

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
