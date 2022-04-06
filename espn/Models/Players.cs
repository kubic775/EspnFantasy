using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NBAFantasy.Models
{
    public partial class Players
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Team { get; set; }
        public int? Age { get; set; }
        public string Misc { get; set; }
        public int? TeamNumber { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
}
