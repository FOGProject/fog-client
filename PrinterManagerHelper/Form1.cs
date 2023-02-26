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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Windows.Forms;
using FOG.Modules.PrinterManager;

namespace PrinterManagerHelper
{
    public partial class Form1 : Form
    {
        private Dictionary<string, ManagementBaseObject> _printers;
        private PrintManagerBridge _instance;
        
        public Form1()
        {
            InitializeComponent();
            _instance = new WindowsPrinterManager();

            _printers = GetPrinters();

            foreach (var printerName in GetPrinterNames())
                printerComboBox.Items.Add(printerName);

            if (printerComboBox.Items.Count > 0)
                printerComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Get a list of all printers attached to a host
        /// </summary>
        /// <returns>A dictionary where the name of the printer is the key, and the printer object is the value</returns>
        private Dictionary<string, ManagementBaseObject> GetPrinters()
        {
            var printers = new Dictionary<string, ManagementBaseObject>();

            using (var query = new ManagementObjectSearcher("SELECT * from Win32_Printer Where Network=false And Local=True"))
            {
                foreach (var printer in query.Get())
                {
                    var name = printer.GetPropertyValue("Name").ToString();
                    printers.Add(name, printer);
                }
            }

            return printers;
        }

        /// <summary>
        /// Extract just the printers' names from the existing list
        /// </summary>
        /// <returns>A list of names</returns>
        private List<string> GetPrinterNames()
        {
            return (from ManagementBaseObject printer
                    in _printers.Values
                    select printer.GetPropertyValue("Name"))
                    .Cast<string>().ToList();
        }

        /// <summary>
        /// Get the IP of a printer
        /// </summary>
        /// <param name="printer"></param>
        /// <returns></returns>
        private string GetPrinterIP(ManagementBaseObject printer)
        {
            // Select a Win32_TCPIPPrinterPort with a matching name as the printer's PortName
            // This will allow us to get the actual address being used by the port
            using (var query = new ManagementObjectSearcher(
                $"SELECT * from Win32_TCPIPPrinterPort WHERE Name LIKE \"{printer.GetPropertyValue("PortName")}\""))
            {
                foreach (var port in query.Get())
                    return port.GetPropertyValue("HostAddress").ToString();
            }

            return null;
        }

        /// <summary>
        /// Get the path to the inf used by a printer
        /// </summary>
        /// <param name="printer"></param>
        /// <returns></returns>
        private string GetPrinterDriver(ManagementBaseObject printer)
        {
            // Get the name of the driver used by the printer and match it to a Win32_PrinterDriver
            var name = printer.GetPropertyValue("DriverName");

            using (var query = new ManagementObjectSearcher("SELECT * from Win32_PrinterDriver"))
            {
                // Select only the drivers which match the naming scheme (driver is in CSV format name,version,32/64bit)
                foreach (var driver in from ManagementBaseObject driver 
                                       in query.Get()
                                       let info = driver.GetPropertyValue("Name").ToString().Split(',')
                                       where info[0].Equals(name)
                                       select driver)
                {
                    // TODO: Rewrite using ternary or null coalescing
                    try
                    {
                        return Path.Combine(driver.GetPropertyValue("FilePath").ToString(),
                            driver.GetPropertyValue("InfName").ToString());
                    }
                    catch (Exception)
                    {
                        return @"C:\Windows\inf\ntprint.inf";
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Update the fields when a new printer is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void printerComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            var printer = _printers[printerComboBox.SelectedItem.ToString()];

            portText.Text = printer.GetPropertyValue("PortName")?.ToString() ?? "NA";
            ipText.Text = GetPrinterIP(printer) ?? "NA";
            modelText.Text = printer.GetPropertyValue("DriverName")?.ToString() ?? "NA";
            driverText.Text = GetPrinterDriver(printer) ?? "NA";
        }

        private void typeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearAll();

            if (typeCombo.SelectedItem.Equals("TCP/IP"))
            {
                aliasBox.ReadOnly = false;
                ipBox.ReadOnly = false;
                portBox.ReadOnly = false;
                modelBox.ReadOnly = false;
                driverBox.ReadOnly = false;
            }
            else if (typeCombo.SelectedItem.Equals("Network"))
            {
                aliasBox.ReadOnly = false;
                ipBox.ReadOnly = true;
                portBox.ReadOnly = true;
                modelBox.ReadOnly = true;
                driverBox.ReadOnly = true;
            }
            else if (typeCombo.SelectedItem.Equals("iPrint"))
            {
                aliasBox.ReadOnly = false;
                ipBox.ReadOnly = true;
                portBox.ReadOnly = false;
                modelBox.ReadOnly = true;
                driverBox.ReadOnly = true;
            }
        }

        private void ClearAll()
        {
            aliasBox.Clear();
            ipBox.Clear();
            portBox.Clear();
            modelBox.Clear();
            driverBox.Clear();
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            aliasBox.Enabled = false;
            ipBox.Enabled = false;
            portBox.Enabled = false;
            modelBox.Enabled = false;
            driverBox.Enabled = false;
            typeCombo.Enabled = false;


            // Create the printer
            var printer = new Printer
            {
                Name = aliasBox.Text,
                File = driverBox.Text,
                IP = ipBox.Text,
                Port = portBox.Text,
                Model = modelBox.Text
            };

            if (typeCombo.SelectedItem.Equals("TCP/IP"))
            {
                printer.Type = Printer.PrinterType.Local;
            }
            else if (typeCombo.SelectedItem.Equals("Network"))
            {
                printer.Type = Printer.PrinterType.Network;

            }
            else if (typeCombo.SelectedItem.Equals("iPrint"))
            {
                printer.Type = Printer.PrinterType.iPrint;
            }

            printer.Add(_instance, true);

            aliasBox.Enabled = true;
            ipBox.Enabled = true;
            portBox.Enabled = true;
            modelBox.Enabled = true;
            driverBox.Enabled = true;
            typeCombo.Enabled = true;
        }
    }
}
