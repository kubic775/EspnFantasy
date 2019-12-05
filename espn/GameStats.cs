using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using MathNet.Numerics.Statistics;

namespace espn
{
    public class GameStats
    {
        public DateTime GameDate;
        public double Pts, Reb, Ast, Tpm, Tpa, Fga, Fgm, Ftm, Fta, Stl, Blk, To, Min, Pf;
        public double FtPer, FgPer, TpPer;
        public double Score;
        public string Opp, Result;
        public int Gp;

        public GameStats()
        {

        }

        public GameStats(Game game)
        {
            GameDate = game.GameDate.Value;
            Pts = game.Pts.Value;
            Reb = game.Reb.Value;
            Ast = game.Ast.Value;
            Tpm = game.Tpm.Value;
            Tpa = game.Tpa.Value;
            Fga = game.Fga.Value;
            Fgm = game.Fgm.Value;
            Ftm = game.Ftm.Value;
            Fta = game.Fta.Value;
            Stl = game.Stl.Value;
            Blk = game.Blk.Value;
            To = game.To.Value;
            Min = game.Min.Value;
            Pf = game.Pf.Value;
            FtPer = (Ftm / Fta) * 100;
            FgPer = (Fgm / Fga) * 100;
            TpPer = (Tpm / Tpa) * 100;
            //Score = game.Score.Value;
            Gp = game.Gp.Value;
            Opp = game.Opp;
        }

        public Game UpdateGame(Game game)
        {
            game.GameDate = GameDate;
            game.Pts = Pts;
            game.Reb = Reb;
            game.Ast = Ast;
            game.Tpm = Tpm;
            game.Tpa = Tpa;
            game.Fga = Fga;
            game.Fgm = Fgm;
            game.Ftm = Ftm;
            game.Fta = Fta;
            game.Stl = Stl;
            game.Blk = Blk;
            game.To = To;
            game.Min = Min;
            game.Pf = Pf;
            game.FtPer = FtPer;
            game.FgPer = FtPer;
            game.TpPer = TpPer;
            game.Opp = Opp;
            return game;
        }

        public GameStats(string gameXml, int year)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(gameXml);
            var val = xmlDoc.FirstChild.ChildNodes.OfType<XmlNode>().Select(node => node.InnerText).ToList();
            //Console.WriteLine(string.Join(",", val));
            var dateInfo = val[0].Remove(0, 4).Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (dateInfo.Length != 2) return;
            var month = dateInfo[0].ToInt();
            var day = dateInfo[1].ToInt();
            if (month == 0 || day == 0) return;
            GameDate = new DateTime(month < 10 ? year : year - 1, month, day);
            Opp = val[1];
            Result = val[2];
            Min = val[3].ToInt();
            Fga = val[4].Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last().ToInt();
            Fgm = val[4].Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).First().ToInt();
            FgPer = Fgm / Fga;
            Tpa = val[6].Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last().ToInt();
            Tpm = val[6].Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).First().ToInt();
            TpPer = Tpm / Tpa;
            Fta = val[8].Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last().ToInt();
            Ftm = val[8].Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).First().ToInt();
            FtPer = Ftm / Fta;
            Reb = val[10].ToInt();
            Ast = val[11].ToInt();
            Blk = val[12].ToInt();
            Stl = val[13].ToInt();
            Pf = val[14].ToInt();
            To = val[15].ToInt();
            Pts = val[16].ToInt();
        }

        public static GameStats GetAvgStats(GameStats[] games)
        {
            GameStats stats = new GameStats();
            FieldInfo[] fieldNames = typeof(GameStats).GetFields();
            games = games.Where(g => g.Min > 0).ToArray();
            if (games.Length > 0)
            {
                foreach (FieldInfo field in fieldNames.Where(f => f.FieldType == typeof(double)))
                {
                    field.SetValue(stats, games.Average(g => (double)field.GetValue(g)));
                }
                stats.FgPer = (stats.Fgm / stats.Fga) * 100;
                stats.FtPer = (stats.Ftm / stats.Fta) * 100;
                stats.TpPer = (stats.Tpm / stats.Tpa) * 100;
                //stats.Score = CalcScore(stats);
                stats.Gp = games.Length;
            }
            else
            {
                stats.Gp = 0;
                foreach (FieldInfo field in fieldNames.Where(f => f.FieldType == typeof(double)))
                {
                    field.SetValue(stats, 0);
                }
            }
            return stats;
        }

        public static GameStats GetSumStats(IEnumerable<GameStats> games)
        {
            GameStats stats = new GameStats();
            if (!games.Any())
                return stats;

            FieldInfo[] fieldNames = typeof(GameStats).GetFields();

            foreach (FieldInfo field in fieldNames.Where(f => f.FieldType == typeof(double)))
            {
                field.SetValue(stats, games.Sum(g => (double)field.GetValue(g)));
            }

            stats.FgPer = (stats.Fgm / stats.Fga) * 100;
            stats.FtPer = (stats.Ftm / stats.Fta) * 100;
            stats.TpPer = (stats.Tpm / stats.Tpa) * 100;
            //stats.Score = CalcScore(stats);
            return stats;
        }

        public static GameStats GetDiffInStats(GameStats stat1, GameStats stat2)
        {
            GameStats stats = new GameStats();
            FieldInfo[] fieldNames = typeof(GameStats).GetFields();

            foreach (FieldInfo field in fieldNames.Where(f => f.FieldType == typeof(double)))
            {
                field.SetValue(stats, (double)field.GetValue(stat2) - (double)field.GetValue(stat1));
            }
            stats.To = stat1.To - stat2.To;
            return stats;
        }

        public static double[] GetYVals(string colName, GameStats[] games)
        {
            FieldInfo field = typeof(GameStats).GetFields().FirstOrDefault(f => f.Name.Equals(colName));
            if (field == null || field.FieldType != typeof(double)) return new Double[] { };

            double[] y = games.Select(g => (double)field.GetValue(g)).Select(v => Double.IsNaN(v) ? 0 : v).ToArray();

            if (colName.Equals("Score"))
            {
                MainForm.PlayerRater.CreateRater(CalcScoreType.Games);
                y = games.Select(g => Math.Round(MainForm.PlayerRater.CalcScore(new[] { g }, CalcScoreType.Games, 0, false), 1)).ToArray();
            }

            return y;
        }

        public static GameStats[] FilterOutliers(GameStats[] games)
        {
            double avg = games.Select(g => g.Min).Average();
            double std = games.Select(g => g.Min).ToArray().StandardDeviation();
            double th = std * 1.5;
            games = games.Where(g => g.Min > avg - th && g.Min < avg + th).ToArray();
            return games;
        }

        public string ToShortString()
        {
            IEnumerable<FieldInfo> fieldNames = typeof(GameStats).GetFields().Where(f => f.FieldType == typeof(double));
            return string.Join(",", fieldNames.Select(f => (double)f.GetValue(this)).ToArray());
        }

        public override string ToString()
        {
            return $"Pts:{Pts:0.0}, Reb:{Reb:0.0}, Ast:{Ast:0.0}, Tpm:{Tpm:0.0}, Stl:{Stl:0.0}, Blk:{Blk:0.0}, To:{To:0.0}, " +
                         $"FgPer:{Fgm:0.0}/{Fga:0.0}({FgPer:0.0}%), FtPer:{Ftm:0.0}/{Fta:0.0}({FtPer:0.0}%), Min:{Min:0.0}, Gp:{Gp}";
        }

    }
}
