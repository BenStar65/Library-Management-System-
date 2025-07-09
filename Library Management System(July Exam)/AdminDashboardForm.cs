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
    public partial class AdminDashboardForm : Form
    {
        DataBase db = new DataBase();

        public AdminDashboardForm()
        {
            InitializeComponent();
        }

        private void AdminDashboardForm_Load(object sender, EventArgs e)
        {
            // UI styling for analytics labels
            lblTotalBooks.BackColor = Color.LightBlue;
            lblTotalBooks.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            

            lblTotalUsers.BackColor = Color.LightGreen;
            lblTotalUsers.Font = new Font("Segoe UI", 12, FontStyle.Bold);
           

            lblOverdueBooks.BackColor = Color.IndianRed;
            lblOverdueBooks.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            


            guna2HtmlLabel3.Text = $"Welcome, {Session.Username}!";
            LoadBooks();
            LoadStats();
            LoadActivityLog();
            
        }

        


        private void LoadBooks()
        {
            string query = "SELECT ID, Title, Author, Genre, AvailableCopies FROM Book";
            MySqlDataAdapter adapter = new MySqlDataAdapter(query, db.GetConnection());
            DataTable table = new DataTable();
            adapter.Fill(table);
            dgvBooks.DataSource = table;
        }

        private void LoadStats()
        {
            db.OpenConnection();

            lblTotalBooks.Text = ExecuteScalar("SELECT COUNT(*) FROM Book").ToString();
            lblTotalUsers.Text = ExecuteScalar("SELECT COUNT(*) FROM Users WHERE Role = 'User'").ToString();
            lblOverdueBooks.Text = ExecuteScalar("SELECT COUNT(*) FROM Borrows WHERE ReturnBy < NOW() AND ReturnedAt IS NULL").ToString();

            db.CloseConnection();
        }

        private int ExecuteScalar(string query)
        {
            MySqlCommand cmd = new MySqlCommand(query, db.GetConnection());
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private void LoadActivityLog()
        {
            string query = @"
                SELECT u.Name AS Borrower,
                       CONCAT(IFNULL(CONCAT('Borrowed ', b.Title), ''), 
                              IFNULL(CONCAT('Returned ', b.Title), '')) AS Action
                FROM Borrows br
                JOIN Users u ON br.UserID = u.ID
                JOIN Book b ON br.BookID = b.ID
                ORDER BY br.BorrowedAt DESC
                LIMIT 10";
            MySqlDataAdapter adapter = new MySqlDataAdapter(query, db.GetConnection());
            DataTable table = new DataTable();
            adapter.Fill(table);
            dgvActivityLog.DataSource = table;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // You can show a modal form or input box for new book data
            AddBook form = new AddBook();
            form.ShowDialog();
            LoadBooks();
            LoadStats();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a book to delete.");
                return;
            }

            int bookId = Convert.ToInt32(dgvBooks.SelectedRows[0].Cells["ID"].Value);
            DialogResult confirm = MessageBox.Show("Are you sure?", "Confirm", MessageBoxButtons.YesNo);
            if (confirm != DialogResult.Yes) return;

            try
            {
                db.OpenConnection();
                string query = "DELETE FROM Book WHERE ID = @id";
                MySqlCommand cmd = new MySqlCommand(query, db.GetConnection());
                cmd.Parameters.AddWithValue("@id", bookId);
                cmd.ExecuteNonQuery();
                db.CloseConnection();

                MessageBox.Show("Book deleted.");
                LoadBooks();
                LoadStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                db.CloseConnection();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Session.UserID = 0;
            Session.Username = null;
            Session.Role = null;
            Session.IsPro = false;

            Login login = new Login();
            login.Show();
            this.Hide();
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtSearchBook.Text.Trim();

            string query = @"SELECT ID, Title, Author, Genre, AvailableCopies 
                     FROM Book
                     WHERE Title LIKE @k OR Author LIKE @k";

            MySqlCommand cmd = new MySqlCommand(query, db.GetConnection());
            cmd.Parameters.AddWithValue("@k", "%" + keyword + "%");

            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            DataTable table = new DataTable();
            adapter.Fill(table);
            dgvBooks.DataSource = table;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            AdminControlCenterForm admin = new AdminControlCenterForm();
            admin.Show();
            this.Hide();
        }
    }
}
