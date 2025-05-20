using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match_Analysis.Model
{
    public class PlayerStatView
    {
        public string FIO { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public string TeamTitle { get; set; }
        public int Total => Goals + Assists; // ✅ Голы + Пас
    }
}
