using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace espn
{
    public static class PlayersList
    {
        public static SortedDictionary<string, int> Players = new SortedDictionary<string, int>();

        public static Dictionary<string, Player> CachePlayers = new Dictionary<string, Player>();

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

        public static void UpdatePlayersIds(string[] playerNames)
        {
            int[] ids = playerNames.AsParallel().Select(Player.GetPlayerId).ToArray();
            for (int i = 0; i < playerNames.Length; i++)
            {
                if (ids[i] != -1)
                    AddNewPlayer(playerNames[i], ids[i], false);
            }
            WritePlayerListToFile();
        }

        public static async Task<List<Player>> CreatePlayersAsync(string[] playerNames)
        {
            Task<List<Player>> t = Task.Run(() => playerNames.AsParallel().Select(CachePlayer).ToList());
            return await t;
        }

        public static async Task<Player> CreatePlayerAsync(string playerName)
        {
            Task<Player> t = Task.Run(() => CachePlayer(playerName));
            return await t;
        }

        private static Player CachePlayer(string playerName)
        {
            if (!CachePlayers.ContainsKey(playerName))
            {
                CachePlayers.Add(playerName, new Player(playerName));
            }
            return CachePlayers[playerName];
        }
    }
}
