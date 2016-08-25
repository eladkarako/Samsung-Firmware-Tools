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
//  07/22/2010 - plaguethenet@gmail.com - Added reflection code that should make maintaining project easier.
//      Well, At least the Fat filesystem code anyway. See Helpers\DataLayout.cs for details.
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using WCS.FatSharp.Helpers;
using System.Diagnostics;

namespace WCS.FatSharp.Structures
{
    public class BootSector
    {
        //Jump over metadata to boot code.
        //0x00, 3
        [Data(0, 3)]
        byte[] jmpInstruction = { 0xE9, 0x09, 0x90 };
        //0x03, 8
        [Data(0x03, 8)]
        internal string OEMName = "        "; //Pad with 0x20 to fill array.
        //0x0B, 2
        [Data(0x0B)]
        internal ushort BytesPerSector = 512; //Usually 512, Can change though.
        //0x0D, 1
        [Data(0x0D)]
        internal byte SectorsPerCluster;
        //Valid values are 1-128 (MUST be power of 2)
        //Additionally (SPC^2) * BPS must not exceed 32*1024
        //This limits SPC to 64? is that right?
        //0x0E, 2
        [Data(0x0E)]
        internal ushort ReservedSectorCount;
        //0x10, 1
        [Data(0x10)]
        internal byte TotalFats = 2; //almost always 2, Can change though.
        //0x11, 2
        [Data(0x11)]
        internal ushort MaxRootEntries;
        //Should be 0 on Fat32.
        //0x13, 2
        [Data(0x13)]
        internal ushort TotalSectors; //If 0x0000, Use 4 byte version.
        //0x15, 1
        //MediaDescriptor Descriptor = MediaDescriptor.FixedDisk; // Probably the only value used these days.
        [Data(0x15)]
        internal byte Descriptor = (byte)MediaDescriptor.FixedDisk;
        //0x16, 2
        [Data(0x16)]
        internal ushort SectorsPerFAT; //Used on 12/16 only?
        //0x18, 2
        [Data(0x18)]
        internal ushort SectorsPerTrack; //Not really used..
        //0x1a, 2
        [Data(0x1A)]
        internal ushort NumberOfHeads; //Not really used...
        //0x1c, 4
        [Data(0x1C)]
        internal uint HiddenSectors; //Count of hidden sectors, Should be 0
        //0x20, 4
        [Data(0x20)]
        internal uint TotalSectors32; //Used if Total > 65535, Otherwise use 16 bit version.
        //0x24, 1
        [Data(0x24)]
        internal byte DiskType = (byte)FatDiskType.Removable;
        //0x25, 1
        [Data(0x25)]
        internal byte CurrentHead = 0x00; //Windows NT uses bit 0 to flag as dirty
        //Bit 1 requests a surface scan.
        //0x26, 1
        [Data(0x26)]
        internal byte ExtendedBootSig = 0x29; //Can be 0x28 too.
        //0x28 indicates an older type of EBPB(Has only the serial after this, Instead of label and type)
        //0x27, 4
        [Data(0x27)]
        internal uint SerialNumber = 2860371753; //0xAA7DCF29 (What was used in FactoryFS)
        //This may be able to be changed, But i recommend we keep this value.
        //0x2B, 11
        [Data(0x2B, 11)]//Is that right? :/
        internal string VolLabel = "KFAT0      "; //Must be 11 bytes, Pad with spaces (0x20)
        //0x36, 8
        [Data(0x36, 8)]
        internal string FSType = "FAT16  "; //Must be 8 bytes, Pad with 0x20.
        //0x3e, 448

        //internal byte[] OSBootCode = new byte[448]; //OS Boot code.

        /* C:\Users\plaguethenet\Desktop\A867\PTFFFSv1.0.ffs (7/16/2010 12:03:14 AM)
   StartOffset: 0000003E, EndOffset: 000001FD, Length: 000001C0 */
        [Data(0x3e, 448)]
        internal byte[] OSBootCode = {
	0x0E, 0x2C, 0x20, 0x1F, 0x2C, 0x20, 0xBE, 0x2C, 0x20, 0x5B, 0x2C, 0x20,
	0x7C, 0x2C, 0x20, 0xAC, 0x2C, 0x20, 0x22, 0x2C, 0x20, 0xC0, 0x2C, 0x20,
	0x74, 0x2C, 0x20, 0x0B, 0x56, 0x2C, 0x20, 0xB4, 0x2C, 0x20, 0x0E, 0x2C,
	0x20, 0xBB, 0x2C, 0x20, 0x07, 0x2C, 0x20, 0x00, 0x2C, 0x20, 0xCD, 0x2C,
	0x20, 0x10, 0x2C, 0x20, 0x5E, 0x2C, 0x20, 0xEB, 0xF0, 0x2C, 0x20, 0x32,
	0x2C, 0x20, 0xE4, 0x2C, 0x20, 0xCD, 0x2C, 0x20, 0x16, 0x2C, 0x20, 0xCD,
	0x2C, 0x20, 0x19, 0x2C, 0x20, 0xEB, 0x2C, 0x20, 0xFE, 0x54, 0x68, 0x69,
	0x73, 0x20, 0x69, 0x73, 0x20, 0x6E, 0x6F, 0x74, 0x20, 0x61, 0x20, 0x62,
	0x6F, 0x6F, 0x74, 0x61, 0x62, 0x6C, 0x65, 0x20, 0x64, 0x69, 0x73, 0x6B,
	0x2E, 0x20, 0x50, 0x6C, 0x65, 0x61, 0x73, 0x65, 0x20, 0x69, 0x6E, 0x73,
	0x65, 0x72, 0x74, 0x20, 0x61, 0x20, 0x62, 0x6F, 0x6F, 0x74, 0x61, 0x62,
	0x6C, 0x65, 0x20, 0x66, 0x6C, 0x6F, 0x6F, 0x70, 0x79, 0x20, 0x61, 0x6E,
	0x64, 0x0D, 0x0A, 0x70, 0x72, 0x65, 0x73, 0x73, 0x20, 0x61, 0x6E, 0x79,
	0x20, 0x6B, 0x65, 0x79, 0x20, 0x74, 0x6F, 0x20, 0x74, 0x72, 0x79, 0x20,
	0x61, 0x67, 0x61, 0x69, 0x6E, 0x2E, 0x2E, 0x2E, 0x0D, 0x0A, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00
};

        //0x1FE, 2
        [Data(0x1FE)]
        internal ushort BootSig = 0xAA55; //Written to disk as 0x55 0xAA
        FileStream FATStream;
        FAT FATEntries;
        public FAT FileTable
        {
            get { return FATEntries; }
        }
        public static byte[] GetBytes(BootSector bs)
        {
            //This is going to be a pain in the arse.
            byte[] ret = new byte[512];
            //Lets copy the easy stuff first.
            //First arrays can be copied with Array.Copy
            Array.Copy(bs.jmpInstruction, ret, 3);
            Array.Copy(bs.OSBootCode, 0, ret, 0x3e, 448);
            //Next single byte values can be written out
            //Directly to the array
            ret[0x0D] = bs.SectorsPerCluster;
            ret[0x10] = bs.TotalFats;
            ret[0x15] = (byte)bs.Descriptor;
            ret[0x24] = (byte)bs.DiskType;
            ret[0x25] = bs.CurrentHead;
            ret[0x26] = bs.ExtendedBootSig;
            //And thats it for the 1 byte values.
            //Now, For the strings...
            Array.Copy(GetFSString(bs.OEMName, 8), 0, ret, 0x03, 8);
            Array.Copy(GetFSString(bs.FSType, 8), 0, ret, 0x36, 8);
            Array.Copy(GetFSString(bs.VolLabel, 11), 0, ret, 0x2b, 11);
            //And finally, All ushorts and uints
            //Array.Copy(BitConverter.GetBytes(bs.BytesPerSector), 0, ret, 0x0B, 2);
            GetAndCopy(bs.BytesPerSector, ret, 0x0B);
            GetAndCopy(bs.ReservedSectorCount, ret, 0x0e);
            GetAndCopy(bs.MaxRootEntries, ret, 0x11);
            GetAndCopy(bs.TotalSectors, ret, 0x13);
            GetAndCopy(bs.SectorsPerFAT, ret, 0x16);
            GetAndCopy(bs.SectorsPerTrack, ret, 0x18);
            GetAndCopy(bs.NumberOfHeads, ret, 0x1A);
            GetAndCopy(bs.HiddenSectors, ret, 0x1c);
            GetAndCopy(bs.TotalSectors32, ret, 0x20);
            GetAndCopy(bs.SerialNumber, ret, 0x27);
            GetAndCopy(bs.BootSig, ret, 0x1FE);
            //Finally, Return ret.
            return ret;
        }
        public static void GetAndCopy(ushort val, byte[] dest, int offset)
        {
            doCopy(BitConverter.GetBytes(val), dest, offset);
        }
        public static void GetAndCopy(uint val, byte[] dest, int offset)
        {
            doCopy(BitConverter.GetBytes(val), dest, offset);
        }
        private static void doCopy(byte[] src, byte[] dest, int doffset)
        {
            Array.Copy(src, 0, dest, doffset, src.Length);
        }
        public static byte[] GetFSString(string input, int length)
        {
            return GetFSString(input, length, Encoding.ASCII);
        }
        public static byte[] GetFSString(string input, int length, Encoding e)
        {
            byte[] output = e.GetBytes(input);
            byte[] ret = new byte[length];
            if (output.Length >= length)
                Array.Copy(output, 0, ret, 0, length);
            else
            {
                Array.Copy(output, 0, ret, 0, output.Length);
                for (int x = output.Length; x < ret.Length; x++)
                {
                    //Pad to length with spaces.
                    ret[x] = 0x20;
                }
            }
            return ret;
        }
        public static BootSector Load(FileStream fstream)
        {
            // If we cant seek... I pray to god we are being
            // fed good data.... Or else the wasps that
            // stare at me at work may get you.
            if (!fstream.CanSeek)
                throw new InvalidOperationException("Need a seekable stream!");
            // They know of me as the Wasp murderer.
            // They plot my destruction.
            byte[] bsBuffer = new byte[512];
            if (fstream.Read(bsBuffer, 0, 512) != 512)
            {
                //Ah fuck, THE WASPS ARE ANGRY.
                //We cant continue without 512 bytes.
                //So we just bitch and bail, Let the
                //Caller deal with the problem. They
                //Should have fed us a good stream anyway.
                throw new InvalidOperationException("Unable to read entire boot sector from stream!");
            }
            //You have successfully escaped the wasps.
            //We should not close the stream though, Less we anger the bee's.
            //Although less harmful than the wasps, They can still
            //Cause problems!
            return Create(bsBuffer, fstream);
        }
        public static BootSector Load(string filename)
        {
            FileInfo fi = new FileInfo(filename);
            //Don't bother error checking, Any exceptions
            //Will be passed up the call chain.
            //They can deal with any bee's or wasps they
            //Decide to anger.
            FileStream fs = fi.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            BootSector bs = Load(fs);
            //We opened it, The bee's won't get angry if we close it. :)
            //fs.Close();
            //Oh yes they will, Now that we use the stream for a long time
            //After its made. DO NOT ANGER THE BEES.
            return bs;
        }
        public static BootSector Create(byte[] block, FileStream fstream)
        {
            BootSector ret = new BootSector();
            ret.FATStream = fstream;
            if (block.Length < 512)
                throw new InvalidOperationException("Array must be >= 512 bytes");
            //So much simpler :)
            Helpers.DataLayout.Fill<BootSector>(block, ret);
            ret.FATEntries = new FAT(ret, ret.FATStream);
            return ret;
        }
        public static string BytesToASCIIString(byte[] data)
        {
            return BytesToASCIIString(data, data.Length);
        }
        public static string BytesToASCIIString(byte[] data, int length)
        {
            return BytesToASCIIString(data, 0, length);
        }
        public static string BytesToASCIIString(byte[] data, int offset, int length)
        {
            return Encoding.ASCII.GetString(data, offset, length);
        }
        public static uint BytesToUInt(byte[] data)
        {
            return BytesToUInt(data, 0);
        }
        public static uint BytesToUInt(byte[] data, int offset)
        {
            if (data.Length < (4 + offset))
                throw new InvalidOperationException("Data must be at least 4 bytes!");
            return (uint)((data[0 + offset] << 24) | (data[1 + offset] << 16) | (data[2 + offset] << 8) | data[3 + offset]);
        }
        public static ushort BytesToUShort(byte[] data)
        {
            return BytesToUShort(data, 0);
        }
        public static ushort BytesToUShort(byte[] data, int offset)
        {
            if (offset < 0)
                throw new IndexOutOfRangeException("Offset < 0?");
            if (data.Length < (2 + offset))
                throw new InvalidOperationException("Data must be at least 2 bytes!");
            return (ushort)((data[0 + offset] << 8) | data[1 + offset]);
        }
        /// <summary>
        /// This computes the position (sector) of a cluster
        /// </summary>
        /// <param name="cluster">Cluster to convert</param>
        /// <returns>Sector position of requested cluster</returns>
        public long GetFileStartSector(long cluster)
        {
            return RootDirectorySector +
                ((MaxRootEntries * 32) / BytesPerSector) +
                ((cluster - 2) * SectorsPerCluster);
        }
        /// <summary>
        /// Converts a cluster to its byte position
        /// </summary>
        /// <param name="cluster">Cluster to get file offset for</param>
        /// <returns>Byte offset of the requested cluster</returns>
        public long GetFileStartBytePosition(long cluster)
        {
            return GetFileStartSector(cluster) * BytesPerSector;
        }
        /// <summary>
        /// Contains the sector location of the root directory
        /// as described by this boot sector. Multiply by sector
        /// size to get the actual byte position.
        /// </summary>
        public int RootDirectorySector
        {
            get
            {
                //1 to skip sector 0, The boot sector doesn't appear to be
                //Included when calculating the position of the root directory.
                return ReservedSectorCount + (TotalFats * SectorsPerFAT);
            }
        }
        /// <summary>
        /// Contains the exact byte number where the root directory is located.
        /// </summary>
        public long RootDirectoryBytePosition
        {
            get { return RootDirectorySector * BytesPerSector; }
        }
        public string FormatOEMName
        {
            get { return OEMName; }
            set
            {
                while (value.Length < 11)
                {
                    value = value + " ";
                }
                while (value.Length > 11)
                {
                    value.Remove(11);
                }
                OEMName = value;
            }
        }
        /// <summary>
        /// Calculate the checksum of a given string.
        /// </summary>
        /// <param name="text">String to calculate checksum of</param>
        /// <param name="e">Encoding to use when calculating checksum</param>
        /// <returns>The calculated checksum</returns>
        public static byte FatCheckSum(string text, Encoding e)
        {
            byte[] str = e.GetBytes(text);
            byte sum = 0;
            for (int x = 0; x < str.Length; x++)
            {
                //We WANT this to overflow. So, Yeah
                //Remove any over flow checking.
                //I can hear wasps buzzing in the distance.
                unchecked
                {
                    sum = (byte)((sum >> 1) + ((sum & 1) << 7));
                    sum += str[x];
                }
            }
            return sum;
        }

        public int SectorToByte(int sector)
        {
            return sector * BytesPerSector;
        }
        public int ClusterToSector(int cluster)
        {
            return ReservedSectorCount +
                (TotalFats * SectorsPerFAT) +
                ((MaxRootEntries * 32) / BytesPerSector) +
                ((cluster - 2) * SectorsPerCluster);
        }
        public Sector ReadSector(int number)
        {
            return new Sector(this, number);
        }
        public byte[] RawReadSector(long sectorNumber)
        {
            byte[] block = new byte[BytesPerSector];
            lock (FATStream)
            {
                FATStream.Seek(sectorNumber * BytesPerSector, SeekOrigin.Begin);
                FATStream.Read(block, 0, BytesPerSector);
            }
            return block;
        }
        public void RawWriteSector(int sectorNumber, Sector val)
        {
            lock (FATStream)
            {
                FATStream.Seek(sectorNumber * BytesPerSector, SeekOrigin.Begin);
                FATStream.Write(val.sectorData, 0, val.sectorData.Length);
            }
        }
        public void WriteSectors(int startSector, int count, Sector[] val)
        {
            for (int x = 0; x < count; x++)
            {
                RawWriteSector(startSector + x, val[x]);
            }
        }
        public Sector[] ReadSectors(int startSector, int count)
        {
            List<Sector> Sectors = new List<Sector>();
            for (int x = 0; x < count; x++)
            {
                //Array.Copy(RawReadSector(startSector + x), 0, ret, BytesPerSector * x, BytesPerSector);
                Sectors.Add(ReadSector(startSector + x));
            }
            return Sectors.ToArray();
        }

        public byte[] RawReadCluster(int ClusterNumber)
        {
            if (ClusterNumber < 2)
                throw new IndexOutOfRangeException("ClusterNumber must be >= 2");
            byte[] clusterBlock = new byte[BytesPerSector * SectorsPerCluster];
            //Read one sector at a time.
            for (int x = 0; x < SectorsPerCluster; x++)
            {
                Array.Copy(RawReadSector((ClusterToSector(ClusterNumber) + (BytesPerSector * x))), 0, clusterBlock, x * BytesPerSector, BytesPerSector);
            }
            return clusterBlock;
        }
        private RootDirectory _r = null;
        public RootDirectory Root
        {
            get
            {
                if (_r == null)
                    _r = new RootDirectory(this);
                return _r;
            }
            set
            {
                _r = value;
                _r.Save();
            }
        }

    }
}
