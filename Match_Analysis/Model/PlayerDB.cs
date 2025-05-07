using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using System.Windows;

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
                MySqlCommand cmd = connection.CreateCommand("insert into `player` Values (0, @Age, @PlayerPosition, @Surname, @Name, @Patrnymic, @TeamId);select LAST_INSERT_ID();");

                // путем добавления значений в запрос через параметры мы используем экранирование опасных символов
                cmd.Parameters.Add(new MySqlParameter("Age", player.Age));
                cmd.Parameters.Add(new MySqlParameter("PlayerPosition", player.PlayerPosition));
                cmd.Parameters.Add(new MySqlParameter("Surname", player.Surname));
                cmd.Parameters.Add(new MySqlParameter("Name", player.Name));
                cmd.Parameters.Add(new MySqlParameter("TeamId", player.TeamId));
                // можно указать параметр через отдельную переменную
                MySqlParameter Patronymic = new MySqlParameter("Patronymic", player.Patronymic);
                cmd.Parameters.Add(Patronymic);
                
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
                var command = connection.CreateCommand("select `id`, `age`, `player_position`, `surname`, `name`, `patronymic`, `team_id` from `team` ");
                try
                {
                    // выполнение запроса, который возвращает результат-таблицу
                    MySqlDataReader dr = command.ExecuteReader();
                    // в цикле читаем построчно всю таблицу
                    while (dr.Read())
                    {
                        int id = dr.GetInt32(0);

                        int team_id = dr.GetInt32(1);

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


                        Player player = new Player
                        {
                            Id = team_id,
                            Age = age,
                            PlayerPosition = player_position,
                            Surname = surname,
                            Name = name,
                            Patronymic = patronymic,
                        };

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            connection.CloseConnection();
            return books;
        }

        internal bool Update(Book edit)
        {
            bool result = false;
            if (connection == null)
                return result;

            if (connection.OpenConnection())
            {
                var mc = connection.CreateCommand($"update `books` set `title`=@title, `year_published`=@year_published, `genre`=@genre, `is_available`=@is_available where `id` = {edit.Id}");
                mc.Parameters.Add(new MySqlParameter("title", edit.Title));
                mc.Parameters.Add(new MySqlParameter("year_published", edit.YearPublished));
                mc.Parameters.Add(new MySqlParameter("genre", edit.Genre));
                mc.Parameters.Add(new MySqlParameter("is_available", edit.IsAvailable));

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


        internal bool Remove(Book remove)
        {
            bool result = false;
            if (connection == null)
                return result;

            if (connection.OpenConnection())
            {
                var mc = connection.CreateCommand($"delete from `books` where `id` = {remove.Id}");
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

        static BookDB db;
        public static BookDB GetDb()
        {
            if (db == null)
                db = new BookDB(DbConnection.GetDbConnection());
            return db;
        }


    }
}
