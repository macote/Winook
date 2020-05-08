namespace Winook.Desktop.Test
{
    partial class Form1
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
            this.mouseButton = new System.Windows.Forms.Button();
            this.mouseLabel = new System.Windows.Forms.Label();
            this.keyboardButton = new System.Windows.Forms.Button();
            this.keyboardLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // mouseButton
            // 
            this.mouseButton.Location = new System.Drawing.Point(7, 7);
            this.mouseButton.Margin = new System.Windows.Forms.Padding(2);
            this.mouseButton.Name = "mouseButton";
            this.mouseButton.Size = new System.Drawing.Size(180, 140);
            this.mouseButton.TabIndex = 0;
            this.mouseButton.Text = "Mouse Hook";
            this.mouseButton.UseVisualStyleBackColor = true;
            this.mouseButton.Click += new System.EventHandler(this.mouseButton_Click);
            // 
            // mouseLabel
            // 
            this.mouseLabel.AutoSize = true;
            this.mouseLabel.Location = new System.Drawing.Point(11, 164);
            this.mouseLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.mouseLabel.Name = "mouseLabel";
            this.mouseLabel.Size = new System.Drawing.Size(89, 13);
            this.mouseLabel.TabIndex = 1;
            this.mouseLabel.Text = "Mouse messages";
            // 
            // keyboardButton
            // 
            this.keyboardButton.Location = new System.Drawing.Point(192, 7);
            this.keyboardButton.Name = "keyboardButton";
            this.keyboardButton.Size = new System.Drawing.Size(180, 140);
            this.keyboardButton.TabIndex = 2;
            this.keyboardButton.Text = "Keyboard Hook";
            this.keyboardButton.UseVisualStyleBackColor = true;
            this.keyboardButton.Click += new System.EventHandler(this.keyboardButton_Click);
            // 
            // keyboardLabel
            // 
            this.keyboardLabel.AutoSize = true;
            this.keyboardLabel.Location = new System.Drawing.Point(12, 186);
            this.keyboardLabel.Name = "keyboardLabel";
            this.keyboardLabel.Size = new System.Drawing.Size(102, 13);
            this.keyboardLabel.TabIndex = 3;
            this.keyboardLabel.Text = "Keyboard messages";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(378, 219);
            this.Controls.Add(this.keyboardLabel);
            this.Controls.Add(this.keyboardButton);
            this.Controls.Add(this.mouseLabel);
            this.Controls.Add(this.mouseButton);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button mouseButton;
        private System.Windows.Forms.Label mouseLabel;
        private System.Windows.Forms.Button keyboardButton;
        private System.Windows.Forms.Label keyboardLabel;
    }
}

