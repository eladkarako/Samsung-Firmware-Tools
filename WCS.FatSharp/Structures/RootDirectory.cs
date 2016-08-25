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
    public class RootDirectory
    {
        BootSector BPB;
        Sector[] RootSectors;
        public DirEntBase[] LoadDirEnts()
        {
            List<DirEntBase> entries = new List<DirEntBase>();
            foreach (Sector s in RootSectors)
            {
                entries.AddRange(s.DirectoryEntries);
            }
            return entries.ToArray();
        }
        private int EntriesPerSector
        {
            get
            {
                return 512 / 32; //AKA, 16. 512 should be replaced with BytesPerSector
            }
        }
        private int TotalEntries
        {
            get
            {
                return BPB.MaxRootEntries;
            }
        }
        private int RootDirectorySectors
        {
            get
            {
                return (BPB.MaxRootEntries * 32) / 512;
            }
        }
        public RootDirectory(BootSector boots)
        {
            BPB = boots;
            RootSectors = BPB.ReadSectors(
                BPB.RootDirectorySector, 
                RootDirectorySectors
                );
        }

        internal void Save()
        {
            foreach (Sector s in RootSectors)
            {
                s.Save();
            }
        }
    }
}
