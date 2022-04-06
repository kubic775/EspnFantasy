using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using NBAFantasy;
using NBAFantasy.YahooLeague;

namespace espn.YahooLeague
{
    public class YahooLeagueManager
    {
        public readonly Dictionary<string, YahooTeam> Teams;
        private List<string> _headers;
        private DateTime _originalDataTime;

        public YahooLeagueManager(int? gamesNormalize = null)
        {
            Teams = new Dictionary<string, YahooTeam>();
            CreateTeams(gamesNormalize);
            Console.WriteLine(ToString());
        }

        private void CreateTeams(int? gamesNormalize = null)
        {
            _originalDataTime = DateTime.Now;
            var page = Utils.DownloadStringFromUrl(ConfigurationManager.AppSettings["YahooStandingsPath"]);

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(page);

            _headers = doc.DocumentNode.SelectSingleNode("//table[2]")
                 .Descendants("tr")
                 .Skip(1)
                 .Where(tr => tr.Elements("th").Count() > 1)
                 .Select(tr => tr.Elements("th").Select(td => td.InnerText.Trim().ToPascalCase().
                     Replace("%", "Pct").Replace("3Ptm", "Tpm")).ToList())
                 .First();

            List<List<string>> teamPoints = doc.DocumentNode.SelectSingleNode("//table[1]")
                .Descendants("tr")
                .Skip(1)
                .Where(tr => tr.Elements("td").Count() > 1)
                .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim().Replace("*", "")).ToList())
                .ToList();

            List<List<string>> teamStats = doc.DocumentNode.SelectSingleNode("//table[2]")
                .Descendants("tr")
                .Skip(1)
                .Where(tr => tr.Elements("td").Count() > 1)
                .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim().Replace("*", "")).ToList())
                .ToList();

            var teamIndex = _headers.IndexOf("Team");
            foreach (List<string> stats in teamStats)
            {
                var teamName = stats[teamIndex];
                var totalPoints = teamPoints.First(t => t[teamIndex].Equals(teamName)).Last().ToDouble();
                Teams.Add(teamName, new YahooTeam(_headers, stats, totalPoints, gamesNormalize));
            }

            if (gamesNormalize != null)
                RankTeams();
        }

        private void RankTeams()
        {
            foreach (string header in _headers.Skip(3))
            {
                var sortedTeamsByHeader = Teams.Values.OrderBy(t => t.GetStat(header)).ToArray();
                for (int i = 0; i < sortedTeamsByHeader.Length; i++)
                {
                    Teams[sortedTeamsByHeader[i].Team].SetRank(header, i + 1);
                }
            }

            var sortedTeams = Teams.Values.OrderBy(t => t.TotalPoints).ToArray();
            for (int i = 0; i < sortedTeams.Length; i++)
                Teams[sortedTeams[i].Team].Rank = sortedTeams.Length - i;
        }

        public List<string> ExportToFile()
        {
            Dictionary<string, object>[] teamsStats = Teams.Values.OrderByDescending(t => t.TotalPoints)
                .Select(t => t.ExportDataToCsvFormat()).ToArray();
            var headers = teamsStats[0].Keys.ToArray();
            List<string> csv = new List<string> { string.Join(",", headers) };
            foreach (var teamStats in teamsStats)
            {
                csv.Add(string.Join(",", teamStats.Values));
            }

            return csv;
        }

        public override string ToString()
        {
            string res = String.Empty;
            foreach (var team in Teams.Values.OrderBy(t => t.Rank))
            {
                res += $"{team} {Environment.NewLine}";
            }
            return res;
        }

        public static List<string> ExportRatesToFile(DateTime startTime)
        {
            var res = new List<string>();
            var teamRates = new List<YahooTeamStatsRate>();
            foreach (var yahooTeamsStat in YahooDBMethods.YahooTeamsStats)
            {
                var relevantDays = yahooTeamsStat.Value.Where(g => g.GameDate.Date >= startTime.Date).ToList();
                if (relevantDays.Any())
                    teamRates.Add(new YahooTeamStatsRate(yahooTeamsStat.Key, relevantDays));
            }

            if (!teamRates.Any()) return res;

            var header = $"TeamName,{string.Join(",", teamRates.First().Rates.Keys)},Days,Games";
            res.Add(header);
            foreach (var rates in teamRates)
            {
                res.Add(rates.ToString());
            }

            return res;
        }
    }
}
