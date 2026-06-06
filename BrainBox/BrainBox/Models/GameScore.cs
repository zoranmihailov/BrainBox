using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainBox.Models
{
    public class GameScore
    {
        public int HighScore { get; set; }
        public double Average { get; set; }
        public int GamesPlayed { get; set; }
        public int MaxScore { get; set; }

        public void UpdateScore(int newScore)
        {
            if (newScore > HighScore) HighScore = newScore;
            Average = ((Average * GamesPlayed) + newScore) / (GamesPlayed + 1);
            GamesPlayed++;
        }
    }
}
