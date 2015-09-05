namespace PrinterManagerHelper
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.printerComboBox = new System.Windows.Forms.ComboBox();
            this.ipLB = new System.Windows.Forms.Label();
            this.portLB = new System.Windows.Forms.Label();
            this.modelLB = new System.Windows.Forms.Label();
            this.driverLB = new System.Windows.Forms.Label();
            this.ipText = new System.Windows.Forms.TextBox();
            this.portText = new System.Windows.Forms.TextBox();
            this.modelText = new System.Windows.Forms.TextBox();
            this.driverText = new System.Windows.Forms.TextBox();
            this.aliasLB = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // printerComboBox
            // 
            this.printerComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.printerComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.printerComboBox.FormattingEnabled = true;
            this.printerComboBox.Location = new System.Drawing.Point(69, 12);
            this.printerComboBox.Name = "printerComboBox";
            this.printerComboBox.Size = new System.Drawing.Size(503, 21);
            this.printerComboBox.TabIndex = 0;
            this.printerComboBox.SelectedIndexChanged += new System.EventHandler(this.printerComboBox_SelectedIndexChanged);
            // 
            // ipLB
            // 
            this.ipLB.AutoSize = true;
            this.ipLB.Location = new System.Drawing.Point(12, 53);
            this.ipLB.Name = "ipLB";
            this.ipLB.Size = new System.Drawing.Size(17, 13);
            this.ipLB.TabIndex = 1;
            this.ipLB.Text = "IP";
            // 
            // portLB
            // 
            this.portLB.AutoSize = true;
            this.portLB.Location = new System.Drawing.Point(12, 80);
            this.portLB.Name = "portLB";
            this.portLB.Size = new System.Drawing.Size(26, 13);
            this.portLB.TabIndex = 2;
            this.portLB.Text = "Port";
            // 
            // modelLB
            // 
            this.modelLB.AutoSize = true;
            this.modelLB.Location = new System.Drawing.Point(12, 107);
            this.modelLB.Name = "modelLB";
            this.modelLB.Size = new System.Drawing.Size(36, 13);
            this.modelLB.TabIndex = 3;
            this.modelLB.Text = "Model";
            // 
            // driverLB
            // 
            this.driverLB.AutoSize = true;
            this.driverLB.Location = new System.Drawing.Point(12, 134);
            this.driverLB.Name = "driverLB";
            this.driverLB.Size = new System.Drawing.Size(35, 13);
            this.driverLB.TabIndex = 4;
            this.driverLB.Text = "Driver";
            // 
            // ipText
            // 
            this.ipText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ipText.Location = new System.Drawing.Point(69, 50);
            this.ipText.Name = "ipText";
            this.ipText.ReadOnly = true;
            this.ipText.Size = new System.Drawing.Size(503, 20);
            this.ipText.TabIndex = 5;
            // 
            // portText
            // 
            this.portText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.portText.Location = new System.Drawing.Point(69, 77);
            this.portText.Name = "portText";
            this.portText.ReadOnly = true;
            this.portText.Size = new System.Drawing.Size(503, 20);
            this.portText.TabIndex = 6;
            // 
            // modelText
            // 
            this.modelText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.modelText.Location = new System.Drawing.Point(69, 104);
            this.modelText.Name = "modelText";
            this.modelText.ReadOnly = true;
            this.modelText.Size = new System.Drawing.Size(503, 20);
            this.modelText.TabIndex = 7;
            // 
            // driverText
            // 
            this.driverText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.driverText.Location = new System.Drawing.Point(69, 131);
            this.driverText.Name = "driverText";
            this.driverText.ReadOnly = true;
            this.driverText.Size = new System.Drawing.Size(503, 20);
            this.driverText.TabIndex = 8;
            // 
            // aliasLB
            // 
            this.aliasLB.AutoSize = true;
            this.aliasLB.Location = new System.Drawing.Point(12, 15);
            this.aliasLB.Name = "aliasLB";
            this.aliasLB.Size = new System.Drawing.Size(29, 13);
            this.aliasLB.TabIndex = 9;
            this.aliasLB.Text = "Alias";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 163);
            this.Controls.Add(this.aliasLB);
            this.Controls.Add(this.driverText);
            this.Controls.Add(this.modelText);
            this.Controls.Add(this.portText);
            this.Controls.Add(this.ipText);
            this.Controls.Add(this.driverLB);
            this.Controls.Add(this.modelLB);
            this.Controls.Add(this.portLB);
            this.Controls.Add(this.ipLB);
            this.Controls.Add(this.printerComboBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(800, 202);
            this.MinimumSize = new System.Drawing.Size(400, 202);
            this.Name = "Form1";
            this.Text = "FOG PrinterManager Helper";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox printerComboBox;
        private System.Windows.Forms.Label ipLB;
        private System.Windows.Forms.Label portLB;
        private System.Windows.Forms.Label modelLB;
        private System.Windows.Forms.Label driverLB;
        private System.Windows.Forms.TextBox ipText;
        private System.Windows.Forms.TextBox portText;
        private System.Windows.Forms.TextBox modelText;
        private System.Windows.Forms.TextBox driverText;
        private System.Windows.Forms.Label aliasLB;
    }
}

