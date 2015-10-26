/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2015 FOG Project
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 3
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Zazzles;

namespace UserNotification
{
    public partial class NotificationGUI : Form
    {
        public NotificationGUI()
        {
            InitializeComponent();
            Location = new Point(0, 0);
        }

        public void SetTitle(string title)
        {
            this.titleLabel.Text = title;
        }

        public void SetBody(string body)
        {
            this.bodyLabel.Text = body;
        }

        public void StartFade()
        {
#pragma warning disable 4014
            FadeIn();
            FadeOut();
#pragma warning restore 4014
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

            this.Opacity = 0;
            this.Close();
        }

        private async Task FadeIn()
        {
            await Task.Delay(500);
            this.Opacity = 0;
            this.Show();
            for (var i = 0.0; i <= 1.0; i = i + 0.01)
            {
                this.Opacity = i;
                Application.DoEvents();
                System.Threading.Thread.Sleep(5);
            }
            this.Opacity = 1.0;
        }

        private void logButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
