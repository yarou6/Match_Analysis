using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Match_Analysis.Model;

namespace Match_Analysis.VM
{
    public class PlayerSelection : BaseVM
    {
        private Player selectedPlayer;

        public Player SelectedPlayer
        {
            get => selectedPlayer;
            set
            {
                selectedPlayer = value;
                Signal();
            }
        }

        public PlayerSelection(Player player)
        {
            selectedPlayer = player;
        }
    }
}
