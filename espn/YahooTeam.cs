using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace espn
{
    public class YahooTeam
    {
        public string Team;
        public int Rank, Gp;
        public double TotalPoints;
        public PctCategoryStats FgPct, FtPct;
        public SumCategoryStats Tpm, Pts, Reb, Ast, St, Blk, To;

        public YahooTeam(List<string> headers, List<string> stats, double totalPoints = 0, int? gamesNormalize = null)
        {
            UpdateStats(headers, stats);
            if (gamesNormalize == null)
                TotalPoints = totalPoints;
            else
                NormalizeStats(gamesNormalize.Value);
        }

        private void UpdateStats(List<string> headers, List<string> stats)
        {

            var fieldNames = typeof(YahooTeam).GetFields().Where(f => f.FieldType == typeof(SumCategoryStats));

            Team = stats[headers.IndexOf(nameof(Team))];
            Rank = stats[headers.IndexOf(nameof(Rank))].Replace(".", "").ToInt();
            Gp = stats[headers.IndexOf(nameof(Gp))].ToInt();

            FgPct = new PctCategoryStats { Avg = stats[headers.IndexOf(nameof(FgPct))].ToDouble() * 100 };
            FtPct = new PctCategoryStats { Avg = stats[headers.IndexOf(nameof(FtPct))].ToDouble() * 100 };

            foreach (FieldInfo field in fieldNames)
            {
                var total = stats[headers.IndexOf(field.Name)].ToInt();
                var rate = (double)total / Gp * 10;
                field.SetValue(this, new SumCategoryStats { Total = total, Rate = rate });
            }
        }

        public void NormalizeStats(int gamesNormalize)
        {
            Gp = gamesNormalize;
            Tpm.Total = (int)(Tpm.Rate / 10 * gamesNormalize);
            Pts.Total = (int)(Pts.Rate / 10 * gamesNormalize);
            Reb.Total = (int)(Reb.Rate / 10 * gamesNormalize);
            Ast.Total = (int)(Ast.Rate / 10 * gamesNormalize);
            St.Total = (int)(St.Rate / 10 * gamesNormalize);
            Blk.Total = (int)(Blk.Rate / 10 * gamesNormalize);
            To.Total = (int)(To.Rate / 10 * gamesNormalize);
        }

        public void SetRank(string header, int rank)
        {
            var info = typeof(YahooTeam).GetFields().FirstOrDefault(f => f.Name.Equals(header));
            if (info == null) return;
            if (info.FieldType == typeof(SumCategoryStats))
            {
                var field = (SumCategoryStats)info.GetValue(this);
                field.Rank = rank;
            }
            else
            {
                var field = (PctCategoryStats)info.GetValue(this);
                field.Rank = rank;
            }

            TotalPoints = FgPct.Rank + FtPct.Rank + Tpm.Rank + Pts.Rank + Reb.Rank + Ast.Rank + St.Rank + Blk.Rank + To.Rank;
        }

        public double GetStat(string header)
        {
            var info = typeof(YahooTeam).GetFields().FirstOrDefault(f => f.Name.Equals(header));
            if (info == null) return Double.NaN;
            if (info.FieldType == typeof(SumCategoryStats))
            {
                var field = (SumCategoryStats)info.GetValue(this);
                return field.Total;
            }
            else
            {
                var field = (PctCategoryStats)info.GetValue(this);
                return field.Avg;
            }
        }

        public double GetRate(string header)
        {
            var info = typeof(YahooTeam).GetFields().FirstOrDefault(f => f.Name.Equals(header));
            if (info == null) return Double.NaN;
            if (info.FieldType == typeof(SumCategoryStats))
            {
                var field = (SumCategoryStats)info.GetValue(this);
                return field.Rate;
            }
            else
                return Double.NaN;
        }

        public override string ToString()
        {
            return $"{Rank}.{Team} ({TotalPoints} Points)";
        }

        public Dictionary<string, object> ExportDataToCsvFormat()
        {
            var res = new Dictionary<string, object>
            {
                { nameof(Rank), Rank },
                { nameof(Team), Team },
                { nameof(Gp), Gp }
            };
            var fieldNames = typeof(YahooTeam).GetFields().Where(f => f.FieldType == typeof(PctCategoryStats));
            foreach (var info in fieldNames)
            {
                var field = (PctCategoryStats)info.GetValue(this);
                res.Add(info.Name, Math.Round(field.Avg, 2));
            }
            fieldNames = typeof(YahooTeam).GetFields().Where(f => f.FieldType == typeof(SumCategoryStats));
            foreach (var info in fieldNames)
            {
                var field = (SumCategoryStats)info.GetValue(this);
                res.Add(info.Name, field.Total);
                res.Add($"{info.Name}Rate", Math.Round(field.Rate, 1));
            }
            res.Add(nameof(TotalPoints), TotalPoints);

            return res;
        }
    }

    //Cumulative Categories
    public class SumCategoryStats
    {
        public int Total, Rank;
        public double Rate;

        public override string ToString()
        {
            return $"Total - {Total}, Rate - {Rate}, Rank - {Rank}";
        }
    }

    //Percent Categories
    public class PctCategoryStats
    {
        public int Rank;
        public double Avg;
        public override string ToString()
        {
            return $"Avg - {Avg}, Rank - {Rank}";
        }
    }
}
