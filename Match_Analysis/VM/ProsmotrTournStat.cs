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
    internal class ProsmotrTournStat:  BaseVM
    {

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
    m.id AS match_id,
    m.date,
    p.surname,
    p.name,
    p.patronymic,
    ps.goal,
    ps.assist,
    t.title AS team_title
FROM player_statistics ps
JOIN player p ON ps.player_id = p.id
LEFT JOIN team t ON p.team_id = t.id
JOIN `match` m ON ps.match_id = m.id
ORDER BY m.date, p.surname;";

            var cmd = db.CreateCommand(query);

            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int matchId = reader.GetInt32("match_id");
                DateTime matchDate = reader.GetDateTime("date");

                string surname = reader.GetString("surname");
                string name = reader.GetString("name");
                string patronymic = reader.IsDBNull("patronymic") ? "" : reader.GetString("patronymic");

                string fio = string.IsNullOrEmpty(patronymic)
                    ? $"{surname} {name[0]}."
                    : $"{surname} {name[0]}. {patronymic[0]}.";

                int goals = reader.GetInt32("goal");
                int assists = reader.GetInt32("assist");
                string teamTitle = reader.IsDBNull("team_title") ? "Без команды" : reader.GetString("team_title");

                Console.WriteLine($"Матч #{matchId} от {matchDate:dd.MM.yyyy} | {fio} ({teamTitle}) — Голы: {goals}, Ассисты: {assists}");
            }
            reader.Close();


        }

        Action close;
        internal void SetClose(Action close) => this.close = close;
    }
}
