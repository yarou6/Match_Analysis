using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match_Analysis.Model
{
    public class Player
    {

        public int Id { get; set; } 
        public int Age { get; set; }
        public string PlayerPosition { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }

        public int TeamId { get; set; }
        public Team Team { get; set; }

        public string FIO => Surname + " " + Name[0] + "." + " " + Patronymic[0] + ".";

    }
}
