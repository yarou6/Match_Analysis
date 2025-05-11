using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using System.Windows;

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
                var command = connection.CreateCommand("select `id`, `date`, `team_score1`, `team_score2`, `team_id1`, `team_id2` from `match` ");
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
                        if (!dr.IsDBNull(3))
                            date = dr.GetDateTime("date");

                        int team_score1 = 0;
                        if (!dr.IsDBNull(4))
                            team_score1 = dr.GetInt32("team_score1");
                        int team_score2 = 0;
                        if (!dr.IsDBNull(5))
                            team_score2 = dr.GetInt32("team_score2");


                        matchs.Add(new Match
                        {
                            Id = id,
                            Date = date,
                            TeamId1 = team_id1,
                            TeamId2 = team_id2,
                            TeamScore1 = team_score1,
                            TeamScore2 = team_score2,
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
