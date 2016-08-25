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
using WCS.FatSharp.Helpers;

namespace WCS.FatSharp.Structures
{
    public class LFNEnt : DirEntBase
    {
        [Data(0x00, 1)]
        internal byte SEQ;
        [Data(0x01, 10, 2)]
        internal string name1;
        [Data(0x0b, 1)]
        internal byte attributes;
        [Data(0x0c, 1)]
        internal byte reserved = 0x00;
        [Data(0x0d, 1)]
        internal byte checksum;
        [Data(0x0e, 12, 2)]
        internal string name2;
        [Data(0x1a, 2)]
        internal ushort start_cluster; //Always 0x0000.
        [Data(0x1c, 4, 2)]
        internal string name3;
    }
    public class DirEnt : DirEntBase
    {
        public byte NameChecksum
        {
            get
            {
                return BootSector.FatCheckSum(RawFilename, Encoding.ASCII);
            }
        }
        public string RawFilename
        {
            get
            {
                return dosFileName + dosExt;
            }
        }
        public string Filename
        {
            get
            {
                return dosFileName.Trim();
            }
        }
        public string Extension
        {
            get
            {
                return dosExt.Trim();
            }
        }
        public string EightDOTThreeName
        {
            get
            {
                string ret = Filename;
                if (!string.IsNullOrEmpty(Extension))
                    ret += "." + Extension;
                return ret;
            }
        }
        public FATFileAttribute Attributes
        {
            get
            {
                return (FATFileAttribute)fAttributes;
            }
            set
            {
                fAttributes = (byte)value;
            }
        }
        public ushort StartCluster
        {
            get { return firstCluster; }
            set { firstCluster = value; }
        }
        public uint Length
        {
            get
            {
                return FileSize;
            }
            set
            {
                FileSize = value;
            }
        }
        //Any strings are padded to their length with 0x20
        [Data(0x00, 8)]
        string dosFileName;
        //0x00, 3
        [Data(0x08, 3)]
        string dosExt;
        //0x0B, 1
        [Data(0x0b, 1)]
        byte fAttributes; // See FATFileAttributes
        //0x0c, 1
        //Windows NT and later use this to encode case
        //information.
        [Data(0x0c, 1)]
        byte reserved;
        //0x0d, 1
        [Data(0x0d, 1)]
        byte CreateFineResTime; //(10ms units, 0-199)
        [Data(0x0e, 2)]
        ushort CreateTime;
        [Data(0x10, 2)]
        ushort CreateDate;
        [Data(0x12, 2)]
        ushort LastAccessDate;
        [Data(0x14, 2)]
        ushort EAIndex;
        [Data(0x16, 2)]
        ushort LastModTime;
        [Data(0x18, 2)]
        ushort LastModDate;
        [Data(0x1A, 2)]
        ushort firstCluster;
        [Data(0x1C, 4)]
        uint FileSize;
    }
}
