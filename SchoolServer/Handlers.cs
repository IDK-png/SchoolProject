using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text.Json;

namespace ServerSide
{
    public class Handlers
    {
        public static void LoginHandler(object obj)
        {
            TcpClient client = (TcpClient)obj; // Получаем клиента из объекта
            NetworkStream stream = client.GetStream(); // Получаем поток клиента
            byte[] buffer = new byte[1024]; // Создаем буфер для получения данных
            int bytesRead; // Переменная для количества прочитанных байт
            try // Обработка исключений
            {
                while (client.Connected && (bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0) // Цикл для получения данных от клиента
                {
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead); // Получаем сообщение от клиента

                    message.Trim(); // Удаляем лишние пробелы

                    if ((message[0] - 0) > 32) // Проверка на то что первый символ в сообщении - это буква
                    {
                        Console.WriteLine("Received: " + message); // {username, password}

                        Dictionary<string, string>? json = JsonHelper.Deserialize<Dictionary<string, string>>(message); // Десериализуем сообщение
                        if (json == null) // Проверка на успешное десериализацию
                        {
                            Console.WriteLine("Error while deserializing message.");
                            // example json format: {"username": "admin", "password": "admin"}
                        }
                        else
                        {
                            if (json.ContainsKey("username") && json.ContainsKey("password"))
                            {
                                Console.WriteLine("Deserialized: " + json["username"] + " " + json["password"]); // Выводим десериализованное сообщение

                                Dictionary<string, string> status = new Dictionary<string, string>(); // Создаем словарь для ответа
                                if (SQLhelper.CheckUser(SchoolServer.connection!, json["username"], json["password"])) // Проверка на наличие пользователя в базе данных
                                {
                                    status.Add("status", "Login successful"); // Создаем ответ
                                    byte[] response = Encoding.ASCII.GetBytes(JsonHelper.Serialize(status) + "\n"); // Создаем ответ
                                    stream.Write(response, 0, response.Length); // Отправляем ответ клиенту
                                    ClientHandler(client);
                                    // И тут переход к следующему Handler
                                }
                                else
                                {
                                    status.Add("status", "Login failed"); // Создаем ответ
                                    byte[] response = Encoding.ASCII.GetBytes(JsonHelper.Serialize(status) + "\n"); // Создаем ответ
                                    stream.Write(response, 0, response.Length); // Отправляем ответ клиенту
                                }
                            }
                        }
                    }
                }
            }
            catch (IOException)
            {
                Console.WriteLine("Client disconnected.");
            }
            finally
            {
                client.Close();
            }
        }

        public static void ClientHandler(object obj)
        {
            TcpClient client = (TcpClient)obj; // Получаем клиента из объекта
            NetworkStream stream = client.GetStream(); // Получаем поток клиента
            byte[] buffer = new byte[1024]; // Создаем буфер для получения данных
            int bytesRead; // Переменная для количества прочитанных байт
            try
            {
                while (client.Connected && (bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0) // Цикл для получения данных от клиента
                {
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead); // Получаем сообщение от клиента

                    message.Trim(); // Удаляем лишние пробелы

                    if ((message[0] - 0) > 32) // Проверка на то что первый символ в сообщении - это буква
                    {
                        Console.WriteLine("Received: " + message); // {username, password}
                    }
                }
            }
            catch (IOException)
            {
                Console.WriteLine("Client disconnected.");
            }
            finally
            {
                client.Close();
            }
        }

        public static string SearchStudentsByParams(Dictionary<string, string> json)
        {
            return SQLhelper.GetStudentsByParams(SchoolServer.connection!, json["name"], json["surname"], json["age"], json["grade"], json["megamot"]);
        }
    }
}