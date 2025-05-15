using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match_Analysis.Model
{
    public class Match
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public Team Team1 { get; set; }

        public Team Team2 { get; set; }

        public int TeamId1 { get; set; }

        public int TeamId2 { get; set; }

        public int TeamScore1 { get; set; }

        public int TeamScore2 { get; set; }

    }
}
