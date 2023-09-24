using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InazumaElevenGoMapEventEditor.Tools;
using InazumaElevenGoMapEventEditor.Level5.Image;
using InazumaElevenGoMapEventEditor.Level5.Binary;
using InazumaElevenGoMapEventEditor.Level5.Mapenv;
using InazumaElevenGoMapEventEditor.InazumaElevenGo.Logic;

namespace InazumaElevenGoMapEventEditor.InazumaElevenGo.Games.IEGO
{
    public class IEGO : IGame
    {
        public FloatTree MapenvTree { get; set; }

        public Bitmap MapImage { get; set; }

        public List<NPC> NPCs { get; set; }

        public Dictionary<int, string> Players => Common.PlayerHeads.IEGO;

        public Dictionary<int, string> NPCN => Common.NPCHeads.IEGO;

        public Dictionary<int, string> NPCA => Common.NPCHeadsOther.IEGO;

        public Dictionary<int, int> PlayablePhase => Common.PlayablePhase.IEGO;

        public void OpenMap(string folderPathOpened)
        {
            string directoryName = Path.GetFileName(folderPathOpened);

            TalkInfo[] talkInfos = new TalkInfo[] { };
            TalkConfig[] talkConfigs = new TalkConfig[] { };

            if (File.Exists(folderPathOpened + "/" + directoryName + ".xi"))
            {
                MapImage = IMGC.ToBitmap(new FileStream(folderPathOpened + "/" + directoryName + ".xi", FileMode.Open));
            }

            if (File.Exists(folderPathOpened + "/" + directoryName + "_mapenv.bin"))
            {
                MapenvTree = Mapenv.MapenvToTree(new FileStream(folderPathOpened + "/" + directoryName + "_mapenv.bin", FileMode.Open));
            }

            if (File.Exists(folderPathOpened + "/" + directoryName + ".talk.bin"))
            {
                CfgBin talkFile = new CfgBin(new FileStream(folderPathOpened + "/" + directoryName + ".talk.bin", FileMode.Open));

                talkInfos = talkFile.Entries
                    .Where(x => x.GetName() == "TALK_INFO_BEGIN")
                    .SelectMany(x => x.Children)
                    .Select(x => new TalkInfo(x.Variables.Select(y => Convert.ToInt32(y.Value)).ToArray()))
                    .ToArray();

                talkConfigs = talkFile.Entries
                    .Where(x => x.GetName() == "TALK_CONFIG_BEGIN")
                    .SelectMany(x => x.Children)
                    .Select(x =>
                    {
                        var variableValues = x.Variables.Select(y =>
                        {
                            if (y.Value is Level5.Logic.OffsetTextPair)
                            {
                                Level5.Logic.OffsetTextPair offsetTextPair = y.Value as Level5.Logic.OffsetTextPair;
                                return offsetTextPair.Offset;
                            }
                            else
                            {
                                return Convert.ToInt32(y.Value);
                            }
                        }).ToArray();

                        return new TalkConfig(variableValues);
                    })
                    .ToArray();
            }

            if (File.Exists(folderPathOpened + "/" + directoryName + ".npc.bin"))
            {
                CfgBin npcFile = new CfgBin(new FileStream(folderPathOpened + "/" + directoryName + ".npc.bin", FileMode.Open));

                NPCBase[] npcBases = npcFile.Entries
                    .Where(x => x.GetName() == "NPC_BASE_BEGIN")
                    .SelectMany(x => x.Children)
                    .Select(x => new NPCBase(x.Variables.Select(y => Convert.ToInt32(y.Value)).ToArray()))
                    .ToArray();

                NPCPreset[] npcPresets = npcFile.Entries
                    .Where(x => x.GetName() == "NPC_PRESET_BEGIN")
                    .SelectMany(x => x.Children)
                    .Select(x => new NPCPreset(x.Variables.Select(y => Convert.ToInt32(y.Value)).ToArray()))
                    .ToArray();

                NPCAppear[] npcAppears = npcFile.Entries
                    .Where(x => x.GetName() == "NPC_APPEAR_BEGIN")
                    .SelectMany(x => x.Children)
                    .Select(x =>
                    {
                        var variableValues = x.Variables.Select(y =>
                        {
                            if (y.Value is Level5.Logic.OffsetTextPair)
                            {
                                Level5.Logic.OffsetTextPair offsetTextPair = y.Value as Level5.Logic.OffsetTextPair;
                                return offsetTextPair.Offset;
                            }
                            else
                            {
                                return Convert.ToInt32(y.Value);
                            }
                        }).ToArray();

                        return new NPCAppear(variableValues);
                    })
                    .ToArray();

                int[] boundaries = null;

                if (MapenvTree != null)
                {
                    boundaries = MapenvTree
                        .FindNode("Mapenv")
                        ?.Children
                        .FirstOrDefault()
                        ?.FindNode("MMModelPos")
                        ?.Children
                        .Select(node => Convert.ToInt32(node.Value))
                        .ToArray();
                }

                NPCs = npcBases.Select((npcBase, index) =>
                {
                    NPC newNPC = new NPC();

                    newNPC.Name = "NPC_" + index;
                    newNPC.Base = npcBase;

                    NPCPreset preset = npcPresets.FirstOrDefault(x => x.ID == npcBase.ID);

                    if (preset != null)
                    {
                        List<NPCAppear> positions = npcAppears.Skip(preset.NPCAppearStartOffset).Take(preset.NPCAppearCount).ToList();

                        foreach(NPCAppear position in positions)
                        {
                            if (MapenvTree != null)
                            {
                                Point trueLocation = Mapenv.CalculatePosition(boundaries, position.LocationX, position.LocationY, 256, 256);
                                position.LocationX = trueLocation.X;
                                position.LocationY = trueLocation.Y;
     
                            }
                        }

                        newNPC.Appears = positions;
                    } else
                    {
                        newNPC.Appears = new List<NPCAppear>();
                    }

                    TalkInfo talkInfo = talkInfos.FirstOrDefault(x => x.ID == npcBase.ID);

                    if (talkInfo != null)
                    {
                        List<TalkConfig> events = talkConfigs.Skip(talkInfo.EventStartIndex).Take(talkInfo.EventCount).ToList();

                        newNPC.Events = events.Select((x, i) =>
                         {
                             return new Event
                             {
                                 Name = "event_" + (talkInfo.EventStartIndex + i),
                                 Config = x,
                             };
                         })
                        .ToList();
                    } else
                    {
                        newNPC.Events = new List<Event>(); 
                    }

                    return newNPC;
                }).ToList();
            }
        }

        public Bitmap DrawMap()
        {
            Bitmap map = new Bitmap(MapImage); 

            foreach (NPC npc in NPCs)
            {
                if (npc.Appears != null)
                {
                    Image npcIcon = Image.FromStream(new ResourceReader("npc_icon_" + npc.Base.IconID + ".png").GetResourceStream());
                    map = Draw.DrawImage(map, (int)npc.Appears[0].LocationX, (int)npc.Appears[0].LocationY, npcIcon);
                }              
            }

            return map;
        }

        public Bitmap DrawMap(NPC selectedNpc)
        {
            Bitmap map = new Bitmap(MapImage);

            if (selectedNpc.Appears != null)
            {
                foreach (NPCAppear appear in selectedNpc.Appears)
                {
                    Image npcIcon = Image.FromStream(new ResourceReader("npc_icon_" + selectedNpc.Base.IconID + ".png").GetResourceStream());
                    map = Draw.DrawImage(map, (int)appear.LocationX, (int)appear.LocationY, npcIcon);
                }
            }

            return map;
        }
    }
}
