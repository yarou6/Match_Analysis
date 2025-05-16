using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match_Analysis.Model
{
    public class PlayerHistory
    {
        public int Id { get; set; }
        public DateTime? EntryDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int PlayerId { get; set; }
        public Player Player { get; set; }
        public Team Team { get; set; }
        public int? TeamId { get; set; }



    }
}
