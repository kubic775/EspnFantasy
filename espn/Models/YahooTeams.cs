﻿using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace espn.Models
{
    public partial class YahooTeams
    {
        public long Pk { get; set; }
        public long TeamId { get; set; }
        public string TeamName { get; set; }
    }
}