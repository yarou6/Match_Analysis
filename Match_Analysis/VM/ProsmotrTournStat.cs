using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Match_Analysis.Model;

namespace Match_Analysis.VM
{
    public enum StatType
    {
        Goals,
        Assists,
        Total
    }
    internal class ProsmotrTournStat:  BaseVM
    {
        private StatType selectedStatType = StatType.Goals; // По умолчанию сортировка по голам
        public StatType SelectedStatType
        {
            get => selectedStatType;
            set
            {
                selectedStatType = value;
                Signal();
                SortStats(); // сортировка при выборе
            }
        }
        private void SortStats()
        {
            IEnumerable<PlayerStatView> sorted;

            if (selectedStatType == StatType.Goals)
            {
                sorted = PlayersStats.OrderByDescending(p => p.Goals)
                                     .ThenBy(p => p.FIO)
                                     .ThenBy(p => p.TeamTitle);
            }
            else if (selectedStatType == StatType.Assists)
            {
                sorted = PlayersStats.OrderByDescending(p => p.Assists)
                                     .ThenBy(p => p.FIO)
                                     .ThenBy(p => p.TeamTitle);
            }
            else if (selectedStatType == StatType.Total)
            {
                sorted = PlayersStats.OrderByDescending(p => p.Goals + p.Assists)
                                     .ThenBy(p => p.FIO)
                                     .ThenBy(p => p.TeamTitle);
            }
            else
            {
                sorted = PlayersStats;
            }

            PlayersStats = new ObservableCollection<PlayerStatView>(sorted);
            Signal(nameof(PlayersStats)); ;
        }
        public ObservableCollection<PlayerStatView> PlayersStats { get; set; } = new();

        public CommandMvvm Vozvrat { get; set; }

        public ProsmotrTournStat()
        {
            Vozvrat = new CommandMvvm(() => close?.Invoke(), () => true);
            LoadTopPlayers(); // Загружаем данные
        }

        private void LoadTopPlayers()
        {
            var db = DbConnection.GetDbConnection();
            if (!db.OpenConnection()) return;

            string query = @"
SELECT 
    p.surname,
    p.name,
    p.patronymic,
    SUM(ps.goal) AS total_goals,
    SUM(ps.assist) AS total_assists,
    t.title AS team_title
FROM player_statistics ps
JOIN player p ON ps.player_id = p.id
LEFT JOIN team t ON p.team_id = t.id
GROUP BY p.id, t.title, p.surname, p.name, p.patronymic
ORDER BY p.surname;";

            var cmd = db.CreateCommand(query);

            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string surname = reader.GetString("surname");
                string name = reader.GetString("name");
                string patronymic = reader.IsDBNull("patronymic") ? "" : reader.GetString("patronymic");

                string fio = string.IsNullOrEmpty(patronymic)
                    ? $"{surname} {name[0]}."
                    : $"{surname} {name[0]}. {patronymic[0]}.";

                int goals = reader.GetInt32("total_goals");
                int assists = reader.GetInt32("total_assists");
                string teamTitle = reader.IsDBNull("team_title") ? "Без команды" : reader.GetString("team_title");

                PlayersStats.Add(new PlayerStatView
                {
                    FIO = fio,
                    Goals = goals,
                    Assists = assists,
                    TeamTitle = teamTitle
                });
            }

            SortStats();
            reader.Close();
        }

        Action close;
        internal void SetClose(Action close) => this.close = close;
    }
}
