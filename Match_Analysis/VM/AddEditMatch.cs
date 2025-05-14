using System;
using System.Collections.Generic;
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
        public CommandMvvm AddMatch { get; set; }
        public CommandMvvm AddInf1 { get; set; }
        public CommandMvvm AddInf2 { get; set; }

        public AddEditMatch()
        {

            AddMatch = new CommandMvvm(() =>
            {

                if (NewMatch.Id == 0)
                {
                    MatchDB.GetDb().Insert(NewMatch);
                }
                else MatchDB.GetDb().Update(NewMatch);
                close?.Invoke();

            }, () =>
                NewMatch.TeamScore1 >= 0 &&
                NewMatch.TeamScore1 <= 11 &&
                NewMatch.TeamScore2 >= 0 &&
                NewMatch.TeamScore2 <= 11 
                );





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

        internal void SetClose(Action close)
        {
            this.close = close;
        }
    }
}
