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
        public int KeyShortcutMode { get; set; }  // 0 = Ctrl+Arrow (Reliable Seek), 1 = Arrow Key, 2 = Ctrl+Shift+F/B
        public int DebounceMs { get; set; }        // Cooldown in ms
        public int CtrlJumpSteps { get; set; }     // Multiplier for Ctrl+Wheel
        public bool IsEnabled { get; set; }

        public AppSettings()
        {
            KeyShortcutMode = 0; // Default to Ctrl+Arrow (works on all WMP versions)
            DebounceMs = 60;     // Fast responsive default
            CtrlJumpSteps = 5;
            IsEnabled = true;
        }
    }

    public class SettingsForm : Form
    {
        public TrackBar debounceSlider;
        public TrackBar ctrlStepsSlider;
        public RadioButton rbCtrlArrow;
        public RadioButton rbArrow;
        public RadioButton rbFastForward;
        public Label lblDebounceVal;
        public Label lblCtrlStepsVal;
        public CheckBox chkEnable;

        private AppSettings _settings;

        public SettingsForm(AppSettings settings)
        {
            _settings = settings;

            this.Text = "WMP Wheel Seek v3 - Settings & Controls";
            this.Size = new Size(450, 420);
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

            chkEnable = new CheckBox();
            chkEnable.Text = "Enable Mouse Wheel Seeking in WMP";
            chkEnable.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            chkEnable.Location = new Point(20, 45);
            chkEnable.AutoSize = true;
            chkEnable.Checked = _settings.IsEnabled;
            this.Controls.Add(chkEnable);

            // --- Key Mode Selection ---
            GroupBox gbMode = new GroupBox();
            gbMode.Text = "Seek Shortcut Method";
            gbMode.Location = new Point(20, 75);
            gbMode.Size = new Size(395, 105);
            gbMode.Font = new Font("Segoe UI", 9);

            rbCtrlArrow = new RadioButton();
            rbCtrlArrow.Text = "Ctrl + Arrow Keys (Standard Seek) [Most Reliable]";
            rbCtrlArrow.Location = new Point(15, 22);
            rbCtrlArrow.AutoSize = true;
            rbCtrlArrow.Checked = (_settings.KeyShortcutMode == 0);

            rbArrow = new RadioButton();
            rbArrow.Text = "Plain Arrow Keys (Short step - requires timeline focus)";
            rbArrow.Location = new Point(15, 47);
            rbArrow.AutoSize = true;
            rbArrow.Checked = (_settings.KeyShortcutMode == 1);

            rbFastForward = new RadioButton();
            rbFastForward.Text = "Ctrl + Shift + F/B (Fast Forward / Rewind)";
            rbFastForward.Location = new Point(15, 72);
            rbFastForward.AutoSize = true;
            rbFastForward.Checked = (_settings.KeyShortcutMode == 2);

            gbMode.Controls.Add(rbCtrlArrow);
            gbMode.Controls.Add(rbArrow);
            gbMode.Controls.Add(rbFastForward);
            this.Controls.Add(gbMode);

            // --- Slider 1: Scroll Cooldown / Debounce ---
            Label lblDebounceTitle = new Label();
            lblDebounceTitle.Text = "Scroll Cooldown / Sensitivity (lower = faster response):";
            lblDebounceTitle.Location = new Point(20, 190);
            lblDebounceTitle.AutoSize = true;
            lblDebounceTitle.Font = new Font("Segoe UI", 9);
            this.Controls.Add(lblDebounceTitle);

            debounceSlider = new TrackBar();
            debounceSlider.Minimum = 0;
            debounceSlider.Maximum = 300;
            debounceSlider.SmallChange = 10;
            debounceSlider.LargeChange = 30;
            debounceSlider.Value = Math.Max(0, Math.Min(300, _settings.DebounceMs));
            debounceSlider.Location = new Point(20, 210);
            debounceSlider.Size = new Size(300, 45);
            debounceSlider.TickFrequency = 30;
            this.Controls.Add(debounceSlider);

            lblDebounceVal = new Label();
            lblDebounceVal.Text = string.Format("{0} ms", debounceSlider.Value);
            lblDebounceVal.Location = new Point(335, 215);
            lblDebounceVal.AutoSize = true;
            lblDebounceVal.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            this.Controls.Add(lblDebounceVal);

            debounceSlider.ValueChanged += (s, e) => {
                lblDebounceVal.Text = string.Format("{0} ms", debounceSlider.Value);
            };

            // --- Slider 2: Ctrl + Wheel Jump Multiplier ---
            Label lblCtrlTitle = new Label();
            lblCtrlTitle.Text = "Ctrl + Wheel Jump Multiplier (Minute Jump Steps):";
            lblCtrlTitle.Location = new Point(20, 260);
            lblCtrlTitle.AutoSize = true;
            lblCtrlTitle.Font = new Font("Segoe UI", 9);
            this.Controls.Add(lblCtrlTitle);

            ctrlStepsSlider = new TrackBar();
            ctrlStepsSlider.Minimum = 1;
            ctrlStepsSlider.Maximum = 15;
            ctrlStepsSlider.SmallChange = 1;
            ctrlStepsSlider.LargeChange = 3;
            ctrlStepsSlider.Value = Math.Max(1, Math.Min(15, _settings.CtrlJumpSteps));
            ctrlStepsSlider.Location = new Point(20, 280);
            ctrlStepsSlider.Size = new Size(300, 45);
            ctrlStepsSlider.TickFrequency = 1;
            this.Controls.Add(ctrlStepsSlider);

            lblCtrlStepsVal = new Label();
            lblCtrlStepsVal.Text = string.Format("{0}x steps", ctrlStepsSlider.Value);
            lblCtrlStepsVal.Location = new Point(335, 285);
            lblCtrlStepsVal.AutoSize = true;
            lblCtrlStepsVal.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            this.Controls.Add(lblCtrlStepsVal);

            ctrlStepsSlider.ValueChanged += (s, e) => {
                lblCtrlStepsVal.Text = string.Format("{0}x steps", ctrlStepsSlider.Value);
            };

            // --- Save Button ---
            Button btnSave = new Button();
            btnSave.Text = "Save & Apply";
            btnSave.Location = new Point(315, 335);
            btnSave.Size = new Size(100, 32);
            btnSave.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnSave.DialogResult = DialogResult.OK;
            btnSave.Click += (s, e) => {
                _settings.IsEnabled = chkEnable.Checked;
                if (rbCtrlArrow.Checked) _settings.KeyShortcutMode = 0;
                else if (rbArrow.Checked) _settings.KeyShortcutMode = 1;
                else if (rbFastForward.Checked) _settings.KeyShortcutMode = 2;

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
        private const byte VK_F = 0x46;
        private const byte VK_B = 0x42;

        private const uint KEYEVENTF_KEYUP = 0x0002;

        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static AppSettings _settings = new AppSettings();
        private static long _lastScrollTimestamp = 0;
        private static SettingsForm _settingsForm = null;
        private static ToolStripMenuItem _enableMenuItem = null;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _hookID = SetHook(_proc);

            NotifyIcon trayIcon = new NotifyIcon();
            trayIcon.Icon = SystemIcons.Application;
            trayIcon.Text = "WMP Wheel Seek v3 (Active)";
            trayIcon.Visible = true;

            ContextMenuStrip trayMenu = new ContextMenuStrip();
            ToolStripMenuItem titleItem = new ToolStripMenuItem("WMP Wheel Seek v3");
            titleItem.Enabled = false;
            trayMenu.Items.Add(titleItem);
            trayMenu.Items.Add(new ToolStripSeparator());

            _enableMenuItem = new ToolStripMenuItem("Wheel Seeking Enabled", null, (s, e) => {
                _settings.IsEnabled = !_settings.IsEnabled;
                _enableMenuItem.Checked = _settings.IsEnabled;
            });
            _enableMenuItem.Checked = _settings.IsEnabled;
            trayMenu.Items.Add(_enableMenuItem);

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
            trayIcon.DoubleClick += (s, e) => ShowSettings();

            Application.Run();

            UnhookWindowsHookEx(_hookID);
        }

        private static void ShowSettings()
        {
            if (_settingsForm == null || _settingsForm.IsDisposed)
            {
                _settingsForm = new SettingsForm(_settings);
                _settingsForm.FormClosed += (s, e) => {
                    if (_enableMenuItem != null)
                        _enableMenuItem.Checked = _settings.IsEnabled;
                };
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
            if (nCode >= 0 && wParam == (IntPtr)WM_MOUSEWHEEL && _settings.IsEnabled)
            {
                IntPtr hwnd = GetForegroundWindow();
                if (IsWmpWindow(hwnd))
                {
                    long now = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
                    if (_settings.DebounceMs > 0 && (now - _lastScrollTimestamp < _settings.DebounceMs))
                    {
                        return (IntPtr)1;
                    }
                    _lastScrollTimestamp = now;

                    MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    short delta = (short)((hookStruct.mouseData >> 16) & 0xFFFF);

                    bool isCtrlHeld = (GetKeyState(VK_CONTROL) & 0x8000) != 0;
                    bool scrollUp = delta > 0;

                    if (isCtrlHeld)
                    {
                        // Multi-step jump (Ctrl + Wheel)
                        for (int i = 0; i < _settings.CtrlJumpSteps; i++)
                        {
                            SendSeekAction(scrollUp, _settings.KeyShortcutMode);
                            System.Threading.Thread.Sleep(20);
                        }
                    }
                    else
                    {
                        // Single step jump (Normal Wheel)
                        SendSeekAction(scrollUp, _settings.KeyShortcutMode);
                    }

                    // Intercept wheel event so WMP doesn't change volume or scroll UI
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
                    procName.Contains("zune") ||
                    procName.Contains("applicationframehost"))
                {
                    return true;
                }
            }
            catch { }

            StringBuilder sb = new StringBuilder(256);
            GetWindowText(hwnd, sb, 256);
            string title = sb.ToString().ToLower();

            if (title.Contains("media player") || 
                title.Contains("windows media player") || 
                title.Contains("films") || 
                title.Contains("movies"))
            {
                return true;
            }

            return false;
        }

        private static void SendSeekAction(bool scrollUp, int mode)
        {
            byte rightKey = VK_RIGHT;
            byte leftKey = VK_LEFT;

            if (mode == 0)
            {
                // Ctrl + Arrow Key
                byte key = scrollUp ? rightKey : leftKey;
                keybd_event((byte)VK_CONTROL, 0, 0, UIntPtr.Zero);
                keybd_event(key, 0, 0, UIntPtr.Zero);
                keybd_event(key, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                keybd_event((byte)VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
            else if (mode == 1)
            {
                // Plain Arrow Key
                byte key = scrollUp ? rightKey : leftKey;
                keybd_event(key, 0, 0, UIntPtr.Zero);
                keybd_event(key, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
            else if (mode == 2)
            {
                // Ctrl + Shift + F (Forward) / B (Backward)
                byte key = scrollUp ? VK_F : VK_B;
                keybd_event((byte)VK_CONTROL, 0, 0, UIntPtr.Zero);
                keybd_event((byte)VK_SHIFT, 0, 0, UIntPtr.Zero);
                keybd_event(key, 0, 0, UIntPtr.Zero);
                keybd_event(key, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                keybd_event((byte)VK_SHIFT, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                keybd_event((byte)VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
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
