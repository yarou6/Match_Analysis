using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Match_Analysis.Model;

namespace Match_Analysis.VM
{
    internal class AddInfPlayer : BaseVM
    {
        public List<Player> Players
        {
            get => players;
            set
            {
                players = value;
                Signal();
            }
        }

        private Match selectedPlayerGoal;
        public Match SelectedPlayerGoal
        {
            get => selectedPlayerGoal;
            set
            {
                selectedPlayerGoal = value;
                Signal();
            }
        }

        private Match selectedPlayerAssist;
        public Match SelectedPlayerAssist
        {
            get => selectedPlayerAssist;
            set
            {
                selectedPlayerAssist = value;
                Signal();
            }
        }



        public CommandMvvm AddPlayer { get; set; }
        public AddInfPlayer()
        {
            SelectAll();
            AddPlayer = new CommandMvvm(() =>
            {

                close?.Invoke();

            }, () => true);


        }
        Action close;
        private List<Player> players = new();
        internal void SetClose(Action close)
        {
            this.close = close;
        }

        private void SelectAll()
        {
            Players = new List<Player>(PlayerDB.GetDb().SelectAll());
        }
    }
}
