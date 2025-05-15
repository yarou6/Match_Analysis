using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using System.Windows;
using System.Data;

namespace Match_Analysis.Model
{
    internal class MatchDB
    {
        DbConnection connection;

        private MatchDB(DbConnection db)
        {
            this.connection = db;
        }

        public bool Insert(Match match)
        {
            bool result = false;
            if (connection == null)
                return result;

            if (connection.OpenConnection())
            {
                MySqlCommand cmd = connection.CreateCommand("insert into `match` Values (0, @Date, @TeamScore1, @TeamScore2, @TeamId1, @TeamId2);select LAST_INSERT_ID();");

                // путем добавления значений в запрос через параметры мы используем экранирование опасных символов
                cmd.Parameters.Add(new MySqlParameter("Date", match.Date));
                cmd.Parameters.Add(new MySqlParameter("TeamScore1", match.TeamScore1));
                cmd.Parameters.Add(new MySqlParameter("TeamScore2", match.TeamScore2));
                cmd.Parameters.Add(new MySqlParameter("TeamId1", match.TeamId1));
                cmd.Parameters.Add(new MySqlParameter("TeamId2", match.TeamId2));
                try
                {
                    // выполняем запрос через ExecuteScalar, получаем id вставленной записи
                    // если нам не нужен id, то в запросе убираем часть select LAST_INSERT_ID(); и выполняем команду через ExecuteNonQuery
                    int id = (int)(ulong)cmd.ExecuteScalar();
                    if (id > 0)
                    {
                        MessageBox.Show(id.ToString());
                        // назначаем полученный id обратно в объект для дальнейшей работы
                        match.Id = id;
                        result = true;
                    }
                    else
                    {
                        MessageBox.Show("Запись не добавлена");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            connection.CloseConnection();
            return result;
        }

        internal List<Match> SelectAll()
        {
            List<Match> matchs = new List<Match>();
            if (connection == null)
                return matchs;

            if (connection.OpenConnection())
            {
                var command = connection.CreateCommand("SELECT m.id, m.team_id1, m.team_id2, m.date, m.team_score1, m.team_score2, t.title, t.city, t.coach, t2.title, t2.city, t2.city FROM `match` m JOIN team t ON m.team_id1 = t.id JOIN team t2 ON m.team_id2 = t2.id");
                try
                {
                    // выполнение запроса, который возвращает результат-таблицу
                    MySqlDataReader dr = command.ExecuteReader();
                    // в цикле читаем построчно всю таблицу
                    while (dr.Read())
                    {
                        int id = dr.GetInt32(0);

                        int team_id1 = dr.GetInt32(1);

                        int team_id2 = dr.GetInt32(2);


                        DateTime date = new DateTime();
                        if (!dr.IsDBNull("m.date"))
                            date = dr.GetDateTime("m.date");

                        int team_score1 = 0;
                        if (!dr.IsDBNull("m.team_score1"))
                            team_score1 = dr.GetInt32("m.team_score1");
                        int team_score2 = 0;
                        if (!dr.IsDBNull("m.team_score2"))
                            team_score2 = dr.GetInt32("m.team_score2");

                        string title = string.Empty;
                        if (!dr.IsDBNull("t.title"))
                            title = dr.GetString("t.title");

                        string coach = string.Empty;
                        if (!dr.IsDBNull("t.coach"))
                            coach = dr.GetString("t.coach");

                        string city = string.Empty;
                        if (!dr.IsDBNull("t.city"))
                            city = dr.GetString("t.city");

                        string title2 = string.Empty;
                        if (!dr.IsDBNull("t2.title"))
                            title2 = dr.GetString("t2.title");

                        string coach2 = string.Empty;
                        if (!dr.IsDBNull("t2.coach"))
                            coach2 = dr.GetString("t2.coach");

                        string city2 = string.Empty;
                        if (!dr.IsDBNull("t2.city"))
                            city2 = dr.GetString("t2.city");

                        Team team1 = new Team
                        {
                            Id = team_id1,
                            Title = title,
                            Coach = coach,
                            City = city,
                        };

                        Team team2 = new Team
                        {
                            Id = team_id2,
                            Title = title2,
                            Coach = coach2,
                            City = city2,
                        };



                        matchs.Add(new Match
                        {
                            Id = id,
                            Date = date,
                            TeamId1 = team_id1,
                            TeamId2 = team_id2,
                            TeamScore1 = team_score1,
                            TeamScore2 = team_score2,
                            Team1 = team1,
                            Team2 = team2,
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            connection.CloseConnection();
            return matchs;
        }

        internal bool Update(Match edit)
        {
            bool result = false;
            if (connection == null)
                return result;

            if (connection.OpenConnection())
            {
                var mc = connection.CreateCommand($"update `match` set `date`=@date, `team_score1`=@team_score1, `team_score2`=@team_score2, `team_id1`=@team_id1, `team_id2`=@team_id2 where `id` = {edit.Id}");
                mc.Parameters.Add(new MySqlParameter("date", edit.Date));
                mc.Parameters.Add(new MySqlParameter("team_score1", edit.TeamScore1));
                mc.Parameters.Add(new MySqlParameter("team_score2", edit.TeamScore2));
                mc.Parameters.Add(new MySqlParameter("team_id1", edit.TeamId1));
                mc.Parameters.Add(new MySqlParameter("team_id2", edit.TeamId2));

                try
                {
                    mc.ExecuteNonQuery();
                    result = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            connection.CloseConnection();
            return result;
        }


        internal bool Remove(Match remove)
        {
            bool result = false;
            if (connection == null)
                return result;

            if (connection.OpenConnection())
            {
                var mc = connection.CreateCommand($"delete from `match` where `id` = {remove.Id}");
                try
                {
                    mc.ExecuteNonQuery();
                    result = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            connection.CloseConnection();
            return result;
        }

        static MatchDB db;
        public static MatchDB GetDb()
        {
            if (db == null)
                db = new MatchDB(DbConnection.GetDbConnection());
            return db;
        }
    }
}
