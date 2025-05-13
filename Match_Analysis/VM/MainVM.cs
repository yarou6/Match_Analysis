using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Match_Analysis.Model;
using Match_Analysis.VM;
using Match_Analysis.View;

namespace Match_Analysis.VM
{
    internal class MainVM : BaseVM
    {

        private Team selectedteam;
        private ObservableCollection<Team> teams = new();
        
        public ObservableCollection<Team> Teams
        {
            get => teams;
            set
            {
                teams = value;
                Signal();
            }
        }

        public Team SelectedTeam
        {
            get => selectedteam;
            set
            {
                selectedteam = value;
                Signal();
            }
        }


        private Player selectedplayer;
        private ObservableCollection<Player> players = new();
        public ObservableCollection<Player> Players
        {
            get => players;
            set
            {
                players = value;
                Signal();
            }
        }

        public Player SelectedPlayer
        {
            get => selectedplayer;
            set
            {
                selectedplayer = value;
                Signal();
            }
        }


        private Match selectedMatch;
        private ObservableCollection<Match> matchs = new();
        public ObservableCollection<Match> Matchs
        {
            get => matchs;
            set
            {
                matchs = value;
                Signal();
            }
        }

        public Match SelectedMatch
        {
            get => selectedMatch;
            set
            {
                selectedMatch = value;
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
        private PlayerHistory selectedPlayerHistory;
        public PlayerHistory SelectedPlayerHistory
        {
            get => selectedPlayerHistory;
            set
            {
                selectedPlayerHistory = value;
                Signal();
            }
        }



        public CommandMvvm AddTeam { get; set; }
        public CommandMvvm EditTeam { get; set; }
        public CommandMvvm RemoveTeam { get; set; }


        public CommandMvvm AddPlayer { get; set; }
        public CommandMvvm EditPlayer { get; set; }
        public CommandMvvm RemovePlayer { get; set; }



        public CommandMvvm AddMatch { get; set; }
        public CommandMvvm HistMatch { get; set; }
        public CommandMvvm HistPlayer { get; set; }
        public CommandMvvm TournTable { get; set; }
        public CommandMvvm StatTourn { get; set; }

        public MainVM()
        {

            SelectAll();

            AddTeam = new CommandMvvm(() =>
            {
                new EditTeam(new Team()).ShowDialog();

                SelectAll();

            }, () => true);

            EditTeam = new CommandMvvm(() =>
            {
                new EditTeam(SelectedTeam).ShowDialog();

                SelectAll();

            }, () => SelectedTeam != null);

            RemoveTeam = new CommandMvvm(() =>
            {
                var teamvozvrta = MessageBox.Show("Вы уверены что хотите удалить игрока?", "Подтверждение", MessageBoxButton.YesNo);

                if (teamvozvrta == MessageBoxResult.Yes)
                {
                    TeamDB.GetDb().Remove(SelectedTeam);
                }
                SelectAll();
            }, () => SelectedTeam != null);







            AddPlayer = new CommandMvvm(() =>
            {
                new EditPlayer(new Player()).ShowDialog();

                SelectAll();

            }, () => true);

            EditPlayer = new CommandMvvm(() =>
            {
                new EditPlayer(SelectedPlayer).ShowDialog();

                SelectAll();

            }, () => SelectedPlayer != null);

            RemovePlayer = new CommandMvvm(() =>
            {
                var playervozvrat = MessageBox.Show("Вы уверены что хотите удалить команду?", "Подтверждение", MessageBoxButton.YesNo);

                if (playervozvrat == MessageBoxResult.Yes)
                {
                    PlayerDB.GetDb().Remove(SelectedPlayer);
                }
                SelectAll();
            }, () => SelectedPlayer != null);







            AddMatch = new CommandMvvm(() =>
            {
                new EditMatch().ShowDialog();

                SelectAll();

            }, () => true);















            HistMatch = new CommandMvvm(() =>
            {
                new MatchHistory().ShowDialog();
                SelectAll();
            }, () => true);

            HistPlayer = new CommandMvvm(() =>
            {
                new PlayerStat().ShowDialog();
                SelectAll();
            }, () => true);

            TournTable = new CommandMvvm(() =>
            {
                new TournamentTable().ShowDialog();
                SelectAll();
            }, () => true);

            StatTourn = new CommandMvvm(() =>
            {
                new TournamentStatistics().ShowDialog();
                SelectAll();
            }, () => true);
        }

        private void SelectAll()
        {
            Teams = new ObservableCollection<Team>(TeamDB.GetDb().SelectAll());
            Players = new ObservableCollection<Player>(PlayerDB.GetDb().SelectAll());
        }

    }




}

