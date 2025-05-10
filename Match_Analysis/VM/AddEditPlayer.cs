using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Match_Analysis.Model;
using System.Windows;

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
        public CommandMvvm AddPlayer { get; set; }
        public AddEditPlayer()
        {

            AddPlayer = new CommandMvvm(() =>
            {

                if (newPlayer.Id == 0)
                {
                    PlayerDB.GetDb().Insert(NewPlayer);
                    close?.Invoke();

                } else PlayerDB.GetDb().Update(newPlayer);
                close?.Invoke();

                

        }, () => 
                !string.IsNullOrEmpty(newPlayer.Surname) &&
                !string.IsNullOrEmpty(newPlayer.Name) &&
                !string.IsNullOrEmpty(newPlayer.PlayerPosition) &&
                newPlayer.Age >= 16
                );

        }
        public void SetPlayer(Player selectedPlayer)
        {
            NewPlayer = selectedPlayer;
        }

        Action close;

        internal void SetClose(Action close)
        {
            this.close = close;
        }
    }
}
