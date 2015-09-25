using System.Drawing;
using MetroFramework.Components;
using MetroFramework.Controls;

namespace UserNotification
{
    partial class NotificationGUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotificationGUI));
            this.titleLabel = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.logButton = new System.Windows.Forms.Button();
            this.bodyLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Open Sans Light", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.Location = new System.Drawing.Point(16, 11);
            this.titleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.titleLabel.MaximumSize = new System.Drawing.Size(453, 62);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(46, 26);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "Title";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
            this.panel1.Location = new System.Drawing.Point(16, 47);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(459, 6);
            this.panel1.TabIndex = 1;
            // 
            // logButton
            // 
            this.logButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("logButton.BackgroundImage")));
            this.logButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.logButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.logButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.logButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGray;
            this.logButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.logButton.Location = new System.Drawing.Point(439, 6);
            this.logButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.logButton.Name = "logButton";
            this.logButton.Size = new System.Drawing.Size(36, 33);
            this.logButton.TabIndex = 2;
            this.logButton.UseVisualStyleBackColor = true;
            this.logButton.Click += new System.EventHandler(this.logButton_Click);
            // 
            // bodyLabel
            // 
            this.bodyLabel.AutoSize = true;
            this.bodyLabel.Font = new System.Drawing.Font("Open Sans Light", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bodyLabel.Location = new System.Drawing.Point(19, 57);
            this.bodyLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.bodyLabel.Name = "bodyLabel";
            this.bodyLabel.Size = new System.Drawing.Size(36, 17);
            this.bodyLabel.TabIndex = 3;
            this.bodyLabel.Text = "Body";
            // 
            // NotificationGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(488, 94);
            this.ControlBox = false;
            this.Controls.Add(this.bodyLabel);
            this.Controls.Add(this.logButton);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.titleLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NotificationGUI";
            this.Opacity = 0D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button logButton;
        private System.Windows.Forms.Label bodyLabel;
    }
}

