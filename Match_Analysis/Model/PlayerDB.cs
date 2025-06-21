using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using System.Windows;
using Match_Analysis.VM;
using System.Windows.Controls;

namespace Match_Analysis.Model
{
    public class PlayerDB
    {
        private readonly IConnectionWrapper connection;

        public PlayerDB(IConnectionWrapper connection)
        {
            this.connection = connection;
        }

        public bool Insert(Player player)
        {
            if (connection == null)
                return false;

            if (connection.OpenConnection())
            {
                var cmd = connection.CreateCommand("insert into `player` Values (0, @age, @PlayerPosition, @Surname, @Name, @Patronymic, @TeamId);select LAST_INSERT_ID();");

                cmd.AddParameter("age", player.Age);
                cmd.AddParameter("PlayerPosition", player.PlayerPosition);
                cmd.AddParameter("Surname", player.Surname);
                cmd.AddParameter("Name", player.Name);
                cmd.AddParameter("TeamId", player.TeamId);
                cmd.AddParameter("Patronymic", player.Patronymic);

                try
                {
                    int id = (int)(ulong)cmd.ExecuteScalar();
                    if (id > 0)
                    {
                        player.Id = id;
                        return true;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    connection.CloseConnection();
                }
            }
            return false;
        }

        internal List<Player> SelectAll()
        {
            List<Player> players = new List<Player>();
            if (connection == null)
                return players;

            if (connection.OpenConnection())
            {
                var command = connection.CreateCommand("SELECT b.id, b.team_id, b.name, b.player_position, b.age, b.surname, b.patronymic, t.title, t.coach, t.city FROM player b LEFT JOIN team t ON b.team_id = t.id");
                try
                {
                    // выполнение запроса, который возвращает результат-таблицу
                    MySqlDataReader dr = command.ExecuteReader();
                    // в цикле читаем построчно всю таблицу
                    while (dr.Read())
                    {
                        int id = dr.GetInt32(0);

                        int? team_id = dr.IsDBNull(1) ? null : dr.GetInt32(1);

                        int age = 0;
                        if (!dr.IsDBNull(2))
                            age = dr.GetInt32("age");

                        string player_position = string.Empty;
                        if (!dr.IsDBNull(3))
                            player_position = dr.GetString("player_position");

                        string surname = string.Empty;
                        if (!dr.IsDBNull(4))
                            surname = dr.GetString("surname");

                        string name = string.Empty;
                        if (!dr.IsDBNull(5))
                            name = dr.GetString("name");

                        string patronymic = string.Empty;
                        if (!dr.IsDBNull(6))
                            patronymic = dr.GetString("patronymic");

                        string title = string.Empty;
                        if (!dr.IsDBNull(7))
                            title = dr.GetString("title");

                        string coach = string.Empty;
                        if (!dr.IsDBNull(8))
                            coach = dr.GetString("coach");

                        string city = string.Empty;
                        if (!dr.IsDBNull(9))
                            city = dr.GetString("city");

                        Team team = new Team
                        {
                            Id = team_id ?? 0,
                            Title = title,
                            Coach = coach,
                            City = city,
                        };
                        
                        
                        players.Add(new Player
                        {
                            Id = id,
                            TeamId = team_id,
                            Age = age,
                            PlayerPosition = player_position,
                            Surname = surname,
                            Name = name,
                            Patronymic = patronymic,
                            Team = team,
                        });


                        

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            connection.CloseConnection();
            return players;
        }

        public bool Update(Player edit)
        {
            if (connection == null)
                return false;

            if (connection.OpenConnection())
            {
                var cmd = connection.CreateCommand($"update `player` set `age`=@age, `player_position`=@player_position, `surname`=@surname, `name`=@name, `patronymic`=@patronymic, `team_id`=@team_id where `id` = {edit.Id}");

                cmd.AddParameter("age", edit.Age);
                cmd.AddParameter("player_position", edit.PlayerPosition);
                cmd.AddParameter("surname", edit.Surname);
                cmd.AddParameter("name", edit.Name);
                cmd.AddParameter("patronymic", edit.Patronymic);
                cmd.AddParameter("team_id", edit.TeamId);

                try
                {
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    connection.CloseConnection();
                }
            }
            return false;
        }


        public bool Remove(Player remove)
        {
            if (connection == null)
                return false;

            if (connection.OpenConnection())
            {
                var cmd = connection.CreateCommand($"delete from `player` where `id` = {remove.Id}");

                try
                {
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    connection.CloseConnection();
                }
            }
            return false;
        }


        static PlayerDB db;
        public static PlayerDB GetDb()
        {
            if (db == null)
            {
                var connection = DbConnection.GetDbConnection();
                var connectionWrapper = new ConnectionWrapper(connection);
                db = new PlayerDB(connectionWrapper);
            }
            return db;
        }

    }
}
