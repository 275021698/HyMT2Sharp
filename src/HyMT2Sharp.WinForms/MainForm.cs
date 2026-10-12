using System.Diagnostics;
using Sdcb.GgufWeights.Hy_MT2_1_8B_2Bit;
using Sdcb.HyMT2Sharp.Backends;
using Sdcb.HyMT2Sharp.Model;

namespace Sdcb.HyMT2Sharp.WinForms;

// Weights come from the Sdcb.GgufWeights.Hy-MT2-1.8B-2Bit package. To use another
// quantization, swap the PackageReference in the csproj and the using above.
public partial class MainForm : Form
{
    private const string SampleText = "SimdPaddleOCR is officially released today (NuGet: Sdcb.SimdPaddleOCR). It is a complete OCR inference engine written entirely in C#. It does not depend on Paddle Inference or ONNX Runtime, and it does not require shipping OpenCV native libraries.";

    private static readonly Engine[] Engines =
    [
        new("自动", "auto"),
        new("CPU", "cpu"),
        new("Vulkan", "vulkan"),
    ];

    // The model's KV cache policy is fixed at load time; changing it reloads the model.
    private static readonly CachePolicy[] CachePolicies =
    [
        new("不缓存", KvCacheConfig.None),
        new("保留上次", KvCacheConfig.Memory),
        new("前缀块池", KvCacheConfig.BlockPolicy()),
    ];

    private static readonly Language[] Languages =
    [
        new("中文", "Chinese"),
        new("英语", "English"),
        new("繁体中文", "Traditional Chinese"),
        new("日语", "Japanese"),
        new("韩语", "Korean"),
        new("法语", "French"),
        new("德语", "German"),
        new("西班牙语", "Spanish"),
        new("葡萄牙语", "Portuguese"),
        new("意大利语", "Italian"),
        new("俄语", "Russian"),
        new("阿拉伯语", "Arabic"),
    ];

    private HunyuanDenseModel? _model;
    private CancellationTokenSource? _cts;
    private bool _loading;
    private bool _closeRequested;

    public MainForm()
    {
        InitializeComponent();
        engineBox.Items.AddRange(Engines);
        cacheBox.Items.AddRange(CachePolicies);
        targetBox.Items.AddRange(Languages);
        targetBox.SelectedIndex = 0;
        inputBox.Text = SampleText;
        // Keep the button exactly as tall as the combo boxes, also after per-monitor DPI changes.
        engineBox.SizeChanged += (_, _) => translateButton.Height = engineBox.Height;
        translateButton.Height = engineBox.Height;
        statusStrip.FontChanged += (_, _) => SizeStatusLabels();
        DpiChanged += (_, _) => BeginInvoke(new Action(SizeStatusLabels));
        SizeStatusLabels();
        UpdateButtons();
    }

    // Fixed-width status fields, measured from sample text in the current (DPI-scaled) font,
    // so empty fields keep their slot and values don't make neighbours jump.
    private void SizeStatusLabels()
    {
        Fit(engineStatus, "vulkan  32 线程  KV blocks");
        Fit(loadStatus, "加载 99.99 s");
        Fit(prefillStatus, "prefill 9999 tok（缓存 9999）9999 ms  9999.9 tok/s");
        Fit(decodeStatus, "decode 9999 tok 99999 ms  999.9 tok/s");

        void Fit(ToolStripStatusLabel label, string sample)
        {
            label.AutoSize = false;
            label.Width = TextRenderer.MeasureText(sample, statusStrip.Font).Width + label.Padding.Horizontal + 8;
        }
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        split.SplitterDistance = split.Width / 2;
        // Focus lands in the input box; don't start with the whole sample text selected.
        inputBox.Select(inputBox.TextLength, 0);
        // Setting the indexes raises SelectedIndexChanged; the model loads once both are set.
        cacheBox.SelectedIndex = 0;
        engineBox.SelectedIndex = 0;
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        // The model is still in use on a worker thread; stop it and close once it returns.
        if (_cts is not null || _loading)
        {
            e.Cancel = true;
            _closeRequested = true;
            _cts?.Cancel();
        }
    }

    private async void ModelOption_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (engineBox.SelectedItem is Engine engine && cacheBox.SelectedItem is CachePolicy cache)
            await LoadModelAsync(engine, cache);
    }

    private async Task LoadModelAsync(Engine engine, CachePolicy cache)
    {
        _loading = true;
        UpdateButtons();
        stateStatus.Text = $"正在加载模型（{engine.Display}）…";
        engineStatus.Text = loadStatus.Text = prefillStatus.Text = decodeStatus.Text = "";

        _model?.Dispose();
        _model = null;
        try
        {
            (HunyuanDenseModel model, string backend, TimeSpan elapsed) = await Task.Run(() =>
            {
                Stopwatch sw = Stopwatch.StartNew();
                // The model owns the backend and disposes it, also when loading fails.
                IComputeBackend? backend = BackendFactory.Create(engine.Key);
                HunyuanDenseModel model = new(Weights.Model(), cacheConfig: cache.Config, backend: backend);
                return (model, backend?.Name ?? "cpu", sw.Elapsed);
            });
            _model = model;
            stateStatus.Text = "就绪";
            engineStatus.Text = $"{backend}  {model.ThreadCount} 线程  KV {model.CacheConfig.PolicyName}";
            loadStatus.Text = $"加载 {elapsed.TotalSeconds:F2} s";
        }
        catch (Exception ex)
        {
            stateStatus.Text = $"加载失败：{ex.Message}";
            MessageBox.Show(this, ex.Message, $"无法使用 {engine.Display} 引擎", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            _loading = false;
            UpdateButtons();
        }
    }

    private void InputBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            translateButton.PerformClick();
        }
        else if (e.Control && e.KeyCode == Keys.A)
        {
            e.SuppressKeyPress = true;
            inputBox.SelectAll();
        }
    }

    private async void TranslateButton_Click(object? sender, EventArgs e)
    {
        if (_cts is not null)
        {
            _cts.Cancel();
            return;
        }

        if (_model is null || string.IsNullOrWhiteSpace(inputBox.Text) || targetBox.SelectedItem is not Language target)
            return;

        HunyuanDenseModel model = _model;
        string prompt = $"Translate the following text into {target.Name}. Note that you should only output the translated result without any additional explanation:\n\n{inputBox.Text.Trim().ReplaceLineEndings("\n")}";
        _cts = new CancellationTokenSource();
        CancellationToken ct = _cts.Token;
        UpdateButtons();
        outputBox.Clear();
        stateStatus.Text = "翻译中…";
        prefillStatus.Text = decodeStatus.Text = "";

        // Progress<T> posts back to the UI thread in order.
        Progress<string> progress = new(piece => outputBox.AppendText(piece.ReplaceLineEndings()));
        try
        {
            TranslateStats stats = await Task.Run(() => Translate(model, prompt, progress, ct));
            stateStatus.Text = ct.IsCancellationRequested ? "已停止" : "完成";
            prefillStatus.Text = $"prefill {stats.PromptTokens} tok（缓存 {stats.CachedTokens}）{Rate(stats.PromptTokens - stats.CachedTokens, stats.PrefillMs)}";
            decodeStatus.Text = $"decode {stats.DecodeTokens} tok {Rate(stats.DecodeTokens, stats.DecodeMs)}";
        }
        catch (Exception ex)
        {
            stateStatus.Text = $"翻译失败：{ex.Message}";
        }
        finally
        {
            _cts.Dispose();
            _cts = null;
            UpdateButtons();
        }
    }

    private static TranslateStats Translate(HunyuanDenseModel model, string user, IProgress<string> progress, CancellationToken ct)
    {
        bool ok = false;
        try
        {
            TranslateStats stats = Generate(model, user, progress, ct);
            ok = !ct.IsCancellationRequested;
            return stats;
        }
        finally
        {
            // Same rule as the server: without KeepFlatCache ("不缓存") the KV cache is dropped
            // after every request; a stopped or failed request never leaves a half-written cache.
            if (!ok || !model.CacheConfig.KeepFlatCache)
                model.EndRequest();
        }
    }

    // Greedy decoding; reports text pieces as they become visible.
    private static TranslateStats Generate(HunyuanDenseModel model, string user, IProgress<string> progress, CancellationToken ct)
    {
        int[] prompt = model.Tokenizer.Encode(ChatTemplate.RenderHunyuanDense([new ChatMessage("user", user)]));
        Stopwatch sw = Stopwatch.StartNew();
        PromptAlignment align = model.AlignPrompt(prompt);
        float[] logits = model.Forward(align.Suffix);
        double prefillMs = sw.Elapsed.TotalMilliseconds;

        sw.Restart();
        List<int> generated = [];
        string visible = "";
        int decoded = 0;
        while (!ct.IsCancellationRequested && generated.Count < 2048)
        {
            int token = Sampler.ArgMax(logits);
            if (model.Tokenizer.IsStop(token))
                break;

            generated.Add(token);
            // A trailing U+FFFD is a UTF-8 sequence split across tokens; wait for the next token.
            string next = model.Tokenizer.DecodeVisible(generated).TrimEnd('\uFFFD');
            if (next.Length > visible.Length && next.StartsWith(visible, StringComparison.Ordinal))
            {
                progress.Report(next[visible.Length..]);
                visible = next;
            }

            logits = model.Forward([token]);
            decoded++;
        }

        return new TranslateStats(prompt.Length, align.Cached, prefillMs, decoded, sw.Elapsed.TotalMilliseconds);
    }

    private static string Rate(int tokens, double ms) => tokens > 0 && ms > 0
        ? $"{ms:F0} ms  {tokens / (ms / 1000.0):F1} tok/s"
        : "";

    private void UpdateButtons()
    {
        if (_closeRequested && _cts is null && !_loading)
        {
            BeginInvoke(new Action(Close));
            return;
        }

        bool translating = _cts is not null;
        engineBox.Enabled = !_loading && !translating;
        cacheBox.Enabled = !_loading && !translating;
        targetBox.Enabled = !translating;
        translateButton.Enabled = !_loading && (_model is not null || translating);
        translateButton.Text = translating ? "停止(&T)" : "翻译(&T)";
    }

    private sealed record Engine(string Display, string Key)
    {
        public override string ToString() => Display;
    }

    private sealed record CachePolicy(string Display, KvCacheConfig Config)
    {
        public override string ToString() => Display;
    }

    private sealed record Language(string Display, string Name)
    {
        public override string ToString() => $"{Display}（{Name}）";
    }

    private sealed record TranslateStats(int PromptTokens, int CachedTokens, double PrefillMs, int DecodeTokens, double DecodeMs);
}
