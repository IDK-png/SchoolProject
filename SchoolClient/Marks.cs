using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Security;

namespace SchoolClient
{
    public partial class Marks : Form
    {
        public Marks()
        {
            InitializeComponent();
            LoadStudentMarks();
        }

        private void LoadStudentMarks()
        {
            Connection connection = Connection.Instance;
            SslStream sslStream = connection.SslStream;

            try
            {
                // Send request to get student marks
                string studentName = Connection.LoginInfo["username"];
                byte[] message = Encoding.ASCII.GetBytes("{\"requestType\":\"GetStudentMarks\", \"studentName\": \"" + studentName + "\"}");
                sslStream.Write(message);
                sslStream.Flush();

                byte[] buffer = new byte[1024];
                int bytesRead = sslStream.Read(buffer, 0, buffer.Length);
                string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                string[] lines = response.Split('\n');
                StringBuilder formattedMarks = new StringBuilder();
                
                foreach (string line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        string[] parts = line.Split(' ');
                        if (parts.Length >= 6)
                        {
                            formattedMarks.AppendLine($"Subject: {parts[5]}");
                            formattedMarks.AppendLine($"Mark: {parts[3]}");
                            formattedMarks.AppendLine($"Date: {parts[4]}");
                            formattedMarks.AppendLine($"Teacher ID: {parts[2]}");
                            formattedMarks.AppendLine("---------------------");
                        }
                    }
                }

                if (formattedMarks.Length == 0)
                {
                    marksLabel.Text = "You have no marks yet";
                }
                else
                {
                    marksLabel.Text = formattedMarks.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading marks: " + ex.Message);
            }
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            HomePage homePage = new HomePage();
            homePage.Show();
            this.Hide();
        }

        private void marksLabel_Click(object sender, EventArgs e)
        {

        }

        private void headerLabel_Click(object sender, EventArgs e)
        {

        }
    }
}