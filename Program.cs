using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace WMPWheelSeek
{
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

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _hookID = SetHook(_proc);

            NotifyIcon trayIcon = new NotifyIcon();
            trayIcon.Icon = SystemIcons.Application;
            trayIcon.Text = "WMP Wheel Seek v1 (Active)";
            trayIcon.Visible = true;

            ContextMenuStrip trayMenu = new ContextMenuStrip();
            ToolStripMenuItem titleItem = new ToolStripMenuItem("WMP Wheel Seek v1 (Running)");
            titleItem.Enabled = false;
            trayMenu.Items.Add(titleItem);
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Exit", null, (s, e) => {
                trayIcon.Visible = false;
                Application.Exit();
            });

            trayIcon.ContextMenuStrip = trayMenu;

            Application.Run();

            UnhookWindowsHookEx(_hookID);
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
                    MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    short delta = (short)((hookStruct.mouseData >> 16) & 0xFFFF);

                    bool isCtrl = (GetKeyState(VK_CONTROL) & 0x8000) != 0;
                    bool isShift = (GetKeyState(VK_SHIFT) & 0x8000) != 0;

                    bool scrollUp = delta > 0;

                    if (isCtrl)
                    {
                        // 1-minute jump (sends multiple seek hotkeys)
                        byte key = scrollUp ? VK_RIGHT : VK_LEFT;
                        for (int i = 0; i < 6; i++)
                        {
                            SendCtrlKey(key);
                            System.Threading.Thread.Sleep(35);
                        }
                    }
                    else
                    {
                        // 1-second / short jump
                        byte key = scrollUp ? VK_RIGHT : VK_LEFT;
                        SendCtrlKey(key);
                    }

                    // Suppress standard mouse wheel volume scroll in WMP
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

        private static void SendCtrlKey(byte keyCode)
        {
            // Send Ctrl + Arrow Key
            keybd_event((byte)VK_CONTROL, 0, 0, UIntPtr.Zero);
            keybd_event(keyCode, 0, 0, UIntPtr.Zero);
            keybd_event(keyCode, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event((byte)VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
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
