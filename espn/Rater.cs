using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;
using NBAFantasy.Models;

namespace NBAFantasy
{
    public class Rater
    {
        //Average, Median, Std
        public Dictionary<string, double> Pts, Reb, Ast, Tpm, Stl, Blk, To, Fta, FtPer, Fga, FgPer;
        public Dictionary<string, double> Factors;
        public IEnumerable<Games> Games;
        public IEnumerable<Players> Players;

        public Rater(IEnumerable<Players> players, IEnumerable<Games> games)
        {
            Players = players;
            Games = games;
            Factors = FactorsForm.GetFactors();
        }


        public IEnumerable<PlayerInfo> CreateRater(CalcScoreType mode, int timePeriod = 0)
        {
            var rater = new List<PlayerInfo>();
            Factors = FactorsForm.GetFactors();

            DateTime startTime = timePeriod == 0 ? default : DateTime.Today - new TimeSpan(timePeriod, 0, 0, 0);

            UpdateFactors(mode, startTime);

            Dictionary<long, List<Games>> playersGames = Games.Where(g => g.GameDate >= startTime).GroupBy(g => g.PlayerId).ToDictionary(k => k.Key.Value, v => v.ToList());
            if (!playersGames.Any()) return new List<PlayerInfo>();

            foreach (KeyValuePair<long, List<Games>> player in playersGames)
            {
                IEnumerable<GameStats> games = player.Value.Select(g => new GameStats(g));
                var scores = CalcScores(games, mode, timePeriod, false);
                GameStats avgGame = GameStats.GetAvgStats(games.ToArray());
                avgGame.Score = scores["Score"];
                var playerT =
                    new PlayerInfo(Players.First(p => p.Id == player.Key), new[] { avgGame })
                    {
                        Scores = scores,
                    };
                rater.Add(playerT);
            }

            return rater.OrderByDescending(p => p.Scores["Score"]).Select((r, i) =>
            {
                r.RaterPos = ++i;
                return r;
            });
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
            Dictionary<string, double> scores = new Dictionary<string, double>();
            try
            {
                DateTime startTime = timePeriod == 0 ? default(DateTime) : DateTime.Today - new TimeSpan(timePeriod, 0, 0, 0);
                if (updateFactors)
                    UpdateFactors(mode, startTime);

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

                scores["To"] *= -1;

                //Apply Factors
                var factorScore = new Dictionary<string, double>();
                foreach (string key in scores.Keys)
                {
                    if (!Factors.ContainsKey(key)) continue;
                    factorScore.Add(key, scores[key] * Factors[key]);
                }

                scores = factorScore;

                scores["Fg"] = Math.Sign(scores["FgPer"]) * Math.Abs(scores["Fga"] * scores["FgPer"]);
                scores["Ft"] = Math.Sign(scores["FtPer"]) * Math.Abs(scores["Fta"] * scores["FtPer"]);

                scores["Score"] = scores["Fg"] + scores["Ft"] + scores["Tpm"] + scores["Reb"] + scores["Ast"] +
                                   scores["Stl"] + scores["Blk"] + scores["To"] + scores["Pts"];

                return scores;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                scores.Add("Score", -1);
                return scores;
            }
        }

        private void UpdateFactors(CalcScoreType mode, DateTime startDate = default)
        {
            FieldInfo[] raterFieldNames = typeof(Rater).GetFields();

           var playersGames = mode == CalcScoreType.Days
                ? Games.Where(g => g.GameDate.Date >= startDate).GroupBy(g => g.PlayerId)
                : Games.GroupBy(g => g.PlayerId);

            if (!playersGames.Any()) return;

            foreach (FieldInfo fieldInfo in raterFieldNames)
            {
                double[] arr;
                var prop = typeof(Games).GetProperties().FirstOrDefault(p => p.Name.Equals(fieldInfo.Name));
                if (prop == null) continue;
                if (prop.Name.Equals("FtPer"))
                    arr = playersGames.Select(p => p.Sum(g => g.Ftm.Value) / p.Sum(g => g.Fta.Value)).ToArray();
                else if (prop.Name.Equals("FgPer"))
                    arr = playersGames.Select(p => p.Sum(g => g.Fgm.Value) / p.Sum(g => g.Fga.Value)).ToArray();
                else
                    arr = playersGames.Select(p => p.Sum(g => (double)prop.GetValue(g)) / (mode == CalcScoreType.Days ? 1 : p.Count(g => g.Min > 0))).ToArray();

                arr = arr.Where(a => !double.IsNaN(a) && !double.IsInfinity(a)).ToArray();

                var val = new Dictionary<string, double> { { "Average", arr.Average() }, { "Median", arr.Median() }, { "Std", arr.StandardDeviation() } };
                fieldInfo.SetValue(this, val);
            }
        }
    }

    public enum CalcScoreType
    {
        Games,
        Days
    }
}
