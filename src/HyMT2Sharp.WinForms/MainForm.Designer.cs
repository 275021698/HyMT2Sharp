#nullable disable
namespace Sdcb.HyMT2Sharp.WinForms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
            _cts?.Cancel();
            _model?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        toolbar = new TableLayoutPanel();
        engineLabel = new Label();
        engineBox = new ComboBox();
        cacheLabel = new Label();
        cacheBox = new ComboBox();
        targetLabel = new Label();
        targetBox = new ComboBox();
        translateButton = new Button();
        split = new SplitContainer();
        inputBox = new TextBox();
        outputBox = new TextBox();
        statusStrip = new StatusStrip();
        stateStatus = new ToolStripStatusLabel();
        engineStatus = new ToolStripStatusLabel();
        loadStatus = new ToolStripStatusLabel();
        prefillStatus = new ToolStripStatusLabel();
        decodeStatus = new ToolStripStatusLabel();
        toolbar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)split).BeginInit();
        split.Panel1.SuspendLayout();
        split.Panel2.SuspendLayout();
        split.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        //
        // toolbar: label | combo | label | combo | label | combo | (fill) | button, one auto-size row.
        //
        toolbar.AutoSize = true;
        toolbar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        toolbar.ColumnCount = 8;
        toolbar.ColumnStyles.Add(new ColumnStyle());
        toolbar.ColumnStyles.Add(new ColumnStyle());
        toolbar.ColumnStyles.Add(new ColumnStyle());
        toolbar.ColumnStyles.Add(new ColumnStyle());
        toolbar.ColumnStyles.Add(new ColumnStyle());
        toolbar.ColumnStyles.Add(new ColumnStyle());
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        toolbar.ColumnStyles.Add(new ColumnStyle());
        toolbar.Controls.Add(engineLabel, 0, 0);
        toolbar.Controls.Add(engineBox, 1, 0);
        toolbar.Controls.Add(cacheLabel, 2, 0);
        toolbar.Controls.Add(cacheBox, 3, 0);
        toolbar.Controls.Add(targetLabel, 4, 0);
        toolbar.Controls.Add(targetBox, 5, 0);
        toolbar.Controls.Add(translateButton, 7, 0);
        toolbar.Dock = DockStyle.Top;
        toolbar.Padding = new Padding(6, 6, 6, 4);
        toolbar.RowCount = 1;
        toolbar.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        //
        // engineLabel
        //
        engineLabel.Anchor = AnchorStyles.Left;
        engineLabel.AutoSize = true;
        engineLabel.Text = "引擎";
        //
        // engineBox
        //
        engineBox.Anchor = AnchorStyles.Left;
        engineBox.DropDownStyle = ComboBoxStyle.DropDownList;
        engineBox.Width = 110;
        engineBox.SelectedIndexChanged += ModelOption_SelectedIndexChanged;
        //
        // cacheLabel
        //
        cacheLabel.Anchor = AnchorStyles.Left;
        cacheLabel.AutoSize = true;
        cacheLabel.Margin = new Padding(16, 0, 3, 0);
        cacheLabel.Text = "KV 缓存";
        //
        // cacheBox
        //
        cacheBox.Anchor = AnchorStyles.Left;
        cacheBox.DropDownStyle = ComboBoxStyle.DropDownList;
        cacheBox.Width = 110;
        cacheBox.SelectedIndexChanged += ModelOption_SelectedIndexChanged;
        //
        // targetLabel
        //
        targetLabel.Anchor = AnchorStyles.Left;
        targetLabel.AutoSize = true;
        targetLabel.Margin = new Padding(16, 0, 3, 0);
        targetLabel.Text = "目标语言";
        //
        // targetBox
        //
        targetBox.Anchor = AnchorStyles.Left;
        targetBox.DropDownStyle = ComboBoxStyle.DropDownList;
        targetBox.Width = 170;
        //
        // translateButton (height follows engineBox, see MainForm)
        //
        translateButton.Anchor = AnchorStyles.Right;
        translateButton.Size = new Size(110, 25);
        translateButton.Text = "翻译(&T)";
        translateButton.Click += TranslateButton_Click;
        //
        // split
        //
        split.Dock = DockStyle.Fill;
        split.Panel1.Controls.Add(inputBox);
        split.Panel1.Padding = new Padding(6, 0, 3, 6);
        split.Panel2.Controls.Add(outputBox);
        split.Panel2.Padding = new Padding(3, 0, 6, 6);
        //
        // inputBox
        //
        inputBox.AcceptsReturn = true;
        inputBox.Dock = DockStyle.Fill;
        inputBox.Font = new Font("Microsoft YaHei UI", 11F);
        inputBox.Multiline = true;
        inputBox.ScrollBars = ScrollBars.Vertical;
        inputBox.KeyDown += InputBox_KeyDown;
        //
        // outputBox
        //
        outputBox.BackColor = SystemColors.Window;
        outputBox.Dock = DockStyle.Fill;
        outputBox.Font = new Font("Microsoft YaHei UI", 11F);
        outputBox.Multiline = true;
        outputBox.ReadOnly = true;
        outputBox.ScrollBars = ScrollBars.Vertical;
        //
        // statusStrip
        //
        statusStrip.Items.AddRange(new ToolStripItem[] { stateStatus, engineStatus, loadStatus, prefillStatus, decodeStatus });
        //
        // stateStatus
        //
        stateStatus.Spring = true;
        stateStatus.TextAlign = ContentAlignment.MiddleLeft;
        //
        // engineStatus / loadStatus / prefillStatus / decodeStatus
        //
        engineStatus.BorderSides = ToolStripStatusLabelBorderSides.Left;
        engineStatus.TextAlign = ContentAlignment.MiddleLeft;
        loadStatus.BorderSides = ToolStripStatusLabelBorderSides.Left;
        loadStatus.TextAlign = ContentAlignment.MiddleLeft;
        prefillStatus.BorderSides = ToolStripStatusLabelBorderSides.Left;
        prefillStatus.TextAlign = ContentAlignment.MiddleLeft;
        decodeStatus.BorderSides = ToolStripStatusLabelBorderSides.Left;
        decodeStatus.TextAlign = ContentAlignment.MiddleLeft;
        //
        // MainForm
        //
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1100, 560);
        Controls.Add(split);
        Controls.Add(toolbar);
        Controls.Add(statusStrip);
        MinimumSize = new Size(640, 360);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Sdcb.HyMT2Sharp - WinForms";
        Shown += MainForm_Shown;
        FormClosing += MainForm_FormClosing;
        toolbar.ResumeLayout(false);
        toolbar.PerformLayout();
        split.Panel1.ResumeLayout(false);
        split.Panel1.PerformLayout();
        split.Panel2.ResumeLayout(false);
        split.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)split).EndInit();
        split.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private TableLayoutPanel toolbar;
    private Label engineLabel;
    private ComboBox engineBox;
    private Label cacheLabel;
    private ComboBox cacheBox;
    private Label targetLabel;
    private ComboBox targetBox;
    private Button translateButton;
    private SplitContainer split;
    private TextBox inputBox;
    private TextBox outputBox;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel stateStatus;
    private ToolStripStatusLabel engineStatus;
    private ToolStripStatusLabel loadStatus;
    private ToolStripStatusLabel prefillStatus;
    private ToolStripStatusLabel decodeStatus;
}
