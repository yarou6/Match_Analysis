using Match_Analysis.Model;
using Match_Analysis.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match_Analysis.VM
{
    internal class AddEditPlayerHistory : BaseVM
    {
        private PlayerHistory newPlayerHistory = new();

        public PlayerHistory NewPlayerHistory
        {
            get => newPlayerHistory;
            set
            {
                newPlayerHistory = value;
                Signal();
            }
        }

        private ObservableCollection<PlayerHistory> playerHistories = new();
        public ObservableCollection<PlayerHistory> PlayerHistories
        {
            get => playerHistories;
            set
            {
                playerHistories = value;
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


        public CommandMvvm AddTeam { get; set; }

        public CommandMvvm Exit { get; set; }

        public AddEditPlayerHistory()
        {
            AddTeam = new CommandMvvm(() =>
            {
                PlayerHistoryDB.GetDb().Insert(NewPlayerHistory);
                SelectAll();

            }, () => true);

            Exit = new CommandMvvm(() =>
            {

                if(newPlayerHistory.ReleaseDate == null)
                {
                
                    PlayerHistoryDB.GetDb().Insert(NewPlayerHistory);

                }
                
                close?.Invoke();

            }, () => true);

        }



        public void SetPlayerHistory(PlayerHistory selectedPlayerHistory)
        {
            NewPlayerHistory = selectedPlayerHistory;
            SelectedTeam();
        }

        Action close;
        private List<Team> teams = new();

        internal void SetClose(Action close)
        {
            this.close = close;
        }

        public void SelectedTeam()
        {
            Teams = TeamDB.GetDb().SelectAll();
            if (NewPlayerHistory.Team != null)
            {
                NewPlayerHistory.Team = Teams.FirstOrDefault(s => s.Id == NewPlayerHistory.TeamId);
            }
        }

        private void SelectAll()
        {
            PlayerHistories = new ObservableCollection<PlayerHistory>(PlayerHistoryDB.GetDb().SelectAll());
        }
    }
}
