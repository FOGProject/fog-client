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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

namespace FOG
{

    public class LogTabPage : TabPage
    {
        internal RichTextBox RTextBox;
        internal LogWatcher Watcher;
        internal Boolean Freeze;
        
        public LogTabPage(String fileName)
        {
            this.Freeze = false;
            this.RTextBox = new RichTextBox();
            RTextBox.Font = new Font(FontFamily.GenericMonospace, RTextBox.Font.Size);
            RTextBox.DetectUrls = true;
            
            this.Text = Path.GetFileName(String.Format("{0}", fileName));
            
            RTextBox.Dock = DockStyle.Fill;
            RTextBox.BackColor = Color.LightGray;
            RTextBox.ReadOnly = true;
            RTextBox.FindForm();
            RTextBox.LinkClicked += mRichTextBox_LinkClicked;
            addContextMenu();
            
            this.Controls.Add(RTextBox);
                                        
            CreateWatcher(fileName);
        }
        
        
        void CreateWatcher(String fileName) {
            RTextBox.Text = File.ReadAllText(fileName);

            
            Watcher = new LogWatcher(fileName);
            Watcher.Path = Path.GetDirectoryName(fileName);
            Watcher.NotifyFilter = (NotifyFilters.LastWrite | NotifyFilters.Size);
            Watcher.Filter = Path.GetFileName(fileName);
            Watcher.TextChanged += new LogWatcher.LogWatcherEventHandler(watcherChanged);
            
            Watcher.EnableRaisingEvents = true;
        }
        
        void addContextMenu()
        {
            if (RTextBox.ContextMenuStrip == null)
            {
                var cms = new ContextMenuStrip { ShowImageMargin = false };
                var tsmiCopy = new ToolStripMenuItem("Copy");
                tsmiCopy.Click += (sender, e) => RTextBox.Copy();
                cms.Items.Add(tsmiCopy);
                RTextBox.ContextMenuStrip = cms;
            }
        }
        
        void mRichTextBox_LinkClicked (object sender, LinkClickedEventArgs e) {
            System.Diagnostics.Process.Start(e.LinkText);
        }
        
        void watcherChanged(object sender, LogWatcherEventArgs e) 
        {
            if (RTextBox.InvokeRequired)
                this.Invoke(new Action(delegate() {
                    appendText(e.Content);
                }));
            else
                appendText(e.Content);
        }
        
        void appendText(String text)
        {
            RTextBox.Text += text;
            
            if (!Freeze) {
                RTextBox.SelectionStart = RTextBox.Text.Length;
                RTextBox.SelectionLength = 0;
                RTextBox.ScrollToCaret();
            }
        }
    }
}
