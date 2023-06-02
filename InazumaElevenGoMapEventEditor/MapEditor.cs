using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using InazumaElevenGoMapEventEditor.Tool;
using InazumaElevenGoMapEventEditor.Level5.Image;
using InazumaElevenGoMapEventEditor.Level5.Binary;
using InazumaElevenGoMapEventEditor.Level5.Mapenv;
using InazumaElevenGoMapEventEditor.InazumaElevenGo.Logic;
using InazumaElevenGoMapEventEditor.InazumaElevenGo.Map;

namespace InazumaElevenGoMapEventEditor
{
    public partial class MapEditor : Form
    {
        private string FolderPathOpened;

        private bool FolderPathHasMMM;

        private Binary NpcFile;

        private bool FolderPathHasTalk;

        private FloatTree MapenvTree;

        private bool FolderPathHasTreasureBox;

        private bool FolderPathrHasTrapt;

        private Bitmap MapImage;

        private List<NPC> NPCs;

        public MapEditor()
        {
            InitializeComponent();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                FolderPathOpened = dialog.FileName;
                string directoryName = Path.GetFileName(FolderPathOpened);

                if (File.Exists(FolderPathOpened + "/" + directoryName + ".xi"))
                {
                    MapImage = IMGC.ToBitmap(new FileStream(FolderPathOpened + "/" + directoryName + ".xi", FileMode.Open));
                    mapPictureBox.Image = MapImage;
                }

                if (File.Exists(FolderPathOpened + "/" + directoryName + "_mapenv.bin"))
                {
                    MapenvTree = Mapenv.MapenvToTree(new FileStream(FolderPathOpened + "/" + directoryName + "_mapenv.bin", FileMode.Open));
                }

                NPCs = new List<NPC>();
                Image npcIcon = Image.FromStream(new ResourceReader("npc_icon.png").GetResourceStream());

                if (File.Exists(FolderPathOpened + "/" + directoryName + ".npc.bin"))
                {
                    NpcFile = new Binary(new FileStream(FolderPathOpened + "/" + directoryName + ".npc.bin", FileMode.Open));
                    foreach (KeyValuePair<string, SubMemoryStream> file in NpcFile.Directory.GetFolder("NPC_APPEAR").Files)
                    {
                        file.Value.Read();
                        using (BinaryDataReader npcAppearReader = new BinaryDataReader(file.Value.ByteContent))
                        {
                            NPCSupport.NPCAppear npcAppearData = npcAppearReader.ReadStruct<NPCSupport.NPCAppear>();
                            Point npcLocation = npcAppearData.GetLocation();

                            if (MapenvTree != null)
                            {
                                int[] boundaries = MapenvTree
                                    .FindNode("Mapenv")
                                    ?.Children
                                    .FirstOrDefault()
                                    ?.FindNode("MMModelPos")
                                    ?.Children
                                    .Select(node => Convert.ToInt32(node.Value))
                                    .ToArray();

                                Console.WriteLine(npcLocation);
                                npcLocation = Mapenv.CalculatePosition(boundaries, npcLocation.X, npcLocation.Y, 256, 256);;
                            }

                            NPC newNPC = new NPC();
                            newNPC.Location = npcLocation;
                            NPCs.Add(newNPC);
                        }
                    }
                }

                for (int i = 0; i < NPCs.Count; i++)
                {
                    NPC npc = NPCs[i];
                    Console.WriteLine(i + " (" + npc.Location.X + ", " + npc.Location.Y + ")");
                    mapPictureBox.Image = Draw.DrawImage((Bitmap)mapPictureBox.Image, npc.Location.X, npc.Location.Y, npcIcon);
                }
            }
        }
    }
}
