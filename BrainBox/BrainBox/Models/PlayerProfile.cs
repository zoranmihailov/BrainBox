using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainBox.Models
{
    public class PlayerProfile
    {
        public string Name { get; set; }
        public Dictionary<string, GameScore> Scores { get; set; } = new()
        {
            { "Wordle",      new GameScore { MaxScore = 20 } },
            { "Matematika",  new GameScore { MaxScore = 30 } },
            { "Kombinacija", new GameScore { MaxScore = 20 } },
            { "Prasanja",    new GameScore { MaxScore = 50 } },
            { "Asocijacija", new GameScore { MaxScore = 40 } }
        };
    }
}
