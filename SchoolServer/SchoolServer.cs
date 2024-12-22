using System;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Data.SQLite;
using System.Reflection;

namespace ServerSide
{
    class SchoolServer
    {
        private static TcpListener? _server; // Сам Listener который отвечает за прослушивание определенного порта
        private static bool _isRunning; // Переменная для проверки работы сервера, не обязательна но удобна
        public static SQLiteConnection? connection;
        private static int port = 76; // Порт сервера
        static int Main(string[] args) // Main функция
        {
            _server = new TcpListener(IPAddress.Any, port); // Создаем новый Listener на порту 76
            _server.Start(); // Запускаем сервер
            _isRunning = true; // Устанавливаем переменную работы сервера в true

            Console.WriteLine("Connecting to database...");
            connection = SQLhelper.CreateDatabase(); // Создаем подключение к базе данных
            if(connection == null) // Проверка на успешное создание подключения
            {
                Console.WriteLine("Error while connecting to database.");
                return 1; // Возвращаем 1 если произошла ошибка при создании/открытии базы данных
            }

            Console.WriteLine("Server started on port " + port); // Выводим сообщение о запуске сервера

            // Add with SQLhelper.AddStudent 10 random students with israeli names and chemistry/biology/math in megamot argument
            // name, surname, age, grade, subject
            SQLhelper.AddStudent(connection, "Moshe", "Cohen", 16, 10, "math");
            SQLhelper.AddStudent(connection, "Yosef", "Ben-David", 15, 9, "chemistry");
            SQLhelper.AddStudent(connection, "Avraham", "Cohen", 16, 10, "math");
            SQLhelper.AddStudent(connection, "Yitzhak", "Levi", 17, 11, "biology");
            SQLhelper.AddStudent(connection, "Yaakov", "Ben-David", 15, 9, "chemistry");
            SQLhelper.AddStudent(connection, "David", "Levi", 17, 11, "biology");
            while (_isRunning) // Бесконечный цикл для принятия множество клиентов
            {
                TcpClient newClient = _server.AcceptTcpClient(); // Принимаем нового клиента
                IPEndPoint clientEndPoint = (IPEndPoint)newClient.Client.RemoteEndPoint!; // Получаем информацию о клиенте
                
                string clientIP = clientEndPoint.Address.ToString(); // Получаем IP клиента
                string logMessage = "New client connected: " + clientIP; // Создаем сообщение о подключении клиента

                Console.WriteLine(logMessage); // Выводим сообщение о подключении нового клиента

                Thread clientThread = new Thread(Handlers.LoginHandler!); // Создаем новый поток для обработки клиента
                clientThread.Start(newClient); // Запускаем поток и передаем в него клиента
            }
            return 0;
        }
    }
}