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
    public partial class HomePage : Form
    {
        public HomePage()
        {
            InitializeComponent();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = false;
            this.label4.MaximumSize = new System.Drawing.Size(192, 313); // Set the maximum size
            this.label4.Size = new System.Drawing.Size(192, 313); // Set the initial size
            this.label4.Text = "Your text here";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopLeft; // Align text to the top left
            // 
            // label5
            // 
            this.label5.AutoSize = false;
            this.label5.MaximumSize = new System.Drawing.Size(192, 313); // Set the maximum size
            this.label5.Size = new System.Drawing.Size(192, 313); // Set the initial size
            this.label5.Text = "Your text here";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopLeft; // Align text to the top left
            // 
            // label6
            // 
            this.label6.AutoSize = false;
            this.label6.MaximumSize = new System.Drawing.Size(544, 156); // Set the maximum size
            this.label6.Size = new System.Drawing.Size(544, 156); // Set the initial size
            this.label6.Text = "Your text here";
            this.label6.TextAlign = System.Drawing.ContentAlignment.TopLeft; // Align text to the top left
            this.ResumeLayout(false);
            LoadAllStudents();
            LoadAllTeachers();
            LoadStudentMarks();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SearchStudents mainScreen = new SearchStudents();
            mainScreen.Show();
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Teachers mainScreen = new Teachers();
            mainScreen.Show();
            this.Hide();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void LoadAllStudents()
        {
            Connection connection = Connection.Instance;
            SslStream sslStream = connection.SslStream;

            try
            {
                // Send request to get all students
                byte[] message = Encoding.ASCII.GetBytes("{\"requestType\":\"GetStudentsByParams\", \"name\": \"\", \"surname\": \"\", \"age\": \"\", \"grade\": \"\", \"megamot\": \"\"}");
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

                label4.Text = response;
            }
            catch (Exception ex)
            {
                MessageBox.Show("SSL communication failed: " + ex.Message);
            }
        }

        private void LoadAllTeachers()
        {
            Connection connection = Connection.Instance;
            SslStream sslStream = connection.SslStream;

            try
            {
                // Send request to get all teachers
                byte[] message = Encoding.ASCII.GetBytes("{\"requestType\":\"GetAllTeachers\"}");
                sslStream.Write(message);
                sslStream.Flush();

                byte[] buffer = new byte[1024];
                int bytesRead = sslStream.Read(buffer, 0, buffer.Length);
                string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                string[] x = response.Split('\n');
                response = "";
                foreach (string line in x)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        string[] parts = line.Split(' ');
                        if (parts.Length >= 3)
                        {
                            response += "Name: " + parts[0] + "\nSurname: " + parts[1] + "\nSubject: " + parts[2] + "\n\n";
                        }
                    }
                }
                if(response.Length == 0)
                {
                    response = "No Marks Found!";
                }
                label5.Text = response;
            }
            catch (Exception ex)
            {
                MessageBox.Show("SSL communication failed: " + ex.Message);
            }
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

                string[] x = response.Split('\n');
                response = "";
                foreach (string line in x)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        string[] parts = line.Split(' ');
                        if (parts.Length >= 6)
                        {
                            response += "ID: " + parts[0] + "\nStudent ID: " + parts[1] + "\nTeacher ID: " + parts[2] + "\nMark: " + parts[3] + "\nDate: " + parts[4] + "\nSubject: " + parts[5] + "\n\n";
                        }
                    }
                }

                label6.Text = response;
            }
            catch (Exception ex)
            {
                MessageBox.Show("SSL communication failed: " + ex.Message);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Marks mainScreen = new Marks();
            mainScreen.Show();
            this.Hide();
        }
    }
}