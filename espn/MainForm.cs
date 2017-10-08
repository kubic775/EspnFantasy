using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;


namespace espn
{
    public delegate void PlayerSelectedDelegate(string name);

    public partial class MainForm : Form
    {
        #region Init & Gui
        public static FactorsForm Factors;
        private Player _player, _player1, _player2;
        private int _year = DateTime.Now.Month > 9 ? DateTime.Now.Year + 1 : DateTime.Now.Year;

        public MainForm()
        {
            //PlayersList.UpdatePlayersFromFile("NewNBAPlayers.txt");
            InitializeComponent();
            PlayersList.CreatePlayersList();
            InitGui();
        }

        private void InitGui()
        {
            Factors = new FactorsForm();

            stat_chart.Series[0].Points.Clear();
            stat_chart.Series[1].Points.Clear();
            stat_chart.Series[0].XValueType = ChartValueType.DateTime;
            stat_chart.Series[1].XValueType = ChartValueType.DateTime;

            compare_chart.Series[0].Points.Clear();
            compare_chart.Series[0].XValueType = ChartValueType.DateTime;
            compare_chart.Series[1].XValueType = ChartValueType.DateTime;

            trade_chart.Series[0].XValueType = ChartValueType.String;
            trade_chart.Series[1].XValueType = ChartValueType.String;

            stats_dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            //Init AutoCompleteTextBox//
            foreach (var textBox in Utils.GetAll(this, typeof(AutoCompleteTextBox)))
            {
                ((AutoCompleteTextBox)textBox).Values = PlayersList.Players.Keys.ToArray();
            }

            playerName_textBox.PlayerSelectedEvent = PlayerInfoSelectPlayerEvent;
            sendPlayer_TextBox.PlayerSelectedEvent = SendPlayerSelectedEvent;
            receivePlayer_TextBox.PlayerSelectedEvent = ReceivedPlayerSelectedEvent;
            player1_TextBox.PlayerSelectedEvent = Player1CompareSelectedPlayerEvent;
            player2_TextBox.PlayerSelectedEvent = Player2CompareSelectedPlayerEvent;

            compareMode_comboBox.SelectedIndex = mode_comboBox.SelectedIndex = tradeMode_comboBox.SelectedIndex = year_comboBox.SelectedIndex = 0;
            compare_last_comboBox.SelectedIndex = tradeLast_comboBox.SelectedIndex = 3;
        }

        public void PrintChartWithString(double[] y, string[] x, string name, Chart chart, int seriesNum = 0)
        {
            chart.Series[seriesNum].Points.Clear();
            chart.Series[seriesNum].Name = name;

            for (int i = 0; i < x.Length; i++)
            {
                chart.Series[seriesNum].Points.AddXY(x[i], y[i]);
                chart.Series[seriesNum].Points[i].ToolTip = x[i] + " , " + y[i];
            }
        }

        private void PrintChartWithDates(double[] y, DateTime[] dates, string name, Chart chart, int seriesNum = 0)
        {
            chart.Series[seriesNum].Points.Clear();
            chart.Series[seriesNum].Name = name;

            for (int i = 0; i < y.Length; i++)
            {
                chart.Series[seriesNum].Points.AddXY(dates[i], y[i]);
                chart.Series[seriesNum].Points[i].ToolTip = y[i] + " - " + dates[i].ToString("d");
            }

            if (smooth_checkBox.Checked && chart == stat_chart)
            {
                y = Utils.Smooth(y, (int)Math.Ceiling((double)y.Length / 10));
                chart.Series["SmoothSeries"].Points.Clear();
                for (int i = 0; i < y.Length; i++)
                {
                    chart.Series["SmoothSeries"].Points.AddXY(dates[i], y[i]);
                    chart.Series["SmoothSeries"].Points[i].ToolTip = y[i].ToString("0.0");
                }
            }

        }

        #endregion

        #region PlayerInfo

        public async void PlayerInfoSelectPlayerEvent(string name)
        {
            UseWaitCursor = true;
            try
            {
                if (PlayersList.Players.ContainsKey(name))
                {
                    _player = await PlayersList.CreatePlayerAsync(name);
                    player_pictureBox.Load(_player.ImagePath);
                    playerNameLabel.Text = _player.PlayerName;
                    playerInfo_label.Text = _player.PlayerInfo + " | " + _player.Team;
                    button_max_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            UseWaitCursor = false;
        }

        private void button_7_Click(object sender, EventArgs e)
        {
            numOf_textBox.Text = Math.Min(7, _player.Games.Count).ToString();
        }

        private void button_15_Click(object sender, EventArgs e)
        {
            numOf_textBox.Text = Math.Min(15, _player.Games.Count).ToString();
        }

        private void button_30_Click(object sender, EventArgs e)
        {
            numOf_textBox.Text = Math.Min(30, _player.Games.Count).ToString();
        }

        private void button_max_Click(object sender, EventArgs e)
        {
            var games = _player.FilterGamesByYear(
                int.Parse(year_comboBox.GetItemText(year_comboBox.Items[year_comboBox.SelectedIndex])));
            numOf_textBox.Text = games.Length.ToString();
        }

        private void mode_comboBox_DropDownClosed(object sender, EventArgs e)
        {
            numOf_textBox_TextChanged(null, null);
        }

        private void year_comboBox_DropDownClosed(object sender, EventArgs e)
        {
            var games = _player.FilterGamesByYear(
                int.Parse(year_comboBox.GetItemText(year_comboBox.Items[year_comboBox.SelectedIndex])));
            if (numOf_textBox.Text.Equals(games.Length.ToString()))
                numOf_textBox_TextChanged(null, null);
            else
                numOf_textBox.Text = games.Length.ToString();
        }

        private void numOf_textBox_TextChanged(object sender, EventArgs e)
        {
            var games = _player?.FilterGames(year_comboBox.GetItemText(year_comboBox.Items[year_comboBox.SelectedIndex]),
                mode_comboBox.GetItemText(mode_comboBox.Items[mode_comboBox.SelectedIndex]), numOf_textBox.Text,
                zeroMinutes_checkBox.Checked, outliersMinutes_checkBox.Checked);

            if (games == null)
                return;
            UpdateTable(GameStats.GetAvgStats(games));
            var colName = stats_dataGridView.SelectedCells.Count > 0
                ? stats_dataGridView.Columns[stats_dataGridView.SelectedCells[0].ColumnIndex].Name
                : "Pts";
            var x = games.Select(g => g.GameDate).ToArray();
            PrintChartWithDates(GameStats.GetYVals(colName, games), x, colName, stat_chart);
        }

        private void zeroMinutes_checkBox_CheckStateChanged(object sender, EventArgs e)
        {
            numOf_textBox_TextChanged(null, null);
        }

        private void outliersMinutes_checkBox_CheckStateChanged(object sender, EventArgs e)
        {
            numOf_textBox_TextChanged(null, null);
        }

        private void UpdateTable(GameStats game)
        {
            stats_dataGridView.Rows[0].Cells["Gp"].Value = game.Gp;

            if (game.Gp == 0)
                return;

            stats_dataGridView.Rows[0].Cells["Min"].Value = game.Min.ToString("0.0");

            stats_dataGridView.Rows[0].Cells["FgmFga"].Value = game.Fgm.ToString("0.0") + "-" + game.Fga.ToString("0.0");
            stats_dataGridView.Rows[0].Cells["FgPer"].Value = game.FgPer.ToString("0.0") + "%";
            stats_dataGridView.Rows[0].Cells["TpmTpa"].Value = game.Tpm.ToString("0.0") + "-" + game.Tpa.ToString("0.0");
            stats_dataGridView.Rows[0].Cells["TpPer"].Value = game.TpPer.ToString("0.0") + "%";
            stats_dataGridView.Rows[0].Cells["FtmFta"].Value = game.Ftm.ToString("0.0") + "-" + game.Fta.ToString("0.0");
            stats_dataGridView.Rows[0].Cells["FtPer"].Value = game.FtPer.ToString("0.0") + "%";

            stats_dataGridView.Rows[0].Cells["Reb"].Value = game.Reb.ToString("0.0");
            stats_dataGridView.Rows[0].Cells["Ast"].Value = game.Ast.ToString("0.0");
            stats_dataGridView.Rows[0].Cells["Blk"].Value = game.Blk.ToString("0.0");
            stats_dataGridView.Rows[0].Cells["Stl"].Value = game.Stl.ToString("0.0");
            stats_dataGridView.Rows[0].Cells["Pf"].Value = game.Pf.ToString("0.0");
            stats_dataGridView.Rows[0].Cells["To"].Value = game.To.ToString("0.0");
            stats_dataGridView.Rows[0].Cells["Pts"].Value = game.Pts.ToString("0.0");
        }


        private void stats_dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var name = stats_dataGridView.Columns[e.ColumnIndex].Name;
            var games = _player.FilterGames(year_comboBox.GetItemText(year_comboBox.Items[year_comboBox.SelectedIndex]),
                mode_comboBox.GetItemText(mode_comboBox.Items[mode_comboBox.SelectedIndex]), numOf_textBox.Text,
                zeroMinutes_checkBox.Checked, outliersMinutes_checkBox.Checked);
            var y = GameStats.GetYVals(name, games);
            var x = games.Select(g => g.GameDate).ToArray();
            if (y.Length > 0)
                PrintChartWithDates(y, x, name, stat_chart);
            //PrintChart(y, name, stat_chart);
        }

        private void copyToClipboard_button_Click(object sender, EventArgs e)
        {
            string text = _player.PlayerName + ", Last " + numOf_textBox.Text + " : " + Environment.NewLine;
            text += "Pts: " + stats_dataGridView.Rows[0].Cells["Pts"].Value + ", " + "Reb: " + stats_dataGridView.Rows[0].Cells["Reb"].Value + ", " +
                          "Ast: " + stats_dataGridView.Rows[0].Cells["Ast"].Value + ", " + "Tpm: " + stats_dataGridView.Rows[0].Cells["TpmTpa"].Value.ToString().Split("-".ToCharArray())[0] + ", " +
                          "Stl: " + stats_dataGridView.Rows[0].Cells["Stl"].Value + ", " + "Blk: " + stats_dataGridView.Rows[0].Cells["Blk"].Value + ", " +
                          "FgPer: " + stats_dataGridView.Rows[0].Cells["FgPer"].Value + ", " + "FtPer: " + stats_dataGridView.Rows[0].Cells["FtPer"].Value + ", ";

            Clipboard.SetText(text);
        }

        private void coptStatChart_button_Click(object sender, EventArgs e)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stat_chart.SaveImage(ms, ChartImageFormat.Bmp);
                Bitmap bm = new Bitmap(ms);
                Clipboard.SetImage(bm);
            }
        }

        #endregion

        #region ComparePlayers

        private void Player1CompareSelectedPlayerEvent(string name)
        {
            UpdateComparePlayer(1);
        }

        private void Player2CompareSelectedPlayerEvent(string name)
        {
            UpdateComparePlayer(2);
        }

        private void compareMode_comboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdateComparePlayer(0);
        }

        private void compare_last_comboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdateComparePlayer(0);
        }

        private async void UpdateComparePlayer(int mode)
        {
            try
            {
                if (mode == 0 || mode == 1)
                {
                    if (compareMode_comboBox.SelectedIndex == -1 || compare_last_comboBox.SelectedIndex == -1 || player1_TextBox.Text == String.Empty)
                        return;
                    if (_player1 == null || _player1.Id != PlayersList.Players[player1_TextBox.Text])
                        _player1 = await PlayersList.CreatePlayerAsync(player1_TextBox.Text);

                    GameStats[] games = _player1.FilterGames("2018", compareMode_comboBox.GetItemText(compareMode_comboBox.SelectedItem),
                            compare_last_comboBox.GetItemText(compare_last_comboBox.SelectedItem));
                    UpdateCompareInfo1(games);
                }
                if (mode == 0 || mode == 2)
                {
                    if (compareMode_comboBox.SelectedIndex == -1 || compare_last_comboBox.SelectedIndex == -1 || player2_TextBox.Text == String.Empty)
                        return;
                    if (_player2 == null || _player2.Id != PlayersList.Players[player2_TextBox.Text])
                        _player2 = await PlayersList.CreatePlayerAsync(player2_TextBox.Text);
                    GameStats[] games = _player2.FilterGames("2018", compareMode_comboBox.GetItemText(compareMode_comboBox.SelectedItem),
                            compare_last_comboBox.GetItemText(compare_last_comboBox.SelectedItem));
                    UpdateCompareInfo2(games);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void UpdateCompareInfo1(GameStats[] games)
        {
            var avgGame = GameStats.GetAvgStats(games);

            gp1_label.Text = avgGame.Gp.ToString();
            min1_label.Text = avgGame.Min.ToString("0.0");
            pts1_label.Text = avgGame.Pts.ToString("0.0");
            reb1_label.Text = avgGame.Reb.ToString("0.0");
            ast1_label.Text = avgGame.Ast.ToString("0.0");
            stl1_label.Text = avgGame.Stl.ToString("0.0");
            blk1_label.Text = avgGame.Blk.ToString("0.0");
            tpm1_label.Text = avgGame.Tpm.ToString("0.0");
            fg1_label.Text = avgGame.Fgm.ToString("0.0") + @"/" + avgGame.Fga.ToString("0.0") + @" - " + avgGame.FgPer.ToString("0.0") + @"%";
            fg1_label.Location = new Point(tpm1_label.Location.X + (tpm1_label.Size.Width / 2) - (fg1_label.Size.Width / 2), fg1_label.Location.Y);
            ft1_label.Text = avgGame.Ftm.ToString("0.0") + @"/" + avgGame.Fta.ToString("0.0") + @" - " + avgGame.FtPer.ToString("0.0") + @"%";
            ft1_label.Location = new Point(tpm1_label.Location.X + (tpm1_label.Size.Width / 2) - (ft1_label.Size.Width / 2), ft1_label.Location.Y);
            to1_label.Text = avgGame.To.ToString("0.0");
        }


        private void UpdateCompareInfo2(GameStats[] games)
        {
            var avgGame = GameStats.GetAvgStats(games);
            gp2_label.Text = avgGame.Gp.ToString();
            min2_label.Text = avgGame.Min.ToString("0.0");
            pts2_label.Text = avgGame.Pts.ToString("0.0");
            reb2_label.Text = avgGame.Reb.ToString("0.0");
            ast2_label.Text = avgGame.Ast.ToString("0.0");
            stl2_label.Text = avgGame.Stl.ToString("0.0");
            blk2_label.Text = avgGame.Blk.ToString("0.0");
            tpm2_label.Text = avgGame.Tpm.ToString("0.0");
            fg2_label.Text = avgGame.Fgm.ToString("0.0") + @"/" + avgGame.Fga.ToString("0.0") + @" - " + avgGame.FgPer.ToString("0.0") + @"%";
            fg2_label.Location = new Point(tpm2_label.Location.X + (tpm2_label.Size.Width / 2) - (fg2_label.Size.Width / 2), fg2_label.Location.Y);
            ft2_label.Text = avgGame.Ftm.ToString("0.0") + @"/" + avgGame.Fta.ToString("0.0") + @" - " + avgGame.FtPer.ToString("0.0") + @"%";
            ft2_label.Location = new Point(tpm2_label.Location.X + (tpm2_label.Size.Width / 2) - (ft2_label.Size.Width / 2), ft2_label.Location.Y);
            to2_label.Text = avgGame.To.ToString("0.0");
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
            if (_player1 != null)
            {
                GameStats[] games = _player1.FilterGames("2018",
                compareMode_comboBox.GetItemText(compareMode_comboBox.SelectedItem),
                compare_last_comboBox.GetItemText(compare_last_comboBox.SelectedItem));

                var y = games.Select(g => double.Parse(fieldInfo.GetValue(g).ToString())).ToArray();
                var x = games.Select(g => g.GameDate).ToArray();
                PrintChartWithDates(y, x, _player1.PlayerName, compare_chart, 0);
                if (_player1?.Id == _player2?.Id)
                    return;
            }


            if (_player2 != null)
            {
                GameStats[] games = _player2.FilterGames("2018",
               compareMode_comboBox.GetItemText(compareMode_comboBox.SelectedItem),
               compare_last_comboBox.GetItemText(compare_last_comboBox.SelectedItem));

                var y = games.Select(g => double.Parse(fieldInfo.GetValue(g).ToString())).ToArray();
                var x = games.Select(g => g.GameDate).ToArray();
                PrintChartWithDates(y, x, _player2.PlayerName, compare_chart, 1);
            }

            compare_chart.Titles[0].Text = _player1?.PlayerName + " Vs " + _player2?.PlayerName + Environment.NewLine + colName;
        }

        private void tradePlayers_button_Click(object sender, EventArgs e)
        {
            tradeMode_comboBox.SelectedIndex = compareMode_comboBox.SelectedIndex;
            tradeLast_comboBox.SelectedIndex = compare_last_comboBox.SelectedIndex;
            sendPlayer_TextBox.Text = player1_TextBox.Text;
            receivePlayer_TextBox.Text = player2_TextBox.Text;
            sendPlayer_TextBox.PlayerSelectedEvent.Invoke(sendPlayer_TextBox.Text);
            receivePlayer_TextBox.PlayerSelectedEvent.Invoke(receivePlayer_TextBox.Text);
            tabControl.SelectedIndex = 2;
        }


        private void copyCompare_button_Click(object sender, EventArgs e)
        {
            string text = _player1.PlayerName + ", Last " + compare_last_comboBox.GetItemText(compare_last_comboBox.SelectedItem) + " : " + Environment.NewLine;
            text += "Pts: " + pts1_label.Text + ", Reb: " + reb1_label.Text + ", Ast: " + ast1_label.Text + ", Stl: " + stl1_label.Text +
                    ", Blk: " + blk1_label.Text + ", Tpm: " + tpm1_label.Text + ", FgPer: " + fg1_label.Text +
                    ", FtPer: " + ft1_label.Text + ", To : " + to1_label.Text + ", Min : " + min1_label.Text;

            text += Environment.NewLine + _player2.PlayerName + ", Last " + compare_last_comboBox.GetItemText(compare_last_comboBox.SelectedItem) + " : " + Environment.NewLine;
            text += "Pts: " + pts2_label.Text + ", Reb: " + reb2_label.Text + ", Ast: " + ast2_label.Text + ", Stl: " + stl2_label.Text +
                    ", Blk: " + blk2_label.Text + ", Tpm: " + tpm2_label.Text + ", FgPer: " + fg2_label.Text +
                    ", FtPer: " + ft2_label.Text + ", To : " + to2_label.Text + ", Min : " + min2_label.Text;

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

        #region Trade Analyzer

        public void SendPlayerSelectedEvent(string name)
        {
            playersSent_textBox.Text = name + "," + playersSent_textBox.Text;
        }

        public void ReceivedPlayerSelectedEvent(string name)
        {
            playersReceived_textBox.Text = name + "," + playersReceived_textBox.Text;
        }


        private void clearList_button_Click(object sender, EventArgs e)
        {
            playersSent_textBox.Text = "";
            playersReceived_textBox.Text = "";
        }

        private void flip_button_Click(object sender, EventArgs e)
        {
            string temp = playersReceived_textBox.Text;
            playersReceived_textBox.Text = playersSent_textBox.Text;
            playersSent_textBox.Text = temp;

            temp = sendPlayer_TextBox.Text;
            sendPlayer_TextBox.Text = receivePlayer_TextBox.Text;
            receivePlayer_TextBox.Text = temp;
        }

        private async void trade_button_Click(object sender, EventArgs e)
        {
            UseWaitCursor = true;
            try
            {
                timePeriod_label.Text = "Last " + tradeLast_comboBox.GetItemText(tradeLast_comboBox.SelectedItem) + " " +
                                   tradeMode_comboBox.GetItemText(tradeMode_comboBox.SelectedItem);

                List<Player> sentPlayers = await PlayersList.CreatePlayersAsync(playersSent_textBox.Text.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
                List<Player> receiviedPlayers = await PlayersList.CreatePlayersAsync(playersReceived_textBox.Text.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));

                GameStats[] avgStatsSend = sentPlayers.Select(
                    p => GameStats.GetAvgStats(p.FilterGames("2018",
                        tradeMode_comboBox.GetItemText(tradeMode_comboBox.SelectedItem),
                        tradeLast_comboBox.GetItemText(tradeLast_comboBox.SelectedItem)))).ToArray();

                GameStats[] avgStatsrecevied = receiviedPlayers.Select(
                   p => GameStats.GetAvgStats(p.FilterGames("2018",
                       tradeMode_comboBox.GetItemText(tradeMode_comboBox.SelectedItem),
                       tradeLast_comboBox.GetItemText(tradeLast_comboBox.SelectedItem)))).ToArray();

                GameStats totalSend = GameStats.GetSumStats(avgStatsSend);
                GameStats totalReceived = GameStats.GetSumStats(avgStatsrecevied);
                GameStats diffStat = GameStats.GetDiffInStats(totalSend, totalReceived);

                UpdateTotalTradeLabelsh(totalSend, totalReceived);
                UpdateDiffTradeLabels(diffStat);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            UseWaitCursor = false;
        }


        private void UpdateTotalTradeLabelsh(GameStats totalSend, GameStats totalReceived)
        {
            pts1Trade_label.Text = totalSend.Pts.ToString("0.0");
            pts2Trade_label.Text = totalReceived.Pts.ToString("0.0");
            reb1Trade_label.Text = totalSend.Reb.ToString("0.0");
            reb2Trade_label.Text = totalReceived.Reb.ToString("0.0");
            ast1Trade_label.Text = totalSend.Ast.ToString("0.0");
            ast2Trade_label.Text = totalReceived.Ast.ToString("0.0");
            stl1Trade_label.Text = totalSend.Stl.ToString("0.0");
            stl2Trade_label.Text = totalReceived.Stl.ToString("0.0");
            blk1Trade_label.Text = totalSend.Blk.ToString("0.0");
            blk2Trade_label.Text = totalReceived.Blk.ToString("0.0");
            to1Trade_label.Text = totalSend.To.ToString("0.0");
            to2Trade_label.Text = totalReceived.To.ToString("0.0");
            tpm1Trade_label.Text = totalSend.Tpm.ToString("0.0");
            tpm2Trade_label.Text = totalReceived.Tpm.ToString("0.0");
            fg1Trade_label.Text = totalSend.Fgm.ToString("0.0") + "/" + totalSend.Fga.ToString("0.0") + " " + totalSend.FgPer.ToString("0.0") + " %";
            fg2Trade_label.Text = totalReceived.Fgm.ToString("0.0") + "/" + totalReceived.Fga.ToString("0.0") + " " + totalReceived.FgPer.ToString("0.0") + " %";
            ft1Trade_label.Text = totalSend.Ftm.ToString("0.0") + "/" + totalSend.Fta.ToString("0.0") + " " + totalSend.FtPer.ToString("0.0") + " %";
            ft2Trade_label.Text = totalReceived.Ftm.ToString("0.0") + "/" + totalReceived.Fta.ToString("0.0") + " " + totalReceived.FtPer.ToString("0.0") + " %";
            score1Trade_label.Text = totalSend.Score.ToString("0.0");
            score2Trade_label.Text = totalReceived.Score.ToString("0.0");

            fg1Trade_label.Location = new Point(tpm1Trade_label.Location.X + (tpm1Trade_label.Size.Width / 2) - (fg1Trade_label.Size.Width / 2), fg1Trade_label.Location.Y);
            fg2Trade_label.Location = new Point(tpm2Trade_label.Location.X + (tpm2Trade_label.Size.Width / 2) - (fg2Trade_label.Size.Width / 2), fg2Trade_label.Location.Y);
            ft1Trade_label.Location = new Point(tpm1Trade_label.Location.X + (tpm1Trade_label.Size.Width / 2) - (ft1Trade_label.Size.Width / 2), ft1Trade_label.Location.Y);
            ft2Trade_label.Location = new Point(tpm2Trade_label.Location.X + (tpm2Trade_label.Size.Width / 2) - (ft2Trade_label.Size.Width / 2), ft2Trade_label.Location.Y);
        }

        private void UpdateDiffTradeLabels(GameStats diffStat)
        {
            UpdateTradeLabel(ptsTrade_label, diffStat.Pts);
            UpdateTradeLabel(rebTrade_label, diffStat.Reb);
            UpdateTradeLabel(astTrade_label, diffStat.Ast);
            UpdateTradeLabel(stlTrade_label, diffStat.Stl);
            UpdateTradeLabel(blkTrade_label, diffStat.Blk);
            UpdateTradeLabel(tpmTrade_label, diffStat.Tpm);
            UpdateTradeLabel(toTrade_label, diffStat.To);
            UpdateTradeLabel(scoreTrade_label, diffStat.Score);
            UpdateTradeLabel(fgPerTrade_label, diffStat.FgPer, "%");
            UpdateTradeLabel(ftPerTrade_label, diffStat.FtPer, "%");

            if (diffStat.Score > 0)
            {
                score_label.Text = "Pass";
                score_label.ForeColor = Color.Green;
            }
            else if (diffStat.Score < 0)
            {
                score_label.Text = "Reject";
                score_label.ForeColor = Color.Red;
            }
            else
            {
                score_label.Text = "Equal";
                score_label.ForeColor = Color.DarkOrange;
            }
        }


        private void UpdateTradeLabel(Label label, double val, string extension = "")
        {
            label.Text = val.ToString("0.0") + " " + extension;
            label.ForeColor = val > 0 ? Color.Green : Color.Red;
        }

        private void copyTradeStats_button_Click(object sender, EventArgs e)
        {
            string text = "Last " + tradeLast_comboBox.GetItemText(tradeLast_comboBox.SelectedItem) + " " + tradeMode_comboBox.GetItemText(tradeMode_comboBox.SelectedItem) + Environment.NewLine;
            text += "Players Sent : " + playersSent_textBox.Text + Environment.NewLine;
            text += "Pts: " + pts1Trade_label.Text + ", Reb: " + reb1Trade_label.Text + ", Ast: " + ast1Trade_label.Text + ", Stl: " + stl1Trade_label.Text +
                    ", Blk: " + blk1Trade_label.Text + ", Tpm: " + tpm1Trade_label.Text + ", FgPer: " + fg1Trade_label.Text +
                    ", FtPer: " + ft1Trade_label.Text + ", To : " + to1Trade_label.Text;

            text += Environment.NewLine + "Players Received : " + playersReceived_textBox.Text + Environment.NewLine;
            text += "Pts: " + pts2Trade_label.Text + ", Reb: " + reb2Trade_label.Text + ", Ast: " + ast2Trade_label.Text + ", Stl: " + stl2Trade_label.Text +
                    ", Blk: " + blk2Trade_label.Text + ", Tpm: " + tpm2Trade_label.Text + ", FgPer: " + fg2Trade_label.Text +
                    ", FtPer: " + ft2Trade_label.Text + ", To : " + to2Trade_label.Text;

            text += Environment.NewLine + "Diff :" + Environment.NewLine;
            text += "Pts: " + ptsTrade_label.Text + ", Reb: " + rebTrade_label.Text + ", Ast: " + astTrade_label.Text + ", Stl: " + stlTrade_label.Text +
                    ", Blk: " + blkTrade_label.Text + ", Tpm: " + tpmTrade_label.Text + ", FgPer: " + fgPerTrade_label.Text +
                    ", FtPer: " + ftPerTrade_label.Text + ", To : " + toTrade_label.Text;

            Clipboard.SetText(text);
        }


        private void copyDiff_button_Click(object sender, EventArgs e)
        {
            string text = "Last " + tradeLast_comboBox.GetItemText(tradeLast_comboBox.SelectedItem) + " " + tradeMode_comboBox.GetItemText(tradeMode_comboBox.SelectedItem) + Environment.NewLine;
            text += "Players Sent : " + playersSent_textBox.Text + Environment.NewLine;
            text += "Players Received : " + playersReceived_textBox.Text + Environment.NewLine;
            text += Environment.NewLine + "Diff :" + Environment.NewLine;
            text += "Pts: " + ptsTrade_label.Text + ", Reb: " + rebTrade_label.Text + ", Ast: " + astTrade_label.Text + ", Stl: " + stlTrade_label.Text +
                    ", Blk: " + blkTrade_label.Text + ", Tpm: " + tpmTrade_label.Text + ", FgPer: " + fgPerTrade_label.Text +
                    ", FtPer: " + ftPerTrade_label.Text + ", To : " + toTrade_label.Text;
            Clipboard.SetText(text);
        }


        private void copyTradeChart_Click(object sender, EventArgs e)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                trade_chart.SaveImage(ms, ChartImageFormat.Bmp);
                Bitmap bm = new Bitmap(ms);
                Clipboard.SetImage(bm);
            }
        }

        private void astChartTrade_label_Click(object sender, EventArgs e)
        {
            UpdateTradeCharts("Ast");
        }

        private void stlChartTrade_label_Click(object sender, EventArgs e)
        {
            UpdateTradeCharts("Stl");
        }

        private void blkChartTrade_label_Click(object sender, EventArgs e)
        {
            UpdateTradeCharts("Blk");
        }

        private void tpmChartTrade_label_Click(object sender, EventArgs e)
        {
            UpdateTradeCharts("Tpm");
        }

        private void fgChartTrade_label_Click(object sender, EventArgs e)
        {
            UpdateTradeCharts("FgPer");
        }

        private void ftChartTrade_label_Click(object sender, EventArgs e)
        {
            UpdateTradeCharts("FtPer");
        }

        private void toChartTrade_label_Click(object sender, EventArgs e)
        {
            UpdateTradeCharts("To");
        }


        private void ptsChartTrade_label_Click(object sender, EventArgs e)
        {
            UpdateTradeCharts("Pts");
        }


        private void rebChartTrade_label_Click(object sender, EventArgs e)
        {
            UpdateTradeCharts("Reb");
        }

        private void playersReceived_textBox_Click(object sender, EventArgs e)
        {
            var playersForm = new PlayersForm(playersReceived_textBox.Text);
            if (playersForm.ShowDialog() == DialogResult.OK)
                playersReceived_textBox.Text = playersForm.PlayersStr;
        }


        private void playersSent_textBox_Click(object sender, EventArgs e)
        {
            var playersForm = new PlayersForm(playersSent_textBox.Text);
            if (playersForm.ShowDialog() == DialogResult.OK)
                playersSent_textBox.Text = playersForm.PlayersStr;
        }

        private void scoreChartTrade_label_Click(object sender, EventArgs e)
        {
            UpdateTradeCharts("Score");
        }

        private async void UpdateTradeCharts(string colName)
        {
            Type gameStatsType = typeof(GameStats);
            FieldInfo fieldInfo = gameStatsType.GetField(colName);


            List<Player> sentPlayers = await PlayersList.CreatePlayersAsync(playersSent_textBox.Text.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
            List<Player> receiviedPlayers = await PlayersList.CreatePlayersAsync(playersReceived_textBox.Text.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));

            var yValSend = new double[4];
            var yValRecevied = new double[4];

            yValSend[0] = double.Parse(fieldInfo.GetValue(GameStats.GetSumStats(sentPlayers.Select(p => GameStats.GetAvgStats(p.FilterGames("2018", "Games", "Max"))).ToArray())).ToString());
            yValRecevied[0] = double.Parse(fieldInfo.GetValue(GameStats.GetSumStats(receiviedPlayers.Select(p => GameStats.GetAvgStats(p.FilterGames("2018", "Games", "Max"))).ToArray())).ToString());
            yValSend[1] = double.Parse(fieldInfo.GetValue(GameStats.GetSumStats(sentPlayers.Select(p => GameStats.GetAvgStats(p.FilterGames(2018, "Games", 30))).ToArray())).ToString());
            yValRecevied[1] = double.Parse(fieldInfo.GetValue(GameStats.GetSumStats(receiviedPlayers.Select(p => GameStats.GetAvgStats(p.FilterGames(2018, "Games", 30))).ToArray())).ToString());
            yValSend[2] = double.Parse(fieldInfo.GetValue(GameStats.GetSumStats(sentPlayers.Select(p => GameStats.GetAvgStats(p.FilterGames(2018, "Games", 15))).ToArray())).ToString());
            yValRecevied[2] = double.Parse(fieldInfo.GetValue(GameStats.GetSumStats(receiviedPlayers.Select(p => GameStats.GetAvgStats(p.FilterGames(2018, "Games", 15))).ToArray())).ToString());
            yValSend[3] = double.Parse(fieldInfo.GetValue(GameStats.GetSumStats(sentPlayers.Select(p => GameStats.GetAvgStats(p.FilterGames(2018, "Games", 7))).ToArray())).ToString());
            yValRecevied[3] = double.Parse(fieldInfo.GetValue(GameStats.GetSumStats(receiviedPlayers.Select(p => GameStats.GetAvgStats(p.FilterGames(2018, "Games", 7))).ToArray())).ToString());

            var xVal = new[] { "Season", "30", "15", "7" };

            PrintChartWithString(yValSend, xVal, "Sent", trade_chart);
            PrintChartWithString(yValRecevied, xVal, "Receivied", trade_chart, 1);
            trade_chart.Titles[0].Text = colName;
        }


        private void screenshot_button_Click(object sender, EventArgs e)
        {
            using (var bmp = new Bitmap(trade_panel.Width, trade_panel.Height))
            {
                trade_panel.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                Clipboard.SetImage(bmp);
            }
        }

        private void loadPlayersToolStripMenuItem1_Click(object sender, EventArgs e)//Sent
        {
            try
            {
                playersSent_textBox.Text = File.ReadAllText("SentPlayers.txt");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void loadPlayersToolStripMenuItem2_Click(object sender, EventArgs e)//Received
        {
            try
            {
                playersReceived_textBox.Text = File.ReadAllText("ReceivedPlayers.txt");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void savePlayersToolStripMenuItem1_Click(object sender, EventArgs e)//Sent
        {
            try
            {
                File.WriteAllText("SentPlayers.txt", playersSent_textBox.Text);
                MessageBox.Show("Done");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void addNewPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox("Please Enter Player Name (For Example: Chris Paul)", "Add New Player", "Default", -1, -1);
            if (!string.Equals(name, ""))
                PlayersList.AddNewPlayer(name);
            InitGui();
        }



        private void savePlayersToolStripMenuItem2_Click(object sender, EventArgs e)//Received
        {
            try
            {
                File.WriteAllText("ReceivedPlayers.txt", playersReceived_textBox.Text);
                MessageBox.Show("Done");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        #endregion

        #region Players Rater

        private void setFactorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Factors.ShowDialog();
        }

        #endregion

    }
}
