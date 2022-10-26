using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NBAFantasy
{
    public partial class PlayersForm : Form
    {
        public string PlayersStr = String.Empty;
        public PlayersForm()
        {
            InitializeComponent();
        }

        public PlayersForm(string players) : this()
        {
            //InitializeComponent();
            InitList(players);
            AcceptButton = OK_button;
        }

        private void InitList(string players)
        {
            var playerList = players.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            players_checkedListBox.Items.Clear();
            players_checkedListBox.Items.AddRange(playerList);
            for (int i = 0; i < players_checkedListBox.Items.Count; i++)
            {
                players_checkedListBox.SetItemChecked(i, true);
            }
        }

        private void OK_button_Click(object sender, EventArgs e)
        {
            List<string> items = (from object itemChecked in players_checkedListBox.CheckedItems select itemChecked.ToString()).ToList();
            PlayersStr = string.Join(",", items) + ",";
            Close();
            DialogResult = DialogResult.OK;
        }
    }
}
