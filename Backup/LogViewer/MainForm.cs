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
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FOG
{
    /// <summary>
    /// Description of MainForm.
    /// </summary>
    public partial class MainForm : Form
    {
        
        public MainForm()
        {
            InitializeComponent();
        }
        
        void OpenLogToolStripMenuItemClick(object sender, EventArgs e)
        {
            var result = logFileDialog.ShowDialog();
            if(result == DialogResult.OK)
            {
                createNewLogWatcher(logFileDialog.FileName);
            }
        }
        
        void createNewLogWatcher(String filepath) {
            
            Boolean exists = false;
            
            foreach(LogTabPage page in tabControl1.Controls) {
                if(page.Watcher.FileName.Equals(filepath)) {
                    tabControl1.SelectedTab = page;
                    exists = true;
                    break;
                }
            }
            
            if (!exists) {
                
                try {
                    var page = new LogTabPage(filepath);
            
                    tabControl1.Controls.Add(page);
                } catch (Exception ex) {
                    MessageBox.Show("Error: " + ex.Message, "Error: " + filepath, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }
        void CloseToolStripMenuItemClick(object sender, EventArgs e)
        {
            if(tabControl1.TabCount > 0)
                tabControl1.SelectedTab.Dispose();
        }
        void ClearToolStripMenuItemClick(object sender, EventArgs e)
        {
          if(tabControl1.TabCount > 0)
          {
                ((LogTabPage)tabControl1.SelectedTab).RTextBox.Text = "";
          }
        }
        void FreezeToolStripMenuItemClick(object sender, EventArgs e)
        {
            if(tabControl1.TabCount > 0)
            {
                if (freezeToolStripMenuItem.Text.Equals("Freeze")) {
                    ((LogTabPage)tabControl1.SelectedTab).Freeze = true;
                    freezeToolStripMenuItem.Text = "Unfreeze";
                } else {
                    ((LogTabPage)tabControl1.SelectedTab).Freeze = false;
                     freezeToolStripMenuItem.Text = "Freeze";
                }
            }         
        }
        
        void TabSelectedChanged(object sender, EventArgs  e)
        {
            if(tabControl1.TabCount > 0)
            {
                freezeToolStripMenuItem.Text = (((LogTabPage)tabControl1.SelectedTab).Freeze) ? "Unfreeze" : "Freeze";
            }                  
        }
        void OpenDefaultsToolStripMenuItemClick(object sender, EventArgs e)
        {
            createNewLogWatcher(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)) + "fog.log");
            createNewLogWatcher(Environment.GetEnvironmentVariable("USERPROFILE") + "\\fog_user.log");
        }
    }
}
