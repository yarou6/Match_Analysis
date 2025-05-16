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



        public CommandMvvm AddTeam { get; set; }
        public CommandMvvm AddPlayer { get; set; }
        public AddEditPlayer()
        {
            AddPlayer = new CommandMvvm(() =>
            {

                NewPlayer.PlayerPosition = Positional;

                if (NewPlayer.Id == 0)
                {
                    PlayerDB.GetDb().Insert(NewPlayer);
                    close?.Invoke();

                } else PlayerDB.GetDb().Update(NewPlayer);
                close?.Invoke();


            }, () => 
                NewPlayer != null &&
                !string.IsNullOrEmpty(NewPlayer.Surname) &&
                !string.IsNullOrEmpty(NewPlayer.Name) &&
                //!string.IsNullOrEmpty(newPlayer.PlayerPosition) &&
                Positional != null &&
                NewPlayer.Age >= 16 &&
                NewPlayer.Age <= 52
                );


            AddTeam = new CommandMvvm(() =>
            {
                NewPlayer.PlayerPosition = Positional;
                
                if (NewPlayer.Id == 0)
                {
                    PlayerDB.GetDb().Insert(NewPlayer); 
                    new EditPlayerHistory(NewPlayer).ShowDialog();

                }
                else
                {
                    
                    PlayerDB.GetDb().Update(NewPlayer);
                    new EditPlayerHistory(NewPlayer).ShowDialog();
                }
                


            }, () => 
            
                NewPlayer != null &&
                !string.IsNullOrEmpty(NewPlayer.Surname) &&
                !string.IsNullOrEmpty(NewPlayer.Name) &&
                Positional != null &&
                NewPlayer.Age >= 16 &&
                NewPlayer.Age <= 52
                );
        }
        public void SetPlayer(Player selectedPlayer)
        {
            NewPlayer = selectedPlayer;
            Positional = selectedPlayer.PlayerPosition; // <- ВАЖНО: для ComboBox SelectedItem
        }

        Action close;
        private List<Team> teams = new();

        internal void SetClose(Action close)
        {
            this.close = close;
        }

        
    }
}
