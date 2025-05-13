using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Match_Analysis.Model;
using System.Windows;
using System.Collections.ObjectModel;
using Match_Analysis.View;

namespace Match_Analysis.VM
{
    internal class AddEditPlayer : BaseVM
    {

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

        private string positional;

        public string Positional
        {
            get => positional;
            set
            {
                positional = value;
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
        public CommandMvvm AddPlayer { get; set; }
        public AddEditPlayer()
        {
            AddPlayer = new CommandMvvm(() =>
            {

                //NewPlayer.TeamId = NewPlayer.Team.Id;
                NewPlayer.PlayerPosition = Positional;
                if (newPlayer.Id == 0)
                {
                    PlayerDB.GetDb().Insert(NewPlayer);
                    close?.Invoke();

                } else PlayerDB.GetDb().Update(newPlayer);
                close?.Invoke();

                

        }, () => 
                NewPlayer != null &&
                //NewPlayer.Team != null &&
                !string.IsNullOrEmpty(newPlayer.Surname) &&
                !string.IsNullOrEmpty(newPlayer.Name) &&
                //!string.IsNullOrEmpty(newPlayer.PlayerPosition) &&
                Positional != null &&
                newPlayer.Age >= 16 &&
                newPlayer.Age <= 52
                );


            AddTeam = new CommandMvvm(() =>
            {
                NewPlayer.PlayerPosition = Positional;
                if (newPlayer.Id == 0)
                {
                    PlayerDB.GetDb().Insert(NewPlayer);
                    new EditPlayerHistory(new PlayerHistory()).ShowDialog();

                }
                else
                { 
                     PlayerDB.GetDb().Update(newPlayer);
                     new EditPlayerHistory(new PlayerHistory()).ShowDialog();
                }
                


            }, () => 
            
                NewPlayer != null &&
                !string.IsNullOrEmpty(newPlayer.Surname) &&
                !string.IsNullOrEmpty(newPlayer.Name) &&
                Positional != null &&
                newPlayer.Age >= 16 &&
                newPlayer.Age <= 52
                );


        }
        public void SetPlayer(Player selectedPlayer)
        {
            NewPlayer = selectedPlayer;
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
            if (NewPlayer.Team != null)
            {
                NewPlayer.Team = Teams.FirstOrDefault(s => s.Id == NewPlayer.TeamId);
            }
        }
    }
}
