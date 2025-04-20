using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Security;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SchoolClient
{
    public partial class Teachers : Form
    {
    public Teachers()
    {
        InitializeComponent();
        LoadTeachers();
    }

        private async void LoadTeachers()
        {
            Connection connection = Connection.Instance;
            SslStream sslStream = connection.SslStream;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    byte[] message = Encoding.ASCII.GetBytes("{\"requestType\":\"GetAllTeachers\"}");
                    sslStream.Write(message);
                    sslStream.Flush();

                    byte[] buffer = new byte[1024];
                    int bytesRead = sslStream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    label1.Text = response;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
        }
        private void label6_Click(object sender, EventArgs e)
        {
        }
        private void textBox5_TextChanged(object sender, EventArgs e)
        {
        }
        private void label5_Click(object sender, EventArgs e)
        {
        }
        private void button6_Click(object sender, EventArgs e)
        {
        }
        private void label4_Click(object sender, EventArgs e)
        {
        }
        private void label3_Click(object sender, EventArgs e)
        {
        }
        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

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

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            Marks mainScreen = new Marks();
            mainScreen.Show();
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            SearchStudents mainScreen = new SearchStudents();
            mainScreen.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            HomePage mainScreen = new HomePage();
            mainScreen.Show();
            this.Hide();
        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }
    }
}
