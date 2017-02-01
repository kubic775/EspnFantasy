using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace espn
{
    public class Player
    {
        public string PlayerName, ImagePath, Team, PlayerInfo;
        public int Id;
        public List<GameStats> Games;

        public Player(string playerName)
        {
            Games = new List<GameStats>();
            PlayerName = playerName;
            Id = PlayersList.Players[playerName];
            string playerStr = DownloadPlayerStr(Id);
            CreatePlayer(playerStr, Id);
        }

        public void CreatePlayer(string playerStr, int id)
        {
            Games = new List<GameStats>();
            ImagePath = ConfigurationManager.AppSettings["PlayerImagePath"] + id + ".png&w=350&h=254";

            int index1 = playerStr.IndexOf("Game By Game Stats and Performance", StringComparison.InvariantCulture);
            int index2 = playerStr.IndexOf("ESPN</title>", index1, StringComparison.InvariantCulture);
            Team = playerStr.Substring(index1 + 34, index2 - index1 - 34).Trim().Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).First().Trim();

            index1 = playerStr.IndexOf("general-info", StringComparison.InvariantCulture);
            index1 = playerStr.IndexOf("first", index1, StringComparison.InvariantCulture);
            index2 = playerStr.IndexOf("last", index1, StringComparison.InvariantCulture);
            var playerInfo = playerStr.Substring(index1 + 6, index2 - index1 - 10).Split(new[] { '>', '<' }, StringSplitOptions.RemoveEmptyEntries);
            PlayerInfo = playerInfo[0] + " | " + playerInfo[3];

            index1 = playerStr.IndexOf("2016-2017 REGULAR SEASON GAME LOG", StringComparison.InvariantCulture);
            index2 = playerStr.IndexOf("REGULAR SEASON STATS", StringComparison.InvariantCulture);
            if (index1 != -1)
            {
                string gamesStr = playerStr.Substring(index1, index2 - index1);
                CreatePlayerGames(gamesStr);
            }
        }

        private string DownloadPlayerStr(int id)
        {
            var task = MakeAsyncRequest(ConfigurationManager.AppSettings["EspnPath"] + id);
            return task.Result;
        }

        public static Task<string> MakeAsyncRequest(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Http.Get;
            request.Timeout = 20000;

            Task<WebResponse> task = Task.Factory.FromAsync(
                request.BeginGetResponse,
                asyncResult => request.EndGetResponse(asyncResult), null);

            return task.ContinueWith(t => ReadStreamFromResponse(t.Result));
        }

        private static string ReadStreamFromResponse(WebResponse response)
        {
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader sr = new StreamReader(responseStream))
            {
                //Need to return this response 
                string strContent = sr.ReadToEnd();
                return strContent;
            }
        }

        public void CreatePlayerGames(string gamesStr)
        {
            string[] lines = gamesStr.Split(new[] { @"<tr>", @"</tr>" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var statLine in lines.Where(line => line.Contains("oddrow team") || line.Contains("evenrow team")))
            {
                try
                {
                    var gameStats = new GameStats(statLine);
                    Games.Add(gameStats);
                }
                catch (Exception ex)
                {
                }
            }
        }

        public GameStats GetAvgStats(int numOfGames)
        {
            var fieldNames = typeof(GameStats).GetFields();
            var games = Games.Where(g => g.Min > 0).Take(numOfGames).ToArray();
            GameStats stats = new GameStats();

            foreach (var field in fieldNames)
            {
                field.SetValue(stats, games.Average(g => double.Parse(field.GetValue(g).ToString())));
            }

            return stats;
        }
    }
}
