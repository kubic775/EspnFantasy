using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NBAFantasy;
using NBAFantasy.Models;

namespace espn
{
    public partial class CustomPlayer : Form
    {
        private readonly List<double> _scoresTotal, _scoresAvg;

        public CustomPlayer()
        {
            InitializeComponent();
            _scoresTotal = MainForm.PlayerRater.CreateRater(CalcScoreType.Days).
                Select(p => p.Scores["Score"]).ToList();
            _scoresAvg = MainForm.PlayerRater.CreateRater(CalcScoreType.Games).
                Select(p => p.Scores["Score"]).ToList();
        }

        private void calc_button_Click(object sender, EventArgs e)
        {
            CreateCustomPlayerRank();
        }

        private void gp_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if(!total_checkBox.Checked) return;
            calc_button_Click(null, null);
        }

        private void CreateCustomPlayerRank()
        {
            try
            {
                int gp = total_checkBox.Checked ? (int)gp_numericUpDown.Value : 1;
                var g = new Games();
                g.GameDate = DateTime.Today;
                g.Pts = pts_textBox.Text.ToInt() * gp;
                g.Reb = reb_textBox.Text.ToInt() * gp;
                g.Ast = ast_textBox.Text.ToInt() * gp;
                g.Tpm = tpm_textBox.Text.ToInt() * gp;
                g.Tpa = g.Tpm * gp;
                g.Fga = fga_textBox.Text.ToInt() * gp;
                g.Fgm = fgm_textBox.Text.ToInt() * gp;
                g.Ftm = ftm_textBox.Text.ToInt() * gp;
                g.Fta = fta_textBox.Text.ToInt() * gp;
                g.Stl = stl_textBox.Text.ToInt() * gp;
                g.Blk = blk_textBox.Text.ToInt() * gp;
                g.To = to_textBox.Text.ToInt() * gp;
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

                score_label.Text = score.ToString("#.##");
                rank_label.Text = raterPos.ToString();
                fgPer_textBox.Text = gameStats.FgPer.ToString("#.##");
                ftPer_textBox.Text = gameStats.FtPer.ToString("#.##");

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
