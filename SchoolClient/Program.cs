using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;

namespace SchoolClient
{
    // Singleton for connection to the server
    public class Connection
    {
        private static Connection instance = null;
        private Connection() 
        { 
            try 
            {
                client = new TcpClient("127.0.0.1", 76);
            }
            catch (Exception e){}
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
        public TcpClient client;
        public bool isConnected()
        {
            if(client == null)
            {
                return false;
            }
            return client.Connected;
        }

    }
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Connection connection = Connection.Instance;
            if(!connection.isConnected())
            {
                MessageBox.Show("Server is not running");
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginScreen());
        }
    }
}
