using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace dingdongwin;

public class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly MainForm _form;

    public TrayApplicationContext()
    {
        _form = new MainForm();

        var icon = IconLoader.Load();
        if (icon != null)
        {
            _form.Icon = icon;
        }

        _notifyIcon = new NotifyIcon
        {
            Text = "dingDong",
            Visible = true,
            Icon = icon ?? SystemIcons.Information
        };

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Öffnen", null, (_, _) => ShowForm());
        contextMenu.Items.Add("Jetzt abspielen", null, (_, _) => _form.PlayNow());
        contextMenu.Items.Add("Beenden", null, (_, _) => ExitThread());
        _notifyIcon.ContextMenuStrip = contextMenu;

        _notifyIcon.MouseClick += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                ShowForm();
            }
        };

        // Start minimized to tray (do nothing, just keep icon visible)
    }

    private void ShowForm()
    {
        if (_form.Visible)
        {
            _form.BringToFront();
            _form.Activate();
        }
        else
        {
            _form.Show();
        }
    }

    protected override void ExitThreadCore()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _form.SafeDispose();
        base.ExitThreadCore();
    }
}

public static class IconLoader
{
    public static Icon? Load()
    {
        try
        {
            // 1) Embedded resource (preferred): any *.ico embedded under assets
            var resIcon = LoadFromEmbedded();
            if (resIcon != null) return resIcon;

            // 2) Optional file next to the EXE (legacy fallback)
            var baseDir = Path.GetDirectoryName(Application.ExecutablePath) ?? AppContext.BaseDirectory;
            foreach (var name in new[] { "bell.ico", "dingdong.ico", "icon.ico" })
            {
                var p = Path.Combine(baseDir, name);
                if (File.Exists(p)) return new Icon(p);
            }
        }
        catch { }
        return null;
    }

    private static Icon? LoadFromEmbedded()
    {
        try
        {
            var asm = Assembly.GetExecutingAssembly();
            var name = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(".ico", StringComparison.OrdinalIgnoreCase));
            if (name == null) return null;
            using var s = asm.GetManifestResourceStream(name);
            if (s == null) return null;
            return new Icon(s);
        }
        catch { return null; }
    }
}
