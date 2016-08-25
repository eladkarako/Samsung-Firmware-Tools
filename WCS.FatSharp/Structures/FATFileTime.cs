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

namespace WCS.FatSharp.Structures
{
    class FATFileDate
    {
        //0x00, 2
        //From ushort
        //15-9 Year (0-1980, 127=2107)
        byte year;
        //8-5 = Month (1- jan, 12 - dec)
        byte month;
        //4-0 = Day (1-31)
        byte day;
    }
    class FATFileTime
    {
        //0x00, 2
        //From ushort
        //15-11 == hour
        byte hour;
        //10-5 == minute
        byte minute;
        //4-0 == Second/2
        byte second;
    }
}
