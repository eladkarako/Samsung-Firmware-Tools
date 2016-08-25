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
using System.Drawing;
using System.IO;

namespace WCS.SGHA867.BitmapImage
{
    public class FirmwareBMP
    {
        //Here are the positions in the RC2
        //For various images
        //Normal Boot Logo (AT&T Branded logo)
        //Start offset: 0x21000
        //Length: 192000
        //Resolution: 240x400
        //16 Bits per pixel (Really 15)
        //This one is padded with 512x 0xff
        //The rest are not.

        //Battery Boot Logo (Black screen, Battery icon)
        //Start Offset: 0x50000
        //Length: 192000
        //Resolution: 240x400
        //16 Bits per pixel (really 15)
        //Padded with 2x 0xff... really samsung? Why?

        //Progress bar background/border
        //Start offset: 0x7EE04
        //Length: 5512
        //Resoultion: 212x13
        //16 Bits per Pixel (really 15)
        //Not padded.
        
        //Progress bar image (Makes up 10% of progress, leaving 1px on each side of the border.)
        //Start offset: 0x8038C
        //Length: 462
        //Resolution: 21x11
        //16 Bits per Pixel (really 15)
        //Padded with Multiloader "Header" (footer really)
        static UInt16 RGBToInt16(Color c)
        {
            return RGBToInt16((byte)c.R, (byte)c.G, (byte)c.B);
        }
        static UInt16 RGBToInt16(byte r, byte g, byte b)
        {
            byte fwr; // Holds red value in FW Format (5bit)
            byte fwg; // Holds green value in FW Format (5bit)
            byte fwb; // Holds blue value in FW Format (5bit)
            //R is based at MSB
            //B is based at LSB
            //Convert 8 bit number to 5 bit number.
            //Retain quality as much as possible.
            fwr = normalize(r, 255, 31);
            fwg = normalize(g, 255, 31);
            fwb = normalize(b, 255, 31);
            return PackFWRGB(fwr, fwg, fwb);
        }
        /// <summary>
        /// This function converts a value with the rage 0-max
        /// And converts it to 0-newmax, Scaling the value
        /// proportinately.
        /// </summary>
        /// <param name="val">Value to normalize</param>
        /// <param name="max">Current maximum value</param>
        /// <param name="newmax">New maximum value.</param>
        /// <returns></returns>
        static byte normalize(byte val, byte max, byte newmax)
        {
            double work = (double)val / (double)max;
            byte ret = (byte)(work * (double)newmax);
            return ret;
        }
        static byte[] UnpackFWRGB(UInt16 packed)
        {
            byte[] ret = new byte[3];
            byte r;
            byte g;
            byte b;
            b = (byte)(packed & 0x1F);
            g = (byte)((packed >> 6) & 0x1F);
            r = (byte)((packed >> 11) & 0x1F);
            r = normalize(r, 31, 255);
            g = normalize(g, 31, 255);
            b = normalize(b, 31, 255);
            //Retarted code, Could probably be simpler. but meh.
            //Anyway, Pack it into the array now. We are done.
            ret[0] = r;
            ret[1] = g;
            ret[2] = b;
            return ret;
        }
        static UInt16 PackFWRGB(byte[] rgb)
        {
            if (rgb.Length != 3)
                throw new InvalidOperationException("rbg value array must contain exactly 3 elements.");
            return PackFWRGB(rgb[0], rgb[1], rgb[2]);
        }
        static UInt16 PackFWRGB(byte r, byte g, byte b)
        {
            //Make sure we dont produce invalid output
            //Even with invalid input.
            r = (byte)(r & 0x1F);
            g = (byte)(g & 0x1F);
            b = (byte)(b & 0x1F);
            UInt16 nr = r, ng = g, nb = b;
            UInt16 ret;
            //Shift red left 11 bits.
            nr = (UInt16)(nr << 11);
            //Shift Green left 6 bits
            ng = (UInt16)(ng << 6);
            //Blue stays blue. It takes up LSB,
            //So there is no need to shift it.
            //Although, We could do ng << 0 :P
            //You know, Just for appearances.

            //And now finally, We pack it all together
            //Into its new int16 format.
            //We have now packed 3 bytes into 2!

            ret = (UInt16)(nr | ng | nb);

            return ret;
        }
        public static byte[] ImageToFWBitmap(Image i)
        {
            Bitmap bmp = new Bitmap(i);

            byte[] buff = new byte[i.Width * i.Height * 2];
            for (int y = 0; y < i.Height; y++)
            {
                for (int x = 0; x < i.Width; x++)
                {
                    Color ctmp = bmp.GetPixel(x, y);
                    UInt16 utmp = RGBToInt16(ctmp);
                    byte[] btmp = BitConverter.GetBytes(utmp);
                    if (btmp.Length != 2)
                        throw new Exception("What the f...?");
                    btmp.CopyTo(buff, (x + (y * i.Width)) * 2);
                }
            }
            return buff;
        }
        public static Image FWBitmapToImage(string filename, int height, int width)
        {
            FileInfo fi = new FileInfo(filename);
            byte[] buffer;
            using (FileStream fs = fi.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (height == -1)
                {
                    height = (int)Math.Floor((double)((fi.Length / width)/2));
                }
                buffer = new byte[fs.Length];
                int posCtr = 0;
                int blockSize = 1024;
                int bytesToRead = blockSize;
                while (posCtr * blockSize < fs.Length)
                {
                    //This ensures that the last block read will be read
                    //correctly, Even if its not on a 1024 byte boundry.
                    if ((fs.Length - (posCtr * blockSize)) < 1024 )
                        bytesToRead = (int)(fs.Length - (posCtr * blockSize));
                   
                    fs.Read(buffer, posCtr * blockSize, bytesToRead);
                    posCtr++;
                }
            }
            return FWBitmapToImage(buffer, height, width);
        }

        public static Image FWBitmapToImage(byte[] rawData, int height, int width)
        {
            if (rawData.Length < (height * width * 2))
            {
                throw new InvalidOperationException("Height and width specified is invalid, The array must be 2 * height * width (" +
                    (2 * height * width).ToString() +
                    ") long.\n The data you passed was only "
                    + (rawData.Length).ToString() +
                    " bytes long!");
            }
            //Accolate space.
            UInt16[] intData = new UInt16[rawData.Length / 2];
            for (int x = 0; x < intData.Length; x++)
            {
                intData[x] = (UInt16)((rawData[(x * 2)+1] * 256) + rawData[(x * 2)]);
            }
            Bitmap i = new Bitmap(width, height);
            for (int x = 0; x < i.Width; x++)
            {
                for (int y = 0; y < i.Height; y++)
                {
                    byte[] rgb = UnpackFWRGB(intData[x + (y * i.Width)]);
                    Color c = Color.FromArgb((int)rgb[0], (int)rgb[1], (int)rgb[2]);
                    i.SetPixel(x, y, c);
                }
            }
            return i;
        }
    }
}
