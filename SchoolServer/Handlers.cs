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
            MethodInfo? method = typeof(SQLhelper).GetMethod(methodName); // Get method by name using GetMethod of Type class
            Console.WriteLine("Method name: " + methodName + "\n Is Found?: " + (method != null));
            if (method == null) // If method not found, throw exception
            {
                throw new ArgumentException("Method not found: " + methodName);
            }

            var methodParams = method.GetParameters(); // Get method parameters
            object[] parameterValues = new object[methodParams.Length];

            for (int i = 0; i < methodParams.Length; i++)
            {
                var param = methodParams[i];
                if (parameters.ContainsKey(param.Name!))
                {
                    string paramValue = parameters[param.Name!];
                    
                    // Convert string value to required type
                    if (param.ParameterType == typeof(int))
                    {
                        parameterValues[i] = int.Parse(paramValue);
                    }
                    else if (param.ParameterType == typeof(bool))
                    {
                        parameterValues[i] = bool.Parse(paramValue);
                    }
                    else if (param.ParameterType == typeof(double))
                    {
                        parameterValues[i] = double.Parse(paramValue);
                    }
                    else if (param.ParameterType == typeof(float))
                    {
                        parameterValues[i] = float.Parse(paramValue);
                    }
                    else
                    {
                        parameterValues[i] = paramValue;
                    }
                }
                else
                {
                    parameterValues[i] = param.ParameterType.IsValueType ? Activator.CreateInstance(param.ParameterType)! : null!;
                }
            }

            Console.WriteLine("Parameters: " + string.Join(", ", parameterValues));
            object? result = method.Invoke(null, parameterValues); // Invoke method
            
            if (result == null) // If method returned null
            {
                return "Operation completed successfully.";
            }
            
            Console.WriteLine("Result: " + result);
            return result.ToString()!; // Return result as string
        }

        public static void LoginHandler(object obj)
        {
            SslStream stream = (SslStream)obj; // Get client stream
            byte[] buffer = new byte[1024]; // Create buffer for receiving data
            int bytesRead; // Variable for number of bytes read
            try // Exception handling
            {
                while (stream.CanRead && (bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0) // Loop for receiving data from client
                {
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead); // Get message from client

                    message = message.Trim(); // Remove extra spaces

                    if ((message[0] - 0) > 32) // Check if first character is a letter
                    {
                        Console.WriteLine("Received: " + message); // {username, password}

                        Dictionary<string, string>? json = JsonHelper.Deserialize<Dictionary<string, string>>(message); // Deserialize message
                        if (json == null) // Check for successful deserialization
                        {
                            Console.WriteLine("Error while deserializing message.");
                            // example json format: {"username": "admin", "password": "admin"}
                        }
                        else
                        {
                            if (json.ContainsKey("username") && json.ContainsKey("password"))
                            {
                                Console.WriteLine("Deserialized: " + json["username"] + " " + json["password"]); // Print deserialized message

                                Dictionary<string, string> status = new Dictionary<string, string>(); // Create response dictionary
                                // Check if user exists
                                if (SQLhelper.IsUserExist(json["username"]))
                                {
                                    // Check password correctness
                                    if (SQLhelper.CheckUser(json["username"], json["password"]))
                                    {
                                        status.Add("status", "OK"); // Create response
                                        if (SQLhelper.IsTeacher(json["username"]))
                                        {
                                            status.Add("role", "teacher");
                                        }
                                        else
                                        {
                                            status.Add("role", "student");
                                        }

                                        byte[] response = Encoding.ASCII.GetBytes(JsonHelper.Serialize(status) + "\n");
                                        stream.Write(response, 0, response.Length); // Send response to client
                                        ClientHandler(stream);
                                        // And here switch to next Handler
                                    }
                                    else
                                    {
                                        status.Add("status", "Login failed"); // Wrong password
                                        byte[] response = Encoding.ASCII.GetBytes(JsonHelper.Serialize(status) + "\n");
                                        stream.Write(response, 0, response.Length);
                                    }
                                }
                                else
                                {
                                    // User does not exist, send login error
                                    status.Add("status", "Login failed"); // User does not exist
                                    status.Add("message", "User does not exist");
                                    byte[] response = Encoding.ASCII.GetBytes(JsonHelper.Serialize(status) + "\n");
                                    stream.Write(response, 0, response.Length);
                                    Console.WriteLine($"Login attempt with non-existent username: {json["username"]}");
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
            SslStream stream = (SslStream)obj; // Get client stream
            byte[] buffer = new byte[1024]; // Create buffer for receiving data

            int bytesRead; // Variable for number of bytes read
            try
            {
                while (stream.CanRead && (bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0) // Loop for receiving data from client
                {
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead); // Get message from client

                    message = message.Trim(); // Remove extra spaces

                    if ((message[0] - 0) > 32) // Check if first character is a letter
                    {
                        Console.WriteLine("Received: " + message); // {username, password}
                        // example json format: {"name": "Moshe", "surname": "Cohen", "age": "16", "grade": "10", "megamot": "math"}
                        // Convert json to dictionary
                        Dictionary<string, string>? json = JsonHelper.Deserialize<Dictionary<string, string>>(message); // Deserialize message
                        if (json == null) // Check for successful deserialization
                        {
                            byte[] response = Encoding.ASCII.GetBytes("Invalid JSON format\n"); // Create response
                            stream.Write(response, 0, response.Length); // Send response to client
                        }
                        else if (json.ContainsKey("requestType"))
                        {
                            try
                            {
                                // Standard processing for other requests
                                string response = InvokeSqlHelperMethod(json["requestType"], json);
                                byte[] responseBytes = Encoding.ASCII.GetBytes(response + "\n");
                                stream.Write(responseBytes, 0, responseBytes.Length);
                            }
                            catch (ArgumentException ex)
                            {
                                byte[] response = Encoding.ASCII.GetBytes(ex.Message + "\n"); // Create response
                                stream.Write(response, 0, response.Length); // Send response to client
                            }
                        }
                        else
                        {
                            byte[] response = Encoding.ASCII.GetBytes("Request type not found!\n"); // Create response
                            stream.Write(response, 0, response.Length); // Send response to client
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
