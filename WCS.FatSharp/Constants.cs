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
using System;
using System.Collections.Generic;
using System.Text;

namespace WCS.FatSharp
{
    enum FatDiskType : byte
    {
        FixedDisk=0x80,
        Removable=0x00
    }
    enum MediaDescriptor : byte
    {
        Floppy35i144m = 0xF0,
        FixedDisk = 0xF8,
        Floppy35i720k = 0xF9,
        Floppy525i320k = 0xFA,
        Floppy35i640k = 0xFB,
        Floppy525i180k = 0xFC,
        Floppy525i360k = 0xFD,
        Floppy525i160k = 0xFE,
        Floppy525i320k40tps = 0xFF
    }
    [Flags]
    public enum FATFileAttribute : byte
    {
        ReadOnly = 0x01,
        Hidden = 0x02,
        System = 0x04,
        VolumeLabel = 0x08,
        SubDirectory = 0x10,
        Archive = 0x20,
        Device = 0x40,
        Unused = 0x80,
        LongFilename = 0x0F
    }

}
