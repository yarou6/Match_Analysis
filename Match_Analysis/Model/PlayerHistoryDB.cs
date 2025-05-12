using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Match_Analysis.Model
{
    internal class PlayerHistoryDB
    {

        DbConnection connection;

        private PlayerHistoryDB(DbConnection db)
        {
            this.connection = db;
        }

        public bool Insert(PlayerHistory playerHistory)
        {
            bool result = false;
            if (connection == null)
                return result;

            if (connection.OpenConnection())
            {
                MySqlCommand cmd = connection.CreateCommand("insert into `player_history` Values (0, @EntryDate, @ReleaseDate, @PlayerId, @TeamId);select LAST_INSERT_ID();");

                // путем добавления значений в запрос через параметры мы используем экранирование опасных символов
                cmd.Parameters.Add(new MySqlParameter("EntryDate", playerHistory.EntryDate));
                cmd.Parameters.Add(new MySqlParameter("ReleaseDate", playerHistory.ReleaseDate));
                cmd.Parameters.Add(new MySqlParameter("PlayerId", playerHistory.PlayerId));
                cmd.Parameters.Add(new MySqlParameter("TeamId", playerHistory.TeamId));
                try
                {
                    // выполняем запрос через ExecuteScalar, получаем id вставленной записи
                    // если нам не нужен id, то в запросе убираем часть select LAST_INSERT_ID(); и выполняем команду через ExecuteNonQuery
                    int id = (int)(ulong)cmd.ExecuteScalar();
                    if (id > 0)
                    {
                        MessageBox.Show(id.ToString());
                        // назначаем полученный id обратно в объект для дальнейшей работы
                        playerHistory.Id = id;
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

        internal List<PlayerHistory> SelectAll()
        {
            List<PlayerHistory> playerHistories = new List<PlayerHistory>();
            if (connection == null)
                return playerHistories;

            if (connection.OpenConnection())
            {
                var command = connection.CreateCommand("select `id`, `entry_date`, `release_date`, `player_id`, `team_id` from `player_history` ");
                try
                {
                    // выполнение запроса, который возвращает результат-таблицу
                    MySqlDataReader dr = command.ExecuteReader();
                    // в цикле читаем построчно всю таблицу
                    while (dr.Read())
                    {
                        int id = dr.GetInt32(0);

                        int player_id = dr.GetInt32(1);

                        int team_id = dr.GetInt32(2);

                        DateTime entry_date = new DateTime();
                        if (!dr.IsDBNull(3))
                            entry_date = dr.GetDateTime("entry_date");

                        DateTime release_date = new DateTime();
                        if (!dr.IsDBNull(4))
                            release_date = dr.GetDateTime("release_date");

                        playerHistories.Add(new PlayerHistory
                        {
                            Id = id,
                            PlayerId = player_id,
                            TeamId = team_id,
                            EntryDate = entry_date,
                            ReleaseDate = release_date,
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            connection.CloseConnection();
            return playerHistories;
        }

        internal bool Update(PlayerHistory edit)
        {
            bool result = false;
            if (connection == null)
                return result;

            if (connection.OpenConnection())
            {
                var mc = connection.CreateCommand($"update `player_history` set `entry_date`=@entry_date, `release_date`=@release_date, `player_id`=@player_id, `team_id`=@team_id where `id` = {edit.Id}");
                mc.Parameters.Add(new MySqlParameter("entry_date", edit.EntryDate));
                mc.Parameters.Add(new MySqlParameter("release_date", edit.ReleaseDate));
                mc.Parameters.Add(new MySqlParameter("player_id", edit.PlayerId));
                mc.Parameters.Add(new MySqlParameter("team_id", edit.TeamId));
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


        internal bool Remove(PlayerHistory remove)
        {
            bool result = false;
            if (connection == null)
                return result;

            if (connection.OpenConnection())
            {
                var mc = connection.CreateCommand($"delete from `player_history` where `id` = {remove.Id}");
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

        static PlayerHistoryDB db;
        public static PlayerHistoryDB GetDb()
        {
            if (db == null)
                db = new PlayerHistoryDB(DbConnection.GetDbConnection());
            return db;
        }
    }
}
