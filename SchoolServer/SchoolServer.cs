using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading;
using System.Text;

namespace ServerSide
{
    class SchoolServer
    {
        private static TcpListener? _server;               // TCP-сервер для прослушивания подключений
        private static bool _isRunning;                   // Флаг для управления состоянием сервера
        private static readonly int port = 76;            // Порт для сервера
        private static X509Certificate2? _serverCertificate; // Сертификат сервера для SSL

        static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            try
            {
                // Загрузка SSL-сертификата (путь и пароль)
                _serverCertificate = new X509Certificate2(
                    "C:/Users/maxda/Documents/GitHub/SchoolProject/SchoolServer/certificate.pfx",
                    "pass"
                );

                _server = new TcpListener(IPAddress.Any, port);
                _server.Start();
                _isRunning = true;

                Console.WriteLine($"✅ Server started on port {port}");
                Console.WriteLine("🔗 Connecting to the database...");

                if (SQLhelper.CreateDatabase() == null)
                {
                    Console.WriteLine("❌ Database connection error.");
                    return 1;
                }

                Console.WriteLine("📚 Adding students to the database...");
                AddSampleStudents();

                Console.WriteLine("🚀 Waiting for client connections...");

                while (_isRunning)
                {
                    TcpClient newClient = _server.AcceptTcpClient();
                    string clientIP = ((IPEndPoint)newClient.Client.RemoteEndPoint!).Address.ToString();

                    Console.WriteLine($"🔔 New client connected: {clientIP}");

                    Thread clientThread = new Thread(() => HandleClient(newClient));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Server error: {ex.Message}");
                return 1;
            }

            return 0;
        }

        private static void HandleClient(TcpClient client)
        {
            using NetworkStream networkStream = client.GetStream();
            using SslStream sslStream = new SslStream(networkStream, false);

            try
            {
                if (_serverCertificate == null)
                    throw new InvalidOperationException("⚠️ Сертификат сервера не загружен.");

                // Аутентификация сервера с использованием сертификата
                sslStream.AuthenticateAsServer(_serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: true);
                Console.WriteLine("🔒 SSL connection established.");

                // Вызов метода обработки логина (или другого обработчика)
                Handlers.LoginHandler(sslStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error handling client: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("🔌 Client connection closed.");
                client.Close();
            }
        }

        private static void AddSampleStudents()
        {
            SQLhelper.AddStudent("Moshe", "Cohen", 16, 10, "math");
            SQLhelper.AddStudent("Yosef", "Ben-David", 15, 9, "chemistry");
            SQLhelper.AddStudent("Avraham", "Cohen", 16, 10, "math");
            SQLhelper.AddStudent("Yitzhak", "Levi", 17, 11, "biology");
            SQLhelper.AddStudent("Yaakov", "Ben-David", 15, 9, "chemistry");
            SQLhelper.AddStudent("David", "Levi", 17, 11, "biology");

            SQLhelper.AddTeacher("Rivka", "Cohen", "math");
            SQLhelper.AddTeacher("Leah", "Ben-David", "chemistry");
            SQLhelper.AddTeacher("Rachel", "Levi", "biology");
        }
    }
}
