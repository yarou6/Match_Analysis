using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Match_Analysis.Model;
using System.Windows;

namespace Match_Analysis.VM
{
    internal class AddEditTeam : BaseVM
    {

        private Team newTeam = new();

        public Team NewTeam
        {
            get => newTeam;
            set
            {
                newTeam = value;
                Signal();
            }
        }   
        public CommandMvvm AddTeam { get; set; }
        public AddEditTeam()
        {

            AddTeam = new CommandMvvm(() =>
            {

                if (newTeam.Id == 0)
                {
                    TeamDB.GetDb().Insert(NewTeam);
                    close?.Invoke();
                } else TeamDB.GetDb().Update(newTeam);
                close?.Invoke();

            }, () =>

                !string.IsNullOrEmpty(newTeam.Title) &&
                !string.IsNullOrEmpty(newTeam.Coach) &&
                !string.IsNullOrEmpty(newTeam.City));

        }
        public void SetTeam(Team selectedTeam)
        {
            NewTeam = selectedTeam;
        }

        Action close;

        internal void SetClose(Action close)
        {
            this.close = close;
        }


    }
}
