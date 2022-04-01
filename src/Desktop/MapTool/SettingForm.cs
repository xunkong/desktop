using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using static Xunkong.Desktop.MapTool.NativeMethod;

namespace Xunkong.Desktop.MapTool
{
    public partial class SettingForm : Form
    {


        public SettingForm()
        {
            InitializeComponent();
            var setting = Form1.Instance.Setting;
            checkBox1.Checked = setting.DontShowTitleBarWhenActived;
            checkBox2.Checked = setting.DisableClickTransparentWhenDeactived;
            trackBar1.Value = (int)(10 * setting.OpacityWhenActived);
            trackBar2.Value = (int)(10 * setting.OpacityWhenDeactived);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1.Instance.Setting.DontShowTitleBarWhenActived = checkBox1.Checked;
            Form1.Instance.Setting.DisableClickTransparentWhenDeactived = checkBox2.Checked;
            Form1.Instance.Setting.OpacityWhenActived = (double)trackBar1.Value / 10;
            Form1.Instance.Setting.OpacityWhenDeactived = (double)trackBar2.Value / 10;
            Form1.Instance.RefreshState();
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var pros = Process.GetProcessesByName("YuanShen").Concat(Process.GetProcessesByName("GenshinImpact")).FirstOrDefault();
            if (pros != null)
            {
                var hwnd = pros.MainWindowHandle;
                if (GetWindowRect(hwnd, out var rect))
                {
                    Form1.Instance.hwnd_genshin = hwnd;
                    Form1.Instance.Left = rect.Left;
                    Form1.Instance.Top = rect.Top;
                    Form1.Instance.Width = rect.Width;
                    Form1.Instance.Height = rect.Height;
                }
            }
        }
    }
}
