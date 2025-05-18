using Match_Analysis.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
        private Match selectedMatch;
        public Match SelectedMatch
        {
            get => selectedMatch;
            set
            {
                selectedMatch = value;
                Signal();
            }
        }
        public CommandMvvm Vozvrat { get; set; }

        public CommandMvvm RemoveHist { get; set; }
        public ProsmotrMatchHistory()
        {
            SelectAll();

            Vozvrat = new CommandMvvm(() =>
            {

                close?.Invoke();

            }, () => true);


           
            RemoveHist = new CommandMvvm(() =>
            {

                var match = MessageBox.Show("Вы уверены что хотите удалить матч?", "Подтверждение", MessageBoxButton.YesNo);

                if (match == MessageBoxResult.Yes)
                {
                    PlayerStatisticsDB.GetDb().DeleteByMatchId(SelectedMatch.Id);
                    MatchDB.GetDb().Remove(SelectedMatch);
                }
                SelectAll();
            }, () => SelectedMatch != null);


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