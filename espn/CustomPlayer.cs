using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NBAFantasy;
using NBAFantasy.Models;
using NBAFantasy.YahooLeague;

namespace espn
{
    public partial class CustomPlayer : Form
    {
        private List<double> _scoresTotal, _scoresAvg;

        public CustomPlayer()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            customPlayer_autoCompleteTextBox.Values = YahooDBMethods.AllPlayers.Select(p => p.Name).ToArray();
            customPlayer_autoCompleteTextBox.PlayerSelectedEvent = PlayerSelectedEvent;
            _scoresTotal = MainForm.PlayerRater.CreateRater(CalcScoreType.Days).
                Select(p => p.Scores["Score"]).ToList();
            _scoresAvg = MainForm.PlayerRater.CreateRater(CalcScoreType.Games).
                Select(p => p.Scores["Score"]).ToList();
        }

        private async void PlayerSelectedEvent(string name)
        {
            try
            {
                var player = await PlayerInfo.GetPlayerInfoAsync(name);
                var games = player.FilterGamesByYear(Utils.GetCurrentYear() + 1);
                if (!games.Any()) return;
                GameStats avgGame = GameStats.GetAvgStats(games.ToArray());
                UpdateGuiByGame(avgGame, games.Length);
                calc_button_Click(null, null);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error while create custom player - {e.Message}");
            }
        }

        private void UpdateGuiByGame(GameStats game, int? gp = null)
        {
            pts_textBox.Text = game.Pts.ToString("#.#");
            reb_textBox.Text = game.Reb.ToString("#.#");
            ast_textBox.Text = game.Ast.ToString("#.#");
            tpm_textBox.Text = game.Tpm.ToString("#.#");
            fga_textBox.Text = game.Fga.ToString("#.#");
            fgm_textBox.Text = game.Fgm.ToString("#.#");
            fgPer_textBox.Text = game.FgPer.ToString("#.##");
            ftm_textBox.Text = game.Ftm.ToString("#.#");
            fta_textBox.Text = game.Fta.ToString("#.#");
            ftPer_textBox.Text = game.FtPer.ToString("#.##");
            stl_textBox.Text = game.Stl.ToString("#0.#");
            blk_textBox.Text = game.Blk.ToString("#0.#");
            to_textBox.Text = game.To.ToString("#0.#");
            gp_numericUpDown.Value = gp ?? gp_numericUpDown.Value;
        }

        private void calc_button_Click(object sender, EventArgs e)
        {
            CreateCustomPlayerRank();
        }

        private void gp_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (!total_checkBox.Checked) return;
            calc_button_Click(null, null);
        }

        private void total_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            calc_button_Click(null, null);
        }

        private void CreateCustomPlayerRank()
        {
            try
            {
                int gp = total_checkBox.Checked ? (int)gp_numericUpDown.Value : 1;
                var g = new Games();
                g.GameDate = DateTime.Today;
                g.Pts = pts_textBox.Text.ToDouble() * gp;
                g.Reb = reb_textBox.Text.ToDouble() * gp;
                g.Ast = ast_textBox.Text.ToDouble() * gp;
                g.Tpm = tpm_textBox.Text.ToDouble() * gp;
                g.Tpa = g.Tpm * gp;
                g.Fga = fga_textBox.Text.ToDouble() * gp;
                g.Fgm = fgm_textBox.Text.ToDouble() * gp;
                g.Ftm = ftm_textBox.Text.ToDouble() * gp;
                g.Fta = fta_textBox.Text.ToDouble() * gp;
                g.Stl = stl_textBox.Text.ToDouble() * gp;
                g.Blk = blk_textBox.Text.ToDouble() * gp;
                g.To = to_textBox.Text.ToDouble() * gp;
                g.Gp = gp;
                g.Min = 48 * gp;
                g.Pf = 0;

                var gameStats = new GameStats(g);

                var score = total_checkBox.Checked ?
                    MainForm.PlayerRater.CalcScore(new[] { gameStats }, CalcScoreType.Days, 0) :
                    MainForm.PlayerRater.CalcScore(new[] { gameStats }, CalcScoreType.Games, 0);

                var raterPos = total_checkBox.Checked
                    ? _scoresTotal.Count(s => s >= score) + 1
                    : _scoresAvg.Count(s => s >= score) + 1;

                score_label.Text = score.ToString("#0.##");
                rank_label.Text = raterPos.ToString();
                fgPer_textBox.Text = gameStats.FgPer.ToString("#.##");
                ftPer_textBox.Text = gameStats.FtPer.ToString("#.##");

            }
            catch (Exception e)
            {
                MessageBox.Show($"Error while create custom player rank - {e.Message}");
            }
        }
    }
}
