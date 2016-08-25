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
using MLFI.Interfaces.Abstracts;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using WCS.SGHA867.BitmapImage;
using System.Drawing.Imaging;

namespace BootImageEditor
{
    public class BootImagePlugin : UIPlugin
    {
        const string BootImageFilename = "A867Boot.bmp";
        const string BattImageFilename = "A867Batt.bmp";
        const string BorderImageFilename = "A867BarBorder.bmp";
        const string BarImageFilename = "A867ProgressBar.bmp";
        public BootImagePlugin()
        {
            this.Author = "plaguethenet";
            this.Name = "Boot Image Exporter";
            this.SupportedTypes.Add("rc2");
            BuildMenus();
            fbd.Description = "Please select the folder for import/export";
            fbd.ShowNewFolderButton = true;
            Print("Loaded.");
        }
        protected string GetImageFolder()
        {
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                return fbd.SelectedPath;
            }
            throw new Exception("User canceled the dialog box");
        }
        void BuildMenus()
        {
            ToolStripMenuItem main = new ToolStripMenuItem("&Boot Images");
            main.Tag = "rc2";
            ToolStripMenuItem tsmi = new ToolStripMenuItem("&Export");
            tsmi.Click += new EventHandler(ExportClick);
            main.DropDownItems.Add(tsmi);
            tsmi = new ToolStripMenuItem("&Import");
            tsmi.Click += new EventHandler(ImportClick);
            main.DropDownItems.Add(tsmi);
            this.Menus.Add(main);
        }
        FolderBrowserDialog fbd = new FolderBrowserDialog();

        void ImportClick(object sender, EventArgs e)
        {
            string targetDirectory = string.Empty;
            try
            {
                targetDirectory = GetImageFolder();
            }
            catch { }
            if (string.IsNullOrEmpty(targetDirectory))
                return;
            DoImport(targetDirectory, this.SelectedFile);
        }
        private void ProcessBootImage(DirectoryInfo di, FileStream fs)
        {
            FileInfo fi = new FileInfo(Path.Combine(di.FullName, BootImageFilename));
            if (fi.Exists)
            {
                try
                {
                    byte[] block = ImportBitmap(fi.FullName, FWImageInfo.Boot.width, FWImageInfo.Boot.height);
                    fs.Seek(FWImageInfo.Boot.offset, SeekOrigin.Begin);
                    fs.Write(block, 0, block.Length);
                }
                catch (SizeException)
                {
                    MessageBox.Show("The image " + fi.Name +
                        " is not the correct size! Please resize it to " +
                        FWImageInfo.Boot.width.ToString() + ", " +
                        FWImageInfo.Boot.height.ToString());
                }
                catch
                {
                    //Fail silently.
                }
            }
        }
        private void DoImport(string targetDirectory, string Filename)
        {
            FileInfo rc2 = new FileInfo(Filename);
            using (FileStream fs = rc2.Open(FileMode.Open, FileAccess.Write, FileShare.Read))
            {
                DirectoryInfo di = new DirectoryInfo(targetDirectory);
                ProcessBootImage(di, fs);
                ProcessBattImage(di, fs);
                ProcessBorderImage(di, fs);
                ProcessBarImage(di, fs);
                fs.Flush();
            }
            MessageBox.Show("Import complete, You may now flash the RC2 file to your phone!");
        }

        private void ProcessBarImage(DirectoryInfo di, FileStream fs)
        {
            FileInfo fi = new FileInfo(Path.Combine(di.FullName, BarImageFilename));
            if (fi.Exists)
            {
                try
                {
                    byte[] block = ImportBitmap(fi.FullName, FWImageInfo.ProgressBar.width, FWImageInfo.ProgressBar.height);
                    fs.Seek(FWImageInfo.ProgressBar.offset, SeekOrigin.Begin);
                    fs.Write(block, 0, block.Length);
                }
                catch (SizeException)
                {
                    MessageBox.Show("The image " + fi.Name +
                        " is not the correct size! Please resize it to " +
                        FWImageInfo.ProgressBar.width.ToString() + ", " +
                        FWImageInfo.ProgressBar.height.ToString());
                }
                catch
                {
                    //Fail silently.
                }
            }
        }

        private void ProcessBorderImage(DirectoryInfo di, FileStream fs)
        {
            FileInfo fi = new FileInfo(Path.Combine(di.FullName, BorderImageFilename));
            if (fi.Exists)
            {
                try
                {
                    byte[] block = ImportBitmap(fi.FullName, FWImageInfo.ProgressBorder.width, FWImageInfo.ProgressBorder.height);
                    fs.Seek(FWImageInfo.ProgressBorder.offset, SeekOrigin.Begin);
                    fs.Write(block, 0, block.Length);
                }
                catch (SizeException)
                {
                    MessageBox.Show("The image " + fi.Name +
                        " is not the correct size! Please resize it to " +
                        FWImageInfo.ProgressBorder.width.ToString() + ", " +
                        FWImageInfo.ProgressBorder.height.ToString());
                }
                catch
                {
                    //Fail silently.
                }
            }
        }

        private void ProcessBattImage(DirectoryInfo di, FileStream fs)
        {
            FileInfo fi = new FileInfo(Path.Combine(di.FullName, BattImageFilename));
            if (fi.Exists)
            {
                try
                {
                    byte[] block = ImportBitmap(fi.FullName, FWImageInfo.Batt.width, FWImageInfo.Batt.height);
                    fs.Seek(FWImageInfo.Batt.offset, SeekOrigin.Begin);
                    fs.Write(block, 0, block.Length);
                }
                catch (SizeException)
                {
                    MessageBox.Show("The image " + fi.Name +
                        " is not the correct size! Please resize it to " +
                        FWImageInfo.Batt.width.ToString() + ", " +
                        FWImageInfo.Batt.height.ToString());
                }
                catch
                {
                    //Fail silently.
                }
            }
        }
        private byte[] ImportBitmap(string filename, int width, int height)
        {
            Bitmap b = new Bitmap(filename);
            if (b.Width != width || b.Height != height)
            {
                throw new SizeException(string.Format("Bitmap {0}\n" +
                    "Requested Size: {1}, {2}\n" +
                    "Actual Size: {3}, {4}", filename, width, height, b.Width, b.Height));
            }
            return FirmwareBMP.ImageToFWBitmap(b);
        }

        void ExportClick(object sender, EventArgs e)
        {
            string targetDirectory = string.Empty;
            try
            {
                targetDirectory = GetImageFolder();
            }
            catch { }
            if (string.IsNullOrEmpty(targetDirectory))
                return;
            DoExport(targetDirectory, this.SelectedFile);
        }

        private void DoExport(string exportDir, string rc2file)
        {
            //240x400
            //Offset 0x21000
            byte[] iBoot = new byte[192000];
            //240x400
            //Offset 0x50000
            byte[] iBatt = new byte[192000];
            //212x13
            //Offset 0x7EE04
            byte[] iPbdr = new byte[5512];
            //21x11
            //Offset 0x8038C
            byte[] iPbar = new byte[462];

            FileInfo fi = new FileInfo(rc2file);
            using (FileStream fs = fi.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                //Load up our images.
                fs.Seek(0x21000, SeekOrigin.Begin);
                if (fs.Read(iBoot, 0, iBoot.Length) != iBoot.Length)
                    throw new InvalidDataException("Couldn't read the boot image from the RC2 file!");
                fs.Seek(0x50000, SeekOrigin.Begin);
                if (fs.Read(iBatt, 0, iBatt.Length) != iBatt.Length)
                    throw new InvalidDataException("Couldn't read the Battery image from the RC2 File!");
                fs.Seek(0x7EE04, SeekOrigin.Begin);
                if (fs.Read(iPbdr, 0, iPbdr.Length) != iPbdr.Length)
                    throw new InvalidDataException("Couldn't read the progress bar border from the RC2 file!");
                //?
                fs.Seek(0x8038C, SeekOrigin.Begin);
                if (fs.Read(iPbar, 0, iPbar.Length) != iPbar.Length)
                    throw new InvalidDataException("Couldn't read the progress bar image from RC2!");
            }
            //Bitmap tmp = new Bitmap(240, 400);
            Image boot = FirmwareBMP.FWBitmapToImage(iBoot, 400, 240);
            Image batt = FirmwareBMP.FWBitmapToImage(iBatt, 400, 240);
            Image brdr = FirmwareBMP.FWBitmapToImage(iPbdr, 13, 212);
            Image pbar = FirmwareBMP.FWBitmapToImage(iPbar, 11, 21);
            //And now...
            DirectoryInfo di = new DirectoryInfo(exportDir);
            boot.Save(Path.Combine(di.FullName, BootImageFilename), ImageFormat.Bmp);
            batt.Save(Path.Combine(di.FullName, BattImageFilename), ImageFormat.Bmp);
            brdr.Save(Path.Combine(di.FullName, BorderImageFilename), ImageFormat.Bmp);
            pbar.Save(Path.Combine(di.FullName, BarImageFilename), ImageFormat.Bmp);
            MessageBox.Show(string.Format("Successfully exported boot images to {0}", di.FullName));


        }
        private struct FWImageInfo
        {
            public int offset;
            public int length
            {
                get { return width * height * 2; }
            }
            public int width;
            public int height;
            public static FWImageInfo Boot
            {
                get
                {
                    FWImageInfo ret = new FWImageInfo();
                    ret.offset = 0x21000;
                    ret.width = 240;
                    ret.height = 400;
                    return ret;
                }
            }
            public static FWImageInfo Batt
            {
                get
                {
                    FWImageInfo ret = FWImageInfo.Boot;
                    ret.offset = 0x50000;
                    return ret;
                }
            }
            public static FWImageInfo ProgressBorder
            {
                get
                {
                    FWImageInfo ret = new FWImageInfo();
                    ret.offset = 0x7EE04;
                    ret.width = 212;
                    ret.height = 13;
                    return ret;
                }
            }
            public static FWImageInfo ProgressBar
            {
                get
                {
                    FWImageInfo ret = new FWImageInfo();
                    ret.offset = 0x8038C;
                    ret.width = 21;
                    ret.height = 11;
                    return ret;
                }
            }
        }
    }

}

public class SizeException : Exception
{
    public SizeException() : base() { }
    public SizeException(string message) : base(message) { }
}