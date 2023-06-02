using System.Runtime.InteropServices;

namespace InazumaElevenGoMapEventEditor.Level5.Binary
{
    public static class BinarySupport
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
