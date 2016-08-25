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
using System.IO;

namespace WCS.MLFileInfo
{
    public class MLHeader
    {
        public MLEndBlock meb = new MLEndBlock();
        public MLHeader(string filename)
        {
            FileInfo fi = new FileInfo(filename);
            byte[] block = new byte[1024];
            try
            {
                FileStream fs = fi.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                fs.Seek(-1024, SeekOrigin.End); //Seek to 1024 bytes before the end of the file.
                fs.Read(block, 0, 1024);
                fs.Close();
            }
            catch (Exception e)
            {
                //Here to follow my personal coding standards.
                throw e;
            }
            byte[] ExpectedHeader = { 0xCD, 0xAB, 0xCD, 0xAB };


            for (int i = 0; i < 4; i++)
            {
                if (ExpectedHeader[i] != block[i])
                    throw new InvalidDataException("Invalid Header in MultiLoader Descriptor Block!");
                else
                    meb.Magic[i] = block[i];
            }
            //We have a good header.
            //Finish loading the block.
            for (int i = 0; i < 4; i++)
            {
                meb.FlashAddr[i] = block[i + 4];
            }
            for (int i = 0; i < 4; i++)
            {
                meb.unkBlock1[i] = block[i + 8];
            }
            for (int i = 0; i < 4; i++)
            {
                meb.Model[i] = block[i + 12];
            }
            for (int i = 0; i < 28; i++)
            {
                meb.unkBlock2[i] = block[i + 16];
            }
            for (int i = 0; i < 3; i++)
            {
                meb.FType[i] = block[i + 44];
            }
            for (int i = 0; i < 17; i++)
            {
                meb.unkBlock3[i] = block[i + 47];
            }
            for (int i = 0; i < 960; i++)
            {
                meb.unkSpecialBlock1[i] = block[i + 64];
            }
        }
    }
}
