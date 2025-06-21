using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace Match_Analysis.Model
{
    // Фейковая реализация команды для юнит-тестов, без обращения к настоящей базе данных
    public class FakeCommandWrapper : ICommandWrapper
    {
        // Список добавленных параметров (имя + значение)
        public List<(string name, object value)> ParametersList { get; } = new();

        // Значение, которое будет возвращено методом ExecuteScalar()
        public object ScalarResult { get; set; }

        // Флаг — нужно ли выбросить исключение при вызове ExecuteScalar
        public bool ThrowOnExecuteScalar { get; set; }

        // Добавление параметра (запоминаем в список)
        public void AddParameter(string name, object value)
        {
            ParametersList.Add((name, value));
        }

        // Эмуляция выполнения запроса и возврат значения или ошибки
        public object ExecuteScalar()
        {
            if (ThrowOnExecuteScalar)
                throw new Exception("DB error");
            return ScalarResult;
        }

        // Эмуляция ExecuteNonQuery (возвращает 1 как будто запрос прошёл)
        public int ExecuteNonQuery() => 1;

        // Метод не реализован, потому что не нужен в текущих тестах
        public MySqlDataReader ExecuteReader()
        {
            throw new NotImplementedException("Не требуется в текущем тестовом контексте");
        }
    }
}
