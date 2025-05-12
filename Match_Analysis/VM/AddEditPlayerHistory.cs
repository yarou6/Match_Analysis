using Match_Analysis.Model;
using Match_Analysis.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match_Analysis.VM
{
    internal class AddEditPlayerHistory : BaseVM
    {
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

        public AddEditPlayerHistory()
        {
            AddTeam = new CommandMvvm(() =>
            {

                close?.Invoke();

            }, () => true);


        }



        public void SetPlayerHistory(PlayerHistory selectedPlayerHistory)
        {
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
        }
        
    }
}
