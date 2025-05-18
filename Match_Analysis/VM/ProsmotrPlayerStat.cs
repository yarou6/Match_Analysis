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

            var result = new List<PlayerTeamStats>();

            foreach (var history in histories)
            {
                var team = allTeams.FirstOrDefault(t => t.Id == history.TeamId);
                if (team == null) continue;

                // Находим матчи этой команды
                var matches = allMatches.Where(m => m.TeamId1 == team.Id || m.TeamId2 == team.Id).ToList();

                // Получаем статистику игрока по найденным матчам
                var goals = 0;
                var assists = 0;

                foreach (var match in matches)
                {
                    var playerStats = stats.Where(s => s.MatchId == match.Id);
                    goals += playerStats.Sum(s => s.Goal);
                    assists += playerStats.Sum(s => s.Assist);
                }

                result.Add(new PlayerTeamStats
                {
                    TeamName = team.Title,
                    Goals = goals,
                    Assists = assists
                });
            }

            PlayerTeamStats = new ObservableCollection<PlayerTeamStats>(result);
        }
    }
}
