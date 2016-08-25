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
// 	07/20/2010 - plaguethenet@gmail.com - Added Library Project.
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using WCS.FatSharp.Structures;
using System.Security.Permissions;

namespace WCS.FatSharp
{
    //Fat values, For us, Are 16 bits.
    //Taken from wikipedia:
    //0x0000 = Free
    //0x0001 = Reserved, Do not use.
    //0x0002 to 0xFFEF = Used cluster (Points to next)
    //0xFFF0 to 0xFFF6 = Reserved, Do not use.
    //0xFFF7 = Bad Cluster
    //0xFFF8 to 0xFFFF = Last Cluster in Chain (EOC)
    public class FatSharp
    {
        FAT FileAcollationTable;
        BootSector BPB;
        [FileIOPermission(SecurityAction.Demand, Unrestricted = true)]
        public FatSharp(string FileName)
            : this(new FileInfo(FileName))
        { }
        [FileIOPermission(SecurityAction.Demand, Unrestricted = true)]
        public FatSharp(FileInfo FatFile)
            : this(FatFile.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        { }
        [FileIOPermission(SecurityAction.Demand, Unrestricted = true)]
        public FatSharp(FileStream fatfs)
        {
            lock (fatfs)
            {
                fatfs.Seek(0, SeekOrigin.Begin);
                byte[] block = new byte[512];
                fatfs.Read(block, 0, 512);
                BPB = BootSector.Create(block, fatfs);
                //FatStream = fatfs;
            }
        }
        public byte[] ReadSector(int sector)
        {
            return BPB.RawReadSector(sector);
        }
        public byte[] ReadCluster(int cluster)
        {
            return BPB.RawReadCluster(cluster);
        }
    }
}
