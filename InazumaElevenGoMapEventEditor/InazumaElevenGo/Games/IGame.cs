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

namespace InazumaElevenGoMapEventEditor.InazumaElevenGo.Games
{
    public interface IGame
    {
        FloatTree MapenvTree { get; set; }

        Bitmap MapImage { get; set; }

        List<NPC> NPCs { get; set; }

        Dictionary<int, string> Players { get;}

        Dictionary<int, string> NPCN { get; }

        Dictionary<int, string> NPCA { get; }

        Dictionary<int, int> PlayablePhase { get; }

        void OpenMap(string folderPathOpened);

        Bitmap DrawMap();

        Bitmap DrawMap(NPC selectedNpc);
    }
}
