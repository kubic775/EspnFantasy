using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using espn;
using NBAFantasy.Models;

namespace NBAFantasy.YahooLeague
{
    public static class YahooDBMethods
    {
        public static List<Players> AllPlayers;
        public static List<YahooTeams> YahooTeams;
        public static Dictionary<int, List<YahooTeamStats>> YahooTeamsStats;
        public static List<LeagueTeams> NbaTeams;
        public static Dictionary<string, Games[]> PlayersGames;
        public static DateTime LastUpdateTime;

        public static void LoadDataFromDB()
        {
            try
            {
                using var db = new YahooDB();
                if (db.GlobalParams.First().LastUpdateTime > LastUpdateTime)
                    Init();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                MessageBox.Show(e.Message);
            }
        }

        private static void Init()
        {
            using var db = new YahooDB();
            AllPlayers = db.Players.ToList();
            YahooTeams = db.YahooTeams.ToList();
            YahooTeamsStats = db.YahooTeamStats.AsEnumerable().GroupBy(t => t.YahooTeamId)
                .ToDictionary(key => key.Key.Value, val => val.ToList());
            NbaTeams = db.LeagueTeams.ToList();
            PlayersGames = db.Games.AsEnumerable().GroupBy(g => g.PlayerId)
                .ToDictionary(key => AllPlayers.First(p => p.Id == key.Key).Name, val => val.ToArray());
            LastUpdateTime = db.GlobalParams.First().LastUpdateTime;

            var currentGames = PlayersGames.Values.SelectMany(g => g)
                .Where(g => g.GameDate > new DateTime(Utils.GetCurrentYear(), 10, 1)).ToList();
            //MainForm.PlayerRater = currentGames.Any() ? new Rater(players, currentGames) : null;
            MainForm.PlayerRater = new Rater(AllPlayers, currentGames);
        }

        public static Players[] GetYahooTeamPlayers(string teamName)
        {
            var id = YahooTeams.First(t => t.TeamName.Equals(teamName)).TeamId;
            return AllPlayers.Where(p => p.TeamNumber == id).ToArray();
        }

        public static Players[] GetPlayersByStatus(string status)
        {
            return AllPlayers.Where(p => p.Status.Equals(status)).ToArray();
        }

        public static void AddPlayerToWatchList(string name)
        {
            using var db = new YahooDB();
            var dbPlayer = db.Players.First(p => p.Name.Equals(name));
            var player = AllPlayers.First(p => p.Name.Equals(name));

            player.Status = !string.IsNullOrEmpty(player.Status) && player.Status.Equals("WatchList") ? null : "WatchList";
            dbPlayer.Status = !string.IsNullOrEmpty(dbPlayer.Status) && dbPlayer.Status.Equals("WatchList") ? null : "WatchList";
            db.SaveChanges();
        }
    }
}
