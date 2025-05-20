using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Match_Analysis.Model;

namespace Match_Analysis.VM
{

    


    internal class ProsmotrTournTable: BaseVM
    {
        public CommandMvvm Vozvrat { get; set; }

        private Action close;

        public ObservableCollection<TeamStats> TournamentTable { get; set; }

        public ProsmotrTournTable()
        {
            Vozvrat = new CommandMvvm(() =>
            {
                close?.Invoke();
            }, () => true);

            LoadTournamentTable();
        }

        internal void SetClose(Action close)
        {
            this.close = close;
        }

        private void LoadTournamentTable()
        {
            var matchDb = MatchDB.GetDb();
            var matches = matchDb.SelectAll();

            if (matches.Count == 0)
            {
                System.Windows.MessageBox.Show("Нет матчей в базе!");
                return;
            }

            var stats = new Dictionary<int, TeamStats>();

            foreach (var match in matches)
            {
                if (!stats.ContainsKey(match.TeamId1))
                {
                    stats[match.TeamId1] = new TeamStats { Team = match.Team1 };
                }
                if (!stats.ContainsKey(match.TeamId2))
                {
                    stats[match.TeamId2] = new TeamStats { Team = match.Team2 };
                }

                var team1 = stats[match.TeamId1];
                var team2 = stats[match.TeamId2];

                team1.Matches++;
                team2.Matches++;

                team1.GoalsFor += match.TeamScore1;
                team1.GoalsAgainst += match.TeamScore2;

                team2.GoalsFor += match.TeamScore2;
                team2.GoalsAgainst += match.TeamScore1;

                if (match.TeamScore1 > match.TeamScore2)
                {
                    team1.Wins++;
                    team2.Losses++;
                    team1.Points += 3;
                }
                else if (match.TeamScore1 < match.TeamScore2)
                {
                    team2.Wins++;
                    team1.Losses++;
                    team2.Points += 3;
                }
                else
                {
                    team1.Draws++;
                    team2.Draws++;
                    team1.Points++;
                    team2.Points++;
                }
            }

            TournamentTable = new ObservableCollection<TeamStats>(stats.Values.OrderByDescending(s => s.Points).ThenByDescending(s => s.GoalDifference));
        }
    }

    public class TeamStats
    {
        public Team Team { get; set; }
        public int Matches { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int Points { get; set; }

        public int GoalDifference => GoalsFor - GoalsAgainst;
    }
}

