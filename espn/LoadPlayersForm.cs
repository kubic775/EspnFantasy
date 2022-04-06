using System;
using System.Linq;
using System.Windows.Forms;
using NBAFantasy.YahooLeague;

namespace NBAFantasy
{
    public partial class LoadPlayersForm : Form
    {
        public LoadPlayersForm()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            AcceptButton = ok_button;
            nbaTeams_comboBox.Items.AddRange(YahooDBMethods.NbaTeams.Select(t => t.Name).ToArray());
            yahooTeams_comboBox.Items.AddRange(YahooDBMethods.YahooTeams.Select(t => t.TeamName).ToArray());
        }

        private void ok_button_Click(object sender, EventArgs e)
        {
            Close();
        }

        public string[] GetPlayersNames()
        {
            if (nbaTeams_comboBox.SelectedIndex < 0 && yahooTeams_comboBox.SelectedIndex < 0) return Array.Empty<string>();

            var allPlayers = YahooDBMethods.AllPlayers;
            if (nbaTeams_comboBox.SelectedIndex >=0)
            {
                string nbaTeam = nbaTeams_comboBox.GetItemText(nbaTeams_comboBox.Items[nbaTeams_comboBox.SelectedIndex]);
                allPlayers = allPlayers.Where(p => p.Team.Equals(nbaTeam)).ToList();
            }

            if (yahooTeams_comboBox.SelectedIndex>=0)
            {
                string yahooTeam = yahooTeams_comboBox.GetItemText(yahooTeams_comboBox.Items[yahooTeams_comboBox.SelectedIndex]);
                int yahooTeamIndex = YahooDBMethods.YahooTeams.First(t => t.TeamName.Equals(yahooTeam)).TeamId;
                allPlayers = allPlayers.Where(p => p.TeamNumber.HasValue && p.TeamNumber == yahooTeamIndex).ToList();
            }

            return allPlayers.Select(p => p.Name).ToArray();
        }
        
    }
}
