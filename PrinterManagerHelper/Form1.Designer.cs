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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.localPage = new System.Windows.Forms.TabPage();
            this.addPrinter = new System.Windows.Forms.TabPage();
            this.aliasBox = new System.Windows.Forms.TextBox();
            this.addButton = new System.Windows.Forms.Button();
            this.aliasLabel = new System.Windows.Forms.Label();
            this.driverBox = new System.Windows.Forms.TextBox();
            this.ipLabel = new System.Windows.Forms.Label();
            this.modelBox = new System.Windows.Forms.TextBox();
            this.portLabel = new System.Windows.Forms.Label();
            this.portBox = new System.Windows.Forms.TextBox();
            this.modelLabel = new System.Windows.Forms.Label();
            this.ipBox = new System.Windows.Forms.TextBox();
            this.driverLabel = new System.Windows.Forms.Label();
            this.typeLabel = new System.Windows.Forms.Label();
            this.typeCombo = new System.Windows.Forms.ComboBox();
            this.tabControl1.SuspendLayout();
            this.localPage.SuspendLayout();
            this.addPrinter.SuspendLayout();
            this.SuspendLayout();
            // 
            // printerComboBox
            // 
            this.printerComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.printerComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.printerComboBox.FormattingEnabled = true;
            this.printerComboBox.Location = new System.Drawing.Point(67, 6);
            this.printerComboBox.Name = "printerComboBox";
            this.printerComboBox.Size = new System.Drawing.Size(836, 21);
            this.printerComboBox.TabIndex = 0;
            this.printerComboBox.SelectedIndexChanged += new System.EventHandler(this.printerComboBox_SelectedIndexChanged);
            // 
            // ipLB
            // 
            this.ipLB.AutoSize = true;
            this.ipLB.Location = new System.Drawing.Point(10, 47);
            this.ipLB.Name = "ipLB";
            this.ipLB.Size = new System.Drawing.Size(17, 13);
            this.ipLB.TabIndex = 1;
            this.ipLB.Text = "IP";
            // 
            // portLB
            // 
            this.portLB.AutoSize = true;
            this.portLB.Location = new System.Drawing.Point(10, 74);
            this.portLB.Name = "portLB";
            this.portLB.Size = new System.Drawing.Size(26, 13);
            this.portLB.TabIndex = 2;
            this.portLB.Text = "Port";
            // 
            // modelLB
            // 
            this.modelLB.AutoSize = true;
            this.modelLB.Location = new System.Drawing.Point(10, 101);
            this.modelLB.Name = "modelLB";
            this.modelLB.Size = new System.Drawing.Size(36, 13);
            this.modelLB.TabIndex = 3;
            this.modelLB.Text = "Model";
            // 
            // driverLB
            // 
            this.driverLB.AutoSize = true;
            this.driverLB.Location = new System.Drawing.Point(10, 128);
            this.driverLB.Name = "driverLB";
            this.driverLB.Size = new System.Drawing.Size(35, 13);
            this.driverLB.TabIndex = 4;
            this.driverLB.Text = "Driver";
            // 
            // ipText
            // 
            this.ipText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ipText.Location = new System.Drawing.Point(67, 44);
            this.ipText.Name = "ipText";
            this.ipText.ReadOnly = true;
            this.ipText.Size = new System.Drawing.Size(836, 20);
            this.ipText.TabIndex = 5;
            // 
            // portText
            // 
            this.portText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.portText.Location = new System.Drawing.Point(67, 71);
            this.portText.Name = "portText";
            this.portText.ReadOnly = true;
            this.portText.Size = new System.Drawing.Size(836, 20);
            this.portText.TabIndex = 6;
            // 
            // modelText
            // 
            this.modelText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.modelText.Location = new System.Drawing.Point(67, 98);
            this.modelText.Name = "modelText";
            this.modelText.ReadOnly = true;
            this.modelText.Size = new System.Drawing.Size(836, 20);
            this.modelText.TabIndex = 7;
            // 
            // driverText
            // 
            this.driverText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.driverText.Cursor = System.Windows.Forms.Cursors.Default;
            this.driverText.ForeColor = System.Drawing.SystemColors.WindowText;
            this.driverText.Location = new System.Drawing.Point(67, 125);
            this.driverText.Name = "driverText";
            this.driverText.ReadOnly = true;
            this.driverText.Size = new System.Drawing.Size(836, 20);
            this.driverText.TabIndex = 8;
            // 
            // aliasLB
            // 
            this.aliasLB.AutoSize = true;
            this.aliasLB.Location = new System.Drawing.Point(10, 9);
            this.aliasLB.Name = "aliasLB";
            this.aliasLB.Size = new System.Drawing.Size(29, 13);
            this.aliasLB.TabIndex = 9;
            this.aliasLB.Text = "Alias";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.localPage);
            this.tabControl1.Controls.Add(this.addPrinter);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(917, 259);
            this.tabControl1.TabIndex = 10;
            // 
            // localPage
            // 
            this.localPage.Controls.Add(this.aliasLB);
            this.localPage.Controls.Add(this.printerComboBox);
            this.localPage.Controls.Add(this.driverText);
            this.localPage.Controls.Add(this.ipLB);
            this.localPage.Controls.Add(this.modelText);
            this.localPage.Controls.Add(this.portLB);
            this.localPage.Controls.Add(this.portText);
            this.localPage.Controls.Add(this.modelLB);
            this.localPage.Controls.Add(this.ipText);
            this.localPage.Controls.Add(this.driverLB);
            this.localPage.Location = new System.Drawing.Point(4, 22);
            this.localPage.Name = "localPage";
            this.localPage.Padding = new System.Windows.Forms.Padding(3);
            this.localPage.Size = new System.Drawing.Size(909, 233);
            this.localPage.TabIndex = 0;
            this.localPage.Text = "Local Printers";
            this.localPage.UseVisualStyleBackColor = true;
            // 
            // addPrinter
            // 
            this.addPrinter.Controls.Add(this.aliasBox);
            this.addPrinter.Controls.Add(this.addButton);
            this.addPrinter.Controls.Add(this.aliasLabel);
            this.addPrinter.Controls.Add(this.driverBox);
            this.addPrinter.Controls.Add(this.ipLabel);
            this.addPrinter.Controls.Add(this.modelBox);
            this.addPrinter.Controls.Add(this.portLabel);
            this.addPrinter.Controls.Add(this.portBox);
            this.addPrinter.Controls.Add(this.modelLabel);
            this.addPrinter.Controls.Add(this.ipBox);
            this.addPrinter.Controls.Add(this.driverLabel);
            this.addPrinter.Controls.Add(this.typeLabel);
            this.addPrinter.Controls.Add(this.typeCombo);
            this.addPrinter.Location = new System.Drawing.Point(4, 22);
            this.addPrinter.Name = "addPrinter";
            this.addPrinter.Padding = new System.Windows.Forms.Padding(3);
            this.addPrinter.Size = new System.Drawing.Size(909, 233);
            this.addPrinter.TabIndex = 2;
            this.addPrinter.Text = "Add New Printer";
            this.addPrinter.UseVisualStyleBackColor = true;
            // 
            // aliasBox
            // 
            this.aliasBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.aliasBox.Location = new System.Drawing.Point(65, 46);
            this.aliasBox.Name = "aliasBox";
            this.aliasBox.ReadOnly = true;
            this.aliasBox.Size = new System.Drawing.Size(836, 20);
            this.aliasBox.TabIndex = 14;
            // 
            // addButton
            // 
            this.addButton.Location = new System.Drawing.Point(11, 203);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(890, 23);
            this.addButton.TabIndex = 20;
            this.addButton.Text = "Add Printer";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.addButton_Click);
            // 
            // aliasLabel
            // 
            this.aliasLabel.AutoSize = true;
            this.aliasLabel.Location = new System.Drawing.Point(8, 46);
            this.aliasLabel.Name = "aliasLabel";
            this.aliasLabel.Size = new System.Drawing.Size(29, 13);
            this.aliasLabel.TabIndex = 19;
            this.aliasLabel.Text = "Alias";
            // 
            // driverBox
            // 
            this.driverBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.driverBox.Cursor = System.Windows.Forms.Cursors.Default;
            this.driverBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.driverBox.Location = new System.Drawing.Point(65, 153);
            this.driverBox.Name = "driverBox";
            this.driverBox.ReadOnly = true;
            this.driverBox.Size = new System.Drawing.Size(836, 20);
            this.driverBox.TabIndex = 18;
            // 
            // ipLabel
            // 
            this.ipLabel.AutoSize = true;
            this.ipLabel.Location = new System.Drawing.Point(8, 75);
            this.ipLabel.Name = "ipLabel";
            this.ipLabel.Size = new System.Drawing.Size(17, 13);
            this.ipLabel.TabIndex = 11;
            this.ipLabel.Text = "IP";
            // 
            // modelBox
            // 
            this.modelBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.modelBox.Location = new System.Drawing.Point(65, 126);
            this.modelBox.Name = "modelBox";
            this.modelBox.ReadOnly = true;
            this.modelBox.Size = new System.Drawing.Size(836, 20);
            this.modelBox.TabIndex = 17;
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(8, 102);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(26, 13);
            this.portLabel.TabIndex = 12;
            this.portLabel.Text = "Port";
            // 
            // portBox
            // 
            this.portBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.portBox.Location = new System.Drawing.Point(65, 99);
            this.portBox.Name = "portBox";
            this.portBox.ReadOnly = true;
            this.portBox.Size = new System.Drawing.Size(836, 20);
            this.portBox.TabIndex = 16;
            // 
            // modelLabel
            // 
            this.modelLabel.AutoSize = true;
            this.modelLabel.Location = new System.Drawing.Point(8, 129);
            this.modelLabel.Name = "modelLabel";
            this.modelLabel.Size = new System.Drawing.Size(36, 13);
            this.modelLabel.TabIndex = 13;
            this.modelLabel.Text = "Model";
            // 
            // ipBox
            // 
            this.ipBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ipBox.Location = new System.Drawing.Point(65, 72);
            this.ipBox.Name = "ipBox";
            this.ipBox.ReadOnly = true;
            this.ipBox.Size = new System.Drawing.Size(836, 20);
            this.ipBox.TabIndex = 15;
            // 
            // driverLabel
            // 
            this.driverLabel.AutoSize = true;
            this.driverLabel.Location = new System.Drawing.Point(8, 156);
            this.driverLabel.Name = "driverLabel";
            this.driverLabel.Size = new System.Drawing.Size(35, 13);
            this.driverLabel.TabIndex = 14;
            this.driverLabel.Text = "Driver";
            // 
            // typeLabel
            // 
            this.typeLabel.AutoSize = true;
            this.typeLabel.Location = new System.Drawing.Point(8, 9);
            this.typeLabel.Name = "typeLabel";
            this.typeLabel.Size = new System.Drawing.Size(31, 13);
            this.typeLabel.TabIndex = 1;
            this.typeLabel.Text = "Type";
            // 
            // typeCombo
            // 
            this.typeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.typeCombo.FormattingEnabled = true;
            this.typeCombo.Items.AddRange(new object[] {
            "TCP/IP",
            "Network",
            "iPrint"});
            this.typeCombo.Location = new System.Drawing.Point(65, 6);
            this.typeCombo.Name = "typeCombo";
            this.typeCombo.Size = new System.Drawing.Size(836, 21);
            this.typeCombo.TabIndex = 13;
            this.typeCombo.SelectedIndexChanged += new System.EventHandler(this.typeCombo_SelectedIndexChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(941, 282);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(400, 202);
            this.Name = "Form1";
            this.Text = "FOG PrinterManager Helper";
            this.tabControl1.ResumeLayout(false);
            this.localPage.ResumeLayout(false);
            this.localPage.PerformLayout();
            this.addPrinter.ResumeLayout(false);
            this.addPrinter.PerformLayout();
            this.ResumeLayout(false);

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
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage localPage;
        private System.Windows.Forms.TabPage addPrinter;
        private System.Windows.Forms.Label typeLabel;
        private System.Windows.Forms.ComboBox typeCombo;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Label aliasLabel;
        private System.Windows.Forms.TextBox driverBox;
        private System.Windows.Forms.Label ipLabel;
        private System.Windows.Forms.TextBox modelBox;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.TextBox portBox;
        private System.Windows.Forms.Label modelLabel;
        private System.Windows.Forms.TextBox ipBox;
        private System.Windows.Forms.Label driverLabel;
        private System.Windows.Forms.TextBox aliasBox;
    }
}

