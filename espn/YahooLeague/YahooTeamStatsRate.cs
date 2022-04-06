using System;
using System.Collections.Generic;
using System.Linq;
using NBAFantasy.Models;
using NBAFantasy.YahooLeague;

namespace espn.YahooLeague
{
    public class YahooTeamStatsRate : YahooTeamStats
    {
        private int _teamId;
        private int _numOfGames, _numOfDays;
        private DateTime _minDateTime;
        private DateTime _maxDateTime;
        private List<YahooTeamStats> _statsList;
        public Dictionary<string, double> Rates { get; }

        public YahooTeamStatsRate(int teamId, List<YahooTeamStats> statsList)
        {
            _teamId = teamId;
            _statsList = statsList;
            _numOfGames = statsList.Sum(s => s.Gp.Value);
            _numOfDays = statsList.Count;
            _minDateTime = statsList.Min(s => s.GameDate);
            _maxDateTime = statsList.Max(s => s.GameDate);
            Rates = new Dictionary<string, double>();
            CalcRates();
        }

        private void CalcRates()
        {
            Rates.Add("FgPer", Math.Round((double)_statsList.Sum(s => s.Fgm.Value) / _statsList.Sum(s => s.Fga.Value) * 100, 5));
            Rates.Add("FtPer", Math.Round((double)_statsList.Sum(s => s.Ftm.Value) / _statsList.Sum(s => s.Fta.Value) * 100, 5));
            Rates.Add("Tpm", Math.Round(_statsList.Sum(s => s.Tpm.Value) / (double)_numOfGames, 2));
            Rates.Add("Pts", Math.Round(_statsList.Sum(s => s.Pts.Value) / (double)_numOfGames, 2));
            Rates.Add("Reb", Math.Round(_statsList.Sum(s => s.Reb.Value) / (double)_numOfGames, 2));
            Rates.Add("Ast", Math.Round(_statsList.Sum(s => s.Ast.Value) / (double)_numOfGames, 2));
            Rates.Add("Stl", Math.Round(_statsList.Sum(s => s.Stl.Value) / (double)_numOfGames, 2));
            Rates.Add("Blk", Math.Round(_statsList.Sum(s => s.Blk.Value) / (double)_numOfGames, 2));
            Rates.Add("To", Math.Round(_statsList.Sum(s => s.To.Value) / (double)_numOfGames, 2));
        }

        public override string ToString()
        {
            return $"{YahooDBMethods.YahooTeams.First(t=>t.TeamId==_teamId).TeamName}," +
                   $"{Rates["FgPer"]},{Rates["FtPer"]},{Rates["Tpm"]},{Rates["Pts"]},{Rates["Reb"]}," +
                   $"{Rates["Ast"]},{Rates["Stl"]},{Rates["Blk"]},{Rates["To"]},{_numOfDays},{_numOfGames}";
        }
    }
}
