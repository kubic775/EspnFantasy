using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace espn.Models
{
    public static class YahooDBMethods
    {
        public static List<Players> AllPlayers;
        public static List<YahooTeams> YahooTeams;
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
            }
        }

        public static void Init()
        {
            using var db = new YahooDB();
            AllPlayers = db.Players.ToList();
            YahooTeams = db.YahooTeams.ToList();
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
            var player = AllPlayers.First(p => p.Name.Equals(name));
            player.Status = "WatchList";
            using var db = new YahooDB();
            var dbPlayer = db.Players.First(p => p.Name.Equals(name));
            dbPlayer.Status = "WatchList";
            db.SaveChanges();
        }
    }
}
