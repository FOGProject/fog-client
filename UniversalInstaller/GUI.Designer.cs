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

using System.Windows.Forms;
using MetroFramework.Controls;

namespace FOG
{
    partial class GUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GUI));
            this.panel1 = new System.Windows.Forms.Panel();
            this.licenseBox = new System.Windows.Forms.RichTextBox();
            this.exitButton = new System.Windows.Forms.Button();
            this.finishBtn = new MetroFramework.Controls.MetroButton();
            this.installBtn = new MetroFramework.Controls.MetroButton();
            this.agreeBtn = new MetroFramework.Controls.MetroButton();
            this.completedTab = new System.Windows.Forms.TabPage();
            this.settingsTab = new System.Windows.Forms.TabPage();
            this.progressBar1 = new MetroFramework.Controls.MetroProgressSpinner();
            this.label1 = new System.Windows.Forms.Label();
            this.logSwitch = new MetroFramework.Controls.MetroToggle();
            this.httpsSwitch = new MetroFramework.Controls.MetroToggle();
            this.httpsLabel = new System.Windows.Forms.Label();
            this.rootLogLabel = new System.Windows.Forms.Label();
            this.webRootTxtBox = new MetroFramework.Controls.MetroTextBox();
            this.webRootLabel = new System.Windows.Forms.Label();
            this.addressTxtBox = new MetroFramework.Controls.MetroTextBox();
            this.addressLabel = new System.Windows.Forms.Label();
            this.licenseTab = new System.Windows.Forms.TabPage();
            this.tabControl = new MetroTabControl();
            this.welcomeTab = new System.Windows.Forms.TabPage();
            this.welcomeText = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button1 = new MetroButton();
            this.progressTab = new System.Windows.Forms.TabPage();
            this.progressBox = new System.Windows.Forms.RichTextBox();
            this.title = new System.Windows.Forms.Label();
            this.completedTab.SuspendLayout();
            this.settingsTab.SuspendLayout();
            this.licenseTab.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.welcomeTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.progressTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
            this.panel1.Location = new System.Drawing.Point(0, -5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(652, 10);
            this.panel1.TabIndex = 5;
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            this.panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseMove);
            this.panel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseUp);
            // 
            // licenseBox
            // 
            this.licenseBox.Location = new System.Drawing.Point(6, 6);
            this.licenseBox.Name = "licenseBox";
            this.licenseBox.ReadOnly = true;
            this.licenseBox.Size = new System.Drawing.Size(599, 314);
            this.licenseBox.TabIndex = 1;
            this.licenseBox.Text = resources.GetString("licenseBox.Text");
            // 
            // exitButton
            // 
            this.exitButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.exitButton.FlatAppearance.BorderSize = 0;
            this.exitButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightGray;
            this.exitButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.exitButton.Location = new System.Drawing.Point(616, 13);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(20, 20);
            this.exitButton.TabIndex = 6;
            this.exitButton.TabStop = false;
            this.exitButton.Text = "X";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // finishBtn
            // 
            this.finishBtn.Location = new System.Drawing.Point(6, 326);
            this.finishBtn.Name = "finishBtn";
            this.finishBtn.Size = new System.Drawing.Size(599, 23);
            this.finishBtn.TabIndex = 9;
            this.finishBtn.Text = "Finish";
            this.finishBtn.UseVisualStyleBackColor = true;
            this.finishBtn.Click += new System.EventHandler(this.FinishBtnOnClick);
            // 
            // installBtn
            // 
            this.installBtn.Location = new System.Drawing.Point(7, 326);
            this.installBtn.Name = "installBtn";
            this.installBtn.Size = new System.Drawing.Size(598, 23);
            this.installBtn.TabIndex = 7;
            this.installBtn.Text = "Install";
            this.installBtn.UseVisualStyleBackColor = true;
            this.installBtn.Click += new System.EventHandler(this.InstallBtnOnClick);
            // 
            // agreeBtn
            // 
            this.agreeBtn.Location = new System.Drawing.Point(6, 326);
            this.agreeBtn.Name = "agreeBtn";
            this.agreeBtn.Size = new System.Drawing.Size(599, 23);
            this.agreeBtn.TabIndex = 0;
            this.agreeBtn.Text = "I Agree";
            this.agreeBtn.UseVisualStyleBackColor = true;
            this.agreeBtn.Click += new System.EventHandler(this.AgreeBtnOnClick);
            // 
            // completedTab
            // 
            this.completedTab.BackColor = System.Drawing.Color.White;
            this.completedTab.Controls.Add(this.finishBtn);
            this.completedTab.Location = new System.Drawing.Point(4, 24);
            this.completedTab.Name = "completedTab";
            this.completedTab.Padding = new System.Windows.Forms.Padding(3);
            this.completedTab.Size = new System.Drawing.Size(611, 355);
            this.completedTab.TabIndex = 4;
            this.completedTab.Text = "Complete";
            this.completedTab.Visible = false;
            // 
            // settingsTab
            // 
            this.settingsTab.BackColor = System.Drawing.Color.White;
            this.settingsTab.Controls.Add(this.progressBar1);
            this.settingsTab.Controls.Add(this.label1);
            this.settingsTab.Controls.Add(this.logSwitch);
            this.settingsTab.Controls.Add(this.httpsSwitch);
            this.settingsTab.Controls.Add(this.httpsLabel);
            this.settingsTab.Controls.Add(this.rootLogLabel);
            this.settingsTab.Controls.Add(this.webRootTxtBox);
            this.settingsTab.Controls.Add(this.webRootLabel);
            this.settingsTab.Controls.Add(this.addressTxtBox);
            this.settingsTab.Controls.Add(this.addressLabel);
            this.settingsTab.Controls.Add(this.installBtn);
            this.settingsTab.Location = new System.Drawing.Point(4, 24);
            this.settingsTab.Name = "settingsTab";
            this.settingsTab.Padding = new System.Windows.Forms.Padding(3);
            this.settingsTab.Size = new System.Drawing.Size(611, 355);
            this.settingsTab.TabIndex = 2;
            this.settingsTab.Text = "Configure";
            this.settingsTab.Visible = false;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(93, 290);
            this.progressBar1.Maximum = 100;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(25, 25);
            this.progressBar1.TabIndex = 19;
            this.progressBar1.Value = 80;
            this.progressBar1.Spinning = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 296);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 15);
            this.label1.TabIndex = 18;
            this.label1.Text = "Server Status:";
            // 
            // logSwitch
            // 
            this.logSwitch.AutoSize = true;
            this.logSwitch.Location = new System.Drawing.Point(138, 67);
            this.logSwitch.Name = "logSwitch";
            this.logSwitch.Size = new System.Drawing.Size(80, 19);
            this.logSwitch.TabIndex = 17;
            this.logSwitch.Text = "Off";
            this.logSwitch.UseVisualStyleBackColor = false;
            // 
            // httpsSwitch
            // 
            this.httpsSwitch.AutoSize = true;
            this.httpsSwitch.Location = new System.Drawing.Point(138, 95);
            this.httpsSwitch.Name = "httpsSwitch";
            this.httpsSwitch.Size = new System.Drawing.Size(80, 19);
            this.httpsSwitch.TabIndex = 16;
            this.httpsSwitch.Text = "Off";
            this.httpsSwitch.UseVisualStyleBackColor = false;

            // 
            // httpsLabel
            // 
            this.httpsLabel.AutoSize = true;
            this.httpsLabel.Font = new System.Drawing.Font("Open Sans", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.httpsLabel.Location = new System.Drawing.Point(6, 91);
            this.httpsLabel.Name = "httpsLabel";
            this.httpsLabel.Size = new System.Drawing.Size(59, 22);
            this.httpsLabel.TabIndex = 14;
            this.httpsLabel.Text = "HTTPS";
            // 
            // rootLogLabel
            // 
            this.rootLogLabel.AutoSize = true;
            this.rootLogLabel.Font = new System.Drawing.Font("Open Sans", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rootLogLabel.Location = new System.Drawing.Point(6, 63);
            this.rootLogLabel.Name = "rootLogLabel";
            this.rootLogLabel.Size = new System.Drawing.Size(117, 22);
            this.rootLogLabel.TabIndex = 12;
            this.rootLogLabel.Text = "Log in root dir";
            // 
            // webRootTxtBox
            // 
            this.webRootTxtBox.Location = new System.Drawing.Point(138, 36);
            this.webRootTxtBox.Name = "webRootTxtBox";
            this.webRootTxtBox.Size = new System.Drawing.Size(464, 22);
            this.webRootTxtBox.TabIndex = 11;
            this.webRootTxtBox.Text = "/fog";
            // 
            // webRootLabel
            // 
            this.webRootLabel.AutoSize = true;
            this.webRootLabel.Font = new System.Drawing.Font("Open Sans", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.webRootLabel.Location = new System.Drawing.Point(6, 35);
            this.webRootLabel.Name = "webRootLabel";
            this.webRootLabel.Size = new System.Drawing.Size(84, 22);
            this.webRootLabel.TabIndex = 10;
            this.webRootLabel.Text = "Web Root";
            // 
            // addressTxtBox
            // 
            this.addressTxtBox.Location = new System.Drawing.Point(138, 8);
            this.addressTxtBox.Name = "addressTxtBox";
            this.addressTxtBox.Size = new System.Drawing.Size(464, 22);
            this.addressTxtBox.TabIndex = 9;
            this.addressTxtBox.Text = "fog-server";

            // 
            // addressLabel
            // 
            this.addressLabel.AutoSize = true;
            this.addressLabel.Font = new System.Drawing.Font("Open Sans", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.addressLabel.Location = new System.Drawing.Point(6, 7);
            this.addressLabel.Name = "addressLabel";
            this.addressLabel.Size = new System.Drawing.Size(125, 22);
            this.addressLabel.TabIndex = 8;
            this.addressLabel.Text = "Server Address";
            // 
            // licenseTab
            // 
            this.licenseTab.BackColor = System.Drawing.Color.White;
            this.licenseTab.Controls.Add(this.licenseBox);
            this.licenseTab.Controls.Add(this.agreeBtn);
            this.licenseTab.Location = new System.Drawing.Point(4, 24);
            this.licenseTab.Name = "licenseTab";
            this.licenseTab.Padding = new System.Windows.Forms.Padding(3);
            this.licenseTab.Size = new System.Drawing.Size(611, 355);
            this.licenseTab.TabIndex = 0;
            this.licenseTab.Text = "License";
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.welcomeTab);
            this.tabControl.Controls.Add(this.licenseTab);
            this.tabControl.Controls.Add(this.settingsTab);
            this.tabControl.Controls.Add(this.progressTab);
            this.tabControl.Controls.Add(this.completedTab);
            this.tabControl.Font = new System.Drawing.Font("Open Sans", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl.Location = new System.Drawing.Point(12, 73);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(619, 383);
            this.tabControl.TabIndex = 3;
            this.tabControl.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tabControl1_Selecting);
            // 
            // welcomeTab
            // 
            this.welcomeTab.Controls.Add(this.welcomeText);
            this.welcomeTab.Controls.Add(this.pictureBox1);
            this.welcomeTab.Controls.Add(this.button1);
            this.welcomeTab.Location = new System.Drawing.Point(4, 24);
            this.welcomeTab.Name = "welcomeTab";
            this.welcomeTab.Padding = new System.Windows.Forms.Padding(3);
            this.welcomeTab.Size = new System.Drawing.Size(611, 355);
            this.welcomeTab.TabIndex = 6;
            this.welcomeTab.Text = "Welcome";
            this.welcomeTab.UseVisualStyleBackColor = true;
            // 
            // welcomeText
            // 
            this.welcomeText.BackColor = System.Drawing.Color.White;
            this.welcomeText.Location = new System.Drawing.Point(7, 127);
            this.welcomeText.Multiline = true;
            this.welcomeText.Name = "welcomeText";
            this.welcomeText.ReadOnly = true;
            this.welcomeText.Size = new System.Drawing.Size(598, 193);
            this.welcomeText.TabIndex = 5;
            this.welcomeText.Text = resources.GetString("welcomeText.Text");
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(-16, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(645, 120);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 326);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(599, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Begin";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // progressTab
            // 
            this.progressTab.Controls.Add(this.progressBox);
            this.progressTab.Location = new System.Drawing.Point(4, 24);
            this.progressTab.Name = "progressTab";
            this.progressTab.Padding = new System.Windows.Forms.Padding(3);
            this.progressTab.Size = new System.Drawing.Size(611, 355);
            this.progressTab.TabIndex = 5;
            this.progressTab.Text = "Progress";
            this.progressTab.UseVisualStyleBackColor = true;
            // 
            // progressBox
            // 
            this.progressBox.Location = new System.Drawing.Point(6, 6);
            this.progressBox.Name = "progressBox";
            this.progressBox.ReadOnly = true;
            this.progressBox.Size = new System.Drawing.Size(599, 343);
            this.progressBox.TabIndex = 11;
            this.progressBox.Text = "";
            // 
            // title
            // 
            this.title.Font = new System.Drawing.Font("Open Sans Light", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.title.Location = new System.Drawing.Point(12, 13);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(598, 43);
            this.title.TabIndex = 8;
            this.title.Text = "FOG Service Installer";
            this.title.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            this.title.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseMove);
            this.title.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseUp);
            // 
            // GUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(648, 468);
            this.ControlBox = false;
            this.Controls.Add(this.title);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(650, 470);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(650, 470);
            this.Name = "GUI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.completedTab.ResumeLayout(false);
            this.settingsTab.ResumeLayout(false);
            this.settingsTab.PerformLayout();
            this.licenseTab.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.welcomeTab.ResumeLayout(false);
            this.welcomeTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.progressTab.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RichTextBox licenseBox;
        private System.Windows.Forms.Button exitButton;
        private MetroButton finishBtn;
        private MetroButton installBtn;
        private MetroButton agreeBtn;
        private TabPage completedTab;
        private TabPage settingsTab;
        private TabPage licenseTab;
        private MetroTabControl tabControl;
        private MetroTextBox addressTxtBox;
        private Label addressLabel;
        private MetroTextBox webRootTxtBox;
        private Label webRootLabel;
        private Label rootLogLabel;
        private Label httpsLabel;
        private Label title;
        private MetroToggle logSwitch;
        private MetroToggle httpsSwitch;
        private TabPage progressTab;
        private RichTextBox progressBox;
        private TabPage welcomeTab;
        private MetroButton button1;
        private PictureBox pictureBox1;
        private TextBox welcomeText;
        private Label label1;
        private MetroProgressSpinner progressBar1;
    }
}