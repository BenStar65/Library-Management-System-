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
    public partial class Login : Form
    {
        DataBase db = new DataBase();

        public Login()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void ShowOverdueNotifications()
        {
            string query = @"SELECT u.Name AS Name, b.Title AS BookTitle, br.ReturnBy
                     FROM Borrows br
                     JOIN Users u ON br.UserID = u.ID
                     JOIN Book b ON br.BookID = b.ID
                     WHERE br.Status = 'Borrowed' AND br.ReturnBy < NOW()";

            using (var conn = new MySqlConnection("server=localhost;port=3363;user=root;password=#Samsunset7;database=Library;SslMode=Required;"))
            {
                MySqlCommand cmd = new MySqlCommand(query, conn);
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();

                StringBuilder overdueMessage = new StringBuilder();
                while (reader.Read())
                {
                    string user = reader["Name"].ToString();
                    string book = reader["BookTitle"].ToString();
                    string dueDate = Convert.ToDateTime(reader["ReturnBy"]).ToString("yyyy-MM-dd");

                    overdueMessage.AppendLine($"• {user} - \"{book}\" (Due: {dueDate})");
                }

                conn.Close();

                if (overdueMessage.Length > 0)
                {
                    MessageBox.Show("📢 Overdue Books:\n\n" + overdueMessage.ToString(), "Overdue Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        } 


        private void button1_Click(object sender, EventArgs e)
        {
            string usernameOrEmail = Username.Text.Trim();
            string password = Password.Text;

            if (usernameOrEmail == "" || password == "")
            {
                label4.Text = "Please fill in all fields.";
                return; 
            }

            try
            {
                db.OpenConnection();

                string query = "SELECT * FROM Users WHERE Name = @u OR Email = @u LIMIT 1";
                MySqlCommand cmd = new MySqlCommand(query, db.GetConnection());
                cmd.Parameters.AddWithValue("@u", usernameOrEmail);

                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string storedHash = reader["Password"].ToString();
                    bool isValid = false;

                    if (storedHash.StartsWith("$2")) // Check for hashed password
                    {
                        isValid = BCrypt.Net.BCrypt.Verify(password, storedHash);
                    }
                    else
                    {
                        isValid = (password == storedHash); // Plain text fallback
                    }

                    if (isValid)
                    {
                        // Set session
                        Session.UserID = Convert.ToInt32(reader["ID"]);
                        Session.Username = reader["Name"].ToString();
                        Session.Role = reader["Role"].ToString();
                        Session.IsPro = Convert.ToBoolean(reader["IsPro"]);

                        db.CloseConnection();

                        // Log successful login
                        DataBase.LogAudit(Session.UserID, "Login", $"User '{Session.Username}' logged in successfully.");

                        // Redirect
                        if (Session.Role == "Admin")
                        {
                            ShowOverdueNotifications(); // <-- insert here
                            AdminDashboardForm admin = new AdminDashboardForm();
                            admin.Show();
                        }

                        else
                        {
                            UserDashboardForm user = new UserDashboardForm();
                            user.Show();
                        }

                        this.Hide();
                    }
                    else
                    {
                        label4.Text = "Incorrect password.";
                        db.CloseConnection();

                        // Log failed login (wrong password)
                        DataBase.LogAudit(0, "Login Failed", $"Incorrect password attempt for user/email: {usernameOrEmail}");
                    }
                }
                else
                {
                    label4.Text = "User not found.";
                    db.CloseConnection();

                    // Log failed login (unknown user)
                    DataBase.LogAudit(0, "Login Failed", $"Login attempt with unknown username/email: {usernameOrEmail}");
                }
            }
            catch (Exception ex)
            {
                label4.Text = "Error: " + ex.Message;
                db.CloseConnection();
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            RegisterForm register = new RegisterForm();
            register.Show();
            this.Hide();
        }
    }
}

