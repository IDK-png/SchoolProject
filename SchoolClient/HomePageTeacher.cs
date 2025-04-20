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
    public partial class HomePageTeacher : Form
    {
        public HomePageTeacher()
        {
            // Remove InitializeComponent() call, as we use manual initialization
            LoadStudents();
        }

        private void LoadStudents()
        {
            // Load list of students
            Connection connection = Connection.Instance;
            SslStream sslStream = connection.SslStream;
            try
            {
                byte[] message = Encoding.ASCII.GetBytes("{\"requestType\":\"GetStudentsByParams\", \"name\": \"\", \"surname\": \"\", \"age\": \"\", \"grade\": \"\", \"megamot\": \"\"}");
                sslStream.Write(message);
                sslStream.Flush();
                byte[] buffer = new byte[4096];
                int bytesRead = sslStream.Read(buffer, 0, buffer.Length);
                string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                // The response is assumed to be a list of students, one per line
                listBoxStudents.Items.Clear();
                string[] lines = response.Split('\n');
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        listBoxStudents.Items.Add(line);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading students: " + ex.Message);
            }
        }

        private void listBoxStudents_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Load marks for the selected student
            if (listBoxStudents.SelectedItem == null) return;
            string[] parts = listBoxStudents.SelectedItem.ToString().Split(' ');
            if (parts.Length < 2) return;
            string studentName = parts[1];
            LoadMarks(studentName);
        }

        private void LoadMarks(string studentName)
        {
            Connection connection = Connection.Instance;
            SslStream sslStream = connection.SslStream;
            try
            {
                byte[] message = Encoding.ASCII.GetBytes($"{{\"requestType\":\"GetStudentMarks\", \"studentName\": \"{studentName}\"}}");
                sslStream.Write(message);
                sslStream.Flush();
                byte[] buffer = new byte[4096];
                int bytesRead = sslStream.Read(buffer, 0, buffer.Length);
                string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                listBoxMarks.Items.Clear();
                string[] lines = response.Split('\n');
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        listBoxMarks.Items.Add(line);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading marks: " + ex.Message);
            }
        }

        private void buttonUpdateMark_Click(object sender, EventArgs e)
        {
            // Update selected mark
            if (listBoxMarks.SelectedItem == null || string.IsNullOrWhiteSpace(textBoxNewMark.Text)) return;
            string[] parts = listBoxMarks.SelectedItem.ToString().Split(' ');
            if (parts.Length < 1) return;
            int markId = int.Parse(parts[0]);
            int newMark = int.Parse(textBoxNewMark.Text);
            Connection connection = Connection.Instance;
            SslStream sslStream = connection.SslStream;
            try
            {
                byte[] message = Encoding.ASCII.GetBytes($"{{\"requestType\":\"UpdateMark\", \"id\": \"{markId}\", \"mark\": \"{newMark}\"}}");
                sslStream.Write(message);
                sslStream.Flush();
                byte[] buffer = new byte[1024];
                int bytesRead = sslStream.Read(buffer, 0, buffer.Length);
                string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                MessageBox.Show(response);
                // Refresh marks
                if (listBoxStudents.SelectedItem != null)
                {
                    string[] studentParts = listBoxStudents.SelectedItem.ToString().Split(' ');
                    if (studentParts.Length >= 2)
                        LoadMarks(studentParts[1]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating mark: " + ex.Message);
            }
        }

        private void buttonAddMark_Click(object sender, EventArgs e)
        {
            if (listBoxStudents.SelectedItem == null || string.IsNullOrWhiteSpace(textBoxNewMarkToAdd.Text)) 
            {
                MessageBox.Show("Select a student and enter a mark");
                return;
            }
            
            // Show student data format for debugging
            string studentData = listBoxStudents.SelectedItem.ToString();
            MessageBox.Show("Student data: " + studentData);
            
            // Try to get student ID in another way
            // Assume student data contains ID in another format
            string studentIdStr = "";
            try {
                // Search for numeric student ID in the string
                foreach (char c in studentData)
                {
                    if (char.IsDigit(c))
                        studentIdStr += c;
                    else if (!string.IsNullOrEmpty(studentIdStr))
                        break; // Found first sequence of digits
                }
                
                // If not found, use item index + 1
                if (string.IsNullOrEmpty(studentIdStr))
                {
                    studentIdStr = (listBoxStudents.SelectedIndex + 1).ToString();
                }
            }
            catch {
                studentIdStr = "1"; // Fallback
            }
            
            MessageBox.Show("Extracted student ID: " + studentIdStr);
            
            if (!int.TryParse(studentIdStr, out int studentId))
            {
                MessageBox.Show("Failed to get student ID. Using ID=1");
                studentId = 1;
            }
            
            if (!int.TryParse(textBoxNewMarkToAdd.Text, out int mark))
            {
                MessageBox.Show("Enter a valid mark (integer)");
                return;
            }
            
            string subject = comboBoxSubject.Text;
            
            if (string.IsNullOrEmpty(subject))
            {
                MessageBox.Show("Select a subject");
                return;
            }
            
            // Set fixed teacher ID for testing
            int teacherId = 1;
            
            // Current date for the mark
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            
            Connection connection = Connection.Instance;
            SslStream sslStream = connection.SslStream;
            try
            {
                string jsonRequest = $"{{\"requestType\":\"AddMark\", \"student_id\": \"{studentId}\", \"teacher_id\": \"{teacherId}\", \"mark\": \"{mark}\", \"date\": \"{date}\", \"megama\": \"{subject}\"}}";
                MessageBox.Show("Request: " + jsonRequest);
                
                byte[] message = Encoding.ASCII.GetBytes(jsonRequest);
                sslStream.Write(message);
                sslStream.Flush();
                byte[] buffer = new byte[1024];
                int bytesRead = sslStream.Read(buffer, 0, buffer.Length);
                string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                MessageBox.Show("Response: " + response);
                
                // Get student name for refreshing marks
                string studentName = studentData;
                if (studentData.Contains(" "))
                {
                    string[] parts = studentData.Split(' ');
                    if (parts.Length >= 2)
                        studentName = parts[1];
                }
                
                LoadMarks(studentName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding mark: " + ex.Message);
            }
        }

        // UI elements
        private ListBox listBoxStudents = new ListBox { Left = 10, Top = 10, Width = 200, Height = 300 };
        private ListBox listBoxMarks = new ListBox { Left = 220, Top = 10, Width = 300, Height = 200 };
        private TextBox textBoxNewMark = new TextBox { Left = 220, Top = 220, Width = 100 };
        private Button buttonUpdateMark = new Button { Left = 330, Top = 220, Width = 120, Text = "Update mark" };
        
        // New UI elements for adding marks
        private Label labelAddMark = new Label { Left = 10, Top = 320, Width = 100, Text = "Add mark:" };
        private TextBox textBoxNewMarkToAdd = new TextBox { Left = 120, Top = 320, Width = 50 };
        private ComboBox comboBoxSubject = new ComboBox { Left = 180, Top = 320, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
        private Button buttonAddMark = new Button { Left = 340, Top = 320, Width = 120, Text = "Add" };

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Text = "Teacher Home Page";
            this.Size = new Size(550, 400); // Increase form height
            
            Controls.Add(listBoxStudents);
            Controls.Add(listBoxMarks);
            Controls.Add(textBoxNewMark);
            Controls.Add(buttonUpdateMark);
            
            // Add new controls
            Controls.Add(labelAddMark);
            Controls.Add(textBoxNewMarkToAdd);
            Controls.Add(comboBoxSubject);
            Controls.Add(buttonAddMark);
            
            // Fill subject dropdown
            comboBoxSubject.Items.AddRange(new string[] { 
                "math", "chemistry", "biology", "physics", "english", 
                "history", "geography", "literature", "computer science", 
                "art", "music", "physical education" 
            });
            
            listBoxStudents.SelectedIndexChanged += listBoxStudents_SelectedIndexChanged;
            buttonUpdateMark.Click += buttonUpdateMark_Click;
            buttonAddMark.Click += buttonAddMark_Click;
        }
    }
}
