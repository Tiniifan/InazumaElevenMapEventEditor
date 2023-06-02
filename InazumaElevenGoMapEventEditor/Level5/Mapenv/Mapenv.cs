using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InazumaElevenGoMapEventEditor.Tool;
using System.Drawing;

namespace InazumaElevenGoMapEventEditor.Level5.Mapenv
{
    public static class Mapenv
    {
        public static Point CalculatePosition(int[] boundaries, int pointX, int pointY, int mapWidth, int mapHeight)
        {
            int minX = boundaries[0];
            int minY = boundaries[1];
            int maxX = boundaries[2];
            int maxY = boundaries[3];

            int rangeX = maxX - minX;
            int rangeY = maxY - minY;

            double scaleX = (double)mapWidth / rangeX;
            double scaleY = (double)mapHeight / rangeY;

            int mapX = (int)((pointX - minX) * scaleX);
            int mapY = (int)((pointY - minY) * scaleY);

            return new Point (mapX, mapY);
        }

        public static FloatTree MapenvToTree(Stream inputStream)
        {
            FloatTree root = new FloatTree("Mapenv");
            FloatTree currentNode = root;

            using (BinaryDataReader data = new BinaryDataReader(inputStream))
            {
                var header = data.ReadStruct<MapenvSupport.Header>();

                data.Seek((uint)(header.StartText + header.LengthText));
                var entriesHeader = data.ReadStruct<MapenvSupport.EntryHeader>();

                // Get tag name according to CRC32
                Dictionary<uint, string> tags = new Dictionary<uint, string>();
                using (BinaryDataReader crc32Reader = new BinaryDataReader(data.GetSection(entriesHeader.TextOffset - 0x10)))
                {
                    using (BinaryDataReader textReader = new BinaryDataReader(data.GetSection(entriesHeader.TextLength)))
                    {
                        for (int i = 0; i < entriesHeader.Count; i++)
                        {
                            tags.Add(crc32Reader.ReadStruct<uint>(), textReader.ReadString(Encoding.GetEncoding("shift-jis")));
                            crc32Reader.Skip(0x04);
                        }
                    }
                }

                data.Seek(0x10);

                using (BinaryDataReader treeReader = new BinaryDataReader(data.GetSection(header.StartText - 0x10)))
                {
                    using (BinaryDataReader textReader = new BinaryDataReader(data.GetSection(header.LengthText)))
                    {
                        for (int i = 0; i < header.FileCount; i++)
                        {
                            uint currentTag = treeReader.ReadValue<uint>();
                            byte[] currentTagConfig = treeReader.GetSection(4);
                            int tagSize = currentTagConfig[0];
                            int typeSize = currentTagConfig[1];

                            if (currentTag == 0x5EDE54A2)
                            {
                                // PTREE
                                if (tagSize == 2)
                                {
                                    treeReader.Skip(0x04);
                                    uint textOffsetValue = treeReader.ReadValue<uint>();

                                    textReader.Seek(textOffsetValue);
                                    string treeText = textReader.ReadString(Encoding.GetEncoding("shift-jis"));
                                    FloatTree childNode = new FloatTree(treeText);
                                    currentNode.AddChild(childNode);
                                    currentNode = childNode;
                                }
                                else
                                {
                                    uint textOffsetValue = treeReader.ReadValue<uint>();

                                    textReader.Seek(textOffsetValue);
                                    string treeText = textReader.ReadString(Encoding.GetEncoding("shift-jis"));
                                    FloatTree childNode = new FloatTree(treeText, 0.0f);
                                    currentNode.AddChild(childNode);
                                    currentNode = childNode;
                                }
                            }
                            else if (currentTag == 0x446781DE)
                            {
                                // PTVAL
                                float value = 0;

                                if (typeSize == 0)
                                {
                                    value = treeReader.ReadValue<int>();
                                }
                                else if (typeSize == 1)
                                {
                                    value = treeReader.ReadValue<int>();
                                }
                                else if (typeSize == 2)
                                {
                                    value = treeReader.ReadValue<float>();
                                }

                                if (tagSize == 2)
                                {
                                    uint textOffsetValue = treeReader.ReadValue<uint>();

                                    textReader.Seek(textOffsetValue);
                                    string treeText = textReader.ReadString(Encoding.GetEncoding("shift-jis"));
                                    FloatTree valueNode = new FloatTree(treeText, value);
                                    currentNode.AddChild(valueNode);
                                }
                                else
                                {
                                    FloatTree valueNode = new FloatTree(value);
                                    currentNode.AddChild(valueNode);
                                }
                            }
                            else if (currentTag == 0xD4E6B83E)
                            {
                                // _PTREE
                                currentNode = currentNode.Parent;

                                bool matchFound = false;

                                while (treeReader.Position < treeReader.Length && !matchFound)
                                {
                                    uint chunk = treeReader.ReadValue<uint>();

                                    foreach (uint tag in tags.Keys)
                                    {
                                        if (tag == chunk)
                                        {
                                            treeReader.Seek((uint)treeReader.Position - 4);
                                            matchFound = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        return root;
                    }
                }
            }
        }
    }
}
