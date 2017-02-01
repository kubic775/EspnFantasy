using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace espn
{
    public static class PlayersList
    {
        public static SortedDictionary<string, int> Players = new SortedDictionary<string, int>();

        public static void CreatePlayersList()
        {
            string[] players = File.ReadAllLines("Players.txt");
            foreach (var player in players)
            {
                var temp = player.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                Players.Add(temp[0].Trim(), int.Parse(temp[1]));
            }
        }

        public static void AddNewPlayer(string name, int id)
        {
            if (!Players.ContainsKey(name))
            {
                Players.Add(name, id);
                WritePlayerListToFile();
            }
        }

        public static void WritePlayerListToFile()
        {
            File.WriteAllLines("Players.txt", Players.Select(p => p.Key + ";" + p.Value).ToArray());
        }
    }
}
