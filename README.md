# 📚 Library Management System - July Exam Edition

This Windows Forms (WinForms) application is a complete **Library Management System** built using **C# (.NET Framework)** and **MySQL**. It allows students or users to borrow and return books, and it provides an Admin Panel with analytics, audit logs, and full user control.

---

## 🔧 Technologies Used

- **C# (WinForms)**
- **.NET Framework 4.8.1**
- **MySQL** (via MySql.Data)
- **BCrypt.Net** for password hashing
- **Guna.UI2.WinForms** for modern UI styling
- **ScottPlot.WinForms** for charting
- **Export to CSV** for reporting

---

## 👤 User Roles

- **Admin**
  - Full access to user management
  - Promote/Demote users to Pro
  - View and filter Audit Logs
  - View analytics: most borrowed books, genre distribution
  - Export books and borrow logs to CSV
  - Manage overdue notifications

- **User**
  - Login/Register
  - Borrow and return books
  - View their borrowed books
  - See notification popups
  - View overdue alerts

---

## ✅ Features

| Feature                        | Description                                                                 |
|-------------------------------|-----------------------------------------------------------------------------|
| 🔐 Secure Login/Register       | Passwords are hashed using BCrypt                                           |
| 📚 Borrow/Return Books         | Real-time tracking of borrow/return activity                               |
| 📈 Dashboard Analytics         | Admins can see most borrowed books and genre distributions                 |
| 📝 Audit Log                   | Tracks login, borrow, return, admin actions with timestamp and details     |
| 🧑‍💻 User Management            | Admins can upgrade/downgrade users to Pro                                  |
| 🔔 Notifications               | Overdue books automatically alert Admins via popup & are stored in table  |
| 📤 CSV Export                  | Books and borrows can be exported by Admin                                 |

---

## 🗄️ Database Tables (MySQL)

### 📌 1. `Users`
| Column     | Type         | Description                         |
|------------|--------------|-------------------------------------|
| ID         | INT (PK)     | Auto-incremented ID                 |
| Name       | VARCHAR      | Full name or username               |
| Email      | VARCHAR      | Email address (unique)             |
| Password   | VARCHAR      | BCrypt hashed password              |
| Role       | VARCHAR      | `User` or `Admin`                   |
| IsPro      | BOOLEAN      | Whether user is a Pro member        |

---

### 📌 2. `Book`
| Column         | Type         | Description                    |
|----------------|--------------|--------------------------------|
| ID             | INT (PK)     | Auto-incremented ID            |
| Title          | VARCHAR      | Book title                     |
| Author         | VARCHAR      | Book author                    |
| Genre          | VARCHAR      | Genre (e.g., Fiction, Sci-Fi)  |
| AvailableCopies| INT          | Number of available copies     |

---

### 📌 3. `Borrows`
| Column       | Type      | Description                                |
|--------------|-----------|--------------------------------------------|
| ID           | INT (PK)  | Auto-incremented ID                        |
| UserID       | INT (FK)  | Refers to `Users.ID`                      |
| BookID       | INT (FK)  | Refers to `Book.ID`                       |
| BorrowedAt   | DATETIME  | Date/time when the book was borrowed      |
| ReturnBy     | DATETIME  | Expected return date                      |
| ReturnedAt   | DATETIME  | Date/time when the book was returned      |
| Status       | VARCHAR   | Either `Borrowed` or `Returned`           |

---

### 📌 4. `AuditLog`
| Column     | Type       | Description                                |
|------------|------------|--------------------------------------------|
| ID         | INT (PK)   | Auto-incremented ID                        |
| UserID     | INT (FK)   | Refers to `Users.ID`                      |
| Action     | VARCHAR    | Action performed (Login, Borrow, etc.)     |
| Timestamp  | DATETIME   | Date/time of action                        |
| Details    | TEXT       | Additional info about the action           |

---

### 📌 5. `Notification`
| Column     | Type       | Description                                |
|------------|------------|--------------------------------------------|
| ID         | INT (PK)   | Auto-incremented ID                        |
| UserID     | INT (FK)   | Refers to `Users.ID`                      |
| Message    | TEXT       | Notification message                       |
| CreatedAt  | DATETIME   | When notification was created              |
| IsRead     | BOOLEAN    | Whether the user has read the message      |
| Type       | VARCHAR    | Type of notification (e.g., Overdue, Info) |

---

## 📂 Folder Structure
Library Management System (WinForms Project)
│
├── Forms/
│ ├── LoginForm.cs
│ ├── RegisterForm.cs
│ ├── AdminDashboardForm.cs
│ ├── AdminControlCenterForm.cs
│ └── UserDashboardForm.cs
│
├── Helpers/
│ ├── Database.cs
│ └── AuditLogger.cs
│
├── Models/
│ └── Session.cs
│
├── App.config
└── README.md

---

## 🚀 How to Run

### Requirements:
- Visual Studio 2019
- .NET Core 5
- MySQL
- MySql.Data NuGet package
- Guna.UI2.WinForms package
- BCrypt.Net package

### Steps:

1. Clone the repo
2. Create the `Library` database and import tables (or let the app create them)
3. Replace connection string in `Database.cs` and other files:
```csharp
"server=localhost;port=3306;user=root;password=yourpassword;database=Library;" NOTE: You will use your own MySQL String connection

4. Run the app (F5)

Log in as admin or register a new user

🧩 Future Ideas
Email notifications for overdue books

Book search by genre/author

PDF export for reports

Librarian role with limited admin access

🧑‍💻 Developer
Siyabonga Shembe
From Soweto, South Africa 🇿🇦
🚀 Passionate about Software Development, Ethical Hacking, and IT Infrastructure

📜 License
This project is built for educational and personal learning purposes. You're free to fork and improve it, but please credit the original author.

“Built with C#, MySQL, and determination.”
---

Let me know if you want me to export this as a file or add your database `CREATE TABLE` SQL commands too!
