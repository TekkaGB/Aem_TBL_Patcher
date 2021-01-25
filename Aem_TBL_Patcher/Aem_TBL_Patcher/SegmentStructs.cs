﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Aem_TBL_Patcher.Segments
{
    public struct Segment
    {
        public int EntrySize { get; }
        public string SegmentName { get; }

        public Segment(int s, string n)
        {
            EntrySize = s;
            SegmentName = n;
        }
    }

    public struct SegmentProps
    {
        public string Tbl { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }
        public int EntrySize { get; set; }
        public int OriginalOffset { get; set; }
        public int ModOffset { get; set; }
        public uint OriginalSize { get; set; }
        public uint ModSize { get; set; }

        /*
        public SegmentProps(string tbl, string name, int index, int originalOffset, int modOffset, uint originalSize, uint modSize, int entrySize)
        {
            Tbl = tbl;
            Name = name;
            Index = index;
            OriginalOffset = originalOffset;
            ModOffset = modOffset;
            OriginalSize = originalSize;
            ModSize = modSize;
            EntrySize = entrySize;
        }
        */
    }
}