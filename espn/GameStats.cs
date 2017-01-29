using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace espn
{
    public class GameStats
    {
        public DateTime GameDate;
        public int Pts, Reb, Ast, Tpm, Tpa, Fga, Fgm, Ftm, Fta, Stl, Blk, To, Min, Pf;
        public double FtPer, FgPer, TpPer;

        public GameStats(string gameStr)
        {
            string[] stats = gameStr.Split(new[] { @"<td>", @"</td>" }, StringSplitOptions.RemoveEmptyEntries);

            var temp = stats[1].Substring(4).Split('/');
            var month = int.Parse(temp[0]);
            var day = int.Parse(temp[1]);
            GameDate = new DateTime(month < 10 ? 2017 : 2016, month, day);

            Min = int.Parse(stats[4].Substring(stats[4].IndexOf(">") + 1));

            temp = stats[5].Substring(stats[5].IndexOf(">") + 1).Split('-');
            Fgm = int.Parse(temp[0]);
            Fga = int.Parse(temp[1]);
            FgPer = double.Parse(stats[6].Substring(stats[6].IndexOf(">") + 1)) * 100;
            temp = stats[7].Substring(stats[7].IndexOf(">") + 1).Split('-');
            Tpm = int.Parse(temp[0]);
            Tpa = int.Parse(temp[1]);
            TpPer = double.Parse(stats[8].Substring(stats[8].IndexOf(">") + 1)) * 100;
            temp = stats[9].Substring(stats[9].IndexOf(">") + 1).Split('-');
            Ftm = int.Parse(temp[0]);
            Fta = int.Parse(temp[1]);
            FtPer = double.Parse(stats[10].Substring(stats[10].IndexOf(">") + 1)) * 100;

            Reb = int.Parse(stats[11].Substring(stats[11].IndexOf(">") + 1));
            Ast = int.Parse(stats[12].Substring(stats[12].IndexOf(">") + 1));
            Blk = int.Parse(stats[13].Substring(stats[13].IndexOf(">") + 1));
            Stl = int.Parse(stats[14].Substring(stats[14].IndexOf(">") + 1));
            Pf = int.Parse(stats[15].Substring(stats[15].IndexOf(">") + 1));
            To = int.Parse(stats[16].Substring(stats[16].IndexOf(">") + 1));
            Pts = int.Parse(stats[17].Substring(stats[17].IndexOf(">") + 1));
        }


    }
}
