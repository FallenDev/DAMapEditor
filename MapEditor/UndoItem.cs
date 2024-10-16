using System;
using System.Collections.Generic;
using System.Text;

namespace MapEditor
{
    public class UndoItem
    {
        public bool bg;
        public bool fgl;
        public bool fgr;
        public int width;
        public int x;
        public int y;
        // what was there before.
        public ushort[] bgData;
        public ushort[] fglData;
        public ushort[] fgrData;
    }
}
