using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace InazumaElevenGoMapEventEditor.InazumaElevenGo.Map
{
    public class NPCSupport
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct NPCBASE
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x08)]
            public byte[] EmptyBlock1;
            public uint ID;
            public int HeadID;
            public int Visible;
            public int Unknown1;
            public int UniformID;
            public int BootID;
            public int GloveID;
            public int IconID;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct NPCPreset
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x08)]
            public byte[] EmptyBlock1;
            public uint ID;
            public int NPCAppearStartOffset;
            public int NPCAppearCount;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct NPCAppear
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x04)]
            public byte[] EmptyBlock1;
            public byte BlockCount;
            public byte BlockType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x06)]
            public byte[] EmptyBlock2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x04)]
            public byte[] LocationX;
            public uint EmptyBlock3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x04)]
            public byte[] LocationY;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x28)]
            public byte[] EmptyBlock4;

            private int GetDirectionLocation(byte[] bytes)
            {
                if (BlockType == 0x55)
                {
                    return System.BitConverter.ToInt32(bytes, 0);
                } else if  (BlockType == 0x66)
                {
                    return Convert.ToInt32(System.BitConverter.ToSingle(bytes, 0));
                } else
                {
                    return 0;
                }
            }

            public Point GetLocation()
            {
                return new Point(GetDirectionLocation(LocationX), GetDirectionLocation(LocationY));
            }
        }
    }
}
