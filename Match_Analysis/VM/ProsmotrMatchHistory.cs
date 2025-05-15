using Match_Analysis.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match_Analysis.VM
{
    internal class ProsmotrMatchHistory : BaseVM
    {

        private ObservableCollection<Match> matches;
        public ObservableCollection<Match> Matches
        {
            get => matches;
            set
            {
                matches = value;
                Signal();
            }
        }

        public CommandMvvm Vozvrat { get; set; }
        public ProsmotrMatchHistory()
        {
            SelectAll();

            Vozvrat = new CommandMvvm(() =>
            {

                close?.Invoke();

            }, () => true);


        }

        Action close;

        internal void SetClose(Action close)
        {
            this.close = close;
        }


        private void SelectAll()
        {
            Matches = new ObservableCollection<Match>(MatchDB.GetDb().SelectAll());
        }
    }
}