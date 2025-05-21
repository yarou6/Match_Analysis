using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Match_Analysis.Model;

namespace Match_Analysis.VM
{
    public class PlayerTeamStats
    {
        public string TeamName { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public int Total => Goals + Assists; // Голы + пас
        public DateTime? EntryDate { get; set; }
        public DateTime? ReleaseDate { get; set; }

        // Обработка EntryDate с учётом MinValue
        public string EntryDateString => EntryDate != null && EntryDate != DateTime.MinValue ?
                                          EntryDate.Value.ToShortDateString() :
                                          "";

        // Обработка ReleaseDate с учётом MinValue
        public string ReleaseDateString => ReleaseDate != null && ReleaseDate != DateTime.MinValue ?
                                           ReleaseDate.Value.ToShortDateString() :
                                           "По настоящее время";
    }

    internal class ProsmotrPlayerStat : BaseVM
    {
        public CommandMvvm Vozvrat { get; set; }

        private ObservableCollection<Player> players;
        public ObservableCollection<Player> Players
        {
            get => players;
            set { players = value; Signal(); }
        }

        private Player selectedPlayer;
        public Player SelectedPlayer
        {
            get => selectedPlayer;
            set
            {
                selectedPlayer = value;
                Signal();
                LoadPlayerStats();
            }
        }

        private ObservableCollection<PlayerTeamStats> playerTeamStats;
        public ObservableCollection<PlayerTeamStats> PlayerTeamStats
        {
            get => playerTeamStats;
            set { playerTeamStats = value; Signal(); }
        }

        public ProsmotrPlayerStat()
        {
            Vozvrat = new CommandMvvm(() =>
            {
                close?.Invoke();
            }, () => true);

            Players = new ObservableCollection<Player>(PlayerDB.GetDb().SelectAll());
        }

        Action close;

        internal void SetClose(Action close)
        {
            this.close = close;
        }

        private void LoadPlayerStats()
        {
            if (SelectedPlayer == null)
            {
                PlayerTeamStats = new ObservableCollection<PlayerTeamStats>();
                return;
            }

            var histories = PlayerHistoryDB.GetDb().SelectPlayer(SelectedPlayer.Id);
            var allTeams = TeamDB.GetDb().SelectAll();
            var allMatches = MatchDB.GetDb().SelectAll();
            var stats = PlayerStatisticsDB.GetDb().SelectAll()
                        .Where(s => s.PlayerId == SelectedPlayer.Id)
                        .ToList();

            var teamStatsDict = new Dictionary<int, PlayerTeamStats>();

            foreach (var stat in stats)
            {
                var match = allMatches.FirstOrDefault(m => m.Id == stat.MatchId);
                if (match == null)
                    continue;

                var matchDate = match.Date;

                // Определяем команду игрока на дату матча
                var historyForMatch = histories
                    .OrderByDescending(h => h.EntryDate)
                    .FirstOrDefault(h =>
                        h.EntryDate <= matchDate &&
                        (h.ReleaseDate == null || h.ReleaseDate == DateTime.MinValue || h.ReleaseDate >= matchDate));

                if (historyForMatch == null || historyForMatch.TeamId == null)
                    continue;

                int teamId = historyForMatch.TeamId.Value;

                if (!teamStatsDict.ContainsKey(teamId))
                {
                    var team = allTeams.FirstOrDefault(t => t.Id == teamId);
                    if (team == null) continue;

                    teamStatsDict[teamId] = new PlayerTeamStats
                    {
                        TeamName = team.Title,
                        Goals = 0,
                        Assists = 0,
                        EntryDate = historyForMatch.EntryDate,
                        ReleaseDate = historyForMatch.ReleaseDate
                    };
                }

                teamStatsDict[teamId].Goals += stat.Goal;
                teamStatsDict[teamId].Assists += stat.Assist;
            }
            foreach (var history in histories)
            {
                if (history.TeamId == null) continue;

                int teamId = history.TeamId.Value;

                // Проверяем, существует ли команда в словаре
                if (!teamStatsDict.ContainsKey(teamId))
                {
                    var team = allTeams.FirstOrDefault(t => t.Id == teamId);
                    if (team == null) continue;

                    teamStatsDict[teamId] = new PlayerTeamStats
                    {
                        TeamName = team.Title,
                        Goals = 0, // Устанавливаем 0, если нет голов
                        Assists = 0, // Устанавливаем 0, если нет передач
                        EntryDate = history.EntryDate,
                        ReleaseDate = history.ReleaseDate
                    };
                }
            }
                PlayerTeamStats = new ObservableCollection<PlayerTeamStats>(teamStatsDict.Values);
        }

    }
}
