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
                    NewUser("admin", "admin", false);
                }
                if (!IsUserExist("student"))
                {
                    NewUser("student", "student", true);
                }
                if (!IsUserExist("teacher"))
                {
                    NewUser("teacher", "teacher", false);
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

        public static void NewUser(string username, string password, bool isStudent)
        {
            if (connection == null) throw new Exception("Connection is not initialized");
            string hashedPassword = GetStringSha256Hash(password);
            // Check: if a user with this username and password already exists, do nothing
            string checkSql = "select * from users where username = @username and password = @password";
            SQLiteCommand checkCommand = new SQLiteCommand(checkSql, connection);
            checkCommand.Parameters.AddWithValue("@username", username);
            checkCommand.Parameters.AddWithValue("@password", hashedPassword);
            SQLiteDataReader reader = checkCommand.ExecuteReader();
            if (reader.Read())
            {
                // Such a user already exists, do nothing
                return;
            }
            reader.Close();
            // Add new user (id will be AUTOINCREMENT)
            string sql = "insert into users (username, password, isStudent) values (@username, @password, @isStudent)";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", hashedPassword);
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
            
            // Проверка: если студент с таким именем и фамилией уже есть, не добавлять
            string checkStudentSql = "select * from students where name = @name and surname = @surname";
            SQLiteCommand checkStudentCommand = new SQLiteCommand(checkStudentSql, connection);
            checkStudentCommand.Parameters.AddWithValue("@name", name);
            checkStudentCommand.Parameters.AddWithValue("@surname", surname);
            SQLiteDataReader studentReader = checkStudentCommand.ExecuteReader();
            if (studentReader.Read())
            {
                Console.WriteLine($"⚠️ Student {name} {surname} already exists. Not added.");
                studentReader.Close();
                return;
            }
            studentReader.Close();
            
            // Check: if a user with this name already exists, do not add
            if (IsUserExist(name))
            {
                Console.WriteLine($"A user with the name {name} already exists. Student not added.");
                return;
            }
            // Add student to the students table
            string sql = "insert into students (name, surname, age, grade, megamot) values (@name, @surname, @age, @grade, @megamot)";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@surname", surname);
            command.Parameters.AddWithValue("@age", age);
            command.Parameters.AddWithValue("@grade", grade);
            command.Parameters.AddWithValue("@megamot", megamot);
            
            try
            {
                command.ExecuteNonQuery();
                Console.WriteLine($"Successfully added student: {name} {surname}");
                
                // Get the ID of the added student
                long lastId = connection.LastInsertRowId;
                
                // Create a unique username based on the student's name
                string username = name;
                int suffix = 0;
                
                // Check if a user with this name exists
                while (IsUserExist(username + (suffix > 0 ? suffix.ToString() : "")))
                {
                    suffix++;
                }
                
                // Add suffix if necessary
                if (suffix > 0)
                {
                    username += suffix;
                }
                
                // Create a new user for the student with the surname as the password
                NewUser(username, surname, true);
                Console.WriteLine($"Created user account for student: {username} with password: {surname}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding student {name} {surname}: {ex.Message}");
            }
        }

        public static void AddTeacher(string name, string surname, string megamot)
        {
            if (connection == null) throw new Exception("Connection is not initialized");
            
            // Проверка: если учитель с таким именем и фамилией уже есть, не добавлять
            string checkTeacherSql = "select * from teachers where name = @name and surname = @surname";
            SQLiteCommand checkTeacherCommand = new SQLiteCommand(checkTeacherSql, connection);
            checkTeacherCommand.Parameters.AddWithValue("@name", name);
            checkTeacherCommand.Parameters.AddWithValue("@surname", surname);
            SQLiteDataReader teacherReader = checkTeacherCommand.ExecuteReader();
            if (teacherReader.Read())
            {
                Console.WriteLine($"⚠️ Teacher {name} {surname} already exists. Not added.");
                teacherReader.Close();
                return;
            }
            teacherReader.Close();
            
            // Check: if a user with this username and password already exists, do not add
            string username = name;
            string password = surname;
            string hashedPassword = GetStringSha256Hash(password);
            string checkSql = "select * from users where username = @username and password = @password";
            SQLiteCommand checkCommand = new SQLiteCommand(checkSql, connection);
            checkCommand.Parameters.AddWithValue("@username", username);
            checkCommand.Parameters.AddWithValue("@password", hashedPassword);
            SQLiteDataReader reader = checkCommand.ExecuteReader();
            if (reader.Read())
            {
                Console.WriteLine($"A user with the name {username} already exists. Teacher not added.");
                reader.Close();
                return;
            }
            reader.Close();
            // Add teacher to the teachers table
            string sql = "insert into teachers (name, surname, megamot) values (@name, @surname, @megamot)";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@surname", surname);
            command.Parameters.AddWithValue("@megamot", megamot);
            
            try
            {
                command.ExecuteNonQuery();
                Console.WriteLine($"Successfully added teacher: {name} {surname}");
                
                // Get the ID of the added teacher
                long lastId = connection.LastInsertRowId;
                
                // Create a unique username based on the teacher's name
                int suffix = 0;
                while (IsUserExist(username + (suffix > 0 ? suffix.ToString() : "")))
                {
                    suffix++;
                }
                if (suffix > 0)
                {
                    username += suffix;
                }
                // Create a new user for the teacher with the surname as the password
                NewUser(username, surname, false); // isStudent = false, because this is a teacher
                Console.WriteLine($"Created user account for teacher: {username} with password: {surname}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding teacher {name} {surname}: {ex.Message}");
            }
        }

        public static bool IsUserExistById(int id)
        {
            if (connection == null) throw new Exception("Connection is not initialized");
            string sql = "select * from users where id = @id";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            SQLiteDataReader reader = command.ExecuteReader();
            return reader.Read();
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

        public static string GetStudentMarks(string studentName)
        {
            if (connection == null) throw new Exception("Connection is not initialized");

            // Get the student's ID by name
            int studentId = GetIdByName("students", studentName);
            if (studentId == -1)
            {
                return "Student does not exist.";
            }

            // Get the student's marks
            string sql = "select * from marks where student_id = @studentId";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("@studentId", studentId);
            SQLiteDataReader reader = command.ExecuteReader();
            StringBuilder result = new StringBuilder();
            while (reader.Read())
            {
                result.AppendLine($"{reader["id"]} {reader["student_id"]} {reader["teacher_id"]} {reader["mark"]} {reader["date"]} {reader["megama"]}");
            }

            if (result.Length == 0)
            {
                return "No marks found for this student.";
            }
            return result.ToString();
        }

        public static string AddMark(int student_id, int teacher_id, int mark, string date, string megama)
        {
            if (connection == null) throw new Exception("Connection is not initialized");
            try
            {
                string sql = "insert into marks (student_id, teacher_id, mark, date, megama) values (@student_id, @teacher_id, @mark, @date, @megama)";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.Parameters.AddWithValue("@student_id", student_id);
                command.Parameters.AddWithValue("@teacher_id", teacher_id);
                command.Parameters.AddWithValue("@mark", mark);
                command.Parameters.AddWithValue("@date", date);
                command.Parameters.AddWithValue("@megama", megama);
                command.ExecuteNonQuery();
                return "Mark successfully added";
            }
            catch (Exception ex)
            {
                return "Error adding mark: " + ex.Message;
            }
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

        public static string GetAllTeachers()
        {
            if (connection == null) throw new Exception("Connection is not initialized");
            string sql = "select * from teachers";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            StringBuilder result = new StringBuilder();
            while (reader.Read())
            {
                result.AppendLine($"{reader["name"]} {reader["surname"]} {reader["megamot"]}");
            }
            return result.ToString();
        }
        
        public static string UpdateUserPassword(string username, string newPassword)
        {
            if (connection == null) throw new Exception("Connection is not initialized");
            try
            {
                // Check if the user exists
                if (!IsUserExist(username))
                {
                    return "User not found";
                }
                
                string sql = "UPDATE users SET password = @password WHERE username = @username";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.Parameters.AddWithValue("@password", GetStringSha256Hash(newPassword));
                command.Parameters.AddWithValue("@username", username);
                int rowsAffected = command.ExecuteNonQuery();
                
                if (rowsAffected > 0)
                {
                    return "Password updated successfully";
                }
                else
                {
                    return "Failed to update password (no changes in DB)";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating password: {ex.Message}");
                return $"Error updating password: {ex.Message}";
            }
        }

    }
}
