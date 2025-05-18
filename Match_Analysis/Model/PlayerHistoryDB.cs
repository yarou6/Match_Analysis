using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
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
                try
                {
                    // ✅ Проверка: есть ли такая же запись уже в таблице
                    var checkCmd = connection.CreateCommand(@"
                SELECT COUNT(*) 
                FROM `player_history` 
                WHERE `player_id` = @PlayerId 
                  AND `team_id` = @TeamId 
                  AND `entry_date` = @EntryDate 
                  AND `release_date` = @ReleaseDate");

                    checkCmd.Parameters.Add(new MySqlParameter("PlayerId", playerHistory.PlayerId));
                    checkCmd.Parameters.Add(new MySqlParameter("TeamId", playerHistory.TeamId));
                    checkCmd.Parameters.Add(new MySqlParameter("EntryDate", playerHistory.EntryDate));
                    checkCmd.Parameters.Add(new MySqlParameter("ReleaseDate", playerHistory.ReleaseDate));

                    long count = (long)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        // Уже существует — не вставляем
                        MessageBox.Show("Такая запись уже существует.");
                        connection.CloseConnection();
                        return false;
                    }

                    // ✅ Вставка новой записи
                    MySqlCommand cmd = connection.CreateCommand("INSERT INTO `player_history` VALUES (0, @EntryDate, @ReleaseDate, @PlayerId, @TeamId); SELECT LAST_INSERT_ID();");
                    cmd.Parameters.Add(new MySqlParameter("EntryDate", playerHistory.EntryDate));
                    cmd.Parameters.Add(new MySqlParameter("ReleaseDate", playerHistory.ReleaseDate));
                    cmd.Parameters.Add(new MySqlParameter("PlayerId", playerHistory.PlayerId));
                    cmd.Parameters.Add(new MySqlParameter("TeamId", playerHistory.TeamId));

                    int id = (int)(ulong)cmd.ExecuteScalar();
                    if (id > 0)
                    {
                        playerHistory.Id = id;
                        result = true;
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить запись.");
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
                var command = connection.CreateCommand("SELECT ps.id, ps.player_id, ps.team_id, ps.entry_date, ps.release_date,  c.name, c.player_position, c.age, c.surname, c.patronymic, t.title, t.coach, t.city FROM player_history ps JOIN player c ON ps.player_id = c.`id` JOIN team t ON ps.team_id = t.`id` ");
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
                        if (!dr.IsDBNull("entry_date"))
                            entry_date = dr.GetDateTime("entry_date");

                        DateTime release_date = new DateTime();
                        if (!dr.IsDBNull("release_date"))
                            release_date = dr.GetDateTime("release_date");

                        int age = 0;
                        if (!dr.IsDBNull("age"))
                            age = dr.GetInt32("age");

                        string player_position = string.Empty;
                        if (!dr.IsDBNull("player_position"))
                            player_position = dr.GetString("player_position");

                        string surname = string.Empty;
                        if (!dr.IsDBNull("surname"))
                            surname = dr.GetString("surname");

                        string name = string.Empty;
                        if (!dr.IsDBNull("name"))
                            name = dr.GetString("name");

                        string patronymic = string.Empty;
                        if (!dr.IsDBNull("patronymic"))
                            patronymic = dr.GetString("patronymic");

                        string title = string.Empty;
                        if (!dr.IsDBNull("title"))
                            title = dr.GetString("title");

                        string coach = string.Empty;
                        if (!dr.IsDBNull("coach"))
                            coach = dr.GetString("coach");

                        string city = string.Empty;
                        if (!dr.IsDBNull("city"))
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

        internal List<PlayerHistory> SelectPlayer(int playerid)
        {
            List<PlayerHistory> playerHistories = new List<PlayerHistory>();
            if (connection == null)
                return playerHistories;

            if (connection.OpenConnection())
            {
                var command = connection.CreateCommand($"SELECT ps.id, ps.player_id, ps.team_id, ps.entry_date, ps.release_date,  c.name, c.player_position, c.age, c.surname, c.patronymic, t.title, t.coach, t.city FROM player_history ps JOIN player c ON ps.player_id = c.`id` JOIN team t ON ps.team_id = t.`id` WHERE ps.player_id = {playerid}");
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
                        if (!dr.IsDBNull("entry_date"))
                            entry_date = dr.GetDateTime("entry_date");

                        DateTime release_date = new DateTime();
                        if (!dr.IsDBNull("release_date"))
                            release_date = dr.GetDateTime("release_date");

                        int age = 0;
                        if (!dr.IsDBNull("age"))
                            age = dr.GetInt32("age");

                        string player_position = string.Empty;
                        if (!dr.IsDBNull("player_position"))
                            player_position = dr.GetString("player_position");

                        string surname = string.Empty;
                        if (!dr.IsDBNull("surname"))
                            surname = dr.GetString("surname");

                        string name = string.Empty;
                        if (!dr.IsDBNull("name"))
                            name = dr.GetString("name");

                        string patronymic = string.Empty;
                        if (!dr.IsDBNull("patronymic"))
                            patronymic = dr.GetString("patronymic");

                        string title = string.Empty;
                        if (!dr.IsDBNull("title"))
                            title = dr.GetString("title");

                        string coach = string.Empty;
                        if (!dr.IsDBNull("coach"))
                            coach = dr.GetString("coach");

                        string city = string.Empty;
                        if (!dr.IsDBNull("city"))
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

        public bool BothTeamsHaveEnoughPlayers(int teamId1, int teamId2, DateTime date)
        {
            if (connection == null)
                return false;

            bool result = false;

            if (connection.OpenConnection())
            {
                try
                {
                    var cmd = connection.CreateCommand(@"
                SELECT 
                    (SELECT COUNT(*) FROM player_history 
                     WHERE team_id = @TeamId1 
                       AND entry_date <= @MatchDate 
                       AND (release_date IS NULL OR release_date >= @MatchDate)) AS count1,
                    (SELECT COUNT(*) FROM player_history 
                     WHERE team_id = @TeamId2 
                       AND entry_date <= @MatchDate 
                       AND (release_date IS NULL OR release_date >= @MatchDate)) AS count2;
            ");

                    cmd.Parameters.Add(new MySqlParameter("TeamId1", teamId1));
                    cmd.Parameters.Add(new MySqlParameter("TeamId2", teamId2));
                    cmd.Parameters.Add(new MySqlParameter("MatchDate", date));

                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        int count1 = reader.GetInt32("count1");
                        int count2 = reader.GetInt32("count2");

                        result = (count1 >= 11 && count2 >= 11);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    connection.CloseConnection();
                }
            }

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
