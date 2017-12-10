using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace espn
{
    public class GameStats
    {
        public DateTime GameDate;
        public double Pts, Reb, Ast, Tpm, Tpa, Fga, Fgm, Ftm, Fta, Stl, Blk, To, Min, Pf;
        public double FtPer, FgPer, TpPer;
        public double Score;
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
            FtPer = game.FtPer.Value;
            FgPer = game.FgPer.Value;
            TpPer = game.TpPer.Value;
            //Score = game.Score.Value;
            Gp = game.Gp.Value;
        }

        public GameStats(string gameStr, int year)
        {
            string[] stats = gameStr.Split(new[] { @"<td>", @"</td>" }, StringSplitOptions.RemoveEmptyEntries);

            var temp = stats[1].Substring(4).Split('/');
            var month = Int32.Parse(temp[0]);
            var day = Int32.Parse(temp[1]);
            GameDate = new DateTime(month < 10 ? year : year - 1, month, day);

            Min = Int32.Parse(stats[4].Substring(stats[4].IndexOf(">") + 1));

            temp = stats[5].Substring(stats[5].IndexOf(">") + 1).Split('-');
            Fgm = Int32.Parse(temp[0]);
            Fga = Int32.Parse(temp[1]);
            FgPer = Double.Parse(stats[6].Substring(stats[6].IndexOf(">") + 1)) * 100;
            temp = stats[7].Substring(stats[7].IndexOf(">") + 1).Split('-');
            Tpm = Int32.Parse(temp[0]);
            Tpa = Int32.Parse(temp[1]);
            TpPer = Double.Parse(stats[8].Substring(stats[8].IndexOf(">") + 1)) * 100;
            temp = stats[9].Substring(stats[9].IndexOf(">") + 1).Split('-');
            Ftm = Int32.Parse(temp[0]);
            Fta = Int32.Parse(temp[1]);
            FtPer = Double.Parse(stats[10].Substring(stats[10].IndexOf(">") + 1)) * 100;

            Reb = Int32.Parse(stats[11].Substring(stats[11].IndexOf(">") + 1));
            Ast = Int32.Parse(stats[12].Substring(stats[12].IndexOf(">") + 1));
            Blk = Int32.Parse(stats[13].Substring(stats[13].IndexOf(">") + 1));
            Stl = Int32.Parse(stats[14].Substring(stats[14].IndexOf(">") + 1));
            Pf = Int32.Parse(stats[15].Substring(stats[15].IndexOf(">") + 1));
            To = Int32.Parse(stats[16].Substring(stats[16].IndexOf(">") + 1));
            Pts = Int32.Parse(stats[17].Substring(stats[17].IndexOf(">") + 1));
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
                stats.Score = CalcScore(stats);
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

        public static GameStats GetSumStats(GameStats[] games)
        {
            GameStats stats = new GameStats();
            if (games.Length == 0)
                return stats;

            FieldInfo[] fieldNames = typeof(GameStats).GetFields();

            foreach (FieldInfo field in fieldNames.Where(f => f.FieldType == typeof(double)))
            {
                field.SetValue(stats, games.Sum(g => (double)field.GetValue(g)));
            }

            stats.FgPer = (stats.Fgm / stats.Fga) * 100;
            stats.FtPer = (stats.Ftm / stats.Fta) * 100;
            stats.TpPer = (stats.Tpm / stats.Tpa) * 100;
            stats.Score = CalcScore(stats);
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
            double[] y = { };

            switch (colName)
            {
                case "Min":
                    y = games.Select(g => g.Min).ToArray();
                    break;
                case "FgPer":
                    y = games.Select(g => g.FgPer).ToArray();
                    break;
                case "TpPer":
                    y = games.Select(g => g.TpPer).ToArray();
                    break;
                case "FtPer":
                    y = games.Select(g => g.FtPer).ToArray();
                    break;
                case "TpmTpa":
                    y = games.Select(g => g.Tpm).ToArray();
                    break;
                case "Reb":
                    y = games.Select(g => g.Reb).ToArray();
                    break;
                case "Ast":
                    y = games.Select(g => g.Ast).ToArray();
                    break;
                case "Blk":
                    y = games.Select(g => g.Blk).ToArray();
                    break;
                case "Stl":
                    y = games.Select(g => g.Stl).ToArray();
                    break;
                case "Pf":
                    y = games.Select(g => g.Pf).ToArray();
                    break;
                case "To":
                    y = games.Select(g => g.To).ToArray();
                    break;
                case "Pts":
                    y = games.Select(g => g.Pts).ToArray();
                    break;
            }

            return y;
        }

        public static double CalcScore(GameStats stat)
        {
            return stat.Pts * MainForm.Factors.Pts + stat.Reb * MainForm.Factors.Reb + stat.Ast * MainForm.Factors.Ast +
                   stat.Stl * MainForm.Factors.Stl + stat.Blk * MainForm.Factors.Blk + stat.Tpm * MainForm.Factors.Tpm +
                   stat.To * MainForm.Factors.To + stat.FgPer * MainForm.Factors.FgPer + stat.FtPer * MainForm.Factors.FtPer +
                   MainForm.Factors.FtVol * (stat.Fta - stat.Ftm) + MainForm.Factors.FgVol * (stat.Fga - stat.Fgm);
        }

        public static GameStats[] FilterOutliers(GameStats[] games)
        {
            double avg = games.Select(g => g.Min).Average();
            double std = games.Select(g => g.Min).ToArray().StandardDeviation();
            double th = std * 1.5;
            games = games.Where(g => g.Min > avg - th && g.Min < avg + th).ToArray();
            return games;
        }
    }
}
