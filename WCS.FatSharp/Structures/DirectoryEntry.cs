using System;
using System.Collections.Generic;
using System.Text;

namespace WCS.FatSharp.Structures
{
    public class DirectoryEntry
    {
        DirEnt myEntry = null;
        List<LFNEnt> LFNEntries = new List<LFNEnt>();
        /// <summary>
        /// Creates a directory entry containing both the long filename and short filename from an array of DirEnts.
        /// </summary>
        /// <param name="entries">Array of DirEnts</param>
        /// <param name="NormalIndex">The index in the array containing the short filename entry.</param>
        public DirectoryEntry(DirEntBase[] entries, int NormalIndex)
        {
            int start = NormalIndex;
            int lfnStart = -1;
            //Find the first long filename entry.
            for (int x = start - 1; x >= 0; x--)
            {
                if (entries[x].IsLFNEnt)
                    continue;
                if (entries[x].IsDIREnt)
                {
                    lfnStart = x - 1;
                    //if (lfnStart == NormalIndex)
                    //    lfnStart = -1;
                }
                else
                    lfnStart = -1;
                break;
            }
            if (lfnStart != -1)
            {
                for (int x = start - 1; x >= lfnStart; x--)
                {
                    LFNEntries.Add(entries[x].LFNEnt);
                }
            }
            myEntry = entries[start].DirEnt;
        }
        public string LongFilename
        {
            get
            {
                if (LFNEntries.Count == 0)
                    return ShortFilename;
                string ret = string.Empty;
                for (int x = 0; x < LFNEntries.Count; x++)
                {
                    ret = ret + LFNEntries[x].name1 + LFNEntries[x].name2 + LFNEntries[x].name3;
                    //This is bad, Don't do it.
                    //ret = ret.Trim();
                }
                for (int x = 0; x < ret.Length; x++)
                {
                    if (ret[x] == '\x0')
                    {
                        ret = ret.Remove(x);
                        break;
                    }
                }
                return ret;
            }
        }
        public string ShortFilename
        {
            get
            {
                return myEntry.EightDOTThreeName;
            }
        }
        public ushort StartCluster
        {
            get
            {
                return myEntry.StartCluster;
            }
            set
            {
                myEntry.StartCluster = value;
            }
        }
        public uint FileLength
        {
            get { return myEntry.Length; }
            set { myEntry.Length = value; }
        }
        public FATFileAttribute Attributes
        {
            get
            {
                return myEntry.Attributes;
            }
            set
            {
                myEntry.Attributes = value;
            }
        }
    }
}
