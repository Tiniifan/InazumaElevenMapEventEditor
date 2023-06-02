using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InazumaElevenGoMapEventEditor.Tool;

namespace InazumaElevenGoMapEventEditor.Level5.Binary
{
    public class Binary
    {
        public Stream BaseStream;

        public VirtualDirectory Directory;

        public BinarySupport.Header Header;

        public Binary(Stream stream)
        {
            BaseStream = stream;
            Directory = Open();
        }

        public VirtualDirectory Open()
        {
            VirtualDirectory folder = new VirtualDirectory();

            BinaryDataReader data = new BinaryDataReader(BaseStream);
            var header = data.ReadStruct<BinarySupport.Header>();
            Header = header;

            int tablePos = header.StartText + header.LengthText;
            int remainder = tablePos % 16;
            if (remainder != 0)
            {
                tablePos += 16 - remainder;
            }

            data.Seek( (uint)tablePos);
            var entryHeader = data.ReadStruct<BinarySupport.EntryHeader>();

            // Get entrie hash
            uint[] entriesHash = new uint[entryHeader.Count];
            for (int i = 0; i < entryHeader.Count; i++)
            {
                entriesHash[i] = data.ReadStruct<uint>();
                data.Skip(0x04);
            }

            data.Seek((uint)(tablePos + entryHeader.TextOffset));

            // Get entries name
            string[] entriesName = data.ReadMultipleSting(entryHeader.Count, Encoding.GetEncoding("shift-jis"));

            // Merge to one dictionnary
            Dictionary<uint, string> entries = entriesHash.Zip(entriesName, (k, v) => new { k, v }).Where(x => x.v.EndsWith("_BEGIN")).ToDictionary(x => x.k, x => x.v);
            Dictionary<uint, string> entriesClose = entriesHash.Zip(entriesName, (k, v) => new { k, v }).Where(x => x.v.EndsWith("_END")).ToDictionary(x => x.k, x => x.v);

            for (int i= 0; i < entries.Count; i++)
            {
                data.SeekOf(entries.ElementAt(i).Key, 0x10);
                uint hash = data.ReadStruct<uint>();
                string folderName = entries[hash].Replace("_BEGIN", "");
                VirtualDirectory newFolder = new VirtualDirectory(folderName);

                data.Skip(0x04);
                int filesCount = data.ReadStruct<int>();
                long startPos = data.Position;

                data.SeekOf(entriesClose.ElementAt(i).Key, 0x10);
                long endPos = data.Position;
                int length = (int) (endPos - startPos) / filesCount;

                data.Seek((uint)startPos);
                for (int j = 0; j < filesCount;j++)
                {
                    newFolder.AddFile(folderName + "_" + j, new SubMemoryStream(data.BaseStream, data.Position, length));
                    data.Skip((uint)length);
                }

                folder.AddFolder(newFolder);
            }

            return folder;
        }
    }
}
