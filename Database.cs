using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public class Database
    {
        // Використання рядка підключення з класу DBConnection
        private static readonly MySqlConnection connection = DBConnection.GetConnection();

        // Метод для виконання асинхронного запиту до бази даних
        public async Task<DataTable> ExecuteQueryAsync(string query)
        {
            // Відкриття з'єднання з базою даних
            DBConnection.OpenConnection();

            using (var command = new MySqlCommand(query, connection))
            {
                using (var adapter = new MySqlDataAdapter(command))
                {
                    var dataTable = new DataTable();
                    await Task.Run(() => adapter.Fill(dataTable));

                    // Закриття з'єднання з базою даних
                    DBConnection.CloseConnection();

                    return dataTable;
                }
            }
        }
    }
}
