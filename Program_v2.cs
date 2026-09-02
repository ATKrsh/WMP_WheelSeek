using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WMPWheelSeek
{
    public class AppSettings
    {
        public int NormalStepKeyMode { get; set; } // 0 = Arrow Key (~1 sec), 1 = Ctrl+Arrow Key (~5-10 sec)
        public int DebounceMs { get; set; }        // Scroll cooldown in ms (prevents runaway jumps)
        public int CtrlJumpSteps { get; set; }     // Number of steps for Ctrl+Wheel

        public AppSettings()
        {
            NormalStepKeyMode = 0;
            DebounceMs = 180;
            CtrlJumpSteps = 5;
        }
    }

    public class SettingsForm : Form
    {
        public TrackBar debounceSlider;
        public TrackBar ctrlStepsSlider;
        public RadioButton rbArrow;
        public RadioButton rbCtrlArrow;
        public Label lblDebounceVal;
        public Label lblCtrlStepsVal;

        private AppSettings _settings;

        public SettingsForm(AppSettings settings)
        {
            _settings = settings;

            this.Text = "WMP Wheel Seek - Settings & Sensitivity";
            this.Size = new Size(430, 370);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = SystemIcons.Application;

            Label title = new Label();
            title.Text = "Mouse Wheel Seek Settings";
            title.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            title.Location = new Point(20, 15);
            title.AutoSize = true;
            this.Controls.Add(title);

            // --- Key Mode Selection ---
            GroupBox gbMode = new GroupBox();
            gbMode.Text = "Normal Wheel Seek Jump Size";
            gbMode.Location = new Point(20, 50);
            gbMode.Size = new Size(375, 75);
            gbMode.Font = new Font("Segoe UI", 9);

            rbArrow = new RadioButton();
            rbArrow.Text = "Arrow Key (~1 second precise step) [Recommended]";
            rbArrow.Location = new Point(15, 22);
            rbArrow.AutoSize = true;
            rbArrow.Checked = (_settings.NormalStepKeyMode == 0);

            rbCtrlArrow = new RadioButton();
            rbCtrlArrow.Text = "Ctrl + Arrow Key (~5-10 second step)";
            rbCtrlArrow.Location = new Point(15, 45);
            rbCtrlArrow.AutoSize = true;
            rbCtrlArrow.Checked = (_settings.NormalStepKeyMode == 1);

            gbMode.Controls.Add(rbArrow);
            gbMode.Controls.Add(rbCtrlArrow);
            this.Controls.Add(gbMode);

            // --- Slider 1: Scroll Cooldown / Debounce ---
            Label lblDebounceTitle = new Label();
            lblDebounceTitle.Text = "Scroll Sensitivity / Cooldown (higher = stops over-jumping):";
            lblDebounceTitle.Location = new Point(20, 135);
            lblDebounceTitle.AutoSize = true;
            lblDebounceTitle.Font = new Font("Segoe UI", 9);
            this.Controls.Add(lblDebounceTitle);

            debounceSlider = new TrackBar();
            debounceSlider.Minimum = 50;
            debounceSlider.Maximum = 500;
            debounceSlider.SmallChange = 25;
            debounceSlider.LargeChange = 50;
            debounceSlider.Value = Math.Max(50, Math.Min(500, _settings.DebounceMs));
            debounceSlider.Location = new Point(20, 155);
            debounceSlider.Size = new Size(290, 45);
            debounceSlider.TickFrequency = 50;
            this.Controls.Add(debounceSlider);

            lblDebounceVal = new Label();
            lblDebounceVal.Text = string.Format("{0} ms", debounceSlider.Value);
            lblDebounceVal.Location = new Point(320, 160);
            lblDebounceVal.AutoSize = true;
            lblDebounceVal.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            this.Controls.Add(lblDebounceVal);

            debounceSlider.ValueChanged += (s, e) => {
                lblDebounceVal.Text = string.Format("{0} ms", debounceSlider.Value);
            };

            // --- Slider 2: Ctrl + Wheel Jump Multiplier ---
            Label lblCtrlTitle = new Label();
            lblCtrlTitle.Text = "Ctrl + Wheel Jump Distance (Multiplier):";
            lblCtrlTitle.Location = new Point(20, 205);
            lblCtrlTitle.AutoSize = true;
            lblCtrlTitle.Font = new Font("Segoe UI", 9);
            this.Controls.Add(lblCtrlTitle);

            ctrlStepsSlider = new TrackBar();
            ctrlStepsSlider.Minimum = 1;
            ctrlStepsSlider.Maximum = 15;
            ctrlStepsSlider.SmallChange = 1;
            ctrlStepsSlider.LargeChange = 3;
            ctrlStepsSlider.Value = Math.Max(1, Math.Min(15, _settings.CtrlJumpSteps));
            ctrlStepsSlider.Location = new Point(20, 225);
            ctrlStepsSlider.Size = new Size(290, 45);
            ctrlStepsSlider.TickFrequency = 1;
            this.Controls.Add(ctrlStepsSlider);

            lblCtrlStepsVal = new Label();
            lblCtrlStepsVal.Text = string.Format("{0}x steps", ctrlStepsSlider.Value);
            lblCtrlStepsVal.Location = new Point(320, 230);
            lblCtrlStepsVal.AutoSize = true;
            lblCtrlStepsVal.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            this.Controls.Add(lblCtrlStepsVal);

            ctrlStepsSlider.ValueChanged += (s, e) => {
                lblCtrlStepsVal.Text = string.Format("{0}x steps", ctrlStepsSlider.Value);
            };

            // --- Save Button ---
            Button btnSave = new Button();
            btnSave.Text = "Save & Apply";
            btnSave.Location = new Point(295, 280);
            btnSave.Size = new Size(100, 32);
            btnSave.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnSave.DialogResult = DialogResult.OK;
            btnSave.Click += (s, e) => {
                _settings.NormalStepKeyMode = rbArrow.Checked ? 0 : 1;
                _settings.DebounceMs = debounceSlider.Value;
                _settings.CtrlJumpSteps = ctrlStepsSlider.Value;
                this.Close();
            };
            this.Controls.Add(btnSave);
        }
    }

    static class Program
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int VK_CONTROL = 0x11;
        private const int VK_SHIFT = 0x10;

        private const byte VK_RIGHT = 0x27;
        private const byte VK_LEFT = 0x25;

        private const uint KEYEVENTF_KEYUP = 0x0002;

        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static AppSettings _settings = new AppSettings();
        private static long _lastScrollTimestamp = 0;
        private static SettingsForm _settingsForm = null;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _hookID = SetHook(_proc);

            NotifyIcon trayIcon = new NotifyIcon();
            trayIcon.Icon = SystemIcons.Application;
            trayIcon.Text = "WMP Wheel Seek v2 (Active)";
            trayIcon.Visible = true;

            ContextMenuStrip trayMenu = new ContextMenuStrip();
            ToolStripMenuItem titleItem = new ToolStripMenuItem("WMP Wheel Seek v2");
            titleItem.Enabled = false;
            trayMenu.Items.Add(titleItem);
            trayMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem settingsItem = new ToolStripMenuItem("Settings & Sensitivity Sliders...", null, (s, e) => {
                ShowSettings();
            });
            settingsItem.Font = new Font(trayMenu.Font, FontStyle.Bold);
            trayMenu.Items.Add(settingsItem);

            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Exit", null, (s, e) => {
                trayIcon.Visible = false;
                Application.Exit();
            });

            trayIcon.ContextMenuStrip = trayMenu;

            // Open settings window on double click of tray icon
            trayIcon.DoubleClick += (s, e) => ShowSettings();

            Application.Run();

            UnhookWindowsHookEx(_hookID);
        }

        private static void ShowSettings()
        {
            if (_settingsForm == null || _settingsForm.IsDisposed)
            {
                _settingsForm = new SettingsForm(_settings);
                _settingsForm.Show();
            }
            else
            {
                _settingsForm.BringToFront();
                _settingsForm.Focus();
            }
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_MOUSEWHEEL)
            {
                IntPtr hwnd = GetForegroundWindow();
                if (IsWmpWindow(hwnd))
                {
                    long now = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
                    if (now - _lastScrollTimestamp < _settings.DebounceMs)
                    {
                        // Throttled / Debounced to prevent excessive jumping
                        return (IntPtr)1;
                    }
                    _lastScrollTimestamp = now;

                    MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    short delta = (short)((hookStruct.mouseData >> 16) & 0xFFFF);

                    bool isCtrl = (GetKeyState(VK_CONTROL) & 0x8000) != 0;
                    bool scrollUp = delta > 0;
                    byte key = scrollUp ? VK_RIGHT : VK_LEFT;

                    if (isCtrl)
                    {
                        // Multi-step jump (Ctrl + Wheel)
                        for (int i = 0; i < _settings.CtrlJumpSteps; i++)
                        {
                            SendSingleKey(key, useCtrl: true);
                            System.Threading.Thread.Sleep(25);
                        }
                    }
                    else
                    {
                        // Single step jump (Normal Wheel)
                        bool useCtrl = (_settings.NormalStepKeyMode == 1);
                        SendSingleKey(key, useCtrl: useCtrl);
                    }

                    // Intercept wheel event so WMP doesn't scroll volume
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static bool IsWmpWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return false;

            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            if (pid == 0) return false;

            try
            {
                Process proc = Process.GetProcessById((int)pid);
                string procName = proc.ProcessName.ToLower();

                if (procName.Contains("wmplayer") || 
                    procName.Contains("microsoft.media.player") || 
                    procName.Contains("windowsmediabackends") || 
                    procName.Contains("mediaplayer") ||
                    procName.Contains("movie"))
                {
                    return true;
                }
            }
            catch { }

            StringBuilder sb = new StringBuilder(256);
            GetWindowText(hwnd, sb, 256);
            string title = sb.ToString().ToLower();

            if (title.Contains("media player") || title.Contains("windows media player"))
            {
                return true;
            }

            return false;
        }

        private static void SendSingleKey(byte keyCode, bool useCtrl)
        {
            if (useCtrl)
            {
                keybd_event((byte)VK_CONTROL, 0, 0, UIntPtr.Zero);
                keybd_event(keyCode, 0, 0, UIntPtr.Zero);
                keybd_event(keyCode, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                keybd_event((byte)VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
            else
            {
                keybd_event(keyCode, 0, 0, UIntPtr.Zero);
                keybd_event(keyCode, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
    }
}
