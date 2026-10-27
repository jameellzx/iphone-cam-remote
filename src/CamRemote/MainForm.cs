using BleHid.Core;
using HidButtons = BleHid.Core.MouseButtons;
using WinButtons = System.Windows.Forms.MouseButtons;

namespace CamRemote;

internal enum CameraState { Stopped, Recording, Paused }

/// <summary>
/// One window: click inside the big pad to drive the iPhone Camera app over the PC's own Bluetooth.
/// Right = start / stop recording, left = pause / resume.
/// Record and stop are Volume Up, which the Camera app treats as its shutter. Pause has no
/// hardware key, so the phone's AssistiveTouch pointer is moved onto the pause button and clicked.
/// </summary>
internal sealed class MainForm : Form
{
    // Pointer movement per report. Every trip uses the same steps and timing, so iOS pointer
    // acceleration lands on the same spot each time.
    private const int StepUnits = 20;
    private const int HomeReports = 12;
    private const int HomeUnits = -1500;

    private readonly BleHidPeripheral _peripheral = new();
    private readonly SemaphoreSlim _sendGate = new(1, 1);
    private readonly RemoteSettings _settings = RemoteSettings.Load();
    private readonly System.Windows.Forms.Timer _poll = new() { Interval = 1000 };

    private readonly Label _connection = new();
    private readonly Panel _pad = new();
    private readonly Label _stateText = new();
    private readonly NumericUpDown _stepsX = new();
    private readonly NumericUpDown _stepsY = new();
    private readonly TextBox _log = new();

    private CameraState _state = CameraState.Stopped;
    private bool _started;
    private bool _pointerOnPause;

    public MainForm()
    {
        Text = "iPhone Cam Remote";
        Width = 640;
        Height = 620;
        MinimumSize = new Size(480, 480);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10f);

        _connection.Dock = DockStyle.Top;
        _connection.Height = 34;
        _connection.TextAlign = ContentAlignment.MiddleCenter;
        _connection.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
        _connection.Text = "Starting Bluetooth...";

        _stateText.Dock = DockStyle.Fill;
        _stateText.TextAlign = ContentAlignment.MiddleCenter;
        _stateText.ForeColor = Color.White;
        _stateText.Font = new Font("Segoe UI", 34f, FontStyle.Bold);
        _stateText.MouseDown += OnPadMouseDown;
        _pad.Controls.Add(_stateText);
        _pad.Dock = DockStyle.Fill;
        _pad.MouseDown += OnPadMouseDown;

        var hint = new Label
        {
            Dock = DockStyle.Top,
            Height = 26,
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "Click in the box:  RIGHT = start / stop recording    LEFT = pause / resume"
        };

        Controls.Add(_pad);
        Controls.Add(hint);
        Controls.Add(_connection);
        Controls.Add(BuildSetupPanel());

        _poll.Tick += (_, _) => RefreshConnection();
        Load += async (_, _) => await StartBluetoothAsync();
        FormClosing += async (_, _) => await _peripheral.DisposeAsync();

        ShowState();
    }

    private Control BuildSetupPanel()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 190,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(8),
            WrapContents = true
        };

        panel.Controls.Add(new Label
        {
            Text = "Pause button spot on the phone (steps from top-left corner):",
            AutoSize = true,
            Margin = new Padding(3, 6, 3, 3)
        });
        panel.SetFlowBreak(panel.Controls[^1], true);

        ConfigureSteps(_stepsX, _settings.PauseStepsX);
        ConfigureSteps(_stepsY, _settings.PauseStepsY);
        panel.Controls.Add(new Label { Text = "Right:", AutoSize = true, Margin = new Padding(3, 8, 0, 3) });
        panel.Controls.Add(_stepsX);
        panel.Controls.Add(new Label { Text = "Down:", AutoSize = true, Margin = new Padding(12, 8, 0, 3) });
        panel.Controls.Add(_stepsY);

        var show = new Button { Text = "Show spot on phone", AutoSize = true };
        show.Click += async (_, _) => await RunAsync(MovePointerToPauseAsync);
        panel.Controls.Add(show);

        var tap = new Button { Text = "Test tap", AutoSize = true };
        tap.Click += async (_, _) => await RunAsync(async () =>
        {
            await EnsurePointerOnPauseAsync();
            await ClickAsync();
        });
        panel.Controls.Add(tap);
        panel.SetFlowBreak(tap, true);

        var reset = new Button { Text = "Phone is stopped (reset)", AutoSize = true };
        reset.Click += (_, _) => { _state = CameraState.Stopped; ShowState(); };
        panel.Controls.Add(reset);
        panel.SetFlowBreak(reset, true);

        _log.Multiline = true;
        _log.ReadOnly = true;
        _log.ScrollBars = ScrollBars.Vertical;
        _log.Width = 600;
        _log.Height = 70;
        _log.Font = new Font("Consolas", 8.5f);
        panel.Controls.Add(_log);
        panel.Resize += (_, _) => _log.Width = Math.Max(200, panel.ClientSize.Width - 24);

        return panel;
    }

    private void ConfigureSteps(NumericUpDown box, int value)
    {
        box.Minimum = 0;
        box.Maximum = 200;
        box.Width = 70;
        box.Value = Math.Clamp(value, 0, 200);
        box.ValueChanged += (_, _) =>
        {
            _settings.PauseStepsX = (int)_stepsX.Value;
            _settings.PauseStepsY = (int)_stepsY.Value;
            _settings.Save();
            _pointerOnPause = false;
        };
    }

    private async Task StartBluetoothAsync()
    {
        _peripheral.Log += message => Append(message);
        try
        {
            await Task.Run(_peripheral.StartAsync);
            _started = true;
            _poll.Start();
            RefreshConnection();
        }
        catch (Exception ex)
        {
            _connection.Text = "Bluetooth failed: " + ex.Message;
            _connection.ForeColor = Color.Firebrick;
            Append("startup failed: " + ex.Message);
        }
    }

    private async void RefreshConnection()
    {
        if (!_started) return;
        var hosts = _peripheral.Hosts();
        if (hosts.Count == 0)
        {
            _connection.Text = $"Waiting for iPhone - pair with \"{Environment.MachineName}\" in iPhone Bluetooth settings";
            _connection.ForeColor = Color.DarkOrange;
            _pointerOnPause = false;
            return;
        }

        // The peripheral drops to "local only" whenever every host disconnects; send to all again.
        if (_peripheral.IsLocalTarget) _peripheral.SelectAllHosts();
        await _peripheral.RefreshHostNamesAsync();
        _connection.Text = "Connected: " + string.Join(", ", _peripheral.Hosts().Select(h => h.Name ?? h.Address));
        _connection.ForeColor = Color.SeaGreen;
    }

    private async void OnPadMouseDown(object? sender, MouseEventArgs e)
    {
        switch (e.Button, _state)
        {
            case (WinButtons.Right, CameraState.Stopped):
                await RunAsync(PressVolumeUpAsync);
                _state = CameraState.Recording;
                break;
            case (WinButtons.Right, CameraState.Recording):
            case (WinButtons.Right, CameraState.Paused):
                await RunAsync(PressVolumeUpAsync);
                _state = CameraState.Stopped;
                break;
            case (WinButtons.Left, CameraState.Recording):
                await RunAsync(TapPauseAsync);
                _state = CameraState.Paused;
                break;
            case (WinButtons.Left, CameraState.Paused):
                await RunAsync(TapPauseAsync);
                _state = CameraState.Recording;
                break;
            default:
                // Left click while stopped, or the middle button - doing nothing keeps a stray
                // click from starting a take.
                System.Media.SystemSounds.Beep.Play();
                return;
        }
        ShowState();

        // Park the pointer on the pause button while recording so the first pause is instant.
        if (_state == CameraState.Recording && !_pointerOnPause)
            await RunAsync(MovePointerToPauseAsync);
    }

    private void ShowState()
    {
        (_stateText.Text, _pad.BackColor) = _state switch
        {
            CameraState.Recording => ("â— RECORDING", Color.FromArgb(200, 30, 40)),
            CameraState.Paused    => ("âšâš PAUSED", Color.FromArgb(215, 140, 0)),
            _                     => ("READY", Color.FromArgb(60, 64, 72))
        };
        _stateText.BackColor = _pad.BackColor;
    }

    /// <summary>Reports go out one sequence at a time so a fast double click cannot interleave them.</summary>
    private async Task RunAsync(Func<Task> work)
    {
        if (!_started) { System.Media.SystemSounds.Beep.Play(); return; }
        if (_peripheral.IsLocalTarget) _peripheral.SelectAllHosts();
        await _sendGate.WaitAsync();
        try { await work(); }
        catch (Exception ex) { Append("send error: " + ex.Message); }
        finally { _sendGate.Release(); }
    }

    private async Task PressVolumeUpAsync()
    {
        await _peripheral.SendConsumerAsync(HidDescriptors.ConsumerVolumeUp);
        await Task.Delay(60);
        await _peripheral.SendConsumerAsync(0);
        Append("volume up (record / stop)");
    }

    private async Task TapPauseAsync()
    {
        await EnsurePointerOnPauseAsync();
        await ClickAsync();
        Append("tapped pause button");
    }

    private async Task EnsurePointerOnPauseAsync()
    {
        if (!_pointerOnPause) await MovePointerToPauseAsync();
    }

    private async Task MovePointerToPauseAsync()
    {
        var delay = Math.Max(_peripheral.MouseReportIntervalMs(15), 15);

        // Slam into the top-left corner first: the pointer clamps there, giving a fixed origin.
        for (var i = 0; i < HomeReports; i++)
        {
            await _peripheral.SendMouseAsync(HidButtons.None, HomeUnits, HomeUnits, 0);
            await Task.Delay(delay);
        }
        await Task.Delay(120);

        for (var i = 0; i < _settings.PauseStepsX; i++)
        {
            await _peripheral.SendMouseAsync(HidButtons.None, StepUnits, 0, 0);
            await Task.Delay(delay);
        }
        for (var i = 0; i < _settings.PauseStepsY; i++)
        {
            await _peripheral.SendMouseAsync(HidButtons.None, 0, StepUnits, 0);
            await Task.Delay(delay);
        }
        await Task.Delay(80);
        _pointerOnPause = true;
    }

    private async Task ClickAsync()
    {
        await _peripheral.SendMouseAsync(HidButtons.Left, 0, 0, 0);
        await Task.Delay(60);
        await _peripheral.SendMouseAsync(HidButtons.None, 0, 0, 0);
    }

    private void Append(string message)
    {
        if (InvokeRequired) { BeginInvoke(() => Append(message)); return; }
        _log.AppendText($"{DateTime.Now:HH:mm:ss} {message}{Environment.NewLine}");
    }
}
