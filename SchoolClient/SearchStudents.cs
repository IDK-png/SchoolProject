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

namespace SchoolClient
{
    public partial class SearchStudents : Form
    {
        public SearchStudents()
        {
            InitializeComponent();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            // Send textBox1.Text and textBox2.Text to the server
            // If the server returns "OK" then open the MainScreen
            // If the server returns "NO" then show a MessageBox with "Invalid username or password"
            Connection connection = Connection.Instance;
            SslStream sslStream = connection.SslStream;

            try
            {
                // Format of the message is : {"name": "", "surname": "", "age": "", "grade": "", "course": ""}
                byte[] message = Encoding.ASCII.GetBytes("{\"requestType\":\"GetStudentsByParams\", \"name\": \"" + textBox1.Text + "\", \"surname\": \"" + textBox2.Text + "\", \"age\": \"" + textBox3.Text + "\", \"grade\": \"" + textBox4.Text + "\", \"megamot\": \"" + textBox5.Text + "\"}");
                sslStream.Write(message);
                sslStream.Flush();

                byte[] buffer = new byte[1024];
                int bytesRead = sslStream.Read(buffer, 0, buffer.Length);
                string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                string[] x = response.Split(' ');
                response = "";
                for (int i = 0; i < x.Length - 5; i += 5)
                {
                    response += "Name: " + x[i + 1] + "\nSurname: " + x[i + 2] + "\nAge: " + x[i + 3] + "\nGrade: " + x[i + 4] + "\n\n";
                }

                MessageBox.Show(response);
            }
            catch (Exception ex)
            {
                MessageBox.Show("SSL communication failed: " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            Teachers mainScreen = new Teachers();
            mainScreen.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void SearchStudents_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            HomePage mainScreen = new HomePage();
            mainScreen.Show();
            this.Hide();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Marks mainScreen = new Marks();
            mainScreen.Show();
            this.Hide();
        }
    }
}
