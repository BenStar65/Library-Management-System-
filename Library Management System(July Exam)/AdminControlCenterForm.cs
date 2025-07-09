using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MySql.Data.MySqlClient;
using System.IO;

namespace Library_Management_System_July_Exam_
{
    public partial class AdminControlCenterForm : Form
    {
        MySqlConnection conn = new MySqlConnection("server=localhost;port=3363;user=root;password=#Samsunset7;database=Library;SslMode=Required;");

        public AdminControlCenterForm()
        {
            InitializeComponent();
            LoadMostBorrowedBooks();
            LoadGenreDistribution();
            LoadAuditLog();
            LoadUsers();
        }
        private void LoadMostBorrowedBooks()
        {
            dgvMostBorrowed.Rows.Clear(); // Clear previous rows

            //  Ensure columns exist
            if (dgvMostBorrowed.Columns.Count == 0)
            {
                dgvMostBorrowed.Columns.Add("Title", "Title");
                dgvMostBorrowed.Columns.Add("BorrowCount", "Borrow Count");
            }

            string query = "SELECT Title, COUNT(*) AS BorrowCount FROM Borrows JOIN Book ON Borrows.BookID = Book.ID GROUP BY Title ORDER BY BorrowCount DESC LIMIT 5";

            MySqlCommand cmd = new MySqlCommand(query, conn);
            conn.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dgvMostBorrowed.Rows.Add(
                    reader["Title"].ToString(),
                    reader["BorrowCount"].ToString()
                );
            }
            conn.Close();
        }


        private void LoadGenreDistribution()
        {
            dgvGenreDistribution.Rows.Clear();

            //  Define columns if they don't exist
            if (dgvGenreDistribution.Columns.Count == 0)
            {
                dgvGenreDistribution.Columns.Add("Genre", "Genre");
                dgvGenreDistribution.Columns.Add("Count", "Count");
            }

            string query = "SELECT Genre, COUNT(*) AS Count FROM Book GROUP BY Genre";

            MySqlCommand cmd = new MySqlCommand(query, conn);
            conn.Open();
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                dgvGenreDistribution.Rows.Add(
                    reader["Genre"].ToString(),
                    reader["Count"].ToString()
                );
            }

            conn.Close();
        }
        private void ExportToCSV(string query, string filename)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.FileName = filename;
                sfd.Filter = "CSV files (*.csv)|*.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter sw = new StreamWriter(sfd.FileName))
                    {
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        conn.Open();
                        MySqlDataReader reader = cmd.ExecuteReader();

                        // Headers
                        for (int i = 0; i < reader.FieldCount; i++)
                            sw.Write(reader.GetName(i) + (i == reader.FieldCount - 1 ? "\n" : ","));

                        // Rows
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                                sw.Write(reader[i].ToString() + (i == reader.FieldCount - 1 ? "\n" : ","));
                        }
                        conn.Close();
                    }
                    MessageBox.Show("Exported successfully!");
                }
            }
        }

        private void LoadAuditLog()
        {

            dgvAuditLog.Rows.Clear();

            // Ensure columns are added only once
            if (dgvAuditLog.Columns.Count == 0)
            {
                dgvAuditLog.Columns.Add("ID", "ID");
                dgvAuditLog.Columns.Add("User", "User");
                dgvAuditLog.Columns.Add("Action", "Action");
                dgvAuditLog.Columns.Add("Timestamp", "Timestamp");
                dgvAuditLog.Columns.Add("Details", "Details");
            }

            string baseQuery = @"
              SELECT A.ID, U.Name AS User, A.Action, A.Timestamp, A.Details
               FROM AuditLog A
               JOIN Users U ON A.UserID = U.ID
               WHERE 1 = 1";

            List<MySqlParameter> parameters = new List<MySqlParameter>();

            // Date filter
            DateTime start = StartDatePicker.Value.Date;
            DateTime end = EndDatePicker.Value.Date.AddDays(1).AddSeconds(-1);
            baseQuery += " AND A.Timestamp BETWEEN @start AND @end";
            parameters.Add(new MySqlParameter("@start", start));
            parameters.Add(new MySqlParameter("@end", end));

            // Action Type Filter
            string filter = cmbFilterType.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(filter) && filter != "All")
            {
                baseQuery += " AND A.Action = @action";
                parameters.Add(new MySqlParameter("@action", filter));
            }

            // User Name or Email Search
            string searchText = txtSearchUser.Text.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                baseQuery += " AND (U.Name LIKE @search OR U.Email LIKE @search)";
                parameters.Add(new MySqlParameter("@search", "%" + searchText + "%"));
            }

            // Order by latest first
            baseQuery += " ORDER BY A.Timestamp DESC";

            MySqlCommand cmd = new MySqlCommand(baseQuery, conn);
            cmd.Parameters.AddRange(parameters.ToArray());

            conn.Open();
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                dgvAuditLog.Rows.Add(
                    reader["ID"].ToString(),
                    reader["User"].ToString(),
                    reader["Action"].ToString(),
                    Convert.ToDateTime(reader["Timestamp"]).ToString("yyyy-MM-dd HH:mm:ss"),
                    reader["Details"].ToString()
                );
            }

            conn.Close();
        }

        private void LoadUsers()
        {
            dgvUsers.Rows.Clear();

            //  Ensure columns are defined
            if (dgvUsers.Columns.Count == 0)
            {
                dgvUsers.Columns.Add("ID", "ID");
                dgvUsers.Columns.Add("Name", "Name");
                dgvUsers.Columns.Add("Email", "Email");
                dgvUsers.Columns.Add("Role", "Role");
                dgvUsers.Columns.Add("IsPro", "Pro User");
            }

            string query = "SELECT ID, Name, Email, Role, IsPro FROM Users";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            conn.Open();
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                dgvUsers.Rows.Add(
                    reader["ID"],
                    reader["Name"],
                    reader["Email"],
                    reader["Role"],
                    Convert.ToBoolean(reader["IsPro"]) ? "✅" : "❌"
                );
            }
            conn.Close();
        }



        private void button2_Click(object sender, EventArgs e)
        {
            string query = "SELECT * FROM Borrows";
            ExportToCSV(query, "borrow_logs.csv");
            StatusLabel.Text = "Book List Exported Successfully";
        }

        private void panelChart1_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }


        private void button1_Click(object sender, EventArgs e)
        {
            string query = "SELECT ID, Title, Author, Genre, AvailableCopies FROM Book";
            ExportToCSV(query, "books.csv");
            StatusLabel.Text = "Book List Exported Successfully";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count > 0)
            {
                string id = dgvUsers.SelectedRows[0].Cells["ID"].Value.ToString();
                string query = "UPDATE Users SET IsPro = 1 WHERE ID = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                MessageBox.Show("User upgraded to Pro");
                LoadUsers();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count > 0)
            {
                string id = dgvUsers.SelectedRows[0].Cells["ID"].Value.ToString();
                string query = "UPDATE Users SET IsPro = 0 WHERE ID = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                MessageBox.Show("User downgraded from Pro");
                LoadUsers();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            AdminDashboardForm admin = new AdminDashboardForm();
            admin.Show();
            this.Hide();
        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            LoadAuditLog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            txtSearchUser.Text = "";
            cmbFilterType.SelectedIndex = 0;
            StartDatePicker.Value = DateTime.Now.AddDays(-30);
            EndDatePicker.Value = DateTime.Now;

            LoadAuditLog();
        }

        private void LoadActionTypes()
        {
            cmbFilterType.Items.Clear();
            cmbFilterType.Items.Add("All");

            string query = "SELECT DISTINCT Action FROM AuditLog ORDER BY Action ASC";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            conn.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                cmbFilterType.Items.Add(reader["Action"].ToString());
            }
            conn.Close();

            cmbFilterType.SelectedIndex = 0;
        }

        private void UserSearch_TextChanged(object sender, EventArgs e)
        {
            LoadAuditLog();
        }

        private void AdminControlCenterForm_Load(object sender, EventArgs e)
        {
            LoadActionTypes();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            Remove re = new Remove();
            re.Show();
            this.Hide();
        }

        private void guna2HtmlLabel4_Click(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }
    }
}
