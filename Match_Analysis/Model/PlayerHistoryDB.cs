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
                var command = connection.CreateCommand("SELECT ps.id, ps.entry_date, ps.release_date, ps.playr_id, ps.team_id, c.name, c.player_position, c.age, c.surname, c.patronymic, t.title, t.coach, t.city FROM player_history ps JOIN player c ON ps.playr_id = c.`id` JOIN team p ON ps.team_id = p.`id` ");
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

                        int age = 0;
                        if (!dr.IsDBNull(5))
                            age = dr.GetInt32("age");

                        string player_position = string.Empty;
                        if (!dr.IsDBNull(6))
                            player_position = dr.GetString("player_position");

                        string surname = string.Empty;
                        if (!dr.IsDBNull(7))
                            surname = dr.GetString("surname");

                        string name = string.Empty;
                        if (!dr.IsDBNull(8))
                            name = dr.GetString("name");

                        string patronymic = string.Empty;
                        if (!dr.IsDBNull(9))
                            patronymic = dr.GetString("patronymic");

                        string title = string.Empty;
                        if (!dr.IsDBNull(10))
                            title = dr.GetString("title");

                        string coach = string.Empty;
                        if (!dr.IsDBNull(11))
                            coach = dr.GetString("coach");

                        string city = string.Empty;
                        if (!dr.IsDBNull(12))
                            city = dr.GetString("city");

                        Team team = new Team
                        {
                            Id = team_id,
                            Title = title,
                            Coach = coach,
                            City = city,
                        };


                        Player player = new Player
                        {
                            Id = player_id,
                            Age = age,
                            PlayerPosition = player_position,
                            Surname = surname,
                            Name = name,
                            Patronymic = patronymic,
                            Team = team,
                        };

                        playerHistories.Add(new PlayerHistory
                        {
                            Id = id,
                            PlayerId = player_id,
                            TeamId = team_id,
                            EntryDate = entry_date,
                            ReleaseDate = release_date,
                            Team = team,
                            Player = player,
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
