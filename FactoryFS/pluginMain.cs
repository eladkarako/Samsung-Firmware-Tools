using System;
using System.Collections.Generic;
using System.Text;
using MLFI.Interfaces.Abstracts;
using System.Windows.Forms;
using System.IO;
using WCS.FatSharp.Structures;
using System.Diagnostics;
using WCS.FatSharp.Helpers;
using WCS.FatSharp;

namespace FactoryFS
{
    public class pluginMain : UIPlugin
    {
        public pluginMain()
        {
            this.Author = "plaguethenet";
            this.Name = "FactoryFS Plugin";
            this.SupportedTypes.Add("ffs");
            ToolStripMenuItem root = new ToolStripMenuItem("Factory File System");
            root.Tag = "ffs";
            this.Menus.Add(root);
            root.DropDownItems.AddRange(GenerateSubMenu());
            Print("Loaded.");
        }

        private ToolStripMenuItem[] GenerateSubMenu()
        {
            List<ToolStripMenuItem> ret = new List<ToolStripMenuItem>();
            ToolStripMenuItem child = new ToolStripMenuItem("Read Bootsector");
            child.Tag = "BS_Read";
            child.Click += new EventHandler(child_Click);
            ret.Add(child);
            return ret.ToArray();
        }

        void child_Click(object sender, EventArgs e)
        {
            string cmd = (string)((ToolStripMenuItem)sender).Tag;
            switch (cmd)
            {
                case "BS_Read":
                    BS_Read();
                    break;
                default:
                    MessageBox.Show("Bug in plugin: " +
                        this.Name +
                        "\nCalled command:" + cmd +
                        "but it isn't registered!");
                    break;
            }
        }

        private void BS_Read()
        {
            FileInfo fi = new FileInfo(SelectedFile);
            //Ok here we go.

            BootSector bs = BootSector.Load(fi.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None));
            DirEntBase[] d = bs.Root.LoadDirEnts();
            Print("Loaded {0} DirEnts in the root directory!", d.Length);
            int free = 0;
            for (int i = 0; i < d.Length; i++)
            {
                DoEvents();
                if (d[i].IsLFNEnt)
                {
                    Print("Entry {0} is a long filename entry.", i);
                }
                else if (d[i].IsDIREnt)
                {
                    string Name = d[i].DirEnt.Filename;
                    if ((Name == "." || Name == ".."))
                    {
                        Print("Entry {0} is a reference to itself or parent directory.");
                        continue;
                    }
                    Print("Entry {0} is a normal Directory Entry.", i);
                    DirectoryEntry de = new DirectoryEntry(d, i);
                    Print("{0}: Short Name: {1}", i, de.ShortFilename);
                    Print("{0}: Long  Name: {1}", i, de.LongFilename);
                    Print("{0}: Attributes: {1}", i, de.Attributes);
                    Print("{0}: Starts at Cluster: {1} and is {2} byte(s) long.", i, de.StartCluster, de.FileLength );
                    if ((de.Attributes & FATFileAttribute.SubDirectory) == FATFileAttribute.SubDirectory)
                    {
                        DumpDirectory("/" + de.LongFilename, bs, de.StartCluster);
                    }
                }
                else if (d[i].IsDeletedFile)
                {
                    Print("Entry {0} contains a deleted file", i);
                    Print("{0}: {1}", i, d[i].DirEnt.EightDOTThreeName);
                }
                else
                {
                    free++;
                }
            }
            Print("There are {0} free entries out of {1} total entries", free, d.Length);
            Print("The root directory is {0}% Full.", Math.Floor(((((double)d.Length - (double)free) / (double)d.Length) * 100)));
            GC.KeepAlive(bs);
        }
        private void DumpDirectory(string Path, BootSector BPB, int ClusterNumber)
        {
            Cluster pointer = Cluster.Load(BPB, (ushort)ClusterNumber);
            Print("In directory:", Path);
            DirEntBase[] d = pointer.DirectoryEntries;
            int free = 0;
            for (int i = 0; i < d.Length; i++)
            {
                DoEvents();
                if (d[i].IsLFNEnt)
                {
                    Print("Entry {0} is a long filename entry.", i);
                }
                else if (d[i].IsDIREnt)
                {
                    string Name = d[i].DirEnt.Filename;
                    if ((Name == "." || Name == ".."))
                    {
                        Print("Entry {0} is a reference to itself or parent directory.");
                        continue;
                    }
                    Print("Entry {0} is a normal Directory Entry.", i);
                    DirectoryEntry de = new DirectoryEntry(d, i);
                    Print("{0}: Short Name: {1}", i, de.ShortFilename);
                    Print("{0}: Long  Name: {1}", i, de.LongFilename);
                    Print("{0}: Attributes: {1}", i, de.Attributes);
                    Print("{0}: Starts at Cluster: {1} and is {2} byte(s) long.", i, de.StartCluster, de.FileLength);
                    if ((de.Attributes & FATFileAttribute.SubDirectory) == FATFileAttribute.SubDirectory)
                    {
                        DumpDirectory(Path + "/" + de.LongFilename, BPB, de.StartCluster);
                    }
                }
                else if (d[i].IsDeletedFile)
                {
                    Print("Entry {0} contains a deleted file", i);
                    Print("{0}: {1}", i, d[i].DirEnt.EightDOTThreeName);
                }
                else
                {
                    free++;
                }
            }
        }
    }
}
