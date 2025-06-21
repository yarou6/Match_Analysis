using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using System.Windows;
using Match_Analysis.View;

namespace Match_Analysis.Model
{
    public class TeamDB
    {

        private readonly IConnectionWrapper connection;

        public TeamDB(IConnectionWrapper connection)
        {
            this.connection = connection;
        }

        public bool Insert(Team team)
        {
            if (connection == null)
                return false;

            if (connection.OpenConnection())
            {
                var cmd = connection.CreateCommand("insert into `team` Values (0, @Title, @Coach, @City);select LAST_INSERT_ID();");

                cmd.AddParameter("Title", team.Title);
                cmd.AddParameter("Coach", team.Coach);
                cmd.AddParameter("City", team.City);

                try
                {
                    int id = (int)(ulong)cmd.ExecuteScalar();
                    if (id > 0)
                    {
                        team.Id = id;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    // Обработка ошибки
                }
                finally
                {
                    connection.CloseConnection();
                }
            }
            return false;
        }
        //DbConnection connection;

        //private TeamDB(DbConnection db)
        //{
        //    this.connection = db;
        //}

        //public bool Insert(Team team)
        //{
        //    bool result = false;
        //    if (connection == null)
        //        return result;

        //    if (connection.OpenConnection())
        //    {
        //        MySqlCommand cmd = connection.CreateCommand("insert into `team` Values (0, @Title, @Coach, @City);select LAST_INSERT_ID();");

        //        // путем добавления значений в запрос через параметры мы используем экранирование опасных символов
        //        cmd.Parameters.Add(new MySqlParameter("Title", team.Title));
        //        cmd.Parameters.Add(new MySqlParameter("Coach", team.Coach));
        //        cmd.Parameters.Add(new MySqlParameter("City", team.City));
        //        try
        //        {
        //            // выполняем запрос через ExecuteScalar, получаем id вставленной записи
        //            // если нам не нужен id, то в запросе убираем часть select LAST_INSERT_ID(); и выполняем команду через ExecuteNonQuery
        //            int id = (int)(ulong)cmd.ExecuteScalar();
        //            if (id > 0)
        //            {
        //                MessageBox.Show(id.ToString());
        //                // назначаем полученный id обратно в объект для дальнейшей работы
        //                team.Id = id;
        //                result = true;
        //            }
        //            else
        //            {
        //                MessageBox.Show("Запись не добавлена");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show(ex.Message);
        //        }
        //    }
        //    connection.CloseConnection();
        //    return result;
        //}

        internal List<Team> SelectAll()
        {
            List<Team> teams = new List<Team>();
            if (connection == null)
                return teams;

            if (connection.OpenConnection())
            {
                var command = connection.CreateCommand("select `id`, `title`, `coach`, `city` from `team` ");
                try
                {
                    // выполнение запроса, который возвращает результат-таблицу
                    MySqlDataReader dr = command.ExecuteReader();
                    // в цикле читаем построчно всю таблицу
                    while (dr.Read())
                    {
                        int id = dr.GetInt32(0);
                        string title = string.Empty;
                        if (!dr.IsDBNull(1))
                            title = dr.GetString("title");
                        string coach = string.Empty;
                        if (!dr.IsDBNull(2))
                            coach = dr.GetString("coach");
                        string city = string.Empty;
                        if (!dr.IsDBNull(3))
                            city = dr.GetString("city");

                        teams.Add(new Team
                        {
                            Id = id,
                            Title = title,
                            Coach = coach,
                            City = city,
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            connection.CloseConnection();
            return teams;
        }

        internal bool Update(Team edit)
        {
            bool result = false;
            if (connection == null)
                return result;

            if (connection.OpenConnection())
            {
                var mc = connection.CreateCommand($"update `team` set `title`=@title, `coach`=@coach, `city`=@city where `id` = {edit.Id}");
                mc.AddParameter("title", edit.Title);
                mc.AddParameter("coach", edit.Coach);
                mc.AddParameter("city", edit.City);

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


        internal bool Remove(Team remove)
        {
            bool result = false;
            if (connection == null)
                return result;

            if (connection.OpenConnection())
            {
                var mc = connection.CreateCommand($"delete from `team` where `id` = {remove.Id}");
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

        static TeamDB db;
        public static TeamDB GetDb()
        {
            if (db == null)
            {
                // Получаем реальный DbConnection
                var realDbConnection = DbConnection.GetDbConnection();

                // Оборачиваем его в ConnectionWrapper, который реализует IConnectionWrapper
                var connectionWrapper = new ConnectionWrapper(realDbConnection);

                // Передаём обёртку в TeamDB
                db = new TeamDB(connectionWrapper);
            }
            return db;
        }
    }
    
}
