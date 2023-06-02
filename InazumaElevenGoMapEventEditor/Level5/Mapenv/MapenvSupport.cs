using System.Runtime.InteropServices;

namespace InazumaElevenGoMapEventEditor.Level5.Mapenv
{
    public static class MapenvSupport
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            public int FileCount;
            public int StartText;
            public int LengthText;
            public int LineNumber;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct EntryHeader
        {
            public int Lenght;
            public int Count;
            public int TextOffset;
            public int TextLength;
        }
    }
}
