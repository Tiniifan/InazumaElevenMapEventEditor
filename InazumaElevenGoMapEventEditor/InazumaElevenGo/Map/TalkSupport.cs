using System.Runtime.InteropServices;

namespace InazumaElevenGoMapEventEditor.InazumaElevenGo.Map
{
    public class TalkSupport
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TalkInfo
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x08)]
            public byte[] EmptyBlock1;
            public uint NPCID;
            public int EventStartIndex;
            public int EventCount;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TalkConfig
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x08)]
            public byte[] EmptyBlock1;
            public int EventType;
            public int EventID;
            public int Unknown1;
            public int Unknown2;
        }
    }
}
