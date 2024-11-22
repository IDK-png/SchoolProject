using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace ServerSide
{
    public class SQLhelper
    {
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
                string sql = "create table if not exists students (id integer primary key, name text, surname text, age integer, grade integer, megamot text)"; // Создаем таблицу студентов
                SQLiteCommand command = new SQLiteCommand(sql, connection); // Создаем команду
                command.ExecuteNonQuery(); // Выполняем команду
                return connection; // Возвращаем подключение к базе данных
            }
            catch
            {
                Console.WriteLine("Error while creating database");
                return null; // Возвращаем 1 если произошла ошибка при создании/открытии базы данных
            }
        }

        public static void AddStudent(SQLiteConnection connection, string name, string surname, int age, int grade, string megamot)
        {
            string sql = "insert into students (name, surname, age, grade, megamot) values ('" + name + "', '" + surname + "', " + age + ", " + grade + ", '" + megamot + "')"; // Создаем запрос на добавление студента
            SQLiteCommand command = new SQLiteCommand(sql, connection); // Создаем команду
            command.ExecuteNonQuery(); // Выполняем команду
        }

        public static void DeleteStudent(SQLiteConnection connection, int id)
        {
            string sql = "delete from students where id = " + id; // Создаем запрос на удаление студента
            SQLiteCommand command = new SQLiteCommand(sql, connection); // Создаем команду
            command.ExecuteNonQuery(); // Выполняем команду
        }
    }
}
