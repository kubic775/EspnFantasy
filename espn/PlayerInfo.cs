using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace espn
{
    public class PlayerInfo
    {
        public readonly string PlayerName;
        public string ImagePath, Team, Misc;
        public List<GameStats> Games;
        public readonly int Id;
        public int Age, Type, RaterPos;
        public Dictionary<string, double> Scores;

        public PlayerInfo(string playerName, int id, int startYear = 2015, LogDelegate log = null)
        {
            try
            {
                //Console.WriteLine(playerName);
                //log?.Invoke("Download " + playerName);
                Games = new List<GameStats>();
                PlayerName = playerName;
                Id = id;
                for (int year = startYear; year <= Utils.GetCurrentYear() + 1; year++)
                {
                    //string playerStr = DownloadPlayerStr(Id, year);
                    string playerStr = DownloadPlayerJson(Id, year);
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

        private void CreatePlayer(string playerStr, int id, int year)
        {
            UpdatePlayerInfo(playerStr, id);
            Games.AddRange(CreateGames(playerStr, year));
        }

        private List<GameStats> CreateGames(string playerStr, int year)
        {
            var games = new List<GameStats>();
            int start = playerStr.IndexOf("Regular Season");
            if (start == -1) return games;
            int end = playerStr.IndexOf("Preseason");
            if (end == -1)
                end = playerStr.IndexOf("Data provided by Elias Sports Bureau");
            if (start == -1 || end == -1) return games;
            var gamesStr = playerStr.Substring(start, end - start);
            int index1 = gamesStr.IndexOf("<tr");
            if (index1 == -1) return games;
            int index2 = gamesStr.IndexOf("</tr>", index1) + "</tr>".Length;
            while (index1 != -1 && index2 != -1)
            {
                var gameStr = gamesStr.Substring(index1, index2 - index1);
                var game = new GameStats(gameStr, year);
                if (game.GameDate != default)
                {
                    games.Add(game);
                }
                gamesStr = gamesStr.Remove(0, index2 - index1);
                index1 = gamesStr.IndexOf("<tr");
                if (index1 == -1) break;
                index2 = gamesStr.IndexOf("</tr>", index1) + "</tr>".Length;
            }

            return games;
        }

        private void UpdatePlayerInfo(string playerStr, int id)
        {
            ImagePath = ConfigurationManager.AppSettings["PlayerImagePath"] + id + ".png&w=350&h=254";

            string pattern = "<script type='text/javascript' >";
            string pattern2 = ";</script>";
            var i1 = playerStr.IndexOf(pattern);
            var i2 = playerStr.IndexOf(pattern, i1 + 30);
            var i3 = playerStr.IndexOf(pattern, i2 + 30);
            if (i1 == -1) return;
            string jsonStr = playerStr.Substring(i2 + 55, i3 - i2 - 70).TrimEnd().Replace(pattern2, "");
            JObject json = JObject.Parse(jsonStr);
            JToken playerInfo = json["page"]["content"]["player"]["plyrHdr"]["ath"];
            Team = (playerInfo["tm"] ?? "").ToString();
            int.TryParse(playerInfo["dob"].ToString().Split("()".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last(), out Age);
            Misc = $"{playerInfo["pos"]} | {playerInfo["sts"]}";
        }

        private string DownloadPlayerStr(int id, int year)
        {
            //var task = Utils.MakeAsyncRequest(ConfigurationManager.AppSettings["EspnPath"] + id + "/year/" + year);
            //return task.Result;
            using (var wc = new System.Net.WebClient())
                return wc.DownloadString(ConfigurationManager.AppSettings["EspnPath"] + id + "/year/" + year);
        }

        private string DownloadPlayerJson(int id, int year)
        {
            using (var client = new HttpClient())
            {
                using (HttpResponseMessage response = client.GetAsync(ConfigurationManager.AppSettings["EspnPath"] + id + "/type/nba/year/" + year).Result)
                {
                    using (HttpContent content = response.Content)
                    {
                        return content.ReadAsStringAsync().Result;
                    }
                }
            }
        }

        private void CreatePlayerGames(string gamesStr, int year)
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

        public IEnumerable<GameStats> FilterGamesByDates(IEnumerable<DateTime> dates)
        {
            return Games.Where(g => dates.Contains(g.GameDate));
        }

        public GameStats GetAvgGame()
        {
            return GameStats.GetAvgStats(Games.ToArray());
        }

        public override string ToString()
        {
            return PlayerName + "," + GameStats.GetAvgStats(Games.ToArray());
        }

        public string ToShortString()
        {
            return PlayerName + "," + GameStats.GetAvgStats(Games.ToArray()).ToShortString();
        }

        public string GetFullHistory(int startYear, int endYear)
        {
            string history = String.Empty;

            for (int year = startYear; year <= endYear; year++)
            {
                var avgGame = GameStats.GetAvgStats(FilterGames(year, "Games", 82));
                if (avgGame.Gp != 0)
                    history += $"{PlayerName}, {year}:{Environment.NewLine}{avgGame}{Environment.NewLine + Environment.NewLine}";
            }

            return history;
        }

        public bool IsOutlier()
        {
            if (!Games.Any()) return false;

            var lastGames = Games.OrderByDescending(g => g.GameDate).Take(7);
            var avgMin = lastGames.Average(g => g.Min);
            var avgPts = lastGames.Average(g => g.Pts);


            return false;
        }
    }
}
