using System;
using System.Collections.Generic;

namespace EvoS.Framework.Network.Static
{
    [Serializable]
    [EvosMessage(533)]
    public class PlayerSkinData
    {
        public PlayerSkinData()
        {
            Patterns = new List<PlayerPatternData>();
        }

        public bool Unlocked { get; set; }

        public PlayerPatternData GetPattern(int patternID)
        {
            while (Patterns.Count <= patternID)
            {
                Patterns.Add(new PlayerPatternData() { Unlocked = true });
            }

            return Patterns[patternID];
        }

        [EvosMessage(534)] public List<PlayerPatternData> Patterns { get; set; }

    }
}
