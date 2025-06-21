using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using System.Windows;

namespace Match_Analysis.Model
{
    public class PlayerStatisticsDB
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
                try
                {
                    // ✅ Проверка: существует ли такая запись
                    var checkCmd = connection.CreateCommand(@"
                SELECT COUNT(*) 
                FROM `player_statistics`
                WHERE `player_id` = @PlayerId 
                  AND `match_id` = @MatchId 
                  AND `goal` = @Goal 
                  AND `assist` = @Assist");

                    checkCmd.Parameters.Add(new MySqlParameter("PlayerId", playerstatistics.PlayerId));
                    checkCmd.Parameters.Add(new MySqlParameter("MatchId", playerstatistics.MatchId));
                    checkCmd.Parameters.Add(new MySqlParameter("Goal", playerstatistics.Goal));
                    checkCmd.Parameters.Add(new MySqlParameter("Assist", playerstatistics.Assist));

                    long count = (long)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        MessageBox.Show("Такая статистика уже существует для данного игрока и матча.");
                        connection.CloseConnection();
                        return false;
                    }

                    // 📝 Вставка новой записи
                    MySqlCommand cmd = connection.CreateCommand(@"
                INSERT INTO `player_statistics` 
                VALUES (0, @Goal, @Assist, @PlayerId, @MatchId);
                SELECT LAST_INSERT_ID();");

                    cmd.Parameters.Add(new MySqlParameter("Goal", playerstatistics.Goal));
                    cmd.Parameters.Add(new MySqlParameter("Assist", playerstatistics.Assist));
                    cmd.Parameters.Add(new MySqlParameter("PlayerId", playerstatistics.PlayerId));
                    cmd.Parameters.Add(new MySqlParameter("MatchId", playerstatistics.MatchId));

                    int id = (int)(ulong)cmd.ExecuteScalar();
                    if (id > 0)
                    {
                        playerstatistics.Id = id;
                        result = true;
                    }
                    else
                    {
                        MessageBox.Show("Запись не добавлена.");
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
        public bool DeleteByMatchId(int matchId)
        {
            if (connection == null)
                return false;

            bool result = false;

            if (connection.OpenConnection())
            {
                using var cmd = connection.CreateCommand("DELETE FROM player_statistics WHERE match_id = @matchId");
                cmd.Parameters.AddWithValue("@matchId", matchId);

                try
                {
                    result = cmd.ExecuteNonQuery() > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                connection.CloseConnection();
            }

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
