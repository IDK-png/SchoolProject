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
                            HomePageTeacher teacherScreen = new HomePageTeacher();
                            teacherScreen.Show();
                            this.Hide();
                        }
                        else
                        {
                            Connection.isTeacher = false;
                            HomePage mainScreen = new HomePage();
                            mainScreen.Show();
                            this.Hide();
                        }
                    }
                    else
                    {
                        // Show a user-friendly login error message
                        string errorMessage = "Login Error: ";
                        if (json.ContainsKey("message"))
                        {
                            // Use the specific message from the server if available
                            errorMessage += json["message"];
                        }
                        else
                        {
                            // Fallback to a generic error message
                            errorMessage += "Invalid username or password";
                        }
                        MessageBox.Show(errorMessage, "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (JsonReaderException ex)
                {
                    MessageBox.Show("Error parsing server response: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error communicating with the server: " + ex.Message);
            }
        }

    }
}
