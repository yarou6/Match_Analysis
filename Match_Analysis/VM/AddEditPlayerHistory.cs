using Match_Analysis.Model;
using Match_Analysis.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

        private Player newPlayer = new();

        public Player NewPlayer
        {
            get => newPlayer;
            set
            {
                newPlayer = value;
                Signal();
            }
        }

        private Team selectedTeam;

        public Team SelectedTeam
        {
            get => selectedTeam;
            set
            {
                selectedTeam = value;
                Signal();
            }
        }
        public CommandMvvm AddTeam { get; set; }

        public CommandMvvm EditTeam { get; set; }

        public CommandMvvm Exit { get; set; }

        public AddEditPlayerHistory()
        {


            AddTeam = new CommandMvvm(() =>
            {
                if (PlayerHistories.FirstOrDefault(p => p.ReleaseDate == null) != null)
                {
                   MessageBox.Show("Вы уверены что хотите удалить игрока?");
                    return;
                }
                    
                NewPlayerHistory.Id = 0;
                NewPlayerHistory.TeamId = SelectedTeam.Id;


                    if (NewPlayerHistory.ReleaseDate == new DateTime())
                        NewPlayerHistory.ReleaseDate = null;
                    
                NewPlayer.TeamId = NewPlayerHistory.TeamId;
                NewPlayerHistory.PlayerId = NewPlayer.Id;
                PlayerHistoryDB.GetDb().Insert(NewPlayerHistory);

                SelectAll();
                NewPlayerHistory = new PlayerHistory();
            }, () => true);


            

            EditTeam = new CommandMvvm(() =>
            {

                NewPlayerHistory.TeamId = SelectedTeam.Id;
                NewPlayer.TeamId = NewPlayerHistory.TeamId;
                NewPlayerHistory.PlayerId = NewPlayer.Id;
                PlayerHistoryDB.GetDb().Update(NewPlayerHistory);

                NewPlayerHistory = new PlayerHistory();

            }, () => NewPlayerHistory.Id != 0); 
            

            Exit = new CommandMvvm(() =>
            {
                NewPlayerHistory.ReleaseDate = null;
                close?.Invoke();

            }, () => true);

        }



        public void SetPlayer(Player editPlayer)
        {
            NewPlayer = editPlayer;
            SelectAll();
        }

        Action close;
        private List<Team> teams = new();

        internal void SetClose(Action close)
        {
            this.close = close;
        }


        private void SelectAll()
        {
            PlayerHistories = new ObservableCollection<PlayerHistory>(PlayerHistoryDB.GetDb().SelectPlayer(NewPlayer.Id));
            Teams = new List<Team>(TeamDB.GetDb().SelectAll());
        }
    }
}
