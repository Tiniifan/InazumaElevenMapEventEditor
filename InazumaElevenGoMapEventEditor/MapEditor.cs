using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using InazumaElevenGoMapEventEditor.Tools;
using InazumaElevenGoMapEventEditor.InazumaElevenGo.Logic;
using InazumaElevenGoMapEventEditor.InazumaElevenGo.Games;
using InazumaElevenGoMapEventEditor.InazumaElevenGo.Games.IEGO;

namespace InazumaElevenGoMapEventEditor
{
    public partial class MapEditor : Form
    {
        private IGame Game;

        private NPC SelectedNPC;

        private TabPage[] NPCTabPages;

        public MapEditor()
        {
            InitializeComponent();

            Game = new IEGO();
            NPCTabPages = npcTypeTabControl.TabPages.Cast<TabPage>().ToArray();
            npcTypeTabControl.TabPages.Clear();
        }

        private void MapEditor_Load(object sender, EventArgs e)
        {
            headComboBox.DataSource = new BindingSource(Game.Players, null);
            headComboBox.ValueMember = "Key";
            headComboBox.DisplayMember = "Value";

            modelNpcComboBox.DataSource = new BindingSource(Game.NPCN, null);
            modelNpcComboBox.ValueMember = "Key";
            modelNpcComboBox.DisplayMember = "Value";

            modeOtherComboBox.DataSource = new BindingSource(Game.NPCA, null);
            modeOtherComboBox.ValueMember = "Key";
            modeOtherComboBox.DisplayMember = "Value";

            playablePhaseComboBox.DataSource = new BindingSource(Game.PlayablePhase, null);
            playablePhaseComboBox.ValueMember = "Key";
            playablePhaseComboBox.DisplayMember = "Value";

            NpcTypeTabControl_SelectedIndexChanged(0);
        }

        private string UIntToHexadecimal(int value, bool isLittleEndian)
        {
            byte[] valueBytes = System.BitConverter.GetBytes(value);

            if (isLittleEndian)
            {
                Array.Reverse(valueBytes);
            }

            uint bigEndianValue = System.BitConverter.ToUInt32(valueBytes, 0);

            return bigEndianValue.ToString("X8");
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Game.OpenMap(dialog.FileName);
                mapPictureBox.Image = Game.DrawMap();

                npcComboBox.Items.Clear();

                npcComboBox.Items.AddRange(Game.NPCs.ToArray());

                npcComboBox.SelectedIndex = 0;

                //npcPanel.Enabled = true;
            }
        }

        private void NpcComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (npcComboBox.SelectedIndex == -1) return;

            mapPictureBox.Image = Game.DrawMap();

            SelectedNPC = Game.NPCs[npcComboBox.SelectedIndex];
            npcIDTextBox.Text = UIntToHexadecimal(SelectedNPC.Base.ID, true);
            iconComboBox.SelectedIndex = SelectedNPC.Base.IconID;

            if (SelectedNPC.Base.Type == 1)
            {
                typeComboBox.SelectedIndex = 0;
                NpcTypeTabControl_SelectedIndexChanged(0);
                ModelComboBox_SelectedIndex(headComboBox, SelectedNPC.Base.HeadID);
                headComboBox.SelectedItem = SelectedNPC.Base.HeadID;
            } else if (SelectedNPC.Base.Type == 2)
            {
                typeComboBox.SelectedIndex = 1;
                NpcTypeTabControl_SelectedIndexChanged(1);
                ModelComboBox_SelectedIndex(modelNpcComboBox, SelectedNPC.Base.HeadID);
                modelNpcComboBox.SelectedItem = SelectedNPC.Base.HeadID;
            } else if (SelectedNPC.Base.Type == 3)
            {
                typeComboBox.SelectedIndex = 2;
                NpcTypeTabControl_SelectedIndexChanged(2);
                ModelComboBox_SelectedIndex(modeOtherComboBox, SelectedNPC.Base.HeadID);
                modeOtherComboBox.SelectedItem = SelectedNPC.Base.HeadID;
            }

            eventComboBox.SelectedIndex = -1;
            eventComboBox.Items.Clear();
            eventComboBox.Items.AddRange(SelectedNPC.Events.ToArray());

            if (eventComboBox.Items.Count > 0)
            {
                eventComboBox.SelectedIndex = 0;
            }

            if (SelectedNPC.Appears != null)
            {
                Image npcIcon = Image.FromStream(new ResourceReader("npc_icon_" + SelectedNPC.Base.IconID + "_selected.png").GetResourceStream());
                mapPictureBox.Image = Draw.DrawImage((Bitmap)mapPictureBox.Image, (int)SelectedNPC.Appears[0].LocationX, (int)SelectedNPC.Appears[0].LocationY, npcIcon);
            }

            npcComboBox.Focus();
        }

        private void NpcTypeTabControl_SelectedIndexChanged(int selectedIndex)
        {
            npcTypeTabControl.TabPages.Clear();
            npcTypeTabControl.TabPages.Add(NPCTabPages[selectedIndex]);
        }

        private void ModelComboBox_SelectedIndex(ComboBox combobox, int keyToFind)
        {
            BindingSource bindingSource = (combobox.DataSource as BindingSource);
            Dictionary<int, string> source = bindingSource.DataSource as Dictionary<int, string>;
            combobox.SelectedIndex = source.Keys.ToList().IndexOf(keyToFind);
        }

        private void EventComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (eventComboBox.SelectedIndex == -1) return;

            if (SelectedNPC.Appears != null)
            {
                NPCAppear npcAppear = null;
                Event npcEvent = SelectedNPC.Events[eventComboBox.SelectedIndex];             

                if (eventComboBox.SelectedIndex < SelectedNPC.Appears.Count)
                {
                    npcAppear = SelectedNPC.Appears[eventComboBox.SelectedIndex];
                } else
                {
                    npcAppear = SelectedNPC.Appears[SelectedNPC.Appears.Count - 1];
                }

                playablePhaseComboBox.SelectedIndex = npcAppear.PhaseAppear;
                eventTypeComboBox.SelectedIndex = npcEvent.Config.EventType - 1;

                if (eventTypeComboBox.SelectedIndex == 2)
                {
                    configTextBox.Text = npcEvent.Config.EventID.ToString();
                } else
                {
                    configTextBox.Text = npcEvent.Config.EventID.ToString("X8");
                }

                Image npcIcon = Image.FromStream(new ResourceReader("npc_icon_" + SelectedNPC.Base.IconID + "_selected.png").GetResourceStream());
                selectedNpcMapPictureBox.Image = Draw.DrawImage(Game.DrawMap(SelectedNPC), (int)npcAppear.LocationX, (int)npcAppear.LocationY, npcIcon);
            }
        }
    }
}
