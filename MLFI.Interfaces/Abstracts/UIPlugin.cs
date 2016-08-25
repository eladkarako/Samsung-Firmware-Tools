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
using System.Text;
using System.Windows.Forms;

namespace MLFI.Interfaces.Abstracts
{
    public abstract class UIPlugin : MLFI.Interfaces.IUIPlugin
    {
        private string _name;
        private string _author;
        private List<string> _supported = new List<string>();
        private List<ToolStripMenuItem> _menus = new List<ToolStripMenuItem>();
        private string _cftc = String.Empty;
        private string _currentFile = string.Empty;
        public void DoEvents()
        {
            Dispatch.Pump();
        }
        public string SelectedFile
        {
            get { return _currentFile; }
            set { _currentFile = value; }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public string Author
        {
            get { return _author; }
            set { _author = value; }
        }
        public List<string> SupportedTypes
        {
            get { return this._supported; }
        }
        public List<ToolStripMenuItem> Menus
        {
            get { return _menus; }
        }
        protected virtual void OnTypeCodeChange(string cftc, string value)
        {
            //The default is to loop through the root menu
            //items and set them to enabled/disabed
            //Based on what is set in their tag.
            foreach (ToolStripMenuItem t in Menus)
            {
                string tmp = string.Empty;
                try
                {
                    tmp = (string)t.Tag;
                }
                catch
                { }
                //Ignore things we do not understand.
                if (string.IsNullOrEmpty(tmp))
                    continue;
                bool state = false;
                foreach (string s in tmp.Split(';'))
                {
                    if (s.Trim().ToLower() == value.Trim().ToLower())
                    {
                        state = true;
                        break;
                    }
                }
                t.Enabled = state;
                t.Visible = state;
            }
        }

        #region IUIPlugin Glue Code
        string IUIPlugin.Name
        {
            get { return Name; }
        }

        string IUIPlugin.Author
        {
            get { return Author; }
        }

        string[] IUIPlugin.SupportedTypes
        {
            get { return SupportedTypes.ToArray(); }
        }
        ToolStripMenuItem[] IUIPlugin.Menus
        {
            get { return Menus.ToArray(); }
        }
        string IUIPlugin.CurrentFileTypeCode
        {
            get { return _cftc; }
            set
            {
                OnTypeCodeChange(_cftc, value);
                _cftc = value;
            }
        }
        string IUIPlugin.SelectedFile
        {
            get { return _currentFile; }
            set { _currentFile = value; }
        }
        #endregion
        #region Helper Methods
        protected void Print(string message)
        {
            Dispatch.AddMessage(this.Name, message);
        }
        protected void Print(string format, params object[] args)
        {
            Dispatch.AddMessage(this.Name, format, args);
        }
        #endregion
    }
}
