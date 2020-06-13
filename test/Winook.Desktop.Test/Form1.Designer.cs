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
                _mouseHook?.Dispose();
                _keyboardHook?.Dispose();
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
            this.radio32bit = new System.Windows.Forms.RadioButton();
            this.radio64bit = new System.Windows.Forms.RadioButton();
            this.testLabel = new System.Windows.Forms.Label();
            this.ignoreMove = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // mouseButton
            // 
            this.mouseButton.Location = new System.Drawing.Point(13, 13);
            this.mouseButton.Margin = new System.Windows.Forms.Padding(6);
            this.mouseButton.Name = "mouseButton";
            this.mouseButton.Size = new System.Drawing.Size(330, 48);
            this.mouseButton.TabIndex = 0;
            this.mouseButton.Text = "Mouse Hook";
            this.mouseButton.UseVisualStyleBackColor = true;
            this.mouseButton.Click += new System.EventHandler(this.mouseButton_Click);
            // 
            // mouseLabel
            // 
            this.mouseLabel.AutoSize = true;
            this.mouseLabel.Location = new System.Drawing.Point(8, 240);
            this.mouseLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.mouseLabel.Name = "mouseLabel";
            this.mouseLabel.Size = new System.Drawing.Size(167, 25);
            this.mouseLabel.TabIndex = 1;
            this.mouseLabel.Text = "Mouse messages";
            // 
            // keyboardButton
            // 
            this.keyboardButton.Location = new System.Drawing.Point(13, 111);
            this.keyboardButton.Margin = new System.Windows.Forms.Padding(6);
            this.keyboardButton.Name = "keyboardButton";
            this.keyboardButton.Size = new System.Drawing.Size(330, 48);
            this.keyboardButton.TabIndex = 2;
            this.keyboardButton.Text = "Keyboard Hook";
            this.keyboardButton.UseVisualStyleBackColor = true;
            this.keyboardButton.Click += new System.EventHandler(this.keyboardButton_Click);
            // 
            // keyboardLabel
            // 
            this.keyboardLabel.AutoSize = true;
            this.keyboardLabel.Location = new System.Drawing.Point(8, 280);
            this.keyboardLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.keyboardLabel.Name = "keyboardLabel";
            this.keyboardLabel.Size = new System.Drawing.Size(192, 25);
            this.keyboardLabel.TabIndex = 3;
            this.keyboardLabel.Text = "Keyboard messages";
            // 
            // radio32bit
            // 
            this.radio32bit.AutoSize = true;
            this.radio32bit.Checked = true;
            this.radio32bit.Location = new System.Drawing.Point(13, 174);
            this.radio32bit.Name = "radio32bit";
            this.radio32bit.Size = new System.Drawing.Size(86, 29);
            this.radio32bit.TabIndex = 4;
            this.radio32bit.TabStop = true;
            this.radio32bit.Text = "32-bit";
            this.radio32bit.UseVisualStyleBackColor = true;
            // 
            // radio64bit
            // 
            this.radio64bit.AutoSize = true;
            this.radio64bit.Location = new System.Drawing.Point(167, 174);
            this.radio64bit.Name = "radio64bit";
            this.radio64bit.Size = new System.Drawing.Size(86, 29);
            this.radio64bit.TabIndex = 5;
            this.radio64bit.TabStop = true;
            this.radio64bit.Text = "64-bit";
            this.radio64bit.UseVisualStyleBackColor = true;
            // 
            // testLabel
            // 
            this.testLabel.AutoSize = true;
            this.testLabel.Location = new System.Drawing.Point(8, 320);
            this.testLabel.Name = "testLabel";
            this.testLabel.Size = new System.Drawing.Size(146, 25);
            this.testLabel.TabIndex = 6;
            this.testLabel.Text = "Test messages";
            // 
            // ignoreMove
            // 
            this.ignoreMove.AutoSize = true;
            this.ignoreMove.Location = new System.Drawing.Point(13, 71);
            this.ignoreMove.Name = "ignoreMove";
            this.ignoreMove.Size = new System.Drawing.Size(241, 29);
            this.ignoreMove.TabIndex = 7;
            this.ignoreMove.Text = "Ignore move messages";
            this.ignoreMove.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(980, 408);
            this.Controls.Add(this.ignoreMove);
            this.Controls.Add(this.testLabel);
            this.Controls.Add(this.radio64bit);
            this.Controls.Add(this.radio32bit);
            this.Controls.Add(this.keyboardLabel);
            this.Controls.Add(this.keyboardButton);
            this.Controls.Add(this.mouseLabel);
            this.Controls.Add(this.mouseButton);
            this.Margin = new System.Windows.Forms.Padding(4);
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
        private System.Windows.Forms.RadioButton radio32bit;
        private System.Windows.Forms.RadioButton radio64bit;
        private System.Windows.Forms.Label testLabel;
        private System.Windows.Forms.CheckBox ignoreMove;
    }
}

