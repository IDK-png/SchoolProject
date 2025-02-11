using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.Common;

namespace ServerSide
{
    public class SQLhelper
    {
        private static SQLiteConnection? connection;

        internal static string GetStringSha256Hash(string text)
        {
            if (String.IsNullOrEmpty(text)) return String.Empty;
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] textData = Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }

        public static SQLiteConnection? CreateDatabase()
        {
            if (!File.Exists("School.db"))
            {
                SQLiteConnection.CreateFile("School.db");
            }
            connection = new SQLiteConnection("Data Source=School.db;Version=3;");
            try
            {
                connection.Open();
                string sql = "CREATE TABLE IF NOT EXISTS students (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL, surname TEXT NOT NULL, age INTEGER NOT NULL, grade INTEGER NOT NULL, megamot TEXT)";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();

                sql = "CREATE TABLE IF NOT EXISTS users (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, username TEXT NOT NULL, password TEXT NOT NULL, isStudent INTEGER NOT NULL)";
                command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();

                sql = "CREATE TABLE IF NOT EXISTS teachers (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL, surname TEXT NOT NULL, megamot TEXT)";
                command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();

                sql = "CREATE TABLE IF NOT EXISTS marks (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, student_id INTEGER NOT NULL, teacher_id INTEGER NOT NULL, mark INTEGER NOT NULL, date TEXT NOT NULL, megama TEXT NOT NULL)";
                command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();

                if (!IsUserExist("admin"))
                {
                    NewUser(0, "admin", "admin", true);
                }
                if (!IsUserExist("student"))
                {
                    NewUser(1, "student", "student", true);
                }
                if (!IsUserExist("teacher"))
                {
                    NewUser(2, "teacher", "teacher", false);
                }
                return connection;
            }
            catch
            {
                Console.WriteLine("Error while creating database");
                return null;
            }
        }

        public static string GetStudentsByParams(string name, string surname, string age, string grade, string megamot)
        {
            if (connection == null) throw new Exception("Connection is not initialized");
            List<string> conditions = new List<string>();
            if (!string.IsNullOrEmpty(name)) conditions.Add("name = '" + name + "'");
            if (!string.IsNullOrEmpty(surname)) conditions.Add("surname = '" + surname + "'");
            if (!string.IsNullOrEmpty(age)) conditions.Add("age = " + age);
            if (!string.IsNullOrEmpty(grade)) conditions.Add("grade = " + grade);
            if (!string.IsNullOrEmpty(megamot)) conditions.Add("megamot = '" + megamot + "'");

            string sql = "select * from students";
            if (conditions.Count > 0)
            {
                sql += " where " + string.Join(" and ", conditions);
            }

            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            StringBuilder result = new StringBuilder();
            while (reader.Read())
            {
                result.AppendLine($"{reader["id"]} {reader["name"]} {reader["surname"]} {reader["age"]} {reader["grade"]} {reader["megamot"]}");
            }
            return result.ToString();
        }

        public static void NewUser(int id, string username, string password, bool isStudent)
        {
            if (connection == null) throw new Exception("Connection is not initialized");
            string sql = "insert into users (id, username, password, isStudent) values (@id, @username, @password, @isStudent)";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", GetStringSha256Hash(password));
            command.Parameters.AddWithValue("@isStudent", isStudent);
            command.ExecuteNonQuery();
        }

        public static bool CheckUser(string username, string password)
        {
            if (connection == null) throw new Exception("Connection is not initialized");
            string sql = "select * from users where username = '" + username + "' and password = '" + GetStringSha256Hash(password) + "'";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            return reader.Read();
        }

        public static bool IsUserExist(string username)
        {
            if (connection == null) throw new Exception("Connection is not initialized");
            string sql = "select * from users where username = '" + username + "'";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            return reader.Read();
        }

        public static void AddStudent(string name, string surname, int age, int grade, string megamot)
        {
            if (connection == null) throw new Exception("Connection is not initialized");
            int id = GetIdByName("students", name);
            if (id != -1)
            {
                Console.WriteLine("Student with this name already exists.");
                return;
            }
            string sql = "insert into students (name, surname, age, grade, megamot) values ('" + name + "', '" + surname + "', " + age + ", " + grade + ", '" + megamot + "')";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            // Если нужно создать пользователя для студента, можно вызвать NewUser после вставки и получения id
            command.ExecuteNonQuery();
        }

        public static void AddTeacher(string name, string surname, string megamot)
        {
            if (connection == null) throw new Exception("Connection is not initialized");
            int id = GetIdByName("teachers", name);
            if (id != -1)
            {
                Console.WriteLine("Teacher with this name already exists.");
                return;
            }
            NewUser(id, name, "123", false);
            string sql = "insert into teachers (name, surname, megamot) values ('" + name + "', '" + surname + "', '" + megamot + "')";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        public static void DeleteStudent(int id)
        {
            if (connection == null) throw new Exception("Connection is not initialized");
            string sql = "delete from students where id = " + id;
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        public static int GetIdByName(string table, string name)
        {
            if (connection == null) throw new Exception("Connection is not initialized");
            string sql = "select id from " + table + " where name = '" + name + "'";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                return int.Parse(reader["id"].ToString()!);
            }
            return -1;
        }

        public static void AddMark(int student_id, int teacher_id, int mark, string date, string megama)
        {
            if (connection == null) throw new Exception("Connection is not initialized");
            string sql = "insert into marks (student_id, teacher_id, mark, date, megama) values (" + student_id + ", " + teacher_id + ", " + mark + ", '" + date + "', '" + megama + "')";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        public static void DeleteMark(int id)
        {
            if (connection == null) throw new Exception("Connection is not initialized");
            string sql = "delete from marks where id = " + id;
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        public static void UpdateMark(int id, int mark)
        {
            if (connection == null) throw new Exception("Connection is not initialized");
            string sql = "update marks set mark = " + mark + " where id = " + id;
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        public static void UpdateStudent(int id, string name, string surname, int age, int grade, string megamot)
        {
            if (connection == null) throw new Exception("Connection is not initialized");
            string sql = "update students set name = '" + name + "', surname = '" + surname + "', age = " + age + ", grade = " + grade + ", megamot = '" + megamot + "' where id = " + id;
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        public static bool IsTeacher(string username)
        {
            if (connection == null) throw new Exception("Connection is not initialized");
            string sql = "select * from users where username = '" + username + "'";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                return reader["isStudent"].ToString() == "0";
            }
            return false;
        }

    }
}
