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
using System.IO;
using WCS.FatSharp.Structures;

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
    public struct FATEntry
    {
        private int cNum;
        ushort FATEnt;
        public static FATEntry Create(ushort data, int cnumb)
        {
            FATEntry ret = new FATEntry();
            ret.FATEnt = data;
            ret.cNum = cnumb;
            return ret;
        }
        public int ClusterNumber
        {
            get { return cNum; }
        }
        public bool IsClusterPointer
        {
            get
            {
                if (FATEnt < 0xFFEF && FATEnt > 2)
                    return true;
                return false;
            }
        }
        public bool IsBadCluster
        {
            get
            {
                return FATEnt == 0xFFF7;
            }
        }
        public bool IsFree
        {
            get
            {
                return FATEnt == 0;
            }
        }
        public bool IsEOC
        {
            get
            {
                return FATEnt >= 0xFFF8;
            }
        }
        public ushort Value
        {
            get { return FATEnt; }
            set { FATEnt = value; }
        }
        public byte[] Bytes
        {
            get { return BitConverter.GetBytes(FATEnt); }
        }
    }
    //this will need to be re-worked, But for now it will allow read only access.
    public class FAT
    {
        BootSector BPB;
        FileStream FATStream;
        public FAT(BootSector bs, FileStream fstr)
        {
            BPB = bs;
            //Woops.
            FATStream = fstr;
        }
        private int Fat1Location
        {
            get { return BPB.ReservedSectorCount; }
        }
        private int Fat2Location
        {
            get { return Fat1Location + BPB.SectorsPerFAT; }
        }
        public void SetFATEntry(int cluster, FATEntry f)
        {
            SetFATEntry(cluster, f.Value);
        }
        public void SetFATEntry(int cluster, ushort val)
        {
            //Super easy :)
            FATStream.Seek((Fat1Location * BPB.BytesPerSector) + (cluster * 2), SeekOrigin.Begin);
            FATStream.Write(
                BitConverter.GetBytes(val),
                0,
                2);
            //Write it to the backup fat too
            FATStream.Seek((Fat2Location * BPB.BytesPerSector) + (cluster * 2), SeekOrigin.Begin);
            FATStream.Write(
                BitConverter.GetBytes(val),
                0,
                2);
        }
        public FATEntry GetFATEntry(int cluster)
        {
            //Lets find the particular cluster in FAT1
            //Fat 1 is easy to find.
            //Actual byte position formula:
            //(FatXLocation*BPB.BytesPerSector) + (cluster*2)
            byte[] fatEnt = new byte[2];

            FATStream.Read(fatEnt, 0, 2);
            ushort data = 0;
            unchecked
            {
                data = (ushort)(fatEnt[0] + (fatEnt[1] << 8));
            }
            return FATEntry.Create(data, cluster);
        }
        public FATEntry NextFreeFATEntry()
        {
            for (int i = 0; i < ((BPB.SectorsPerFAT * BPB.BytesPerSector) / 2); i++)
            {
                if (this.GetFATEntry(i).IsFree)
                    return GetFATEntry(i);
            }
            //Ah shit...
            throw new NoFreeFATEntriesException();
        }
    }
}
public class NoFreeFATEntriesException : Exception
{
    public NoFreeFATEntriesException() { }
}