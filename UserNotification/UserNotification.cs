using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using FOG.Core;

namespace UserNotification
{
    public partial class UserNotification : Form
    {
        public UserNotification()
        {
            var args = Environment.GetCommandLineArgs();

            InitializeComponent();

            var workingArea = Screen.GetWorkingArea(this);
            var xPoint = workingArea.Right - Size.Width;
            var height = (Settings.OS == Settings.OSType.Windows) ? workingArea.Bottom - Size.Height : 0;
            Location = new Point(xPoint, height);

            FadeIn();
            FadeOut();
        }

        private async Task FadeOut()
        {
            await Task.Delay(6500);

            for (var i = 1.0; i > 0; i = i - 0.01)
            {
                this.Opacity = i;
                Application.DoEvents();
                System.Threading.Thread.Sleep(5);
            }
            Application.Exit();
        }

        private async Task FadeIn()
        {
            await Task.Delay(500);

            for (var i = 0.0; i <= 1.0; i = i + 0.01)
            {
                this.Opacity = i;
                Application.DoEvents();
                System.Threading.Thread.Sleep(5);
            }
            this.Opacity = 1.0;
        }

        private void SpawnCenter()
        {
            ProcessHandler.RunClientEXE("NotificationCenter.exe", "", false);
            Application.Exit();
        }

        private void logButton_Click(object sender, EventArgs e)
        {
            SpawnCenter();
        }
    }
}
