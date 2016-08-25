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
using WCS.FatSharp.Helpers;
using WCS.FatSharp.Base;

namespace WCS.FatSharp.Structures
{
    public class Cluster : FATObjectBase
    {
        ushort number;
        Sector[] Sectors;
        public DirEntBase[] DirectoryEntries
        {
            get
            {
                List<DirEntBase> deb = new List<DirEntBase>();
                for (int x = 0; x < Sectors.Length; x++)
                {
                    deb.AddRange(Sectors[x].DirectoryEntries);
                }
                Cluster c = this.next;
                if (c != null)
                    deb.AddRange(c.DirectoryEntries);
                return deb.ToArray();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public byte this[int index]
        {
            get
            {
                int sIndex = 0;
                int bIndex = index;
                Cluster belongsTo = this;
                while (bIndex > 512)
                {
                    sIndex++;
                    bIndex -= 512;
                }
                if (sIndex >= this.Sectors.Length)
                {
                    while (sIndex >= this.Sectors.Length)
                    {
                        sIndex -= this.Sectors.Length;
                        if (belongsTo.next == null)
                        {
                            //For setter only.
                            //belongsTo.AccolateCluster();
                            //For getter only.
                            throw new IndexOutOfRangeException("The index was greater than the accolated cluster count!");
                        }
                        belongsTo = belongsTo.next;
                    }
                    return belongsTo.Sectors[sIndex].sectorData[bIndex];
                }
                else
                    return this.Sectors[sIndex].sectorData[bIndex];
            }
            set
            {
                int sIndex = 0;
                int bIndex = index;
                Cluster belongsTo = this;
                while (bIndex > 512)
                {
                    sIndex++;
                    bIndex -= 512;
                }
                if (sIndex >= this.Sectors.Length)
                {
                    while (sIndex >= this.Sectors.Length)
                    {
                        sIndex -= this.Sectors.Length;
                        if (belongsTo.next == null)
                        {
                            //For setter only.
                            belongsTo.AccolateCluster();
                            //For getter only.
                            //throw new IndexOutOfRangeException("The index was greater than the accolated cluster count!");
                        }
                        belongsTo = belongsTo.next;
                    }
                    belongsTo.Sectors[sIndex].sectorData[bIndex] = value;
                }
                else
                    this.Sectors[sIndex].sectorData[bIndex] = value;

            }
        }

        private void AccolateCluster()
        {
            FATEntry target = BPB.FileTable.NextFreeFATEntry();


        }
        public FATEntry FATInfo
        {
            get
            {
                return BPB.FileTable.GetFATEntry(number);
            }
            set
            {
                BPB.FileTable.SetFATEntry(number, value);
            }
        }
        public bool IsEOC
        {
            get
            {
                return FATInfo.IsEOC;
            }
        }
        public Cluster next
        {
            get
            {
                if (!FATInfo.IsClusterPointer) return null;
                return Cluster.Load(BPB, FATInfo.Value);
            }
        }
        public void SaveChain()
        {
            this.Save();
            if (next != null)
                next.SaveChain();
        }
        public void Save()
        {
            //BPB.WriteSectors(BPB.ClusterToSector(number), Sectors.Length, Sectors);
            foreach (Sector s in Sectors)
            {
                s.Save();
            }
        }
        public static Cluster Load(BootSector bs, ushort clusterNum)
        {
            Cluster ret = new Cluster();
            List<Sector> Sectors = new List<Sector>();
            for (int x = 0; x < bs.SectorsPerCluster; x++)
            {
                Sector tmp = new Sector(bs, x + bs.ClusterToSector(clusterNum));
                Sectors.Add(tmp);
            }
            ret.Sectors = Sectors.ToArray();
            ret.number = clusterNum;
            ret.BPB = bs;
            return ret;
        }
    }
}
