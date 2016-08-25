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
using WCS.MLFileInfo;
using System.Drawing;

namespace libTest
{
    class Program
    {
        static string DumpByteArray(byte[] input)
        {
            int ctr = 0;
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                if ((ctr * 16) >= input.Length)
                    break;
                //Dump 16 bytes per line. 1024 bytes should make up 64 lines.
                sb.AppendLine(BitConverter.ToString(input, ctr * 16, 16));
                ctr += 1;
            }
            return sb.ToString();
        }
        static void Main(string[] args)
        {
            Image i = WCS.SGHA867.BitmapImage.FirmwareBMP.FWBitmapToImage("test.raw", 11, 21);
            Bitmap bmp = new Bitmap(i);
            bmp.Save("test.bmp");
            Console.ReadLine();
        }
    }
}
