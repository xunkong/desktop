using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Windows.Forms;
using static Xunkong.Desktop.MapTool.NativeMethod;

namespace Xunkong.Desktop.MapTool
{
    public partial class Form1 : Form
    {

        public static Form1 Instance;

        private readonly IntPtr _hwnd;

        public Setting Setting { get; set; }

        private bool isActived = true;

        public IntPtr hwnd_genshin;

        public Form1()
        {
            InitializeComponent();
            Instance = this;
            _hwnd = this.Handle;
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            var pros = Process.GetProcessesByName("YuanShen").Concat(Process.GetProcessesByName("GenshinImpact")).FirstOrDefault();
            if (pros != null)
            {
                hwnd_genshin = pros.MainWindowHandle;
            }

            try
            {
                var iniFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Xunkong\MapTool\mapsetting.json");
                if (File.Exists(iniFile))
                {
                    var str = File.ReadAllText(iniFile);
                    Setting = JsonConvert.DeserializeObject<Setting>(str);
                    Left = Setting.Left;
                    Top = Setting.Top;
                    Width = Setting.Width;
                    Height = Setting.Height;
                    if (Setting.DontShowTitleBarWhenActived)
                    {
                        titleBar.Visible = false;
                    }
                }
                if (Setting is null)
                {
                    Setting = new Setting();
                }
            }
            catch
            {
                Setting = new Setting();
            }

            RegisterHotKey(_hwnd, 1001, HotKeyModifiers.MOD_ALT, (uint)Keys.Q);
            RegisterHotKey(_hwnd, 1002, HotKeyModifiers.MOD_ALT, (uint)Keys.S);
            RegisterHotKey(_hwnd, 1003, HotKeyModifiers.MOD_ALT, (uint)Keys.Up);
            RegisterHotKey(_hwnd, 1004, HotKeyModifiers.MOD_ALT, (uint)Keys.Down);
        }



        protected override void WndProc(ref Message m)
        {
            // WM_HOTKEY
            if (m.Msg == 0x0312)
            {
                switch (m.WParam.ToInt32())
                {
                    case 1001:
                        if (isActived)
                        {
                            DeactivateForm();
                        }
                        else
                        {
                            ActivateForm();
                        }
                        isActived = !isActived;
                        break;
                    case 1002:
                        ShowSettingForm();
                        break;
                    case 1003:
                        Setting.OpacityWhenDeactived = Clamp(Setting.OpacityWhenDeactived + 0.1, 0, 1);
                        RefreshState();
                        break;
                    case 1004:
                        Setting.OpacityWhenDeactived = Clamp(Setting.OpacityWhenDeactived - 0.1, 0, 1);
                        RefreshState();
                        break;
                    default:
                        break;
                }

            }

            base.WndProc(ref m);
        }


        private double Clamp(double value, double min, double max)
        {
            if (value > max)
            {
                value = max;
            }
            if (value < min)
            {
                value = min;
            }
            return value;
        }


        private void ShowSettingForm()
        {
            var settingForm = new SettingForm();
            settingForm.Show();
            SetForegroundWindow(settingForm.Handle);
        }

        public void ActivateForm()
        {
            if (Setting.DontShowTitleBarWhenActived)
            {
                titleBar.Visible = false;
            }
            else
            {
                titleBar.Visible = true;
            }
            var exstyle = (WindowStylesEx)GetWindowLong(_hwnd, WindowLongFlags.GWL_EXSTYLE);
            exstyle &= ~WindowStylesEx.WS_EX_TRANSPARENT;
            SetWindowLong(_hwnd, WindowLongFlags.GWL_EXSTYLE, (uint)exstyle);
            Opacity = Setting.OpacityWhenActived;
            SetForegroundWindow(_hwnd);
        }


        public void DeactivateForm()
        {
            titleBar.Visible = false;
            var exstyle = (WindowStylesEx)GetWindowLong(_hwnd, WindowLongFlags.GWL_EXSTYLE);
            if (Setting.DisableClickTransparentWhenDeactived)
            {
                exstyle &= ~WindowStylesEx.WS_EX_TRANSPARENT;
            }
            else
            {
                exstyle |= WindowStylesEx.WS_EX_TRANSPARENT;
            }
            SetWindowLong(_hwnd, WindowLongFlags.GWL_EXSTYLE, (uint)exstyle);
            Opacity = Setting.OpacityWhenDeactived;
            SetForegroundWindow(hwnd_genshin);
        }


        public void RefreshState()
        {
            if (isActived)
            {
                ActivateForm();
            }
            else
            {
                DeactivateForm();
            }
        }



        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            webView21.Width = this.Width;
            webView21.Height = this.Height;
            titleBar.Width = this.Width;
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            webView21.Width = this.Width;
            webView21.Height = this.Height;
            titleBar.Width = this.Width;
        }


        Point mPoint;

        private void titleBar_MouseDown(object sender, MouseEventArgs e)
        {
            mPoint = new Point(e.X, e.Y);
        }

        private void titleBar_MouseMove(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                this.Location = new Point(this.Location.X + e.X - mPoint.X, this.Location.Y + e.Y - mPoint.Y);
            }
        }



        private void button_close_Click(object sender, EventArgs e)
        {
            Close();
        }



        private void button_resize_Click(object sender, EventArgs e)
        {
            if (FormBorderStyle == FormBorderStyle.None)
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.None;
            }
        }


        private void button_seting_Click(object sender, EventArgs e)
        {
            ShowSettingForm();
        }


        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Setting.Left = Left;
            Setting.Top = Top;
            Setting.Width = Width;
            Setting.Height = Height;
            var str = JsonConvert.SerializeObject(Setting, Formatting.Indented);
            var iniFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Xunkong\MapTool\mapsetting.json");
            Directory.CreateDirectory(Path.GetDirectoryName(iniFile));
            File.WriteAllText(iniFile, str);
        }


    }
}