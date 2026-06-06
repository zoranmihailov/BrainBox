using BrainBox.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BrainBox.Services
{
    public class ScoreManager
    {
        private static readonly string FilePath = "player_data.json";

        public static PlayerProfile Load()
        {
            if (!File.Exists(FilePath)) return new PlayerProfile { Name = "Player" };
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<PlayerProfile>(json)!;
        }

        public static void Save(PlayerProfile profile)
        {
            var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
    }
}
