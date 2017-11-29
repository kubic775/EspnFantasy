using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace espn
{
    public static class PlayersList
    {
        public static SortedDictionary<string, int> Players = new SortedDictionary<string, int>();

        public static Dictionary<string, PlayerInfo> CachePlayers = new Dictionary<string, PlayerInfo>();

        public static void CreatePlayersList()
        {
            string[] players = File.ReadAllLines("Players.txt");
            foreach (var player in players)
            {
                var temp = player.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                Players.Add(temp[0].Trim(), int.Parse(temp[1]));
            }
        }

        public static void AddNewPlayer(string name, int id, bool updatePlayersFile = true)
        {
            if (!Players.ContainsKey(name) && !Players.ContainsValue(id))
            {
                Players.Add(name, id);
                if (updatePlayersFile)
                    WritePlayerListToFile();
            }
        }

        public static void WritePlayerListToFile()
        {
            File.WriteAllLines("Players.txt", Players.Select(p => p.Key + ";" + p.Value).ToArray());
        }

        public static void UpdatePlayersFromFile(string filePath)
        {
            UpdatePlayersIds(File.ReadAllLines(filePath));
        }

        public static void AddNewPlayer(string name)
        {
            try
            {
                int id = PlayerInfo.GetPlayerId(name);
                if (id == -1)
                    id = int.Parse(Microsoft.VisualBasic.Interaction.InputBox("Can't Found Player, Please Insert Id", "Add New Player", "Default", -1, -1));

                AddNewPlayer(name, id, true);
                MessageBox.Show("Done");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't Create New Player - " + ex.Message);
            }

        }

        public static void UpdatePlayersIds(string[] playerNames)
        {
            int[] ids = playerNames.AsParallel().Select(PlayerInfo.GetPlayerId).ToArray();
            for (int i = 0; i < playerNames.Length; i++)
            {
                if (ids[i] != -1)
                    AddNewPlayer(playerNames[i], ids[i], false);
            }
            WritePlayerListToFile();
        }

        public static async Task<List<PlayerInfo>> CreatePlayersAsync(string[] playerNames)
        {
            Task<List<PlayerInfo>> t = Task.Run(() => playerNames.AsParallel().Select(CachePlayer).ToList());
            return await t;
        }

        public static async Task<PlayerInfo> CreatePlayerAsync(string playerName)
        {
            Task<PlayerInfo> t = Task.Run(() => CachePlayer(playerName));
            return await t;
        }

        private static PlayerInfo CachePlayer(string playerName)
        {
            if (!CachePlayers.ContainsKey(playerName))
            {
                var player = new PlayerInfo(playerName);
                if (player.Games != null)
                    CachePlayers.Add(playerName, new PlayerInfo(playerName));
            }
            return CachePlayers[playerName];
        }
    }
}
