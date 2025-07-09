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
    public partial class AddBook : Form
    {
        DataBase db = new DataBase();

        public AddBook()
        {
            InitializeComponent();
        }

        private void AddBook_Load(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string title = txtTitle.Text.Trim();
            string author = txtAuthor.Text.Trim();
            string genre = txtGenre.Text.Trim();
            int copies = (int)numCopies.Value;

            if (title == "" || author == "" || genre == "" || copies <= 0)
            {
                MessageBox.Show("Please fill in all fields correctly.");
                return;
            }

            try
            {
                db.OpenConnection();
                string query = "INSERT INTO Book (Title, Author, Genre, AvailableCopies) VALUES (@t, @a, @g, @c)";
                MySqlCommand cmd = new MySqlCommand(query, db.GetConnection());
                cmd.Parameters.AddWithValue("@t", title);
                cmd.Parameters.AddWithValue("@a", author);
                cmd.Parameters.AddWithValue("@g", genre);
                cmd.Parameters.AddWithValue("@c", copies);
                cmd.ExecuteNonQuery();
                db.CloseConnection();

                MessageBox.Show("Book added successfully!");
                this.Close();
            }
            catch (Exception ex)
            {
                db.CloseConnection();
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
