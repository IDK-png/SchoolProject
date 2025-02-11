using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;

namespace SchoolClient
{
    public partial class LoginScreen : Form
    {
        public LoginScreen()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Send textBox1.Text and textBox2.Text to the server
            // If the server returns "OK" then open the MainScreen
            // If the server returns "NO" then show a MessageBox with "Invalid username or password"
            Connection connection = Connection.Instance;
            TcpClient client = connection.client;
            // Format of the message is : {"username": "admin", "password": "admin"}
            client.Client.Send(Encoding.ASCII.GetBytes("{\"username\": \"" + textBox1.Text + "\", \"password\": \"" + textBox2.Text + "\"}"));

            byte[] buffer = new byte[1024];
            int bytesRead = client.Client.Receive(buffer);
            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            // Deserialize the response
            Dictionary<string, string> json = JsonHelper.Deserialize<Dictionary<string, string>>(response);

            if (json.ContainsKey("status") && json["status"] == "OK")
            {
                Connection.LoginInfo["username"] = textBox1.Text;
                Connection.LoginInfo["password"] = textBox2.Text;
                if (json.ContainsKey("role") && json["role"] == "teacher")
                {
                    Connection.isTeacher = true;
                }
                else
                {
                    Connection.isTeacher = false;
                }

                HomePage mainScreen = new HomePage();
                mainScreen.Show();
                this.Hide();
            }

            MessageBox.Show(response);
        }
    }
}