/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2023 FOG Project
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.settingsTab = new System.Windows.Forms.TabPage();
            this.logSwitch = new MetroFramework.Controls.MetroToggle();
            this.traySwitch = new MetroFramework.Controls.MetroToggle();
            this.rootLogLabel = new System.Windows.Forms.Label();
            this.webRootTxtBox = new System.Windows.Forms.TextBox();
            this.webRootLabel = new System.Windows.Forms.Label();
            this.addressTxtBox = new System.Windows.Forms.TextBox();
            this.addressLabel = new System.Windows.Forms.Label();
            this.licenseTab = new System.Windows.Forms.TabPage();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.welcomeTab = new System.Windows.Forms.TabPage();
            this.welcomeText = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.beginButton = new MetroFramework.Controls.MetroButton();
            this.progressTab = new System.Windows.Forms.TabPage();
            this.logBox = new System.Windows.Forms.RichTextBox();
            this.nextButton = new MetroFramework.Controls.MetroButton();
            this.showLogButton = new MetroFramework.Controls.MetroButton();
            this.encryptionSpinner = new MetroFramework.Controls.MetroProgressSpinner();
            this.configSpinner = new MetroFramework.Controls.MetroProgressSpinner();
            this.filesSpinner = new MetroFramework.Controls.MetroProgressSpinner();
            this.busyWorkSpinner = new MetroFramework.Controls.MetroProgressSpinner();
            this.encryptLabel = new System.Windows.Forms.Label();
            this.configuringLabel = new System.Windows.Forms.Label();
            this.installFileLabel = new System.Windows.Forms.Label();
            this.busyWorkLabel = new System.Windows.Forms.Label();
            this.title = new System.Windows.Forms.Label();
            this.metroToggle1 = new MetroFramework.Controls.MetroToggle();
            this.label2 = new System.Windows.Forms.Label();
            this.completedTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
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
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoSize = true;
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
            this.panel1.Location = new System.Drawing.Point(0, -5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(641, 10);
            this.panel1.TabIndex = 5;
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form_MouseDown);
            this.panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form_MouseMove);
            this.panel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form_MouseUp);
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
            this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.exitButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.exitButton.FlatAppearance.BorderSize = 0;
            this.exitButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightGray;
            this.exitButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.exitButton.Location = new System.Drawing.Point(608, 13);
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
            this.completedTab.Controls.Add(this.textBox1);
            this.completedTab.Controls.Add(this.pictureBox2);
            this.completedTab.Controls.Add(this.finishBtn);
            this.completedTab.Location = new System.Drawing.Point(4, 24);
            this.completedTab.Name = "completedTab";
            this.completedTab.Padding = new System.Windows.Forms.Padding(3);
            this.completedTab.Size = new System.Drawing.Size(611, 355);
            this.completedTab.TabIndex = 4;
            this.completedTab.Text = "Complete";
            this.completedTab.Visible = false;
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.Color.White;
            this.textBox1.Location = new System.Drawing.Point(7, 127);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(598, 193);
            this.textBox1.TabIndex = 11;
            this.textBox1.Text = "FOG Service is installed and ready for use.\r\n";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(-16, 0);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(645, 120);
            this.pictureBox2.TabIndex = 10;
            this.pictureBox2.TabStop = false;
            // 
            // settingsTab
            // 
            this.settingsTab.BackColor = System.Drawing.Color.White;
            this.settingsTab.Controls.Add(this.label2);
            this.settingsTab.Controls.Add(this.logSwitch);
            this.settingsTab.Controls.Add(this.traySwitch);
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
            // logSwitch
            // 
            this.logSwitch.AutoSize = true;
            this.logSwitch.Location = new System.Drawing.Point(138, 95);
            this.logSwitch.Name = "logSwitch";
            this.logSwitch.Size = new System.Drawing.Size(80, 19);
            this.logSwitch.TabIndex = 17;
            this.logSwitch.UseVisualStyleBackColor = false;

            // 
            // traySwitch
            // 
            this.traySwitch.AutoSize = true;
            this.traySwitch.Location = new System.Drawing.Point(138, 67);
            this.traySwitch.Name = "traySwitch";
            this.traySwitch.Size = new System.Drawing.Size(80, 19);
            this.traySwitch.TabIndex = 17;
            this.traySwitch.Checked = true;
            this.traySwitch.UseVisualStyleBackColor = false;

            // 
            // rootLogLabel
            // 
            this.rootLogLabel.AutoSize = true;
            this.rootLogLabel.Font = new System.Drawing.Font("Open Sans", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rootLogLabel.Location = new System.Drawing.Point(6, 94);
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
            this.addressTxtBox.Text = "fogserver";
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
            this.welcomeTab.Controls.Add(this.beginButton);
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
            this.welcomeText.Text = "Welcome to the FOG Service Smart Installer. \r\n\r\nThis installer has detected that " +
    "you are running a {OPERATING_SYSTEM} based operating system and has configured i" +
    "tself accordingly.\r\n";
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
            // beginButton
            // 
            this.beginButton.Location = new System.Drawing.Point(6, 326);
            this.beginButton.Name = "beginButton";
            this.beginButton.Size = new System.Drawing.Size(599, 23);
            this.beginButton.TabIndex = 0;
            this.beginButton.Text = "Begin";
            this.beginButton.UseVisualStyleBackColor = true;
            this.beginButton.Click += new System.EventHandler(this.beginClick);
            // 
            // progressTab
            // 
            this.progressTab.Controls.Add(this.logBox);
            this.progressTab.Controls.Add(this.nextButton);
            this.progressTab.Controls.Add(this.showLogButton);
            this.progressTab.Controls.Add(this.encryptionSpinner);
            this.progressTab.Controls.Add(this.configSpinner);
            this.progressTab.Controls.Add(this.filesSpinner);
            this.progressTab.Controls.Add(this.busyWorkSpinner);
            this.progressTab.Controls.Add(this.encryptLabel);
            this.progressTab.Controls.Add(this.configuringLabel);
            this.progressTab.Controls.Add(this.installFileLabel);
            this.progressTab.Controls.Add(this.busyWorkLabel);
            this.progressTab.Location = new System.Drawing.Point(4, 24);
            this.progressTab.Name = "progressTab";
            this.progressTab.Padding = new System.Windows.Forms.Padding(3);
            this.progressTab.Size = new System.Drawing.Size(611, 355);
            this.progressTab.TabIndex = 5;
            this.progressTab.Text = "Progress";
            this.progressTab.UseVisualStyleBackColor = true;
            // 
            // logBox
            // 
            this.logBox.Location = new System.Drawing.Point(10, 213);
            this.logBox.Name = "logBox";
            this.logBox.ReadOnly = true;
            this.logBox.Size = new System.Drawing.Size(584, 106);
            this.logBox.TabIndex = 21;
            this.logBox.Text = "";
            this.logBox.Visible = false;
            // 
            // nextButton
            // 
            this.nextButton.Enabled = false;
            this.nextButton.Location = new System.Drawing.Point(330, 326);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(275, 23);
            this.nextButton.TabIndex = 20;
            this.nextButton.Text = "Next";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.NextBtnOnClick);
            // 
            // showLogButton
            // 
            this.showLogButton.Location = new System.Drawing.Point(6, 326);
            this.showLogButton.Name = "showLogButton";
            this.showLogButton.Size = new System.Drawing.Size(275, 23);
            this.showLogButton.TabIndex = 19;
            this.showLogButton.Text = "Show Log";
            this.showLogButton.UseVisualStyleBackColor = true;
            this.showLogButton.Click += new System.EventHandler(this.ToggleLogButtonClick);
            // 
            // encryptionSpinner
            // 
            this.encryptionSpinner.EnsureVisible = false;
            this.encryptionSpinner.Location = new System.Drawing.Point(569, 111);
            this.encryptionSpinner.Maximum = 100;
            this.encryptionSpinner.Name = "encryptionSpinner";
            this.encryptionSpinner.Size = new System.Drawing.Size(25, 25);
            this.encryptionSpinner.TabIndex = 17;
            // 
            // configSpinner
            // 
            this.configSpinner.EnsureVisible = false;
            this.configSpinner.Location = new System.Drawing.Point(569, 80);
            this.configSpinner.Maximum = 100;
            this.configSpinner.Name = "configSpinner";
            this.configSpinner.Size = new System.Drawing.Size(25, 25);
            this.configSpinner.TabIndex = 16;
            // 
            // filesSpinner
            // 
            this.filesSpinner.EnsureVisible = false;
            this.filesSpinner.Location = new System.Drawing.Point(569, 49);
            this.filesSpinner.Maximum = 100;
            this.filesSpinner.Name = "filesSpinner";
            this.filesSpinner.Size = new System.Drawing.Size(25, 25);
            this.filesSpinner.TabIndex = 15;
            // 
            // busyWorkSpinner
            // 
            this.busyWorkSpinner.EnsureVisible = false;
            this.busyWorkSpinner.Location = new System.Drawing.Point(569, 18);
            this.busyWorkSpinner.Maximum = 100;
            this.busyWorkSpinner.Name = "busyWorkSpinner";
            this.busyWorkSpinner.Size = new System.Drawing.Size(25, 25);
            this.busyWorkSpinner.TabIndex = 14;
            // 
            // encryptLabel
            // 
            this.encryptLabel.AutoSize = true;
            this.encryptLabel.Font = new System.Drawing.Font("Open Sans", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.encryptLabel.ForeColor = System.Drawing.Color.Silver;
            this.encryptLabel.Location = new System.Drawing.Point(6, 114);
            this.encryptLabel.Name = "encryptLabel";
            this.encryptLabel.Size = new System.Drawing.Size(76, 22);
            this.encryptLabel.TabIndex = 12;
            this.encryptLabel.Text = "Securing";
            // 
            // configuringLabel
            // 
            this.configuringLabel.AutoSize = true;
            this.configuringLabel.Font = new System.Drawing.Font("Open Sans", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.configuringLabel.ForeColor = System.Drawing.Color.Silver;
            this.configuringLabel.Location = new System.Drawing.Point(6, 83);
            this.configuringLabel.Name = "configuringLabel";
            this.configuringLabel.Size = new System.Drawing.Size(183, 22);
            this.configuringLabel.TabIndex = 11;
            this.configuringLabel.Text = "Applying Configuration";
            // 
            // installFileLabel
            // 
            this.installFileLabel.AutoSize = true;
            this.installFileLabel.Font = new System.Drawing.Font("Open Sans", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.installFileLabel.ForeColor = System.Drawing.Color.Silver;
            this.installFileLabel.Location = new System.Drawing.Point(6, 52);
            this.installFileLabel.Name = "installFileLabel";
            this.installFileLabel.Size = new System.Drawing.Size(115, 22);
            this.installFileLabel.TabIndex = 10;
            this.installFileLabel.Text = "Installing Files";
            // 
            // busyWorkLabel
            // 
            this.busyWorkLabel.AutoSize = true;
            this.busyWorkLabel.Font = new System.Drawing.Font("Open Sans", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.busyWorkLabel.ForeColor = System.Drawing.Color.Silver;
            this.busyWorkLabel.Location = new System.Drawing.Point(6, 18);
            this.busyWorkLabel.Name = "busyWorkLabel";
            this.busyWorkLabel.Size = new System.Drawing.Size(170, 22);
            this.busyWorkLabel.TabIndex = 9;
            this.busyWorkLabel.Text = "Getting Things Ready";
            // 
            // title
            // 
            this.title.Font = new System.Drawing.Font("Open Sans Light", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.title.Location = new System.Drawing.Point(12, 13);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(598, 43);
            this.title.TabIndex = 8;
            this.title.Text = "FOG Service Installer";
            this.title.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form_MouseDown);
            this.title.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form_MouseMove);
            this.title.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form_MouseUp);
            // 
            // metroToggle1
            // 
            this.metroToggle1.AutoSize = true;
            this.metroToggle1.Location = new System.Drawing.Point(138, 95);
            this.metroToggle1.Name = "httpsSwitch";
            this.metroToggle1.Size = new System.Drawing.Size(80, 19);
            this.metroToggle1.TabIndex = 16;
            this.metroToggle1.Text = "Off";
            this.metroToggle1.UseVisualStyleBackColor = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Open Sans", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(6, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 22);
            this.label2.TabIndex = 20;
            this.label2.Text = "Tray Icon";
            // 
            // GUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(640, 470);
            this.ControlBox = false;
            this.Controls.Add(this.title);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "GUI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.FormLoad);
            this.completedTab.ResumeLayout(false);
            this.completedTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.settingsTab.ResumeLayout(false);
            this.settingsTab.PerformLayout();
            this.licenseTab.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.welcomeTab.ResumeLayout(false);
            this.welcomeTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.progressTab.ResumeLayout(false);
            this.progressTab.PerformLayout();
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
        private TabControl tabControl;
        private TextBox addressTxtBox;
        private Label addressLabel;
        private TextBox webRootTxtBox;
        private Label webRootLabel;
        private Label rootLogLabel;
        private Label title;
        private MetroToggle logSwitch;
        private MetroToggle traySwitch;
        private TabPage progressTab;
        private TabPage welcomeTab;
        private MetroButton beginButton;
        private PictureBox pictureBox1;
        private TextBox welcomeText;
        private Label configuringLabel;
        private Label installFileLabel;
        private Label busyWorkLabel;
        private Label encryptLabel;
        private MetroProgressSpinner busyWorkSpinner;
        private MetroProgressSpinner encryptionSpinner;
        private MetroProgressSpinner configSpinner;
        private MetroProgressSpinner filesSpinner;
        private MetroButton showLogButton;
        private MetroButton nextButton;
        private RichTextBox logBox;
        private TextBox textBox1;
        private PictureBox pictureBox2;
        private Label label2;
        private MetroToggle metroToggle1;
    }
}