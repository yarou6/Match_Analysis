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
                if (SelectedGoalPlayers.Any(p => p.SelectedPlayer == null) || SelectedAssistPlayers.Any(p => p.SelectedPlayer == null))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля выбора игроков.");
                    return;
                }
                var validPlayersAtMatchDate = GetPlayersInTeamAtDate(Team, MatchDB.GetDb().SelectById(MatchId).Date);

                var validPlayerIds = validPlayersAtMatchDate.Select(p => p.Id).ToHashSet();

                var goalPlayerIds = SelectedGoalPlayers.Where(p => p.SelectedPlayer != null).Select(p => p.SelectedPlayer.Id).ToHashSet();
                var assistPlayerIds = SelectedAssistPlayers.Where(p => p.SelectedPlayer != null).Select(p => p.SelectedPlayer.Id).ToHashSet();

                // Проверка, что все игроки принадлежат команде в дату матча
                var invalidGoalPlayers = goalPlayerIds.Except(validPlayerIds).ToList();
                var invalidAssistPlayers = assistPlayerIds.Except(validPlayerIds).ToList();

                if (invalidGoalPlayers.Any() || invalidAssistPlayers.Any())
                {
                    MessageBox.Show("Некоторые игроки не состоят в команде на дату матча. Исправьте выбор.");
                    return;
                }

                var goalsPerPlayer = SelectedGoalPlayers
         .Where(p => p.SelectedPlayer != null)
         .GroupBy(p => p.SelectedPlayer.Id)
         .ToDictionary(g => g.Key, g => g.Count());

                // Считаем количество ассистов на игрока
                var assistsPerPlayer = SelectedAssistPlayers
                    .Where(p => p.SelectedPlayer != null)
                    .GroupBy(p => p.SelectedPlayer.Id)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Общие голы команды
                int totalGoals = SelectedGoalPlayers.Count;

                // Проверяем ограничения ассистов по игрокам
                foreach (var playerId in assistsPerPlayer.Keys)
                {
                    int assists = assistsPerPlayer[playerId];
                    goalsPerPlayer.TryGetValue(playerId, out int playerGoals);

                    // Правила, например:
                    //  - ассистов у игрока не может быть больше 2, если у него 3 гола
                    //  - ассистов не может быть больше (общее количество голов команды - голов игрока)
                    if (playerGoals >= 3 && assists > 2)
                    {
                        MessageBox.Show($"Игрок с ID {playerId} не может иметь больше 2 ассистов при 3 и более голах.");
                        return;
                    }

                    int maxAssistsByTeam = totalGoals - playerGoals;
                    if (assists > maxAssistsByTeam)
                    {
                        MessageBox.Show($"Игрок с ID {playerId} не может иметь больше ассистов ({assists}), чем голов команды без его голов ({maxAssistsByTeam}).");
                        return;
                    }
                }

                // Проверка суммарных ассистов: не больше количества голов команды
                int totalAssists = SelectedAssistPlayers.Count;
                if (totalAssists > totalGoals)
                {
                    MessageBox.Show("Общее количество ассистов не может превышать количество голов команды.");
                    return;
                }

                // Если проверки пройдены — сохраняем статистику

                // Сохраняем голы
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

                // Сохраняем ассисты
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
