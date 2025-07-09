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
    public partial class RegisterForm : Form
    {
        DataBase db = new DataBase();

        public RegisterForm()
        {
            InitializeComponent();
        }

        private void RegisterForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = Username.Text.Trim();
            string email = Email.Text.Trim();
            string password = Password.Text;
            string confirm = ConfirmPassword.Text;

            if (username == "" || email == "" || password == "" || confirm == "")
            {
                label6.Text = "Please fill in all fields.";
                label6.Visible = true;
                return;
            }

            if (password != confirm)
            {
                label6.Text = "Passwords do not match.";
                label6.Visible = true;
                return;
            }

            if (password.Length < 6)
            {
                label6.Text = "Password must be at least 6 characters.";
                label6.Visible = true;
                return;
            }

            try
            {
                db.OpenConnection();

                // Check if email or username already exists
                string checkQuery = "SELECT * FROM Users WHERE Name = @u OR Email = @e LIMIT 1";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, db.GetConnection());
                checkCmd.Parameters.AddWithValue("@u", username);
                checkCmd.Parameters.AddWithValue("@e", email);

                MySqlDataReader reader = checkCmd.ExecuteReader();
                if (reader.Read())
                {
                    label6.Text = "Username or email already exists.";
                    label6.Visible = true;
                    db.CloseConnection();
                    return;
                }
                reader.Close();

                // Hash password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                // Insert new user
                string insertQuery = "INSERT INTO Users (Name, Email, Password, Role, IsPro, CreatedAt) VALUES (@u, @e, @p, 'User', FALSE, NOW())";
                MySqlCommand insertCmd = new MySqlCommand(insertQuery, db.GetConnection());
                insertCmd.Parameters.AddWithValue("@u", username);
                insertCmd.Parameters.AddWithValue("@e", email);
                insertCmd.Parameters.AddWithValue("@p", hashedPassword);

                

                insertCmd.ExecuteNonQuery();
                db.CloseConnection();

                MessageBox.Show("Account created! Please login.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Redirect to login
                Login login = new Login();
                login.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                db.CloseConnection();
                label6.Text = "Error: " + ex.Message;
                label6.Visible = true;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            this.Hide();
        }
    }
}
