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
using WCS.FatSharp.Base;

namespace WCS.FatSharp.Structures
{
    public class Sector : FATObjectBase
    {
        //Since sectors should almost always be 512 bytes.
        //We do a no no here and assume 512 bytes.
        //this should work fine for our purposes.
        int sNumber;
        [Data(0, 512)]
        internal byte[] sectorData;
        public Sector(BootSector bs, int SectorNumber)
        {
            sectorData = new byte[0]; //This is to shut the compiler up.
            BPB = bs;
            sNumber = SectorNumber;
            ReRead();
        }
        public void Save()
        {
            BPB.RawWriteSector(this.Number, this);
        }
        public void ReRead()
        {
            DataLayout.Fill<Sector>(BPB.RawReadSector(Number), this);
        }
        public int Number
        {
            get { return sNumber; }
            set { sNumber = value; }
        }
        public DirEntBase[] DirectoryEntries
        {
            get
            {
                List<DirEntBase> entries = new List<DirEntBase>();
                for (int x = 0; x < (this.sectorData.Length / 32); x++)
                {
                    DirEntBase d = new DirEntBase();
                    DataLayout.Fill<DirEntBase>(sectorData, d, x * 32);
                    entries.Add(d);
                }
                return entries.ToArray();
            }
            set
            {
                if (value.Length > (sectorData.Length / 32))
                    throw new InvalidOperationException("Too many directory entires to write to a sector!");
                //Okay, first we have to clear the sectors data.
                sectorData = new byte[512];
                byte[] data = new byte[32];
                for(int x = 0; x < value.Length; x++)
                {
                    data = DataLayout.Dump<DirEntBase>(value[x]);
                    Array.Copy(data, 0, sectorData, x * 32, 32);
                }
                this.Save();
            }
        }
    }
}
