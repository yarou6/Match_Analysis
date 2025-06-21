using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace Match_Analysis.Model
{
    // Интерфейс обёртки команды — используется вместо прямой зависимости от MySqlCommand
    public interface ICommandWrapper
    {
        // Добавление параметра (например, "@title", "TeamName")
        void AddParameter(string name, object value);

        // Выполнение команды и получение одного значения (обычно используется с SELECT LAST_INSERT_ID())
        object ExecuteScalar();

        // Выполнение команды, не возвращающей значения (например, UPDATE, DELETE)
        int ExecuteNonQuery();

        // Выполнение команды, возвращающей таблицу (например, SELECT)
        MySqlDataReader ExecuteReader();
    }

    // Интерфейс обёртки подключения к базе данных
    public interface IConnectionWrapper
    {
        // Открытие соединения
        bool OpenConnection();

        // Закрытие соединения
        void CloseConnection();

        // Создание обёрнутой команды на основе SQL-запроса
        ICommandWrapper CreateCommand(string sql);
    }
}
