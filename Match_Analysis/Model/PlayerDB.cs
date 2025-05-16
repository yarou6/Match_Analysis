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
    internal class PlayerDB
    {
        DbConnection connection;

        private PlayerDB(DbConnection db)
        {
            this.connection = db;
        }

        public bool Insert(Player player)
        {
            bool result = false;
            if (connection == null)
                return result;

            if (connection.OpenConnection())
            {
                MySqlCommand cmd = connection.CreateCommand("insert into `player` Values (0, @age, @PlayerPosition, @Surname, @Name, @Patronymic, @TeamId);select LAST_INSERT_ID();");

                // путем добавления значений в запрос через параметры мы используем экранирование опасных символов
                cmd.Parameters.Add(new MySqlParameter("age", player.Age));
                cmd.Parameters.Add(new MySqlParameter("PlayerPosition", player.PlayerPosition));
                cmd.Parameters.Add(new MySqlParameter("Surname", player.Surname));
                cmd.Parameters.Add(new MySqlParameter("Name", player.Name));
                cmd.Parameters.Add(new MySqlParameter("TeamId", player.TeamId));
                cmd.Parameters.Add(new MySqlParameter("Patronymic", player.Patronymic));

                try
                {
                    // выполняем запрос через ExecuteScalar, получаем id вставленной записи
                    // если нам не нужен id, то в запросе убираем часть select LAST_INSERT_ID(); и выполняем команду через ExecuteNonQuery
                    int id = (int)(ulong)cmd.ExecuteScalar();
                    if (id > 0)
                    {
                        MessageBox.Show(id.ToString());
                        // назначаем полученный id обратно в объект для дальнейшей работы
                        player.Id = id;
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

        internal bool Update(Player edit)
        {
            bool result = false;
            if (connection == null)
                return result;

            if (connection.OpenConnection())
            {
                var mc = connection.CreateCommand($"update `player` set `age`=@age, `player_position`=@player_position, `surname`=@surname, `name`=@name, `patronymic`=@patronymic, `team_id`=@team_id where `id` = {edit.Id}");
                mc.Parameters.Add(new MySqlParameter("age", edit.Age));
                mc.Parameters.Add(new MySqlParameter("player_position", edit.PlayerPosition));
                mc.Parameters.Add(new MySqlParameter("surname", edit.Surname));
                mc.Parameters.Add(new MySqlParameter("name", edit.Name));
                mc.Parameters.Add(new MySqlParameter("patronymic", edit.Patronymic));
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


        internal bool Remove(Player remove)
        {
            bool result = false;
            if (connection == null)
                return result;

            if (connection.OpenConnection())
            {
                var mc = connection.CreateCommand($"delete from `player` where `id` = {remove.Id}");
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

        static PlayerDB db;
        public static PlayerDB GetDb()
        {
            if (db == null)
                db = new PlayerDB(DbConnection.GetDbConnection());
            return db;
        }


    }
}
