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

namespace Library_Management_System_July_Exam_
{
    public partial class Remove : Form
    {
        private string connectionString = "server=localhost;port=3363;user=root;password=#Samsunset7;database=Library;SslMode=Required;";

        private DataTable usersTable = new DataTable();

        public Remove()
        {
            InitializeComponent();
            LoadUsers();

         
        }

        private void LoadUsers()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = "SELECT ID, Name FROM Users";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                usersTable.Clear();
                adapter.Fill(usersTable);

                ViewUsers.DataSource = usersTable;
            }
        }

        private void DeleteUser(int userId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string deleteAuditLog = "DELETE FROM Auditlog WHERE UserID = @UserId";
                    using (MySqlCommand cmd = new MySqlCommand(deleteAuditLog, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.ExecuteNonQuery();
                    }

                    string deleteUser = "DELETE FROM Users WHERE ID = @UserId";
                    using (MySqlCommand cmd = new MySqlCommand(deleteUser, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Error deleting user: " + ex.Message);
            }
        }

        private void Remove_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string filterText = textBox1.Text.Trim().Replace("'", "''"); // Sanitize

            if (string.IsNullOrEmpty(filterText))
            {
                ViewUsers.DataSource = usersTable;
            }
            else
            {
                // Use DataView for filtering by Name, Email, or Role
                DataView dv = new DataView(usersTable);
                dv.RowFilter = $"Name LIKE '%{filterText}%'";
                ViewUsers.DataSource = dv;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (ViewUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a user to remove.");
                return;
            }

            // Assuming multi-select is off, get the first selected row
            var selectedRow = ViewUsers.SelectedRows[0];
            int userId = Convert.ToInt32(selectedRow.Cells["ID"].Value);
            string userName = selectedRow.Cells["Name"].Value.ToString();

            var confirmResult = MessageBox.Show($"Are you sure you want to remove user '{userName}'?",
                                                "Confirm Delete",
                                                MessageBoxButtons.YesNo);

            if (confirmResult == DialogResult.Yes)
            {
                DeleteUser(userId);
                MessageBox.Show("User has been deleted successfully!");
                LoadUsers(); // Reload updated data
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AdminControlCenterForm admin = new AdminControlCenterForm();
            admin.Show();
            this.Hide();
        }
    }
}
