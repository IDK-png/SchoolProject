using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text.Json;
using System.Reflection;
using System.Net.Security;

namespace ServerSide
{
    public class Handlers
    {
        public static string InvokeSqlHelperMethod(string methodName, Dictionary<string, string> parameters)
        {
            MethodInfo? method = typeof(SQLhelper).GetMethod(methodName); // Получаем метод по имени, с помощью вызова метода GetMethod у класса Type
            Console.WriteLine("Method name: " + methodName + "\n Is Found?: " + (method != null));
            if (method == null) // Если метод не найден то посылай нахуй
            {
                throw new ArgumentException("Method not found: " + methodName);
            }

            var parameterValues = method.GetParameters() // Получаем параметры метода
                                        .Select(p => parameters.ContainsKey(p.Name!) ? parameters[p.Name!] : null) // Выбираем значения параметров из словаря
                                        .ToArray(); // Преобразуем в массив
            Console.WriteLine("Parameters: " + string.Join(", ", parameterValues));
            object? result = method.Invoke(null, parameterValues); // Вызываем метод
            if (result == null) // Если метод вернул null то посылай нахуй
            {
                throw new InvalidOperationException("Method invocation returned null.");
            }
            Console.WriteLine("Result: " + result);
            return (string)result; // Возвращаем результат перед этим приобразовав его в строку
        }

        public static void LoginHandler(object obj)
        {
            SslStream stream = (SslStream)obj; // Получаем поток клиента
            byte[] buffer = new byte[1024]; // Создаем буфер для получения данных
            int bytesRead; // Переменная для количества прочитанных байт
            try // Обработка исключений
            {
                while (stream.CanRead && (bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0) // Цикл для получения данных от клиента
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
                                if (SQLhelper.CheckUser(json["username"], json["password"])) // Проверка на наличие пользователя в базе данных
                                {
                                    status.Add("status", "OK"); // Создаем ответ
                                    if (SQLhelper.IsTeacher(json["username"]))
                                    {
                                        status.Add("role", "teacher");
                                    }
                                    else
                                    {
                                        status.Add("role", "student");
                                    }

                                    byte[] response = Encoding.ASCII.GetBytes(JsonHelper.Serialize(status) + "\n"); // Создаем ответ
                                    stream.Write(response, 0, response.Length); // Отправляем ответ клиенту
                                    ClientHandler(stream);
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
                stream.Close();
            }
        }

        public static void ClientHandler(object obj)
        {
            SslStream stream = (SslStream)obj; // Получаем поток клиента
            byte[] buffer = new byte[1024]; // Создаем буфер для получения данных

            int bytesRead; // Переменная для количества прочитанных байт
            try
            {
                while (stream.CanRead && (bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0) // Цикл для получения данных от клиента
                {
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead); // Получаем сообщение от клиента

                    message.Trim(); // Удаляем лишние пробелы

                    if ((message[0] - 0) > 32) // Проверка на то что первый символ в сообщении - это буква
                    {
                        Console.WriteLine("Received: " + message); // {username, password}
                        // example json format: {"name": "Moshe", "surname": "Cohen", "age": "16", "grade": "10", "megamot": "math"}
                        // Convert json to dictionary
                        Dictionary<string, string>? json = JsonHelper.Deserialize<Dictionary<string, string>>(message); // Десериализуем сообщение
                        if (json == null) // Проверка на успешное десериализацию
                        {
                            byte[] response = Encoding.ASCII.GetBytes("Invalid JSON format\n"); // Создаем ответ
                            stream.Write(response, 0, response.Length); // Отправляем ответ клиенту
                        }
                        else if (json.ContainsKey("requestType"))
                        {
                            try
                            {
                                string response = InvokeSqlHelperMethod(json["requestType"], json);
                                byte[] responseBytes = Encoding.ASCII.GetBytes(response + "\n");
                                stream.Write(responseBytes, 0, responseBytes.Length);
                            }
                            catch (ArgumentException ex)
                            {
                                byte[] response = Encoding.ASCII.GetBytes(ex.Message + "\n"); // Создаем ответ
                                stream.Write(response, 0, response.Length); // Отправляем ответ клиенту
                            }
                        }
                        else
                        {
                            byte[] response = Encoding.ASCII.GetBytes("Request type not found!\n"); // Создаем ответ
                            stream.Write(response, 0, response.Length); // Отправляем ответ клиенту
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
                stream.Close();
            }
        }

        public static string SearchStudentsByParams(Dictionary<string, string> json)
        {
            return SQLhelper.GetStudentsByParams(json["name"], json["surname"], json["age"], json["grade"], json["megamot"]);
        }
    }
}
