using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Match_Analysis.Model;
using Match_Analysis.View;

namespace Match_Analysis.VM
{
    internal class AddEditMatch : BaseVM
    {

        private Match newMatch = new();

        public Match NewMatch
        {
            get => newMatch;
            set
            {
                newMatch = value;
                Signal();
            }
        }

        public List<Team> Teams
        {
            get => teams;
            set
            {
                teams = value;
                Signal();
            }
        }

        private Team selectedMatch1;
        public Team SelectedMatch1
        {
            get => selectedMatch1;
            set
            {
                selectedMatch1 = value;
                Signal();
            }
        }

        private Team selectedMatch2;
        public Team SelectedMatch2
        {
            get => selectedMatch2;
            set
            {
                selectedMatch2 = value;
                Signal();
            }
        }

        private ObservableCollection<Match> matches = new();
        public ObservableCollection<Match> Matches
        {
            get => matches;
            set
            {
                matches = value;
                Signal();
            }
        }

        public CommandMvvm AddMatch { get; set; }
        public CommandMvvm AddInf1 { get; set; }
        public CommandMvvm AddInf2 { get; set; }

        public AddEditMatch()
        {
            SelectAll();

            AddMatch = new CommandMvvm(() =>
            {
                
                if (NewMatch.Id == 0)
                {
                    NewMatch.TeamId1 = SelectedMatch1.Id;
                    NewMatch.TeamId2 = SelectedMatch2.Id;

                    MatchDB.GetDb().Insert(NewMatch);
                }
                else MatchDB.GetDb().Update(NewMatch);
                close?.Invoke();







                SelectAll();
            }, () =>
                NewMatch.TeamScore1 >= 0 &&
                NewMatch.TeamScore1 <= 11 &&
                NewMatch.TeamScore2 >= 0 &&
                NewMatch.TeamScore2 <= 11 );


            AddInf1 = new CommandMvvm(() =>
            {
                new DobavInfPlayer().ShowDialog();

            }, () => true);

            AddInf2 = new CommandMvvm(() =>
            {
                new DobavInfPlayer().ShowDialog();

            }, () => true);

        }

        public void SetMatch(Match selectedMatch)
        {
            NewMatch = selectedMatch;
        }


        Action close;
        private List<Team> teams = new();

        internal void SetClose(Action close)
        {
            this.close = close;
        }

        private void SelectAll()
        {
            Teams = new List<Team>(TeamDB.GetDb().SelectAll());
            Matches = new ObservableCollection<Match>(MatchDB.GetDb().SelectAll());
        }
    }
}
