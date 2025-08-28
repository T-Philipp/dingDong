using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;

namespace dingdongwin;

public partial class MainForm : Form
{
    private readonly HttpClient _http = new();
    private readonly System.Windows.Forms.Timer _uiTimer;
    private readonly Scheduler _scheduler;
    private readonly AppSettings _settings;
    private WaveOutEvent? _outputDevice;
    private AudioFileReader? _audioFile;
    private readonly CancellationTokenSource _cts = new();
    private TextBox txtTimesUrl = new();

    private Label lblCountdown = new();
    private ListBox lstTimes = new();
    private Button btnPickMp3 = new();
    private ComboBox cmbMp3 = new();
    private FileSystemWatcher? _audioWatcher;
    private TrackBar trkVolume = new();
    private CheckBox chkAutostart = new();

    public MainForm()
    {
    Text = "dingDong";
    FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = true;
        StartPosition = FormStartPosition.CenterScreen;
    Size = new Size(440, 440);
    BackColor = Color.FromArgb(20, 24, 33);
    ForeColor = Color.Gainsboro;

    _settings = SettingsManager.Load();
    _scheduler = new Scheduler(_http);
    var lbl1 = new Label { Text = "Zeiten-URL (JSON)", AutoSize = true, Location = new Point(14, 14) };
    lbl1.ForeColor = ForeColor;
    txtTimesUrl.Location = new Point(14, 36);
    txtTimesUrl.Size = new Size(400, 26);
    txtTimesUrl.Text = _settings.TimesUrl ?? "https://dingdong.tavra.de/zus-zeiten.json";

    var lbl2 = new Label { Text = "Countdown bis zum nächsten Abspielen:", AutoSize = true, Location = new Point(14, 72) };
    lbl2.ForeColor = ForeColor;
    lblCountdown.Location = new Point(14, 94);
    lblCountdown.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        lblCountdown.AutoSize = true;

    var lbl3 = new Label { Text = "Zeiten vom Server:", AutoSize = true, Location = new Point(14, 124) };
    lbl3.ForeColor = ForeColor;
    lstTimes.Location = new Point(14, 144);
    lstTimes.Size = new Size(400, 104);
    lstTimes.BackColor = Color.FromArgb(30, 36, 48);
    lstTimes.ForeColor = ForeColor;

    var lbl4 = new Label { Text = "MP3-Datei:", AutoSize = true, Location = new Point(14, 254) };
    lbl4.ForeColor = ForeColor;
    var txtMp3 = new TextBox { Location = new Point(14, 274), Size = new Size(224, 26), ReadOnly = true, Text = _settings.Mp3Path ?? string.Empty, BackColor = Color.FromArgb(30,36,48), ForeColor = ForeColor };
    cmbMp3.Location = new Point(244, 274);
    cmbMp3.Size = new Size(120, 26);
    cmbMp3.DropDownStyle = ComboBoxStyle.DropDownList;
    cmbMp3.SelectedIndexChanged += (_, _) =>
    {
        var sel = cmbMp3.SelectedItem as string;
        if (!string.IsNullOrWhiteSpace(sel))
        {
            _settings.Mp3Path = sel;
            txtMp3.Text = sel;
            _settings.Save();
            PrepareAudio();
        }
    };

    btnPickMp3.Text = "Durchsuchen";
    btnPickMp3.Location = new Point(370, 272);
    btnPickMp3.FlatStyle = FlatStyle.Flat;
    btnPickMp3.BackColor = Color.FromArgb(59, 130, 246);
    btnPickMp3.ForeColor = Color.White;
        btnPickMp3.Click += (_, _) =>
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "MP3 Dateien (*.mp3)|*.mp3|Alle Dateien (*.*)|*.*",
                CheckFileExists = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _settings.Mp3Path = ofd.FileName;
                txtMp3.Text = ofd.FileName;
                _settings.Save();
                PrepareAudio();
            }
        };

        var lbl5 = new Label { Text = "Lautstärke:", AutoSize = true, Location = new Point(14, 306) };
        lbl5.ForeColor = ForeColor;
        trkVolume.Location = new Point(14, 326);
        trkVolume.Minimum = 0; trkVolume.Maximum = 100; trkVolume.TickFrequency = 10;
        trkVolume.Value = (int)Math.Round((_settings.Volume ?? 0.3f) * 100);
        trkVolume.ValueChanged += (_, _) => { _settings.Volume = trkVolume.Value / 100f; _settings.Save(); ApplyVolume(); };

        chkAutostart.Text = "Autostart (manuell setzen, siehe README)";
        chkAutostart.AutoSize = true;
        chkAutostart.Location = new Point(14, 366);
        chkAutostart.ForeColor = Color.Gray;
    chkAutostart.Enabled = false; // Autostart wird manuell per Startup-Ordner gesetzt

        var btnSave = new Button { Text = "Speichern", Location = new Point(322, 364), Size = new Size(92, 30) };
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.BackColor = Color.FromArgb(34, 197, 94);
        btnSave.ForeColor = Color.White;
        btnSave.Click += (_, _) =>
        {
            _settings.TimesUrl = txtTimesUrl.Text.Trim();
            _settings.Save();
            _ = RefreshFromServerAsync();
        };

    Controls.AddRange(new Control[] { lbl1, txtTimesUrl, lbl2, lblCountdown, lbl3, lstTimes, lbl4, txtMp3, cmbMp3, btnPickMp3, lbl5, trkVolume, chkAutostart, btnSave });

        // timers
    _uiTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _uiTimer.Tick += (_, _) => UpdateCountdownLabel();
        _uiTimer.Start();

        // initial load
    PopulateAudioList();
    PrepareAudio();
        _ = RefreshFromServerAsync();

        // schedule loop
        _ = RunSchedulerLoopAsync();
    }

    public void SafeDispose()
    {
        try { _cts.Cancel(); } catch { }
        try
        {
            if (_audioWatcher != null)
            {
                _audioWatcher.EnableRaisingEvents = false;
                _audioWatcher.Dispose();
            }
            _outputDevice?.Stop();
            _outputDevice?.Dispose();
            _audioFile?.Dispose();
            _http.Dispose();
        }
        catch { }
    }

    public void PlayNow()
    {
        if (PrepareAudio())
        {
            _outputDevice?.Stop();
            _audioFile!.Position = 0;
            _outputDevice!.Play();
        }
    }

    private bool PrepareAudio()
    {
        try
        {
            _outputDevice?.Stop();
            _outputDevice?.Dispose();
            _audioFile?.Dispose();

            if (string.IsNullOrWhiteSpace(_settings.Mp3Path) || !File.Exists(_settings.Mp3Path))
            {
                // Fallback: erste aus Liste nehmen, falls vorhanden
                var first = cmbMp3.Items.Cast<string?>().FirstOrDefault(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p));
                if (!string.IsNullOrWhiteSpace(first))
                {
                    _settings.Mp3Path = first;
                    _settings.Save();
                }
            }
            if (string.IsNullOrWhiteSpace(_settings.Mp3Path) || !File.Exists(_settings.Mp3Path))
                return false;

            _audioFile = new AudioFileReader(_settings.Mp3Path);
            _outputDevice = new WaveOutEvent();
            _outputDevice.Init(_audioFile);
            ApplyVolume();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return false;
        }
    }

    private void ApplyVolume()
    {
        if (_audioFile != null)
        {
            _audioFile.Volume = Math.Clamp(_settings.Volume ?? 0.3f, 0f, 1f);
        }
    }

    private async Task RefreshFromServerAsync()
    {
        try
        {
            var url = string.IsNullOrWhiteSpace(_settings.TimesUrl) ? txtTimesUrl.Text.Trim() : _settings.TimesUrl;
            if (string.IsNullOrWhiteSpace(url)) return;
            _settings.TimesUrl = url; // persist last good
            var timesJson = await _http.GetStringAsync(url);
            var times = JsonHelpers.ParseTimes(timesJson);
            var nowUtc = await TimeSync.GetNetworkTimeUtcAsync();
            _scheduler.Update(times, nowUtc);
            lstTimes.Items.Clear();
            foreach (var t in times)
            {
                lstTimes.Items.Add($"{t.day} {t.time}");
            }
        }
        catch (Exception ex)
        {
            AppLog.Write($"Refresh error: {ex.Message}");
        }
    }

    private void UpdateCountdownLabel()
    {
        var nextAt = _scheduler.NextPlayAtLocal;
        if (nextAt == null)
        {
            lblCountdown.Text = "Keine nächste Zeit gefunden";
            return;
        }
        var remaining = nextAt.Value - DateTime.Now;
    if (remaining.TotalSeconds < 0) remaining = TimeSpan.Zero;
        lblCountdown.Text = $"Nächstes Abspielen um {nextAt:ddd, dd.MM.yyyy HH:mm:ss} (in {remaining:hh\\:mm\\:ss})";
    }

    private async Task RunSchedulerLoopAsync()
    {
        var lastTriggeredAt = DateTime.MinValue;
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), _cts.Token);
                var now = DateTime.Now;
                var nextAt = _scheduler.NextPlayAtLocal;
                if (nextAt != null && lastTriggeredAt != nextAt.Value && now >= nextAt.Value && now < nextAt.Value.AddSeconds(2))
                {
                    PlayNow();
                    lastTriggeredAt = nextAt.Value;
                }

                if (now.Second % 60 == 0)
                {
                    await RefreshFromServerAsync();
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                AppLog.Write($"Loop error: {ex}");
            }
        }
    }

    private void PopulateAudioList()
    {
        try
        {
            var baseDir = Path.GetDirectoryName(Application.ExecutablePath) ?? AppContext.BaseDirectory;
            var audioDir = Path.Combine(baseDir, "audio");
            Directory.CreateDirectory(audioDir);

            var files = Directory.GetFiles(audioDir, "*.mp3").OrderBy(f => Path.GetFileName(f)).ToList();
            var current = _settings.Mp3Path;
            cmbMp3.Items.Clear();
            foreach (var f in files) cmbMp3.Items.Add(f);

            // Watch for changes to auto-refresh
            _audioWatcher?.Dispose();
            _audioWatcher = new FileSystemWatcher(audioDir, "*.mp3")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true
            };
            _audioWatcher.Created += (_, __) => BeginInvoke(new Action(PopulateAudioList));
            _audioWatcher.Deleted += (_, __) => BeginInvoke(new Action(PopulateAudioList));
            _audioWatcher.Renamed += (_, __) => BeginInvoke(new Action(PopulateAudioList));

            // Select current if present; otherwise first
            if (!string.IsNullOrWhiteSpace(current) && files.Contains(current))
            {
                cmbMp3.SelectedItem = current;
            }
            else if (files.Count > 0)
            {
                cmbMp3.SelectedIndex = 0;
            }
        }
        catch { }
    }
}

public record ZusZeit(string day, string time);

public class Scheduler
{
    private readonly HttpClient _http;
    private List<ZusZeit> _times = new();
    public DateTime? NextPlayAtLocal { get; private set; }

    public Scheduler(HttpClient http)
    {
        _http = http;
    }

    public void Update(List<ZusZeit> times, DateTime nowUtc)
    {
        _times = times;
        // Ermittle die aktuelle Zeit in Berlin aus UTC und berechne nächsten Termin.
        NextPlayAtLocal = ComputeNextLocal(nowUtc);
    }

    private static readonly Dictionary<string, DayOfWeek> DayMap = new()
    {
        ["Mo"] = DayOfWeek.Monday,
        ["Di"] = DayOfWeek.Tuesday,
        ["Mi"] = DayOfWeek.Wednesday,
        ["Do"] = DayOfWeek.Thursday,
        ["Fr"] = DayOfWeek.Friday,
        ["Sa"] = DayOfWeek.Saturday,
        ["So"] = DayOfWeek.Sunday,
    };

    private DateTime? ComputeNextLocal(DateTime nowUtc)
    {
        // Konvertiere UTC nach Europe/Berlin (inkl. DST)
        var berlin = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        var serverNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc), berlin);

        var candidates = new List<DateTime>();
        foreach (var t in _times)
        {
            if (!DayMap.TryGetValue(t.day, out var dow)) continue;
            if (!TimeSpan.TryParse(t.time, out var tod)) continue;

            // Nächste Server-Datum/Uhrzeit
            var daysUntil = ((int)dow - (int)serverNow.DayOfWeek + 7) % 7;
            var date = serverNow.Date.AddDays(daysUntil).Add(tod);
            if (date <= serverNow) date = date.AddDays(7);
            candidates.Add(date);
        }
        var nextServer = candidates.OrderBy(x => x).FirstOrDefault();
        if (nextServer == default) return null;
        // Übertrage Differenz Server->lokal
        var delta = nextServer - serverNow;
        return DateTime.Now + delta;
    }
}

public static class TimeSync
{
    // Sehr leichter NTP-Client (UDP zu pool.ntp.org). Fällt bei Fehlern auf lokale Zeit zurück.
    public static async Task<DateTime> GetNetworkTimeUtcAsync()
    {
        try
        {
            var servers = new[] { "pool.ntp.org", "time.windows.com", "time.google.com" };
            foreach (var host in servers)
            {
                var dt = await QueryNtpAsync(host, TimeSpan.FromSeconds(3));
                if (dt != null) return dt.Value;
            }
        }
        catch { }
        return DateTime.UtcNow;
    }

    private static async Task<DateTime?> QueryNtpAsync(string ntpServer, TimeSpan timeout)
    {
        using var udp = new System.Net.Sockets.UdpClient();
        udp.Client.ReceiveTimeout = (int)timeout.TotalMilliseconds;
        udp.Client.SendTimeout = (int)timeout.TotalMilliseconds;
        var ip = (await System.Net.Dns.GetHostAddressesAsync(ntpServer)).FirstOrDefault();
        if (ip == null) return null;
        var endPoint = new System.Net.IPEndPoint(ip, 123);

        var ntpData = new byte[48];
        ntpData[0] = 0x1B; // LI, Version, Mode
        await udp.SendAsync(ntpData, ntpData.Length, endPoint);
        var receiveTask = udp.ReceiveAsync();
        var completed = await Task.WhenAny(receiveTask, Task.Delay(timeout));
        if (completed != receiveTask) return null;
        var data = receiveTask.Result.Buffer;
        if (data.Length < 48) return null;

        const byte transmitTimeOffset = 40;
        ulong intPart = (ulong)data[transmitTimeOffset] << 24 | (ulong)data[transmitTimeOffset + 1] << 16 | (ulong)data[transmitTimeOffset + 2] << 8 | data[transmitTimeOffset + 3];
        ulong fractPart = (ulong)data[transmitTimeOffset + 4] << 24 | (ulong)data[transmitTimeOffset + 5] << 16 | (ulong)data[transmitTimeOffset + 6] << 8 | data[transmitTimeOffset + 7];
        var milliseconds = (intPart * 1000) + (fractPart * 1000) / 0x100000000UL;
        var ntpEpoch = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return ntpEpoch.AddMilliseconds((long)milliseconds);
    }
}

internal static class JsonHelpers
{
    public static List<ZusZeit> ParseTimes(string json)
    {
        var list = new List<ZusZeit>();
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    var day = el.TryGetProperty("day", out var d) ? d.GetString() : null;
                    var time = el.TryGetProperty("time", out var t) ? t.GetString() : null;
                    if (!string.IsNullOrWhiteSpace(day) && !string.IsNullOrWhiteSpace(time))
                    {
                        list.Add(new ZusZeit(day!, time!));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            AppLog.Write($"ParseTimes error: {ex.Message}");
        }
        return list;
    }
}

public static class AppLog
{
    private static readonly object _gate = new();
    private static readonly string Dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "dingDong", "windows-client");
    private static readonly string FilePath = Path.Combine(Dir, "app.log");
    private const long MaxBytes = 256 * 1024; // 256 KB

    public static void Write(string message)
    {
        try
        {
            Directory.CreateDirectory(Dir);
            lock (_gate)
            {
                RotateIfNeeded();
                File.AppendAllText(FilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n");
            }
        }
        catch { }
    }

    private static void RotateIfNeeded()
    {
        try
        {
            if (!File.Exists(FilePath)) return;
            var fi = new FileInfo(FilePath);
            if (fi.Length < MaxBytes) return;
            var archive = Path.Combine(Dir, $"app-{DateTime.Now:yyyyMMdd-HHmmss}.log");
            File.Move(FilePath, archive, overwrite: true);
            // Clean old archives, keep last 5
            var archives = Directory.GetFiles(Dir, "app-*.log").OrderByDescending(f => f).Skip(5).ToList();
            foreach (var a in archives) { try { File.Delete(a); } catch { } }
        }
        catch { }
    }
}

// Autostart wird manuell vom Nutzer gesetzt (siehe README).
