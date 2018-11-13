using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace espn
{
    public class PlayerInfo
    {
        public string PlayerName, ImagePath, Team, Misc;
        public List<GameStats> Games;
        public int Id, Age, Type;
        public Dictionary<string, double> Scores;

        public PlayerInfo(string playerName, int id, int startYear = 2014, LogDelegate log = null)
        {
            try
            {
                //Console.WriteLine(playerName);
                //log?.Invoke("Download " + playerName);
                Games = new List<GameStats>();
                PlayerName = playerName;
                Id = id;
                for (int year = startYear; year <= 2019; year++)
                {
                    string playerStr = DownloadPlayerStr(Id, year);
                    CreatePlayer(playerStr, Id, year);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't Create Player - " + playerName + ", " + ex.Message);
                Games = null;
            }
        }

        public PlayerInfo(Player player, Game[] games)
        {
            PlayerName = player.Name;
            Id = player.ID;
            ImagePath = ConfigurationManager.AppSettings["PlayerImagePath"] + Id + ".png&w=350&h=254";
            Team = player.Team;
            Misc = player.Misc;
            Age = player.Age ?? -1;
            Type = (int)(player.Type ?? 0);
            Games = games.Select(g => new GameStats(g)).ToList();
        }

        public void CreatePlayer(string playerStr, int id, int year)
        {
            ImagePath = ConfigurationManager.AppSettings["PlayerImagePath"] + id + ".png&w=350&h=254";

            int index1 = playerStr.IndexOf("Game By Game Stats and Performance", StringComparison.InvariantCulture);
            int index2 = playerStr.IndexOf("ESPN</title>", index1, StringComparison.InvariantCulture);
            Team = playerStr.Substring(index1 + 34, index2 - index1 - 34).Trim().Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();

            index1 = playerStr.IndexOf("general-info", StringComparison.InvariantCulture);
            index1 = playerStr.IndexOf("first", index1, StringComparison.InvariantCulture);
            index2 = playerStr.IndexOf("lbs", index1, StringComparison.InvariantCulture);
            var playerInfo = playerStr.Substring(index1 + 6, index2 - index1).Split(new[] { '>', '<' }, StringSplitOptions.RemoveEmptyEntries);
            Age = int.Parse(playerStr.Substring(playerStr.IndexOf("Age:", index1, StringComparison.InvariantCulture) + 5, 2));
            Misc = playerInfo[0] + " | " + playerInfo[3];

            index1 = playerStr.IndexOf((year - 1) + "-" + year + " REGULAR SEASON GAME LOG", StringComparison.InvariantCulture);
            index2 = playerStr.IndexOf("REGULAR SEASON STATS", StringComparison.InvariantCulture);
            if (index1 != -1 && index2 != -1)
            {
                string gamesStr = playerStr.Substring(index1, index2 - index1);
                CreatePlayerGames(gamesStr, year);
            }
        }

        private string DownloadPlayerStr(int id, int year)
        {
            var task = Utils.MakeAsyncRequest(ConfigurationManager.AppSettings["EspnPath"] + id + "/year/" + year);
            return task.Result;
        }

        public void CreatePlayerGames(string gamesStr, int year)
        {
            var lines = gamesStr.Split(new[] { @"<tr>", @"</tr>" }, StringSplitOptions.RemoveEmptyEntries).Where(line => line.Contains("oddrow team") || line.Contains("evenrow team"));
            var games = lines.Select(l => new GameStats(l, year));
            Games.AddRange(games);
            Games = Games.OrderBy(g => g.GameDate).ToList();
        }

        public static int GetPlayerId(string playerName)
        {
            try
            {
                string uriString = @"https://www.bing.com/search?q=" + playerName + " espn";
                string[] searchPatterns = { @"www.espn.com/nba/player/_/id/", @"www.espn.com/nba/player/gamelog/_/id/" };
                string res = Utils.DownloadStringFromUrl(uriString);

                int i1 = -1;
                foreach (string searchPattern in searchPatterns)
                {
                    i1 = res.IndexOf(searchPattern, StringComparison.Ordinal);
                    if (i1 != -1)
                    {
                        i1 += searchPattern.Length;
                        break;
                    }
                }
                if (i1 == -1)
                    return -1;

                int i2 = i1;
                while (Char.IsDigit(res[i2]))
                    i2++;

                return int.Parse(res.Substring(i1, i2 - i1));
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public GameStats[] FilterGamesByYear(int year)
        {
            return Games.Where(g => g.GameDate > new DateTime(year - 1, 10, 1) & g.GameDate < new DateTime(year, 5, 1)).ToArray();
        }

        public GameStats[] FilterGames(string year, string mode, string numOfGames, bool filterZeroMinutes = false, bool filterOutliers = false)
        {
            int num = numOfGames.Equals("Max") ? Games.Count : int.Parse(numOfGames);
            GameStats[] games = FilterGames(int.Parse(year), mode, num);

            if (filterZeroMinutes)
                games = games.Where(g => g.Min > 0).ToArray();

            if (filterOutliers)
                games = GameStats.FilterOutliers(games);

            return games;
        }

        public GameStats[] FilterGames(int year, string mode, int numOfGames)
        {
            var releventGames = Games.Where(g => g.GameDate > new DateTime(year - 1, 10, 1) & g.GameDate < new DateTime(year, 5, 1)).ToList();
            if (mode.Equals("Games"))
            {
                return releventGames.OrderByDescending(g => g.GameDate).Take(numOfGames).OrderBy(g => g.GameDate).ToArray();
            }
            else//Days
            {
                DateTime minDate = DateTime.Now - new TimeSpan(numOfGames, 0, 0, 0);
                return releventGames.Where(g => g.GameDate >= minDate).OrderBy(g => g.GameDate).ToArray();
            }
        }

        public GameStats[] FilterGamesByPlayerInjury(GameStats[] originalGames, string playerInjuredName)
        {
            List<Game> injuredGames;
            using (var db = new EspnEntities())
            {
                var playerInjured = db.Players.First(p => p.Name.Equals(playerInjuredName));
                injuredGames = db.Games.Where(g => g.PlayerId == playerInjured.ID).Where(g => g.Min < 10).ToList();
            }

            List<DateTime> injuredGamesDates = injuredGames.Select(g => g.GameDate.Value).ToList();
            return originalGames.Where(g => injuredGamesDates.Contains(g.GameDate)).ToArray();
        }

        public GameStats GetAvgGame()
        {
            return GameStats.GetAvgStats(Games.ToArray());
        }

        public override string ToString()
        {
            return PlayerName + "," + GameStats.GetAvgStats(Games.ToArray()).ToString();
        }
    }
}
