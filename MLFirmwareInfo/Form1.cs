//    Samsung Firmware Toolchain - A toolchain to edit and modify Samsung firmware files.
//    Copyright (C) 2010  Jason Couture
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see http://www.gnu.org/licenses/.


// Change Log
// 	<date> - <name/email> - Short description of changes
// 	07/18/2010 - plaguethenet@gmail.com - Added GPL to all files.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using WCS.MLFileInfo;
using System.IO;
using MLFI.Interfaces;
using System.Diagnostics;
using MLFirmwareInfo.Properties;

namespace MLFirmwareInfo
{
    public partial class Form1 : Form
    {
        private Settings _mySettings;
        internal Settings MySettings
        {
            get
            {
                if (_mySettings == null)
                    _mySettings = new Settings();
                return _mySettings;
            }
        }
        public Form1()
        {
            InitializeComponent();
            if (MySettings.MRU == null)
                MySettings.MRU = new System.Collections.Specialized.StringCollection();
        }
        List<IUIPlugin> myPlugins = new List<IUIPlugin>();
        private void Form1_Load(object sender, EventArgs e)
        {
            //Before we get to loading the plugins, Subscribe to the message event
            Dispatch.Message += new EventHandler<MessageEventArgs>(Dispatch_Message);
            Dispatch.MessagePump += new EventHandler<EventArgs>(Dispatch_MessagePump);
            DirectoryInfo di = new DirectoryInfo("plugins");
            if (!di.Exists)
            {
                di.Create();
                di.Refresh();
            }
            FileInfo[] dlls = di.GetFiles("*.dll", SearchOption.AllDirectories);
            foreach (FileInfo fi in dlls)
            {
                try
                {
                    IUIPlugin[] plugins = MLFI.Interfaces.Loader.Loader.LoadAndCreateInstance<IUIPlugin>(fi.FullName);
                    if (plugins.Length == 0)
                    {
                        Dispatch.AddMessage("Firmware Editor", 
                            "Successfully loaded assembly " +
                            fi.Name +
                            ", but the required interface (IUIPlugin) was not found.");
                        continue;
                    }
                    myPlugins.AddRange(plugins);
                }
                catch
                {
                    Dispatch.AddMessage("Firmware Editor", "A plugin failed to load due to an exception: {0}", fi.FullName);
                }
            }
#if (DEBUG)
            //Alright, Now...
            Debug.Print("Loaded {0} Plugin{1}", myPlugins.Count, myPlugins.Count == 1 ? "." : "s.");
#endif
            for (int x = 0; x < myPlugins.Count; x++)
            {
#if (DEBUG)
                Debug.WriteLine(string.Format("Plugin {0} of {1}", x + 1, myPlugins.Count));
                Debug.WriteLine("Name: " + myPlugins[x].Name);
                Debug.WriteLine("Author: " + myPlugins[x].Author);
                Debug.Write("Supported File Types: ");
                foreach (string s in myPlugins[x].SupportedTypes)
                {
                    Debug.Write(s + " ");
                }
                Debug.WriteLine("");
#endif
                //Attach the plugins menu items, And update the current type
                //This is really all we need to do.
                pluginsToolStripMenuItem.DropDownItems.AddRange(myPlugins[x].Menus);
                myPlugins[x].CurrentFileTypeCode = "NULL";
            }
            UpdateMRUMenu();
        }

        void Dispatch_MessagePump(object sender, EventArgs e)
        {
            Application.DoEvents();
        }

        void Dispatch_Message(object sender, MessageEventArgs e)
        {
            lbMessages.Items.Add(e.ToString());
        }

        private void DeleteMRU(string filename)
        {
        }
        private void UpdateMRU(string filename)
        {

            for (int x = 0; x < MySettings.MRU.Count; x++)
            {
                if (MySettings.MRU[x].ToLower().Trim() == filename.ToLower().Trim())
                {
                    if (x == 0)
                        return; //Already most recent, No need to update.
                    else
                    {
                        string tmp = MySettings.MRU[x];
                        MySettings.MRU.RemoveAt(x);
                        MySettings.MRU.Insert(0, tmp);
                        //string i0 = MySettings.MRU[0];
                        //MySettings.MRU[0] = MySettings.MRU[x];
                        //MySettings.MRU.Insert(1, i0);
                        UpdateMRUMenu();
                        return;
                    }
                }
            }
            MySettings.MRU.Insert(0, filename);
            UpdateMRUMenu();
            //If we get here, it wasn't in the list.
        }
        private void UpdateMRUMenu()
        {
            if (MySettings.MRU.Count == 0)
                return;
            while (MySettings.MRU.Count > 10)
            {
                MySettings.MRU.RemoveAt(10);
            }
            MySettings.Save();
            recentToolStripMenuItem.DropDownItems.Clear();
            for (int x = 0; x < MySettings.MRU.Count; x++)
            {
                ToolStripMenuItem RecentFile = new ToolStripMenuItem(
                    Path.GetFileName(
                    MySettings.MRU[x]));
                RecentFile.Click += new EventHandler(RecentFile_Click);
                RecentFile.Tag = MySettings.MRU[x];
                RecentFile.ToolTipText = MySettings.MRU[x];
                recentToolStripMenuItem.DropDownItems.Add(RecentFile);
            }
        }

        void RecentFile_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem RecentFile = (ToolStripMenuItem)sender;
            string fullPath = (string)RecentFile.Tag;
            fdOpen.FileName = fullPath;
            try
            {
                MLHeader ml = new MLHeader(fdOpen.FileName);
                this.CurrentHeader = ml;
            }
            catch (InvalidDataException)
            {
                MessageBox.Show("Failed to open the file you specified, The data was invalid!", "Open Failed!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to open the file you specified, An unknown error occured. Is the file open in another program?", "Open Failed!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            CurrentFile = fdOpen.FileName;
        }
        public MLEndBlock Header
        {
            get
            {
                if (CurrentHeader != null)
                    return CurrentHeader.meb;
                else
                    return new MLEndBlock();
            }
        }
        void UpdatePluginState()
        {
            foreach (IUIPlugin p in myPlugins)
            {
                p.CurrentFileTypeCode = Header.ImageType;
                p.SelectedFile = CurrentFile;
            }
            //This code does not work as intended.
            /*
            bool hasVisible = false;
            foreach (ToolStripMenuItem i in pluginsToolStripMenuItem.DropDownItems)
            {
                if (i.Visible == true && i.Enabled == true)
                {
                    hasVisible = true;
                    break;
                }

            }
            if (!hasVisible)
            {
                pluginsToolStripMenuItem.Visible = false;
                pluginsToolStripMenuItem.Enabled = false;
            }
            else
            {
                pluginsToolStripMenuItem.Visible = true;
                pluginsToolStripMenuItem.Enabled = true;
            }*/
        }
        void UpdateErrorState()
        {
            bool hasErrors = false;
            frmErrProvider.Clear();
            Int32 val = 0;
            if (!Int32.TryParse(tbAddr.Text, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out val))
            {
                hasErrors = true;
                frmErrProvider.SetError(tbAddr, "Invalid address, Please specify address as a 32 bit hex number.");
            }
            else
            {
                Header.flashAddress = val;
            }
            if (tbModel.Text.Length != 4)
            {
                hasErrors = true;
                frmErrProvider.SetError(tbModel, "Model must be exactly 4 bytes.");
            }
            else
            {
                Header.PhoneModel = tbModel.Text;
            }
            if (tbType.Text.Length != 3)
            {
                hasErrors = true;
                frmErrProvider.SetError(tbType, "Flash type must be exactly 3 characters.");
            }
            else
            {
                Header.ImageType = tbType.Text;
            }
            //We will do things with hasErrors later.
            if (hasErrors)
            {
                saveToolStripMenuItem.Enabled = false;
                saveAsToolStripMenuItem.Enabled = false;
            }
            else
            {
                saveToolStripMenuItem.Enabled = true;
                saveAsToolStripMenuItem.Enabled = true;
            }
            UpdatePluginState();
        }
        private void tbAddr_Leave(object sender, EventArgs e)
        {
            UpdateErrorState();
        }

        private void tbModel_Leave(object sender, EventArgs e)
        {
            UpdateErrorState();
        }

        private void tbType_Leave(object sender, EventArgs e)
        {
            UpdateErrorState();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAbout abtbx = new frmAbout();
            abtbx.ShowDialog();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (
                MessageBox.Show(this, "Are you sure you want to quit?", "Really Exit?", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                == DialogResult.Yes)
                Application.Exit();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fdOpen.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    MLHeader ml = new MLHeader(fdOpen.FileName);
                    this.CurrentHeader = ml;
                }
                catch (InvalidDataException)
                {
                    MessageBox.Show("Failed to open the file you specified, The data was invalid!", "Open Failed!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception)
                {
                    MessageBox.Show("Failed to open the file you specified, An unknown error occured. Is the file open in another program?", "Open Failed!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                CurrentFile = fdOpen.FileName;

            }

        }
        private string _currentFile = null;
        protected string CurrentFile
        {
            get
            {
                return _currentFile;
            }
            set
            {
                _currentFile = value;
                lblFile.Text = Path.GetFileName(value);
                UpdatePluginState();
                UpdateMRU(value);
            }
        }
        MLHeader _CurrentHeader = null;
        public MLHeader CurrentHeader
        {
            get
            {
                return _CurrentHeader;
            }
            set
            {
                //Update our boxes with our info.
                _CurrentHeader = value;
                tbModel.Text = Header.PhoneModel;
                tbType.Text = Header.ImageType;
                tbAddr.Text = BitConverter.ToString(BitConverter.GetBytes(Header.flashAddress)).Replace("-", "").ToUpper();

            }
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.CurrentFile))
            {
                saveAsToolStripMenuItem_Click(sender, e);
                return;
            }
            //Ok we have a filename, Lets open it yeah?

            try
            {
                FileInfo fi = new FileInfo(CurrentFile);
                FileStream fs = fi.Open(FileMode.Open, FileAccess.Write, FileShare.Read);
                fs.Seek(-1024, SeekOrigin.End);
                fs.Write(Header.rawData, 0, Header.rawData.Length);
                fs.Flush();
                fs.Close();
                MessageBox.Show("Saved!", "Save Successful!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("The file you specified does not exist, I wont write the header to an empty file!", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                MessageBox.Show("An unknown error has occured. The file may have been damaged!", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }


        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fdSave.ShowDialog() == DialogResult.OK)
            {
                CurrentFile = fdSave.FileName;
            }
            else
                return;
            saveToolStripMenuItem_Click(sender, e);
        }

        private void saveLogFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fdSaveLog.ShowDialog(this) != DialogResult.OK)
                return;
            FileInfo fi = new FileInfo(fdSaveLog.FileName);
            using (FileStream fs = fi.Open(FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    for (int x = 0; x < lbMessages.Items.Count; x++)
                    {
                        sw.WriteLine((string)lbMessages.Items[x]);
                    }
                    sw.Flush();
                    sw.Close();
                }
            }
            Dispatch.AddMessage("Log", "Log file saved to: {0}", fdSaveLog.FileName);
        }

        private void clearLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lbMessages.Items.Clear();
            Dispatch.AddMessage("Log", "Cleared.");
        }
    }
}
