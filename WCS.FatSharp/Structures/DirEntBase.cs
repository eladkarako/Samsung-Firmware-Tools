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
using System;
using System.Collections.Generic;
using System.Text;
using WCS.FatSharp.Helpers;

namespace WCS.FatSharp.Structures
{
    public class DirEntBase
    {
        [Data(0,32)]
        internal byte[] raw;
        public bool IsLFNEnt
        {
            get
            {
                if (raw[0x0B] == 0x0F) return true;
                return false;
            }
        }
        public DirEnt DirEnt
        {
            get
            {
                DirEnt ret = new DirEnt();
                DataLayout.Fill<DirEnt>(raw, ret);
                return ret;
            }
        }
        public LFNEnt LFNEnt
        {
            get
            {
                LFNEnt ret = new LFNEnt();
                DataLayout.Fill<LFNEnt>(raw, ret);
                return ret;
            }
        }
        public bool IsDIREnt
        {
            get
            {
                if (raw[0x0B] != 0x0F && (raw[0] != 0x00 && !IsDeletedFile))
                    return true;
                return false;
            }
        }
        public bool IsDeletedFile
        {
            get
            {
                if (raw[0] == 0xE5)
                    return true;
                return false;
            }
        }
        public bool IsFree
        {
            get
            {
                if (IsDIREnt || IsLFNEnt)
                    return false;
                return true;
            }
        }
    }
}
