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
        private static TcpListener? _server;               // TCP-Server for listening to incoming connections
        private static bool _isRunning;                   // Flag indicating if the server is running
        private static readonly int port = 76;            // Port number for the server to listen on
        private static X509Certificate2? _serverCertificate; // Certificate for SSL/TLS encryption

        static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            try
            {
                // Load the server certificate from a file
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
                    throw new InvalidOperationException("⚠️ Server certificate not loaded.");

                // Authenticate the server using the certificate
                sslStream.AuthenticateAsServer(_serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: true);
                Console.WriteLine("🔒 SSL connection established.");

                // Call the login handler to process the client's request
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
            SQLhelper.AddStudent("Avraham", "Cohen", 16, 10, "math");
            SQLhelper.AddStudent("Yitzhak", "Levi", 17, 11, "biology");
            SQLhelper.AddStudent("Yaakov", "Ben-David", 15, 9, "chemistry");
            SQLhelper.AddStudent("David", "Levi", 17, 11, "biology");
            SQLhelper.AddStudent("Shimon", "Barak", 16, 10, "physics");
            SQLhelper.AddStudent("Eli", "Mizrahi", 15, 9, "english");
            SQLhelper.AddStudent("Avi", "Rubin", 17, 11, "history");
            SQLhelper.AddStudent("Noam", "Katz", 16, 10, "geography");
            SQLhelper.AddStudent("Daniel", "Greenberg", 15, 9, "literature");
            SQLhelper.AddStudent("Yonatan", "Shapiro", 17, 11, "computer science");
            SQLhelper.AddStudent("Omer", "Rosenberg", 16, 10, "art");
            SQLhelper.AddStudent("Itai", "Segal", 15, 9, "music");
            SQLhelper.AddStudent("Nadav", "Klein", 17, 11, "physical education");

            SQLhelper.AddTeacher("Rivka", "Cohen", "math");
            SQLhelper.AddTeacher("Leah", "Ben-David", "chemistry");
            SQLhelper.AddTeacher("Rachel", "Levi", "biology");
            SQLhelper.AddTeacher("Sarah", "Goldberg", "physics");
            SQLhelper.AddTeacher("Miriam", "Friedman", "english");
            SQLhelper.AddTeacher("Esther", "Katz", "history");
            SQLhelper.AddTeacher("Chana", "Weiss", "geography");
            SQLhelper.AddTeacher("Tamar", "Greenberg", "literature");
            SQLhelper.AddTeacher("Naomi", "Shapiro", "computer science");
            SQLhelper.AddTeacher("Dina", "Rosenberg", "art");
            SQLhelper.AddTeacher("Shoshana", "Mizrahi", "music");
            SQLhelper.AddTeacher("Batya", "Segal", "physical education");
            SQLhelper.AddTeacher("Malka", "Klein", "math");
            SQLhelper.AddTeacher("Gila", "Stein", "chemistry");
            SQLhelper.AddTeacher("Yehudit", "Baron", "biology");
            SQLhelper.AddTeacher("Hannah", "Abramson", "physics");
            SQLhelper.AddTeacher("Ora", "Rubin", "english");
            SQLhelper.AddTeacher("Zahava", "Cohen", "history");
            SQLhelper.AddTeacher("Tzivia", "Levin", "geography");
            SQLhelper.AddTeacher("Pnina", "Gross", "literature");
        }
    }
}
