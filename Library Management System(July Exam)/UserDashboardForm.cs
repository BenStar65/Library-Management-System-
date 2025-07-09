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
    public partial class UserDashboardForm : Form
    {
        DataBase db = new DataBase();

        public UserDashboardForm()
        {
            InitializeComponent();
        }

        private void UserDashboardForm_Load(object sender, EventArgs e)
        {
            UserWelcome.Text = $"Welcome, {Library_Management_System_July_Exam_.Session.Username}";
            Account.Visible = Library_Management_System_July_Exam_.Session.IsPro;

            LoadAvailableBooks();
            LoadUserBorrowedBooks();
            
        }



        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
          
        }

        private void LoadAvailableBooks()
        {
            try
            {
                db.OpenConnection();
                string query = "SELECT ID, Title, Author, Genre, AvailableCopies FROM Book WHERE AvailableCopies > 0";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, db.GetConnection());
                DataTable table = new DataTable();
                adapter.Fill(table);
                dgvBooks.DataSource = table;
                db.CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading books: " + ex.Message);
                db.CloseConnection();
            }
        }

        private void LoadUserBorrowedBooks()
        {
            try
            {
                db.OpenConnection();
                string query = @"
                    SELECT b.Title, br.ReturnBy, 
                        CASE 
                            WHEN br.ReturnedAt IS NOT NULL THEN 'Returned'
                            WHEN br.ReturnBy < NOW() THEN 'Overdue'
                            ELSE 'Borrowed' 
                        END AS Status
                    FROM Borrows br
                    JOIN Book b ON br.BookID = b.ID
                    WHERE br.UserID = @uid";
                MySqlCommand cmd = new MySqlCommand(query, db.GetConnection());
                cmd.Parameters.AddWithValue("@uid", Session.UserID);

                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable table = new DataTable();
                adapter.Fill(table);
                dgvBorrowedBooks.DataSource = table;
                db.CloseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading borrowed books: " + ex.Message);
                db.CloseConnection();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a book to borrow.");
                return;
            }

            

            try
            {
                db.OpenConnection();

                int bookId = Convert.ToInt32(dgvBooks.SelectedRows[0].Cells["ID"].Value);
                string bookTitle = dgvBooks.SelectedRows[0].Cells["Title"].Value.ToString(); // Optional for log details
                DateTime returnBy = dateTimePicker1.Value;

                // Insert into Borrows
                string insert = "INSERT INTO Borrows (UserID, BookID, BorrowedAt, ReturnBy, Status) VALUES (@uid, @bid, NOW(), @returnBy, 'Borrowed')";
                MySqlCommand cmd = new MySqlCommand(insert, db.GetConnection());
                cmd.Parameters.AddWithValue("@uid", Session.UserID);
                cmd.Parameters.AddWithValue("@bid", bookId);
                cmd.Parameters.AddWithValue("@returnBy", returnBy);
                cmd.ExecuteNonQuery();

                // Decrease book availability
                string update = "UPDATE Book SET AvailableCopies = AvailableCopies - 1 WHERE ID = @bid";
                MySqlCommand updateCmd = new MySqlCommand(update, db.GetConnection());
                updateCmd.Parameters.AddWithValue("@bid", bookId);
                updateCmd.ExecuteNonQuery();

                // Audit Log
                DataBase.LogAudit(Session.UserID, "Borrow Book", $"Borrowed Book ID: {bookId} - '{bookTitle}', Return By: {returnBy:yyyy-MM-dd}");



                db.CloseConnection();

                MessageBox.Show("Book borrowed successfully!");
                LoadAvailableBooks();
                LoadUserBorrowedBooks();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Borrowing failed: " + ex.Message);
                db.CloseConnection();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dgvBorrowedBooks.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a borrowed book to return.");
                return;
            }

            string bookTitle = dgvBorrowedBooks.SelectedRows[0].Cells["Title"].Value.ToString();

            try
            {
                db.OpenConnection();

                // Find borrow ID and book ID
                string find = @"SELECT br.ID, b.ID AS BookID FROM Borrows br
                    JOIN Book b ON br.BookID = b.ID
                    WHERE br.UserID = @uid AND b.Title = @title AND br.ReturnedAt IS NULL LIMIT 1";
                MySqlCommand cmd = new MySqlCommand(find, db.GetConnection());
                cmd.Parameters.AddWithValue("@uid", Session.UserID);
                cmd.Parameters.AddWithValue("@title", bookTitle);

                MySqlDataReader reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    MessageBox.Show("Borrow record not found.");
                    db.CloseConnection();
                    return;
                }

                int borrowId = Convert.ToInt32(reader["ID"]);
                int bookId = Convert.ToInt32(reader["BookID"]);
                reader.Close();

                // Update borrow status
                string returnQuery = "UPDATE Borrows SET ReturnedAt = NOW(), Status = 'Returned' WHERE ID = @id";
                MySqlCommand returnCmd = new MySqlCommand(returnQuery, db.GetConnection());
                returnCmd.Parameters.AddWithValue("@id", borrowId);
                returnCmd.ExecuteNonQuery();

                // Increase availability
                string updateBook = "UPDATE Book SET AvailableCopies = AvailableCopies + 1 WHERE ID = @bid";
                MySqlCommand updateCmd = new MySqlCommand(updateBook, db.GetConnection());
                updateCmd.Parameters.AddWithValue("@bid", bookId);
                updateCmd.ExecuteNonQuery();

                //  Log return to Audit
                DataBase.LogAudit(Session.UserID, "Return Book", $"Returned Book ID: {bookId} - '{bookTitle}' (Borrow ID: {borrowId})");

                db.CloseConnection();

                MessageBox.Show("Book returned successfully!");
                LoadAvailableBooks();
                LoadUserBorrowedBooks();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Return failed: " + ex.Message);
                db.CloseConnection();
            }

        }

        private void Session_Click(object sender, EventArgs e)
        {
            Session.UserID = 0;
            Session.Username = null;
            Session.Role = null;
            Session.IsPro = false;

            Login login = new Login();
            login.Show();
            this.Hide();
        }

        private void dgvBorrowedBooks_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            foreach (DataGridViewRow row in dgvBorrowedBooks.Rows)
            {
                if (row.Cells["Status"].Value?.ToString() == "Overdue")
                {
                    row.DefaultCellStyle.BackColor = Color.IndianRed;
                    row.DefaultCellStyle.ForeColor = Color.White;
                }
            }
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {
            string keyword = SearchBook.Text.Trim();

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
    }
}
