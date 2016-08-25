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
//  07/18/2010 - plaguethenet@gmail.com - Added static functions to reproduce the GPL.
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using WCS.MLFileInfo.Properties;

//Code reuse? How about... Resource re-use? :P
public static class GPL
{
    public static string Short
    {
        get
        {
            return Resources.shortGPL;
        }
    }
    public static string Full
    {
        get
        {
            return Resources.fullGPL;
        }
    }
}

namespace WCS.MLFileInfo
{
    public class MLEndBlock
    {
        
        public Int32 magic
        {
            get
            {
                int rVal = 0;
                for (int i = 0; i < 4; i++)
                {
                    rVal *= 256;
                    rVal += (int)Magic[i];
                }
                return rVal;
            }
        }
        public Int32 flashAddress
        {
            get
            {
                int rVal = 0;
                for (int i = 0; i < 4; i++)
                {
                    rVal *= 256;
                    rVal += (int)FlashAddr[i];
                }
                return rVal;
            }
            set
            {
                byte[] addr = BitConverter.GetBytes(value);
                for (int i = 0; i < 4; i++)
                {
                    FlashAddr[i] = addr[i];
                }
            }
        }
        public string PhoneModel
        {
            get
            {
                return Encoding.ASCII.GetString(Model);
            }
            set
            {
                if(value.Length != 4)
                    throw new InvalidDataException("Phone Model must be EXACTLY 4 bytes!");
                byte[] str = Encoding.ASCII.GetBytes(value);
                for (int i = 0; i < 4; i++)
                {
                    Model[i] = str[i];
                }
            }
        }
        public string ImageType
        {
            get
            {
                return Encoding.ASCII.GetString(FType);
            }
            set
            {
                byte[] str = Encoding.ASCII.GetBytes(value);
                if (value.Length != 3)
                    throw new InvalidDataException("Image type must be exactly 3 bytes!");
                for (int i = 0; i < 3; i++)
                {
                    FType[i] = str[i];
                }
            }
        }
        //0-3
        internal byte[] Magic = new Byte[4]; //Must be 0xCDABCDAB
        //4-7
        internal byte[] FlashAddr = new Byte[4]; //Bytes need to be read in reverse.
        //8-11
        internal byte[] unkBlock1 = new Byte[4];
        //12-15
        internal byte[] Model = new Byte[4]; // Convers directly to ASCII string.

        internal byte[] unkBlock2 = new Byte[28];
        internal byte[] FType = new byte[3]; // Same as model.
        internal byte[] unkBlock3 = new Byte[17];
        internal byte[] unkSpecialBlock1 = new Byte[960]; // Special Block, See below.
        public byte[] rawData
        {
            get
            {
                byte[] block = new byte[1024];
                for (int i = 0; i < 4; i++)
                {
                    block[i] = Magic[i];
                }
                for (int i = 0; i < 4; i++)
                {
                    block[i + 4] = FlashAddr[i];
                }
                for (int i = 0; i < 4; i++)
                {
                    block[i + 8] = unkBlock1[i];
                }
                for (int i = 0; i < 4; i++)
                {
                    block[i + 12] = Model[i];
                }
                for (int i = 0; i < 28; i++)
                {
                    block[i + 16] = unkBlock2[i];
                }
                for (int i = 0; i < 3; i++)
                {
                    block[i + 44] = FType[i];
                }
                for (int i = 0; i < 17; i++)
                {
                    block[i + 47] = unkBlock3[i];
                }
                for (int i = 0; i < 960; i++)
                {
                    block[i + 64] = unkSpecialBlock1[i];
                }
                return block;
            }
        }
        //This block is only present in AMSS.BIN
        //It is all zeros in every other file.
    }

}
