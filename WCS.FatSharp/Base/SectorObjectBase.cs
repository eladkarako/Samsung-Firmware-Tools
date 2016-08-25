using System;
using System.Collections.Generic;
using System.Text;
using WCS.FatSharp.Structures;

namespace WCS.FatSharp.Base
{
    public abstract class MultiSectorObjectBase : FATObjectBase
    {
        private List<Sector> plSectors;
        protected internal Sector[] Sectors
        {
            get
            {
                return plSectors.ToArray();
            }
            set
            {
                plSectors.Clear();
                plSectors.AddRange(value);
            }
        }
        protected internal void SaveSectors()
        {
            for (int x = 0; x < plSectors.Count; x++)
            {
                plSectors[x].Save();
            }
        }
    }
}
