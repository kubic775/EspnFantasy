using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NBAFantasy.Models
{
    public partial class YahooTeamStats
    {
        public long Pk { get; set; }
        public DateTime GameDate { get; set; }
        public int? YahooTeamId { get; set; }
        public long? Pts { get; set; }
        public long? Reb { get; set; }
        public long? Ast { get; set; }
        public long? Tpm { get; set; }
        public long? Fga { get; set; }
        public long? Fgm { get; set; }
        public double? FgPer { get; set; }
        public long? Fta { get; set; }
        public long? Ftm { get; set; }
        public double? FtPer { get; set; }
        public long? Stl { get; set; }
        public long? Blk { get; set; }
        public long? To { get; set; }
        public int? Gp { get; set; }
    }
}
