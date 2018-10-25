using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace espn
{
    public class Rater
    {
        //Average, Median, Std
        public Dictionary<string, double> Pts, Reb, Ast, Tpm, Stl, Blk, To, Fta, FtPer, Fga, FgPer;

        public IEnumerable<Game> Games;
        public IEnumerable<Player> Players;

        public Rater(IEnumerable<Player> players, IEnumerable<Game> games)
        {
            Players = players;
            Games = games;
        }


        public IEnumerable<PlayerInfo> CreateRater(CalcScoreType mode, int timePeriod = 0)
        {
            var rater = new List<PlayerInfo>();

            DateTime startTime = timePeriod == 0 ? default(DateTime) : DateTime.Today - new TimeSpan(timePeriod, 0, 0, 0);

            UpdateFactors(mode, startTime);

            Dictionary<int, List<Game>> playersGames = Games.Where(g => g.GameDate >= startTime).GroupBy(g => g.PlayerId).ToDictionary(k => k.Key.Value, v => v.ToList());

            foreach (var player in playersGames)
            {
                var games = player.Value.Select(g => new GameStats(g));
                var scores = CalcScores(games, mode, timePeriod, false);
                var avgGame = GameStats.GetAvgStats(games.ToArray());
                avgGame.Score = scores["Score"];
                var playerT = new PlayerInfo(Players.First(p => p.ID == player.Key),
                    new[] { new Game(avgGame, -1, player.Key) })
                { Scores = scores };
                rater.Add(playerT);
            }
            return rater.OrderByDescending(g => g.Scores["Score"]);
        }

        /// <summary>
        ///     Calc Player Score For Selected Games
        ///</summary>
        /// <param name="mode">Games For Calc Score By Avg, Or Days For Calc Score By Time</param>
        /// <param name="timePeriod">Calc Score For The Last 'timePeriod' Days, Else No Relevant </param>
        /// <param name="updateFactors">True For Update Factors Before Calc Player Score </param>
        public double CalcScore(IEnumerable<GameStats> games, CalcScoreType mode, int timePeriod, bool updateFactors = true)//Games Of Single Player
        {
            Dictionary<string, double> scores = CalcScores(games, mode, timePeriod, updateFactors);
            return scores?["Score"] ?? -1;
        }

        public Dictionary<string, double> CalcScores(IEnumerable<GameStats> games, CalcScoreType mode, int timePeriod, bool updateFactors = true)
        {
            try
            {
                DateTime startTime = timePeriod == 0 ? default(DateTime) : DateTime.Today - new TimeSpan(timePeriod, 0, 0, 0);
                if (updateFactors)
                    UpdateFactors(mode, startTime);

                Dictionary<string, double> scores = new Dictionary<string, double>();
                FieldInfo[] raterFieldNames = typeof(Rater).GetFields();
                FieldInfo[] gameFieldNames = typeof(GameStats).GetFields();

                foreach (var fieldInfo in raterFieldNames)
                {
                    double currentCategory;
                    var currrentTuple = fieldInfo.GetValue(this) as Dictionary<string, double>;
                    var currrentGameField = gameFieldNames.FirstOrDefault(f => f.Name.Equals(fieldInfo.Name));

                    if (currrentGameField == null || currrentTuple == null) continue;

                    if (fieldInfo.Name.Equals("FgPer"))
                        currentCategory = games.Sum(g => g.Fgm) / games.Sum(g => g.Fga);
                    else if (fieldInfo.Name.Equals("FtPer"))
                        currentCategory = games.Sum(g => g.Ftm) / games.Sum(g => g.Fta);
                    else
                        currentCategory = games.Select(g => (double)currrentGameField.GetValue(g)).Sum() / (mode == CalcScoreType.Days ? 1 : games.Count(g => g.Min > 0));

                    var score = ((double.IsNaN(currentCategory) ? 0 : currentCategory) - currrentTuple["Median"]) / currrentTuple["Std"];
                    scores.Add(fieldInfo.Name, score);
                }

                scores["To"] = scores["To"] * -1;

                scores["Fg"] = Math.Sign(scores["FgPer"]) * Math.Abs(scores["Fga"] * scores["FgPer"]);
                scores["Ft"] = Math.Sign(scores["FtPer"]) * Math.Abs(scores["Fta"] * scores["FtPer"]);

                //scores.Remove("Fta");
                //scores.Remove("FtPer");
                //scores.Remove("Fga");
                //scores.Remove("FgPer");

                scores["Score"] = scores["Fg"] + scores["Ft"] + scores["Tpm"] + scores["Reb"] + scores["Ast"] +
                                  scores["Stl"] + scores["Blk"] + scores["To"] + scores["Pts"];

                return scores;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private void UpdateFactors(CalcScoreType mode, DateTime startDate = default(DateTime))
        {
            FieldInfo[] raterFieldNames = typeof(Rater).GetFields();

            IEnumerable<IGrouping<int?, Game>> playersGames = mode == CalcScoreType.Days
                ? Games.Where(g => g.GameDate.Value.Date >= startDate).GroupBy(g => g.PlayerId)
                : Games.GroupBy(g => g.PlayerId);

            foreach (FieldInfo fieldInfo in raterFieldNames)
            {
                double[] arr;
                var prop = typeof(Game).GetProperties().FirstOrDefault(p => p.Name.Equals(fieldInfo.Name));
                if (prop == null) continue;
                if (prop.Name.Equals("FtPer"))
                    arr = playersGames.Select(p => p.Sum(g => g.Ftm.Value) / p.Sum(g => g.Fta.Value)).ToArray();
                else if (prop.Name.Equals("FgPer"))
                    arr = playersGames.Select(p => p.Sum(g => g.Fgm.Value) / p.Sum(g => g.Fga.Value)).ToArray();
                else
                    arr = playersGames.Select(p => p.Sum(g => (double)prop.GetValue(g)) / (mode == CalcScoreType.Days ? 1 : p.Count(g => g.Min > 0))).ToArray();

                arr = arr.Where(a => !double.IsNaN(a)).ToArray();

                var val = new Dictionary<string, double> { { "Average", arr.Average() }, { "Median", CalcMedian(arr) }, { "Std", CalcStdDev(arr) } };
                fieldInfo.SetValue(this, val);
            }
        }

        private static double CalcMedian(IEnumerable<double> sourceNumbers)
        {
            //Framework 2.0 version of this method. there is an easier way in F4        
            if (sourceNumbers == null || !sourceNumbers.Any())
                throw new System.Exception("Median of empty array not defined.");

            //make sure the list is sorted, but use a new array
            double[] sortedPNumbers = (double[])sourceNumbers.ToArray().Clone();
            Array.Sort(sortedPNumbers);

            //get the median
            int size = sortedPNumbers.Length;
            int mid = size / 2;
            double median = (size % 2 != 0) ? sortedPNumbers[mid] : (sortedPNumbers[mid] + sortedPNumbers[mid - 1]) / 2;
            return median;
        }

        private static double CalcStdDev(IEnumerable<double> values)
        {
            double ret = 0;
            int count = values.Count();
            if (count > 1)
            {
                //Compute the Average
                double avg = values.Average();

                //Perform the Sum of (value-avg)^2
                double sum = values.Sum(d => (d - avg) * (d - avg));

                //Put it all together
                ret = Math.Sqrt(sum / (count - 1));
            }
            return ret;
        }
    }

    public enum CalcScoreType
    {
        Games,
        Days
    }
}
