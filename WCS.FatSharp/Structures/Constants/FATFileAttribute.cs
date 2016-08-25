using System;
using System.Collections.Generic;
using System.Text;

namespace WCS.FatSharp.Structures.Constants
{
    public static class FAT_ATTRIBUTES
    {
        public const byte RDONLY = 0x01;
        public const byte HIDDEN = 0x02;
        public const byte SYSTEM = 0x04;
        public const byte VOLLBL = 0x08;
        public const byte DIRENT = 0x10;
        public const byte ARCHIV = 0x20;
        public const byte DEVICE = 0x40;
        public const byte UNUSED = 0x80;
    }
}
