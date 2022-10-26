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
    public partial class DraftPicksForm : Form
    {
        public DraftPicksForm()
        {
            InitializeComponent();
        }

        private void calc_button_Click(object sender, EventArgs e)
        {
            try
            {
                int numOfTeams = int.Parse(numOfTeams_textBox.Text);
                int pos = int.Parse(draftPos_textBox.Text);

                var first = Enumerable.Range(1, numOfTeams).ToArray();
                var total = Enumerable.Empty<int>();
                for (int i = 0; i < 6; i++)
                    total = total.Concat(first).Concat(first.Reverse());
                var picksOrder = total.Concat(first).ToArray();

                var result = Enumerable.Range(0, picksOrder.Count())
                    .Where(i => picksOrder[i] == pos).Select(i => i + 1).ToList();

                richTextBox.AppendText(pos + " - " + string.Join(", ", result) + Environment.NewLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void clear_button_Click(object sender, EventArgs e)
        {
            richTextBox.Clear();
        }

        private void copy_button_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextBox.Text);
        }
    }
}
