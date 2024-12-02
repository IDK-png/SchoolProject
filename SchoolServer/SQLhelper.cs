using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.Common;

namespace ServerSide
{
    public class SQLhelper
    {
        internal static string GetStringSha256Hash(string text)
        {
            if (String.IsNullOrEmpty(text)) return String.Empty;

            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }
        public static SQLiteConnection? CreateDatabase()
        {
            if (!File.Exists("School.db"))
            {
                SQLiteConnection.CreateFile("School.db"); // Создаем базу данных, если она не существует
            }
            SQLiteConnection connection = new SQLiteConnection("Data Source=School.db;Version=3;"); // Создаем подключение к базе данных

            try
            {
                connection.Open(); // Открываем подключение
                string sql = "CREATE TABLE IF NOT EXISTS students (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL, surname TEXT NOT NULL, age INTEGER NOT NULL, grade INTEGER NOT NULL, megamot TEXT)"; // Создаем таблицу студентов
                SQLiteCommand command = new SQLiteCommand(sql, connection); // Создаем команду
                command.ExecuteNonQuery(); // Выполняем команду

                sql = "CREATE TABLE IF NOT EXISTS users (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, username TEXT NOT NULL, password TEXT NOT NULL, isStudent INTEGER NOT NULL)"; // Создаем таблицу пользователей
                command = new SQLiteCommand(sql, connection); // Создаем команду
                command.ExecuteNonQuery(); // Выполняем команду

                sql = "CREATE TABLE IF NOT EXISTS teachers (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL, surname TEXT NOT NULL, megamot TEXT)"; // Создаем таблицу пользователей
                command = new SQLiteCommand(sql, connection); // Создаем команду
                command.ExecuteNonQuery(); // Выполняем команду

                sql = "CREATE TABLE IF NOT EXISTS marks (id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, student_id INTEGER NOT NULL, teacher_id INTEGER NOT NULL, mark INTEGER NOT NULL, date TEXT NOT NULL, megama TEXT NOT NULL)"; // Создаем таблицу пользователей
                command = new SQLiteCommand(sql, connection); // Создаем команду
                command.ExecuteNonQuery(); // Выполняем команду

                // Create default users if they don't exist
                if (!IsUserExist(connection, "admin"))
                {
                    NewUser(connection, 0, "admin", "admin", true);
                }
                if (!IsUserExist(connection, "student"))
                {
                    NewUser(connection, 1, "student", "student", true);
                }
                if (!IsUserExist(connection, "teacher"))
                {
                    NewUser(connection, 2, "teacher", "teacher", false);
                }
                return connection; // Возвращаем подключение к базе данных
            }
            catch
            {
                Console.WriteLine("Error while creating database");
                return null; // Возвращаем 1 если произошла ошибка при создании/открытии базы данных
            }
        }

        public static void NewUser(SQLiteConnection connection, int id, string username, string password, bool isStudent)
        {
            string sql = "insert into users (id, username, password, isStudent) values (@id, @username, @password, @isStudent)"; // Создаем запрос на добавление пользователя
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", GetStringSha256Hash(password));
            command.Parameters.AddWithValue("@isStudent", isStudent);
            command.ExecuteNonQuery();
        }

        public static bool CheckUser(SQLiteConnection connection, string username, string password)
        {
            string sql = "select * from users where username = '" + username + "' and password = '" + GetStringSha256Hash(password) + "'"; // Создаем запрос на проверку пользователя
            // example sql query: select * from users where username = 'admin' and password = '8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918'
            SQLiteCommand command = new SQLiteCommand(sql, connection); // Создаем команду
            SQLiteDataReader reader = command.ExecuteReader(); // Создаем читателя
            return reader.Read(); // Возвращаем результат проверки
        }

        public static bool IsUserExist(SQLiteConnection connection, string username)
        {
            string sql = "select * from users where username = '" + username + "'"; // Создаем запрос на проверку пользователя
            SQLiteCommand command = new SQLiteCommand(sql, connection); // Создаем команду
            SQLiteDataReader reader = command.ExecuteReader(); // Создаем читателя
            return reader.Read(); // Возвращаем результат проверки
        }

        public static void AddStudent(SQLiteConnection connection, string name, string surname, int age, int grade, string megamot)
        {
            string sql = "insert into students (name, surname, age, grade, megamot) values ('" + name + "', '" + surname + "', " + age + ", " + grade + ", '" + megamot + "')"; // Создаем запрос на добавление студента
            SQLiteCommand command = new SQLiteCommand(sql, connection); // Создаем команду
            int id = GetIdByName(connection, "students", name); // Получаем id студента
            if (id != -1) // Проверяем что студент с таким именем уже существует
            {
                Console.WriteLine("Student with this name already exists.");
                return; // Возвращаемся если студент с таким именем уже существует
            }
            NewUser(connection, id, name, "123", true); // Создаем нового пользователя(Пароль он меняет после входа)
            command.ExecuteNonQuery(); // Выполняем команду
        }

        public static void AddTeacher(SQLiteConnection connection, string name, string surname, string megamot)
        {
            string sql = "insert into teachers (name, surname, megamot) values ('" + name + "', '" + surname + "', '" + megamot + "')"; // Создаем запрос на добавление учителя
            SQLiteCommand command = new SQLiteCommand(sql, connection); // Создаем команду
            int id = GetIdByName(connection, "teachers", name); // Получаем id учителя
            if (id != -1) // Проверяем что учитель с таким именем уже существует
            {
                Console.WriteLine("Teacher with this name already exists.");
                return; // Возвращаемся если учитель с таким именем уже существует
            }
            NewUser(connection, id, name, "123", false); // Создаем нового пользователя(Пароль он меняет после входа)
            command.ExecuteNonQuery(); // Выполняем команду
        }

        public static void DeleteStudent(SQLiteConnection connection, int id)
        {
            string sql = "delete from students where id = " + id; // Создаем запрос на удаление студента
            SQLiteCommand command = new SQLiteCommand(sql, connection); // Создаем команду
            command.ExecuteNonQuery(); // Выполняем команду
        }

        public static int GetIdByName(SQLiteConnection connection, string table, string name)
        {
            string sql = "select id from " + table + " where name = '" + name + "'"; // Создаем запрос на получение id студента
            SQLiteCommand command = new SQLiteCommand(sql, connection); // Создаем команду
            SQLiteDataReader reader = command.ExecuteReader(); // Создаем читателя
            while (reader.Read()) // Читаем результат
            {
                return int.Parse(reader["id"].ToString()!); // Возвращаем id студента
            }
            return -1; // Возвращаем -1 если студент не найден
        }

        public static void AddMark(SQLiteConnection connection, int student_id, int teacher_id, int mark, string date, string megama)
        {
            string sql = "insert into marks (student_id, teacher_id, mark, date, megama) values (" + student_id + ", " + teacher_id + ", " + mark + ", '" + date + "', '" + megama + "')"; // Создаем запрос на добавление оценки
            SQLiteCommand command = new SQLiteCommand(sql, connection); // Создаем команду
            command.ExecuteNonQuery(); // Выполняем команду
        }

        public static void DeleteMark(SQLiteConnection connection, int id)
        {
            string sql = "delete from marks where id = " + id; // Создаем запрос на удаление оценки
            SQLiteCommand command = new SQLiteCommand(sql, connection); // Создаем команду
            command.ExecuteNonQuery(); // Выполняем команду
        }

        public static void UpdateMark(SQLiteConnection connection, int id, int mark)
        {
            string sql = "update marks set mark = " + mark + " where id = " + id; // Создаем запрос на обновление оценки
            SQLiteCommand command = new SQLiteCommand(sql, connection); // Создаем команду
            command.ExecuteNonQuery(); // Выполняем команду
        }

        public static void UpdateStudent(SQLiteConnection connection, int id, string name, string surname, int age, int grade, string megamot)
        {
            string sql = "update students set name = '" + name + "', surname = '" + surname + "', age = " + age + ", grade = " + grade + ", megamot = '" + megamot + "' where id = " + id; // Создаем запрос на обновление студента
            SQLiteCommand command = new SQLiteCommand(sql, connection); // Создаем команду
            command.ExecuteNonQuery(); // Выполняем команду
        }
    }
}
