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
using System.Net.Security;
using Newtonsoft.Json;

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
            SslStream sslStream = connection.SslStream;

            try
            {
                // Format of the message is : {"username": "admin", "password": "admin"}
                byte[] message = Encoding.ASCII.GetBytes("{\"username\": \"" + textBox1.Text + "\", \"password\": \"" + textBox2.Text + "\"}");
                sslStream.Write(message);
                sslStream.Flush();

                byte[] buffer = new byte[1024];
                int bytesRead = sslStream.Read(buffer, 0, buffer.Length);
                string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                try
                {
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
                catch (JsonReaderException ex)
                {
                    MessageBox.Show("Invalid JSON response: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("SSL communication failed: " + ex.Message);
            }
        }
    }
}
