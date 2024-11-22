using System;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Data.SQLite;

namespace ServerSide
{
    internal class SchoolServer
    {
        private static TcpListener? _server; // Сам Listener который отвечает за прослушивание определенного порта
        private static bool _isRunning; // Переменная для проверки работы сервера, не обязательна но удобна

        static int Main(string[] args) // Main функция
        {
            _server = new TcpListener(IPAddress.Any, 1488); // Создаем новый Listener на порту 1488
            _server.Start(); // Запускаем сервер
            _isRunning = true; // Устанавливаем переменную работы сервера в true

            Console.WriteLine("Connecting to database...");
            SQLiteConnection? connection = SQLhelper.CreateDatabase(); // Создаем подключение к базе данных
            if(connection == null) // Проверка на успешное создание подключения
            {
                Console.WriteLine("Error while connecting to database.");
                return 1; // Возвращаем 1 если произошла ошибка при создании/открытии базы данных
            }

            Console.WriteLine("Server started on port 1488.");  


            while (_isRunning) // Бесконечный цикл для принятия множество клиентов
            {
                TcpClient newClient = _server.AcceptTcpClient(); // Принимаем нового клиента
                IPEndPoint clientEndPoint = (IPEndPoint)newClient.Client.RemoteEndPoint; // Получаем информацию о клиенте
                
                string clientIP = clientEndPoint.Address.ToString(); // Получаем IP клиента
                string logMessage = "New client connected: " + clientIP; // Создаем сообщение о подключении клиента

                Console.WriteLine(logMessage); // Выводим сообщение о подключении нового клиента

                Thread clientThread = new Thread(HandleClient); // Создаем новый поток для обработки клиента
                clientThread.Start(newClient); // Запускаем поток и передаем в него клиента
            }
            return 0;
        }

        private static void HandleClient(object obj) // Метод отвечающий за обработку запросов клиета
        {
            TcpClient client = (TcpClient)obj; // Получаем клиента из объекта
            NetworkStream stream = client.GetStream(); // Получаем поток клиента
            byte[] buffer = new byte[1024]; // Создаем буфер для получения данных
            int bytesRead; // Переменная для количества прочитанных байт

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0) // Цикл для получения данных от клиента
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead); // Получаем сообщение от клиента

                message.Trim(); // Удаляем лишние пробелы

                if ((message[0]-0)>32) // Проверка на то что первый символ в сообщении - это буква
                {
                    Console.WriteLine("Received: " + message); 
                    byte[] response = Encoding.ASCII.GetBytes("Message received\n");
                    stream.Write(response, 0, response.Length);
                }
            }

            client.Close();
            Console.WriteLine("Client disconnected.");
        }
    }
}