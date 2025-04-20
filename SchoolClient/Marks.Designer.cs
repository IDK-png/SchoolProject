namespace SchoolClient
{
    partial class Marks
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.marksLabel = new System.Windows.Forms.Label();
            this.buttonBack = new System.Windows.Forms.Button();
            this.headerLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // marksLabel
            // 
            this.marksLabel.Location = new System.Drawing.Point(19, 61);
            this.marksLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.marksLabel.MaximumSize = new System.Drawing.Size(450, 325);
            this.marksLabel.Name = "marksLabel";
            this.marksLabel.Size = new System.Drawing.Size(450, 325);
            this.marksLabel.TabIndex = 0;
            this.marksLabel.Text = "Loading grades...";
            this.marksLabel.Click += new System.EventHandler(this.marksLabel_Click);
            // 
            // buttonBack
            // 
            this.buttonBack.Location = new System.Drawing.Point(19, 398);
            this.buttonBack.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonBack.Name = "buttonBack";
            this.buttonBack.Size = new System.Drawing.Size(112, 32);
            this.buttonBack.TabIndex = 1;
            this.buttonBack.Text = "Back to home page";
            this.buttonBack.UseVisualStyleBackColor = true;
            this.buttonBack.Click += new System.EventHandler(this.buttonBack_Click);
            // 
            // headerLabel
            // 
            this.headerLabel.AutoSize = true;
            this.headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.headerLabel.Location = new System.Drawing.Point(19, 20);
            this.headerLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.headerLabel.Name = "headerLabel";
            this.headerLabel.Size = new System.Drawing.Size(127, 24);
            this.headerLabel.TabIndex = 2;
            this.headerLabel.Text = "Your Grades";
            this.headerLabel.Click += new System.EventHandler(this.headerLabel_Click);
            // 
            // Marks
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(488, 447);
            this.Controls.Add(this.headerLabel);
            this.Controls.Add(this.buttonBack);
            this.Controls.Add(this.marksLabel);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Marks";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Оценки";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label marksLabel;
        private System.Windows.Forms.Button buttonBack;
        private System.Windows.Forms.Label headerLabel;
    }
}