using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Match_Analysis.Model;

namespace Match_Analysis.VM
{
    internal class AddInfPlayer : BaseVM
    {
        public ObservableCollection<Player> Players
        {
            get => players;
            set
            {
                players = value;
                Signal();
            }
        }


        public ObservableCollection<Player> GetPlayersInTeamAtDate(Team team, DateTime date)
        {
            var allPlayers = PlayerDB.GetDb().SelectAll();

            var validPlayers = allPlayers.Where(p =>
            {
                var histories = PlayerHistoryDB.GetDb().SelectPlayer(p.Id);

                return histories.Any(h =>
                    h.TeamId == team.Id &&
                    h.EntryDate <= date &&
                    (h.ReleaseDate == null || h.ReleaseDate == DateTime.MinValue || h.ReleaseDate >= date));
            });

            return new ObservableCollection<Player>(validPlayers);
        }

        private ObservableCollection<Player> selectedGoalPlayers = new();

        private ObservableCollection<Player> selectedAssistPlayers = new();
        public ObservableCollection<Player> SelectedGoalPlayers
        {
            get => selectedGoalPlayers;
            set
            {
                selectedGoalPlayers = value;
                Signal();
            }
        }

        public ObservableCollection<Player> SelectedAssistPlayers
        {
            get => selectedAssistPlayers;
            set
            {
                selectedAssistPlayers = value;
                Signal();
            }
        }

        public int MatchId { get; set; }
        public int GoalCount { get; set; }
        public int AssistCount { get; set; }
        public Team Team { get; private set; }
        public void InitializePlayers(int matchId, int goals, Team team, DateTime date)
        {
            MatchId = matchId;
            GoalCount = goals;

            var match = MatchDB.GetDb().SelectById(matchId);

            Random rand = new Random();
            int minAssists = 0;
            if (goals >= 3)
            {
                minAssists = Math.Min(1, goals); // гарантируем, что minAssists <= goals
                if (goals >= 6)
                {
                    minAssists = Math.Min(4, goals); // гарантируем, что minAssists <= goals
                }
            }

            int assists = rand.Next(minAssists, goals + 1);

            AssistCount = assists;

            Team = team;

            Players = GetPlayersInTeamAtDate(team, date);

            SelectedGoalPlayers = new ObservableCollection<Player>(new Player[goals]);
            SelectedAssistPlayers = new ObservableCollection<Player>(new Player[assists]);

            Signal(nameof(Players));
            Signal(nameof(SelectedGoalPlayers));
            Signal(nameof(SelectedAssistPlayers));
            Signal(nameof(Team));
        }



        public CommandMvvm AddPlayer { get; set; }
        public AddInfPlayer()
        {
            SelectAll();
            AddPlayer = new CommandMvvm(() =>
            {
                foreach (var player in SelectedGoalPlayers.Where(p => p != null))
                {
                    PlayerStatistics stats = new()
                    {
                        PlayerId = player.Id,
                        MatchId = MatchId,
                        Goal = 1,
                        Assist = 0
                    };
                    PlayerStatisticsDB.GetDb().Insert(stats);
                }

                foreach (var player in SelectedAssistPlayers.Where(p => p != null))
                {
                    PlayerStatistics stats = new()
                    {
                        PlayerId = player.Id,
                        MatchId = MatchId,
                        Goal = 0,
                        Assist = 1
                    };
                    PlayerStatisticsDB.GetDb().Insert(stats);
                }

                close?.Invoke();

            }, () => true);


        }
        Action close;
        private ObservableCollection<Player> players = new();
        internal void SetClose(Action close)
        {
            this.close = close;
        }

        private void SelectAll()
        {
            Players = new ObservableCollection<Player>(PlayerDB.GetDb().SelectAll());
        }
    }
}
