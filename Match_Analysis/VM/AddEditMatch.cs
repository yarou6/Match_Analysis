using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Match_Analysis.Model;
using Match_Analysis.View;

namespace Match_Analysis.VM
{
    internal class AddEditMatch : BaseVM
    {

        private Match_Analysis.Model.Match newMatch = new();

        public Match_Analysis.Model.Match NewMatch
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

        private ObservableCollection<Match_Analysis.Model.Match> matches = new();
        public ObservableCollection<Match_Analysis.Model.Match> Matches
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
                NewMatch.TeamId1 = SelectedMatch1.Id;
                NewMatch.TeamId2 = SelectedMatch2.Id;
                
                var playerHistoryDb = PlayerHistoryDB.GetDb();
                if (!playerHistoryDb.BothTeamsHaveEnoughPlayers(NewMatch.TeamId1, NewMatch.TeamId2, NewMatch.Date))
                {
                    MessageBox.Show("В обеих командах должно быть не менее 11 игроков на дату матча.");
                    return; // Прервать сохранение
                }
                

                if (NewMatch.Id == 0)
                    MatchDB.GetDb().Insert(NewMatch);
                else
                    MatchDB.GetDb().Update(NewMatch);

                close?.Invoke();
                SelectAll();
            }, () =>
                SelectedMatch1 != null &&
                SelectedMatch2 != null &&
                SelectedMatch1.Id != SelectedMatch2.Id &&
                NewMatch.TeamScore1 >= 0 &&
                NewMatch.TeamScore1 <= 11 &&
                NewMatch.TeamScore2 >= 0 &&
                NewMatch.TeamScore2 <= 11);


            AddInf1 = new CommandMvvm(() =>
            {
               
                NewMatch.TeamId1 = SelectedMatch1.Id;
                NewMatch.TeamId2 = SelectedMatch2.Id;

                var playerHistoryDb = PlayerHistoryDB.GetDb();
                if (!playerHistoryDb.BothTeamsHaveEnoughPlayers(NewMatch.TeamId1, NewMatch.TeamId2, NewMatch.Date))
                {
                    MessageBox.Show("В обеих командах должно быть не менее 11 игроков на дату матча.");
                    return; // Прервать сохранение
                }

                if (NewMatch.Id == 0)
                {
                    MatchDB.GetDb().Insert(NewMatch); // 💾 Сохраняем матч
                }
                var vm = new AddInfPlayer();
                vm.InitializePlayers(NewMatch.Id, NewMatch.TeamScore1, SelectedMatch1, NewMatch.Date);
                var window = new DobavInfPlayer { DataContext = vm };
                vm.SetClose(window.Close);
                window.ShowDialog();
            }, () =>
            SelectedMatch1 != null &&
            SelectedMatch2 != null &&
            SelectedMatch1.Id != SelectedMatch2.Id &&
            NewMatch.TeamScore1 >= 0 &&
            NewMatch.TeamScore1 <= 11 &&
            NewMatch.TeamScore2 >= 0 &&
            NewMatch.TeamScore2 <= 11);




            AddInf2 = new CommandMvvm(() =>
            {
                NewMatch.TeamId1 = SelectedMatch1.Id;
                NewMatch.TeamId2 = SelectedMatch2.Id;
                

                var playerHistoryDb = PlayerHistoryDB.GetDb();
                if (!playerHistoryDb.BothTeamsHaveEnoughPlayers(NewMatch.TeamId1, NewMatch.TeamId2, NewMatch.Date))
                {
                    MessageBox.Show("В обеих командах должно быть не менее 11 игроков на дату матча.");
                    return; // Прервать сохранение
                }

                if (NewMatch.Id == 0)
                {
                    MatchDB.GetDb().Insert(NewMatch); // 💾 Сохраняем матч
                }
                var vm = new AddInfPlayer();
                vm.InitializePlayers(NewMatch.Id, NewMatch.TeamScore2, SelectedMatch2, NewMatch.Date);
                var window = new DobavInfPlayer { DataContext = vm };
                vm.SetClose(window.Close);
                window.ShowDialog();
            }, () =>
            SelectedMatch1 != null &&
            SelectedMatch2 != null &&
            SelectedMatch1.Id != SelectedMatch2.Id &&
            NewMatch.TeamScore1 >= 0 &&
            NewMatch.TeamScore1 <= 11 &&
            NewMatch.TeamScore2 >= 0 &&
            NewMatch.TeamScore2 <= 11);

        }

        public void SetMatch(Match_Analysis.Model.Match selectedMatch)
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
            Matches = new ObservableCollection<Match_Analysis.Model.Match>(MatchDB.GetDb().SelectAll());
        }
    }
}
