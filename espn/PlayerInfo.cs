using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using NBAFantasy.Models;
using NBAFantasy.YahooLeague;

namespace NBAFantasy
{
    public class PlayerInfo
    {
        public readonly long Id;
        public readonly string PlayerName;
        public string ImagePath, Team, Misc, Status;
        public List<GameStats> Games;
        public int Age, RaterPos;
        public int? YahooTeamNumber;
        public Dictionary<string, double> Scores;

        public static async Task<List<PlayerInfo>> GetPlayersInfoAsync(string[] names)
        {
            List<PlayerInfo> players = new List<PlayerInfo>();
            foreach (string name in names)
            {
                players.Add(await Task.Run(() => GetPlayerInfo(name)));
            }
            return players;
        }

        public static async Task<PlayerInfo> GetPlayerInfoAsync(string name)
        {
            return await Task.Run(() => GetPlayerInfo(name));
        }

        private static PlayerInfo GetPlayerInfo(string name)
        {
            var player = YahooDBMethods.AllPlayers.FirstOrDefault(p => p.Name.Equals(name));
            if (player == null) return null;
            return YahooDBMethods.PlayersGames.ContainsKey(name)
                ? new PlayerInfo(player, YahooDBMethods.PlayersGames[name])
                : new PlayerInfo(player);
        }

        public PlayerInfo(Players player, Games[] games = null) : 
            this(player, games?.Select(g => new GameStats(g)).ToArray() ?? Array.Empty<GameStats>())
        {
        }

        public PlayerInfo(Players player, GameStats[] games)
        {
            Id = player.Id;
            PlayerName = player.Name;
            ImagePath = ConfigurationManager.AppSettings["PlayerImagePath"] + Id + ".png&w=350&h=254";
            Team = player.Team;
            Misc = player.Misc;
            Age = player.Age.Value;
            Status = player.Status;
            YahooTeamNumber = player.TeamNumber;
            Games = games?.ToList();
        }

        //private List<GameStats> CreateGames(string playerStr, int year)
        //{
        //    var games = new List<GameStats>();
        //    int start = playerStr.IndexOf("Regular Season");
        //    if (start == -1) return games;
        //    int end = playerStr.IndexOf("Preseason");
        //    if (end == -1)
        //        end = playerStr.IndexOf("Data provided by Elias Sports Bureau");
        //    if (end == -1)
        //        end = playerStr.IndexOf("glossary");
        //    if (start == -1 || end == -1) return games;
        //    var gamesStr = playerStr.Substring(start, end - start);
        //    int index1 = gamesStr.IndexOf("<tr");
        //    if (index1 == -1) return games;
        //    int index2 = gamesStr.IndexOf("</tr>", index1) + "</tr>".Length;
        //    while (index1 != -1 && index2 != -1)
        //    {
        //        var gameStr = gamesStr.Substring(index1, index2 - index1);
        //        var game = new GameStats(gameStr, year);
        //        if (game.GameDate != default)
        //        {
        //            games.Add(game);
        //        }
        //        gamesStr = gamesStr.Remove(0, index2 - index1);
        //        index1 = gamesStr.IndexOf("<tr");
        //        if (index1 == -1) break;
        //        index2 = gamesStr.IndexOf("</tr>", index1) + "</tr>".Length;
        //    }

        //    return games;
        //}

        //private void UpdatePlayerInfo(string playerStr, int id)
        //{
        //    ImagePath = ConfigurationManager.AppSettings["PlayerImagePath"] + id + ".png&w=350&h=254";

        //    string pattern = "{\"app\"";
        //    string pattern2 = ";</script>";
        //    var i1 = playerStr.IndexOf(pattern);
        //    var i2 = playerStr.IndexOf(pattern2);
        //    if (i1 == -1) return;
        //    string jsonStr = playerStr.Substring(i1, i2 - i1);
        //    JObject json = JObject.Parse(jsonStr);
        //    JToken playerInfo = json["page"]["content"]["player"]["plyrHdr"]["ath"];
        //    Team = (playerInfo["tm"] ?? "").ToString();
        //    int.TryParse(playerInfo["dob"].ToString().Split("()".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last(), out Age);
        //    Misc = $"{playerInfo["pos"]} | {playerInfo["sts"]}";
        //}

        //private string DownloadPlayerStr(int id, int year)
        //{
        //    //var task = Utils.MakeAsyncRequest(ConfigurationManager.AppSettings["EspnPath"] + id + "/year/" + year);
        //    //return task.Result;
        //    using (var wc = new System.Net.WebClient())
        //        return wc.DownloadString(ConfigurationManager.AppSettings["EspnPath"] + id + "/year/" + year);
        //}

        //private string DownloadPlayerJson(long id, int year)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        using (HttpResponseMessage response = client.GetAsync(ConfigurationManager.AppSettings["EspnPath"] + id + "/type/nba/year/" + year).Result)
        //        {
        //            using (HttpContent content = response.Content)
        //            {
        //                return content.ReadAsStringAsync().Result;
        //            }
        //        }
        //    }
        //}

        //private void CreatePlayerGames(string gamesStr, int year)
        //{
        //    var lines = gamesStr.Split(new[] { @"<tr>", @"</tr>" }, StringSplitOptions.RemoveEmptyEntries).Where(line => line.Contains("oddrow team") || line.Contains("evenrow team"));
        //    var games = lines.Select(l => new GameStats(l, year));
        //    Games.AddRange(games);
        //    Games = Games.OrderBy(g => g.GameDate).ToList();
        //}

        //public static int GetPlayerId(string playerName)
        //{
        //    try
        //    {
        //        string uriString = @"https://www.bing.com/search?q=" + playerName + " espn";
        //        string[] searchPatterns = { @"www.espn.com/nba/player/_/id/", @"www.espn.com/nba/player/gamelog/_/id/" };
        //        string res = Utils.DownloadStringFromUrl(uriString);

        //        int i1 = -1;
        //        foreach (string searchPattern in searchPatterns)
        //        {
        //            i1 = res.IndexOf(searchPattern, StringComparison.Ordinal);
        //            if (i1 != -1)
        //            {
        //                i1 += searchPattern.Length;
        //                break;
        //            }
        //        }
        //        if (i1 == -1)
        //            return -1;

        //        int i2 = i1;
        //        while (Char.IsDigit(res[i2]))
        //            i2++;

        //        return int.Parse(res.Substring(i1, i2 - i1));
        //    }
        //    catch (Exception)
        //    {
        //        return -1;
        //    }
        //}

        public GameStats[] FilterGamesByYear(int year)
        {
            return Games.Where(g => g.GameDate > new DateTime(year - 1, 10, 1) & g.GameDate < new DateTime(year, 9, 1)).ToArray();
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
            var releventGames = Games.Where(g => g.GameDate > new DateTime(year - 1, 10, 1) & g.GameDate < new DateTime(year, 10, 1)).ToList();
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

        public GameStats[] FilterGamesWithoutOtherPlayer(GameStats[] originalGames, PlayerInfo otherPlayer, string year)
        {
            var otherPlayerGamesDates =
                otherPlayer.FilterGamesByYear(int.Parse(year)).Select(g => g.GameDate).ToList();

            return originalGames.Where(g => !otherPlayerGamesDates.Contains(g.GameDate)).ToArray();
        }

        public GameStats[] FilterGamesWithOtherPlayer(GameStats[] originalGames, PlayerInfo otherPlayer, string year)
        {
            var otherPlayerGamesDates =
                otherPlayer.FilterGamesByYear(int.Parse(year)).Select(g => g.GameDate).ToList();

            return originalGames.Where(g => otherPlayerGamesDates.Contains(g.GameDate)).ToArray();
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

    public enum PlayerStatus
    {
        Roster,
        Available,
        WatchList,
        Outliers
    }
}
