using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace SchoolClient
{
    // Singleton для подключения к серверу с SSL/TLS
    public class Connection
    {
        private static Connection instance = null;
        public static Dictionary<string, string> LoginInfo = new Dictionary<string, string>();
        public static bool isTeacher = false;

        public TcpClient Client { get; private set; }
        public SslStream SslStream { get; private set; }

        private Connection()
        {
            try
            {
                Client = new TcpClient("127.0.0.1", 76);
                SslStream = new SslStream(
                    Client.GetStream(),
                    false,
                    (sender, certificate, chain, sslPolicyErrors) => true);

                // Аутентификация клиента с указанием имени сервера
                SslStream.AuthenticateAsClient("localhost");

                Console.WriteLine("SSL connection successfully established.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static Connection Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Connection();
                }
                return instance;
            }
        }

        public bool IsConnected()
        {
            return Client?.Connected ?? false;
        }
    }

    internal static class Program
    {
        /// <summary>
        /// Основная точка входа в приложение.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var connection = Connection.Instance;

            if (!connection.IsConnected())
            {
                MessageBox.Show("Server is not running or connection failed.", "Connection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginScreen());  // Замените на вашу форму входа
        }
    }
}
