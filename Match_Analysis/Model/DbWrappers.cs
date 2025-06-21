using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace Match_Analysis.Model
{
    // Реализация ICommandWrapper — адаптер над MySqlCommand
    public class CommandWrapper : ICommandWrapper
    {
        private readonly MySqlCommand _command;

        public CommandWrapper(MySqlCommand command)
        {
            _command = command;
        }

        // Добавление параметра в реальную команду
        public void AddParameter(string name, object value)
        {
            _command.Parameters.Add(new MySqlParameter(name, value));
        }

        // Вызов ExecuteScalar у MySqlCommand
        public object ExecuteScalar()
        {
            return _command.ExecuteScalar();
        }

        // Вызов ExecuteNonQuery у MySqlCommand
        public int ExecuteNonQuery()
        {
            return _command.ExecuteNonQuery();
        }

        // Вызов ExecuteReader у MySqlCommand
        public MySqlDataReader ExecuteReader()
        {
            return _command.ExecuteReader();
        }
    }

    // Обёртка для подключения — работает через DbConnection
    public class ConnectionWrapper : IConnectionWrapper
    {
        private readonly DbConnection _connection;

        public ConnectionWrapper(DbConnection connection)
        {
            _connection = connection;
        }

        // Делегируем открытие соединения
        public bool OpenConnection() => _connection.OpenConnection();

        // Делегируем закрытие соединения
        public void CloseConnection() => _connection.CloseConnection();

        // Создаём обёрнутую команду (CommandWrapper)
        public ICommandWrapper CreateCommand(string sql)
        {
            var cmd = _connection.CreateCommand(sql);
            return new CommandWrapper(cmd);
        }
    }
}
