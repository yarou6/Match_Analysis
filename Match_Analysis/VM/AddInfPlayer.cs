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

        private ObservableCollection<PlayerSelection> selectedGoalPlayers = new();
        private ObservableCollection<PlayerSelection> selectedAssistPlayers = new();

        public ObservableCollection<PlayerSelection> SelectedGoalPlayers
        {
            get => selectedGoalPlayers;
            set
            {
                selectedGoalPlayers = value;
                Signal();
            }
        }

        public ObservableCollection<PlayerSelection> SelectedAssistPlayers
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

            SelectedGoalPlayers = new ObservableCollection<PlayerSelection>(
                Enumerable.Range(0, goals).Select(i => new PlayerSelection(null)));

            SelectedAssistPlayers = new ObservableCollection<PlayerSelection>(
                Enumerable.Range(0, assists).Select(i => new PlayerSelection(null)));

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

                foreach (var group in SelectedGoalPlayers
                .Where(p => p.SelectedPlayer != null)
                .GroupBy(p => p.SelectedPlayer.Id))
                {
                    int playerId = group.Key;
                    int goals = group.Count();
                    var player = Players.FirstOrDefault(p => p.Id == playerId);

                    PlayerStatistics stats = new()
                    {
                        PlayerId = playerId,
                        MatchId = MatchId,
                        Goal = goals,
                        Assist = 0
                    };

                    bool inserted = PlayerStatisticsDB.GetDb().Insert(stats);

                    if (!inserted)
                    {
                        MessageBox.Show($"Не удалось добавить статистику для игрока {player?.Surname ?? "неизвестен"}");
                    }
                    else
                    {
                        MessageBox.Show($"Добавлена статистика: {player?.Surname} - {goals} гол(ов)");
                    }
                }

                foreach (var group in SelectedAssistPlayers
                .Where(p => p.SelectedPlayer != null)
                .GroupBy(p => p.SelectedPlayer.Id))
                {
                    int playerId = group.Key;
                    int assists = group.Count();
                    var player = Players.FirstOrDefault(p => p.Id == playerId);

                    PlayerStatistics stats = new()
                    {
                        PlayerId = playerId,
                        MatchId = MatchId,
                        Goal = 0,
                        Assist = assists
                    };

                    bool inserted = PlayerStatisticsDB.GetDb().Insert(stats);

                    if (!inserted)
                    {
                        MessageBox.Show($"Не удалось добавить статистику для игрока {player?.Surname ?? "неизвестен"}");
                    }
                    else
                    {
                        MessageBox.Show($"Добавлена статистика: {player?.Surname} - {assists} ассист(ов)");
                    }
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
