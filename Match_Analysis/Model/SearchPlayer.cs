using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace Match_Analysis.Model
{
    public class SearchPlayer
    {

        private readonly DbConnection myConnection;


        public List<Player> SearchPlayers(string search)
        {
            List<Player> result = new();
            List<Team> teams = new();
            string query = $"SELECT p.id AS 'playerkid', p.age, p.player_position, p.surname, p.name, p.patronymic, p.team_id, t.title, t.city, t.coach\r\nFROM player p \r\nJOIN team t ON p.team_id = t.id\r\nWHERE t.title LIKE @search OR p.surname LIKE @search";

            if (myConnection.OpenConnection())
            {// using уничтожает объект после окончания блока (вызывает Dispose)
                using (var mc = myConnection.CreateCommand(query))
                {
                    // передача поиска через переменную в запрос
                    mc.Parameters.Add(new MySqlParameter("@search", $"%{search}%"));
                    using (var dr = mc.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            // создание книги на каждую строку в результате
                            var player = new Player();
                            player.Id = dr.GetInt32("playerkid");
                            player.Age = dr.GetInt32("age");
                            player.PlayerPosition = dr.GetString("player_position");
                            player.Surname = dr.GetString("surname");
                            player.Name = dr.GetString("name");
                            player.Patronymic = dr.IsDBNull(dr.GetOrdinal("patronymic")) ? null : dr.GetString("patronymic");
                            player.TeamId = dr.GetInt32("team_id");


                            // поиск объекта-автора в коллекции authors по ID
                            if (player.TeamId.HasValue)
                            {
                                var team = teams.FirstOrDefault(s => s.Id == player.TeamId.Value);
                                if (team == null)
                                {
                                    team = new Team();
                                    team.Id = player.TeamId.Value;
                                    team.Title = dr.GetString("title");
                                    team.Coach = dr.GetString("coach");
                                    team.City = dr.GetString("city");
                                    teams.Add(team);
                                }

                                // player.Team = team; // если нужно
                            }
                            //// добавление книги автору
                            //team.Players.Add(player);
                            //// указание книге автора
                            //player.Team = team;

                            // добавление книги в результат запроса
                            result.Add(player);
                        }
                    }
                }
                myConnection.CloseConnection();
            }
            return result;

        }

        // синглтон start
        static SearchPlayer table;
        private SearchPlayer(DbConnection myConnection)
        {
            this.myConnection = myConnection;
        }
        public static SearchPlayer GetTable()
        {
            if (table == null)
                table = new SearchPlayer(DbConnection.GetDbConnection());
            return table;
        }



    }
}
