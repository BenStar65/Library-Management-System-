using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Library_Management_System_July_Exam_
{
    public class DataBase
    {
        private MySqlConnection connection;

        public DataBase()
        {
            string connString = "server=localhost;port=3363;user=root;password=#Samsunset7;database=Library;SslMode=Required;"; //Database link
            connection = new MySqlConnection(connString);
        }

        public static void LogAudit(int userId, string action, string details) 
        {
            string query = "INSERT INTO Auditlog (UserID, Action, Timestamp, Details) VALUES (@userId, @action, @timestamp, @details)";

            using (var conn = new MySqlConnection("server=localhost;port=3363;user=root;password=#Samsunset7;database=Library;SslMode=Required;"))
            {
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@action", action);
                cmd.Parameters.AddWithValue("@timestamp", DateTime.Now);
                cmd.Parameters.AddWithValue("@details", details);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public MySqlConnection GetConnection()
        {
            return connection;
        }

        public void OpenConnection()
        {
            if (connection.State == System.Data.ConnectionState.Closed)
                connection.Open();
        }

        public void CloseConnection()
        {
            if (connection.State == System.Data.ConnectionState.Open)
                connection.Close();
        }
    }
}
