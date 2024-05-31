using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public class DBConnection
    {
        //створюємо підключення до бази даних
        static MySqlConnection connection = new MySqlConnection($"server = localhost; port = 3306; username = " +
            $"{Environment.GetEnvironmentVariable("DB_USER")}; password = {Environment.GetEnvironmentVariable("DB_PASS")}; " +
            $"database = logistics");

        //метод для відкриття з'єднання з бд
        public static void OpenConnection()
        {
            if (connection.State == System.Data.ConnectionState.Closed)
                connection.Open();
        }

        //метод для закриття з'єднання з бд
        public static void CloseConnection()
        {
            if (connection.State == System.Data.ConnectionState.Open)
                connection.Close();
        }

        public static MySqlConnection GetConnection() { return connection; }
    }
}
