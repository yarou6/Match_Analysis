using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using System.Windows;

namespace Match_Analysis.Model
{
    internal class PlayerStatisticsDB
    {
        DbConnection connection;

        private PlayerStatisticsDB(DbConnection db)
        {
            this.connection = db;
        }

        public bool Insert(PlayerStatistics playerstatistics)
        {
            bool result = false;
            if (connection == null)
                return result;

            if (connection.OpenConnection())
            {
                MySqlCommand cmd = connection.CreateCommand("insert into `player_statistics` Values (0, @Goal, @Assist, @PlayerId, @MatchId);select LAST_INSERT_ID();");

                // путем добавления значений в запрос через параметры мы используем экранирование опасных символов
                cmd.Parameters.Add(new MySqlParameter("Goal", playerstatistics.Goal));
                cmd.Parameters.Add(new MySqlParameter("Assist", playerstatistics.Assist));
                cmd.Parameters.Add(new MySqlParameter("PlayerId", playerstatistics.PlayerId));
                cmd.Parameters.Add(new MySqlParameter("MatchId", playerstatistics.MatchId));
                try
                {
                    // выполняем запрос через ExecuteScalar, получаем id вставленной записи
                    // если нам не нужен id, то в запросе убираем часть select LAST_INSERT_ID(); и выполняем команду через ExecuteNonQuery
                    int id = (int)(ulong)cmd.ExecuteScalar();
                    if (id > 0)
                    {
                        MessageBox.Show(id.ToString());
                        // назначаем полученный id обратно в объект для дальнейшей работы
                        playerstatistics.Id = id;
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

        internal List<PlayerStatistics> SelectAll()
        {
            List<PlayerStatistics> playerstatisticss = new List<PlayerStatistics>();
            if (connection == null)
                return playerstatisticss;

            if (connection.OpenConnection())
            {
                var command = connection.CreateCommand("select `id`, `player_id`, `match_id`, `goal`, `assist` from `player_statistics` ");
                try
                {
                    // выполнение запроса, который возвращает результат-таблицу
                    MySqlDataReader dr = command.ExecuteReader();
                    // в цикле читаем построчно всю таблицу
                    while (dr.Read())
                    {
                        int id = dr.GetInt32(0);

                        int player_id = dr.GetInt32(1);

                        int match_id = dr.GetInt32(2);

                        int goal = 0;
                        if (!dr.IsDBNull(3))
                            goal = dr.GetInt32("goal");
                        int assist = 0;
                        if (!dr.IsDBNull(4))
                            assist = dr.GetInt32("assist");

                        playerstatisticss.Add(new PlayerStatistics
                        {
                            Id = id,
                            PlayerId = player_id,
                            MatchId = match_id,
                            Goal = goal,
                            Assist = assist,
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            connection.CloseConnection();
            return playerstatisticss;
        }

        internal bool Update(PlayerStatistics edit)
        {
            bool result = false;
            if (connection == null)
                return result;

            if (connection.OpenConnection())
            {
                var mc = connection.CreateCommand($"update `player_statistics` set `goal`=@goal, `assist`=@assist, `player_id`=@player_id, `match_id`=@match_id where `id` = {edit.Id}");
                mc.Parameters.Add(new MySqlParameter("goal", edit.Goal));
                mc.Parameters.Add(new MySqlParameter("assist", edit.Assist));
                mc.Parameters.Add(new MySqlParameter("player_id", edit.PlayerId));
                mc.Parameters.Add(new MySqlParameter("match_id", edit.MatchId));
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


        internal bool Remove(PlayerStatistics remove)
        {
            bool result = false;
            if (connection == null)
                return result;

            if (connection.OpenConnection())
            {
                var mc = connection.CreateCommand($"delete from `player_statistics` where `id` = {remove.Id}");
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

        static PlayerStatisticsDB db;
        public static PlayerStatisticsDB GetDb()
        {
            if (db == null)
                db = new PlayerStatisticsDB(DbConnection.GetDbConnection());
            return db;
        }
    }
}
