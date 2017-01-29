using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml;

namespace espn
{
    public partial class MainForm : Form
    {
        private Player player, player1, player2;

        public MainForm()
        {
            InitializeComponent();
            PlayersList.CreatePlayersList();
            InitGUI();
        }

        private void InitGUI()
        {
            mode_comboBox.SelectedIndex = 0;
            compareMode_comboBox.SelectedIndex = 0;
            compare_last_comboBox.SelectedIndex = 3;
            stats_dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            stat_chart.Series[0].Points.Clear();
            compare_chart.Series[0].Points.Clear();
            player1_comboBox.Items.AddRange(PlayersList.Players.Keys.ToArray());
            player2_comboBox.Items.AddRange(PlayersList.Players.Keys.ToArray());
        }

        #region PlayerInfo


        private void playerName_textBox_TextChanged(object sender, EventArgs e)
        {
            if (playerName_textBox.Text.Length < 2)
            {
                players_listBox.Items.Clear();
                return;
            }

            List<string> releventPlayers = PlayersList.Players.Keys.Where(p => p.ToLower().Contains(playerName_textBox.Text.ToLower())).ToList();
            players_listBox.Items.Clear();
            players_listBox.Items.AddRange(releventPlayers.ToArray());
        }

        private void players_listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UseWaitCursor = true;
            string name = players_listBox.GetItemText(players_listBox.SelectedItem);
            player = new Player(name);
            player_pictureBox.Load(player.ImagePath);
            playerNameLabel.Text = player.PlayerName;
            playerInfo_label.Text = player.PlayerInfo + " | " + player.Team;
            button_max_Click(null, null);
            UseWaitCursor = false;
        }

        private void button_7_Click(object sender, EventArgs e)
        {
            numOf_textBox.Text = Math.Min(7, player.Games.Count).ToString();
        }

        private void button_15_Click(object sender, EventArgs e)
        {
            numOf_textBox.Text = Math.Min(15, player.Games.Count).ToString();
        }

        private void button_30_Click(object sender, EventArgs e)
        {
            numOf_textBox.Text = Math.Min(30, player.Games.Count).ToString();
        }

        private void button_max_Click(object sender, EventArgs e)
        {
            numOf_textBox.Text = player.Games.Count.ToString();
        }

        private void numOf_textBox_TextChanged(object sender, EventArgs e)
        {
            int num;

            if (int.TryParse(numOf_textBox.Text, out num))
            {
                var games = FilterReleventGames(mode_comboBox.GetItemText(mode_comboBox.Items[mode_comboBox.SelectedIndex]), numOf_textBox.Text, player);
                if (games == null)
                    return;
                UpdateTable(games);
                var colName = stats_dataGridView.SelectedCells.Count > 0
                    ? stats_dataGridView.Columns[stats_dataGridView.SelectedCells[0].ColumnIndex].Name
                    : "Pts";
                PrintChart(GetYVals(colName, num), colName, stat_chart);
            }
        }

        private GameStats[] FilterReleventGames(string mode, string numStr, Player playerToFilter)
        {
            var numOfGames = numStr.Equals("Max") ? playerToFilter.Games.Count : int.Parse(numStr);
            switch (mode)
            {
                case "Games":
                    return playerToFilter.Games.Take(numOfGames).ToArray();

                case "Days":
                    DateTime minDay = DateTime.Now - new TimeSpan(numOfGames, 0, 0, 0);
                    return playerToFilter.Games.Where(g => g.GameDate >= minDay).ToArray();
            }
            return null;
        }

        private void UpdateTable(GameStats[] games)
        {
            games = games.Where(g => g.Min > 0).ToArray();
            stats_dataGridView.Rows[0].Cells["Gp"].Value = games.Count(g => g.Min > 0);

            if (games.Length == 0)
                return;

            stats_dataGridView.Rows[0].Cells["Min"].Value = games.Average(g => g.Min).ToString("0.0");

            stats_dataGridView.Rows[0].Cells["FgmFga"].Value = games.Average(g => g.Fgm).ToString("0.0") + "-" + games.Average(g => g.Fga).ToString("0.0");
            stats_dataGridView.Rows[0].Cells["FgPer"].Value = games.Average(g => g.FgPer).ToString("0.0") + "%";
            stats_dataGridView.Rows[0].Cells["TpmTpa"].Value = games.Average(g => g.Tpm).ToString("0.0") + "-" + games.Average(g => g.Tpa).ToString("0.0");
            stats_dataGridView.Rows[0].Cells["TpPer"].Value = games.Average(g => g.TpPer).ToString("0.0") + "%";
            stats_dataGridView.Rows[0].Cells["FtmFta"].Value = games.Average(g => g.Ftm).ToString("0.0") + "-" + games.Average(g => g.Fta).ToString("0.0");
            stats_dataGridView.Rows[0].Cells["FtPer"].Value = games.Average(g => g.FtPer).ToString("0.0") + "%";

            stats_dataGridView.Rows[0].Cells["Reb"].Value = games.Average(g => g.Reb).ToString("0.0");
            stats_dataGridView.Rows[0].Cells["Ast"].Value = games.Average(g => g.Ast).ToString("0.0");
            stats_dataGridView.Rows[0].Cells["Blk"].Value = games.Average(g => g.Blk).ToString("0.0");
            stats_dataGridView.Rows[0].Cells["Stl"].Value = games.Average(g => g.Stl).ToString("0.0");
            stats_dataGridView.Rows[0].Cells["Pf"].Value = games.Average(g => g.Pf).ToString("0.0");
            stats_dataGridView.Rows[0].Cells["To"].Value = games.Average(g => g.To).ToString("0.0");
            stats_dataGridView.Rows[0].Cells["Pts"].Value = games.Average(g => g.Pts).ToString("0.0");

        }



        private void stats_dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var name = stats_dataGridView.Columns[e.ColumnIndex].Name;
            var y = GetYVals(name, int.Parse(numOf_textBox.Text));
            if (y.Length > 0)
                PrintChart(y, name, stat_chart);
        }

        public double[] GetYVals(string colName, int numOfGames)
        {
            double[] y = { };

            switch (colName)
            {
                case "Min":
                    y = player.Games.Take(numOfGames).Select(g => (double)g.Min).ToArray();
                    break;
                case "FgPer":
                    y = player.Games.Take(numOfGames).Select(g => g.FgPer).ToArray();
                    break;
                case "TpPer":
                    y = player.Games.Take(numOfGames).Select(g => g.TpPer).ToArray();
                    break;
                case "FtPer":
                    y = player.Games.Take(numOfGames).Select(g => g.FtPer).ToArray();
                    break;
                case "TpmTpa":
                    y = player.Games.Take(numOfGames).Select(g => (double)g.Tpm).ToArray();
                    break;
                case "Reb":
                    y = player.Games.Take(numOfGames).Select(g => (double)g.Reb).ToArray();
                    break;
                case "Ast":
                    y = player.Games.Take(numOfGames).Select(g => (double)g.Ast).ToArray();
                    break;
                case "Blk":
                    y = player.Games.Take(numOfGames).Select(g => (double)g.Blk).ToArray();
                    break;
                case "Stl":
                    y = player.Games.Take(numOfGames).Select(g => (double)g.Stl).ToArray();
                    break;
                case "Pf":
                    y = player.Games.Take(numOfGames).Select(g => (double)g.Pf).ToArray();
                    break;
                case "To":
                    y = player.Games.Take(numOfGames).Select(g => (double)g.To).ToArray();
                    break;
                case "Pts":
                    y = player.Games.Take(numOfGames).Select(g => (double)g.Pts).ToArray();
                    break;
            }

            return y;
        }

        public void PrintChart(double[] y, string name, Chart chart, int seriesNum = 0)
        {
            int[] x = Enumerable.Range(1, y.Length).ToArray();

            chart.Series[seriesNum].Points.Clear();
            chart.Series[seriesNum].Name = name;

            for (int i = 0; i < x.Length; i++)
            {
                chart.Series[seriesNum].Points.Add(new DataPoint(x[i], y[i]));
            }
        }

        private void copyToClipboard_button_Click(object sender, EventArgs e)
        {
            string text = player.PlayerName + ", Last " + numOf_textBox.Text + " : " + Environment.NewLine;
            text += "Pts: " + stats_dataGridView.Rows[0].Cells["Pts"].Value + ", " + "Reb: " + stats_dataGridView.Rows[0].Cells["Reb"].Value + ", " +
                          "Ast: " + stats_dataGridView.Rows[0].Cells["Ast"].Value + ", " + "Tpm: " + stats_dataGridView.Rows[0].Cells["TpmTpa"].Value.ToString().Split("-".ToCharArray())[0] + ", " +
                          "Stl: " + stats_dataGridView.Rows[0].Cells["Stl"].Value + ", " + "Blk: " + stats_dataGridView.Rows[0].Cells["Blk"].Value + ", " +
                          "FgPer: " + stats_dataGridView.Rows[0].Cells["FgPer"].Value + ", " + "FtPer: " + stats_dataGridView.Rows[0].Cells["FtPer"].Value + ", ";

            Clipboard.SetText(text);
        }

        #endregion

        #region ComparePlayers

        private void player1_comboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdatePlayer1();
        }

        private void player2_comboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdatePlayer2();
        }

        private void compareMode_comboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdatePlayer1();
            UpdatePlayer2();
        }

        private void compare_last_comboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdatePlayer1();
            UpdatePlayer2();
        }

        private void UpdatePlayer1()
        {
            try
            {
                if (compareMode_comboBox.SelectedIndex == -1 || compare_last_comboBox.SelectedIndex == -1 || player1_comboBox.SelectedIndex == -1)
                    return;

                if (player1 == null || player1.Id != PlayersList.Players[players_listBox.GetItemText(player1_comboBox.SelectedItem)])
                {
                    player1 = new Player(players_listBox.GetItemText(player1_comboBox.SelectedItem));
                }

                GameStats[] games = FilterReleventGames(compareMode_comboBox.GetItemText(compareMode_comboBox.SelectedItem),
                    compare_last_comboBox.GetItemText(compare_last_comboBox.SelectedItem), player1);
                UpdateCompareInfo1(games);

            }
            catch (Exception)
            {
            }
        }
        private void UpdatePlayer2()
        {
            try
            {
                if (compareMode_comboBox.SelectedIndex == -1 || compare_last_comboBox.SelectedIndex == -1 || player2_comboBox.SelectedIndex == -1)
                    return;
                if (player2 == null || player2.Id != PlayersList.Players[players_listBox.GetItemText(player2_comboBox.SelectedItem)])
                    player2 = new Player(players_listBox.GetItemText(player2_comboBox.SelectedItem));

                GameStats[] games = FilterReleventGames(compareMode_comboBox.GetItemText(compareMode_comboBox.SelectedItem),
                     compare_last_comboBox.GetItemText(compare_last_comboBox.SelectedItem), player2);
                UpdateCompareInfo2(games);
            }
            catch (Exception ex)
            {
            }
        }


        private void UpdateCompareInfo1(GameStats[] games)
        {
            games = games.Where(g => g.Min > 0).ToArray();
            gp1_label.Text = games.Length.ToString();
            min1_label.Text = games.Average(g => g.Min).ToString("0.0");
            pts1_label.Text = games.Average(g => g.Pts).ToString("0.0");
            reb1_label.Text = games.Average(g => g.Reb).ToString("0.0");
            ast1_label.Text = games.Average(g => g.Ast).ToString("0.0");
            stl1_label.Text = games.Average(g => g.Stl).ToString("0.0");
            blk1_label.Text = games.Average(g => g.Blk).ToString("0.0");
            tpm1_label.Text = games.Average(g => g.Tpm).ToString("0.0");
            fg1_label.Text = games.Average(g => g.Fgm).ToString("0.0") + @"/" + games.Average(g => g.Fga).ToString("0.0") + @", " + games.Average(g => g.FgPer).ToString("0.0") + @"%";
            fg1_label.Location = new Point(tpm1_label.Location.X + (tpm1_label.Size.Width / 2) - (fg1_label.Size.Width / 2), fg1_label.Location.Y);
            ft1_label.Text = games.Average(g => g.Ftm).ToString("0.0") + @"/" + games.Average(g => g.Fta).ToString("0.0") + @", " + games.Average(g => g.FtPer).ToString("0.0") + @"%";
            ft1_label.Location = new Point(tpm1_label.Location.X + (tpm1_label.Size.Width / 2) - (ft1_label.Size.Width / 2), ft1_label.Location.Y);
            to1_label.Text = games.Average(g => g.To).ToString("0.0");
        }


        private void UpdateCompareInfo2(GameStats[] games)
        {
            games = games.Where(g => g.Min > 0).ToArray();
            gp2_label.Text = games.Length.ToString();
            min2_label.Text = games.Average(g => g.Min).ToString("0.0");
            pts2_label.Text = games.Average(g => g.Pts).ToString("0.0");
            reb2_label.Text = games.Average(g => g.Reb).ToString("0.0");
            ast2_label.Text = games.Average(g => g.Ast).ToString("0.0");
            stl2_label.Text = games.Average(g => g.Stl).ToString("0.0");
            blk2_label.Text = games.Average(g => g.Blk).ToString("0.0");
            tpm2_label.Text = games.Average(g => g.Tpm).ToString("0.0");
            fg2_label.Text = games.Average(g => g.Fgm).ToString("0.0") + @"/" + games.Average(g => g.Fga).ToString("0.0") + @" - " + games.Average(g => g.FgPer).ToString("0.0") + @"%";
            fg2_label.Location = new Point(tpm2_label.Location.X + (tpm2_label.Size.Width / 2) - (fg2_label.Size.Width / 2), fg2_label.Location.Y);
            ft2_label.Text = games.Average(g => g.Ftm).ToString("0.0") + @"/" + games.Average(g => g.Fta).ToString("0.0") + @" - " + games.Average(g => g.FtPer).ToString("0.0") + @"%";
            ft2_label.Location = new Point(tpm2_label.Location.X + (tpm2_label.Size.Width / 2) - (ft2_label.Size.Width / 2), ft2_label.Location.Y);
            to2_label.Text = games.Average(g => g.To).ToString("0.0");
        }

        private void min_label_Click(object sender, EventArgs e)
        {
            UpdateCompareCharts("Min");
        }

        private void reb_label_Click(object sender, EventArgs e)
        {
            UpdateCompareCharts("Reb");
        }

        private void ast_label_Click(object sender, EventArgs e)
        {
            UpdateCompareCharts("Ast");
        }

        private void stl_label_Click(object sender, EventArgs e)
        {
            UpdateCompareCharts("Stl");
        }

        private void blk_label_Click(object sender, EventArgs e)
        {
            UpdateCompareCharts("Blk");
        }

        private void tpm_label_Click(object sender, EventArgs e)
        {
            UpdateCompareCharts("Tpm");
        }

        private void fg_label_Click(object sender, EventArgs e)
        {
            UpdateCompareCharts("FgPer");
        }

        private void ft_label_Click(object sender, EventArgs e)
        {
            UpdateCompareCharts("FtPer");
        }

        private void to_label_Click(object sender, EventArgs e)
        {
            UpdateCompareCharts("To");
        }



        private void pts_label_Click(object sender, EventArgs e)
        {
            UpdateCompareCharts("Pts");
        }



        private void UpdateCompareCharts(string colName)
        {
            Type gameStatsType = typeof(GameStats);
            FieldInfo fieldInfo = gameStatsType.GetField(colName);
            GameStats[] games = FilterReleventGames(compareMode_comboBox.GetItemText(compareMode_comboBox.SelectedItem),
                    compare_last_comboBox.GetItemText(compare_last_comboBox.SelectedItem), player1);

            var y = games.Select(g => double.Parse(fieldInfo.GetValue(g).ToString())).ToArray();
            var nameArray = player1.PlayerName.Split(" ".ToCharArray());
            PrintChart(y, nameArray[0].Substring(0, 1) + "." + nameArray[1].Substring(0, 1) + ".", compare_chart, 0);

            games = FilterReleventGames(compareMode_comboBox.GetItemText(compareMode_comboBox.SelectedItem),
                   compare_last_comboBox.GetItemText(compare_last_comboBox.SelectedItem), player2);
            y = games.Select(g => double.Parse(fieldInfo.GetValue(g).ToString())).ToArray();
            nameArray = player2.PlayerName.Split(" ".ToCharArray());
            PrintChart(y, nameArray[0].Substring(0, 1) + "." + nameArray[1].Substring(0, 1) + ".", compare_chart, 1);

            compare_chart.Titles[0].Text = player1.PlayerName + " Vs " + player2.PlayerName + Environment.NewLine + colName;
        }

        private void copyCompare_button_Click(object sender, EventArgs e)
        {
            string text = player1.PlayerName + ", Last " + compare_last_comboBox.GetItemText(compare_last_comboBox.SelectedItem) + " : " + Environment.NewLine;
            text += "Pts: " + pts1_label.Text + ", Reb: " + reb1_label.Text + ", Ast: " + ast1_label.Text + ", Stl: " + stl1_label.Text +
                    ", Blk: " + blk1_label.Text + ", Tpm: " + tpm1_label.Text + ", FgPer: " + fg1_label.Text +
                    ", FtPer: " + ft1_label.Text + ", Min : " + min1_label.Text;

            text += Environment.NewLine + player2.PlayerName + ", Last " + compare_last_comboBox.GetItemText(compare_last_comboBox.SelectedItem) + " : " + Environment.NewLine;
            text += "Pts: " + pts2_label.Text + ", Reb: " + reb2_label.Text + ", Ast: " + ast2_label.Text + ", Stl: " + stl2_label.Text +
                    ", Blk: " + blk2_label.Text + ", Tpm: " + tpm2_label.Text + ", FgPer: " + fg2_label.Text +
                    ", FtPer: " + ft2_label.Text + ", Min : " + min2_label.Text;

            Clipboard.SetText(text);
        }

        private void copyChart_button_Click(object sender, EventArgs e)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                compare_chart.SaveImage(ms, ChartImageFormat.Bmp);
                Bitmap bm = new Bitmap(ms);
                Clipboard.SetImage(bm);
            }
        }
        #endregion


    }
}
