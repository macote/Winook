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
            mouseButton = new System.Windows.Forms.Button();
            keyboardButton = new System.Windows.Forms.Button();
            radio32bit = new System.Windows.Forms.RadioButton();
            radio64bit = new System.Windows.Forms.RadioButton();
            ignoreMove = new System.Windows.Forms.CheckBox();
            messagesGroupBox = new System.Windows.Forms.GroupBox();
            recentMessagesTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            recentMessageLabel01 = new System.Windows.Forms.Label();
            recentMessageLabel02 = new System.Windows.Forms.Label();
            recentMessageLabel03 = new System.Windows.Forms.Label();
            recentMessageLabel04 = new System.Windows.Forms.Label();
            recentMessageLabel05 = new System.Windows.Forms.Label();
            recentMessageLabel06 = new System.Windows.Forms.Label();
            recentMessageLabel07 = new System.Windows.Forms.Label();
            recentMessageLabel08 = new System.Windows.Forms.Label();
            recentMessageLabel09 = new System.Windows.Forms.Label();
            recentMessageLabel10 = new System.Windows.Forms.Label();
            recentMessageLabel11 = new System.Windows.Forms.Label();
            recentMessageLabel12 = new System.Windows.Forms.Label();
            recentMessageLabel13 = new System.Windows.Forms.Label();
            recentMessageLabel14 = new System.Windows.Forms.Label();
            recentMessageLabel15 = new System.Windows.Forms.Label();
            recentMessageLabel16 = new System.Windows.Forms.Label();
            recentMessageLabel17 = new System.Windows.Forms.Label();
            recentMessageLabel18 = new System.Windows.Forms.Label();
            recentMessageLabel19 = new System.Windows.Forms.Label();
            recentMessageLabel20 = new System.Windows.Forms.Label();
            messagesGroupBox.SuspendLayout();
            recentMessagesTableLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mouseButton
            // 
            mouseButton.Location = new System.Drawing.Point(8, 8);
            mouseButton.Margin = new System.Windows.Forms.Padding(4);
            mouseButton.Name = "mouseButton";
            mouseButton.Size = new System.Drawing.Size(210, 30);
            mouseButton.TabIndex = 0;
            mouseButton.Text = "Mouse Hook";
            mouseButton.UseVisualStyleBackColor = true;
            mouseButton.Click += MouseButton_Click;
            // 
            // keyboardButton
            // 
            keyboardButton.Location = new System.Drawing.Point(8, 46);
            keyboardButton.Margin = new System.Windows.Forms.Padding(4);
            keyboardButton.Name = "keyboardButton";
            keyboardButton.Size = new System.Drawing.Size(210, 30);
            keyboardButton.TabIndex = 2;
            keyboardButton.Text = "Keyboard Hook";
            keyboardButton.UseVisualStyleBackColor = true;
            keyboardButton.Click += KeyboardButton_Click;
            // 
            // radio32bit
            // 
            radio32bit.AutoSize = true;
            radio32bit.Checked = true;
            radio32bit.Location = new System.Drawing.Point(224, 52);
            radio32bit.Margin = new System.Windows.Forms.Padding(2);
            radio32bit.Name = "radio32bit";
            radio32bit.Size = new System.Drawing.Size(56, 19);
            radio32bit.TabIndex = 4;
            radio32bit.TabStop = true;
            radio32bit.Text = "32-bit";
            radio32bit.UseVisualStyleBackColor = true;
            // 
            // radio64bit
            // 
            radio64bit.AutoSize = true;
            radio64bit.Location = new System.Drawing.Point(284, 52);
            radio64bit.Margin = new System.Windows.Forms.Padding(2);
            radio64bit.Name = "radio64bit";
            radio64bit.Size = new System.Drawing.Size(56, 19);
            radio64bit.TabIndex = 5;
            radio64bit.TabStop = true;
            radio64bit.Text = "64-bit";
            radio64bit.UseVisualStyleBackColor = true;
            // 
            // ignoreMove
            // 
            ignoreMove.AutoSize = true;
            ignoreMove.Location = new System.Drawing.Point(224, 15);
            ignoreMove.Margin = new System.Windows.Forms.Padding(2);
            ignoreMove.Name = "ignoreMove";
            ignoreMove.Size = new System.Drawing.Size(147, 19);
            ignoreMove.TabIndex = 7;
            ignoreMove.Text = "Ignore move messages";
            ignoreMove.UseVisualStyleBackColor = true;
            // 
            // messagesGroupBox
            // 
            messagesGroupBox.Controls.Add(recentMessagesTableLayoutPanel);
            messagesGroupBox.Location = new System.Drawing.Point(8, 80);
            messagesGroupBox.Margin = new System.Windows.Forms.Padding(2);
            messagesGroupBox.Name = "messagesGroupBox";
            messagesGroupBox.Padding = new System.Windows.Forms.Padding(8, 6, 8, 8);
            messagesGroupBox.Size = new System.Drawing.Size(848, 334);
            messagesGroupBox.TabIndex = 8;
            messagesGroupBox.TabStop = false;
            messagesGroupBox.Text = "Messages";
            // 
            // recentMessagesTableLayoutPanel
            // 
            recentMessagesTableLayoutPanel.ColumnCount = 1;
            recentMessagesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            recentMessagesTableLayoutPanel.Controls.Add(recentMessageLabel01, 0, 0);
            recentMessagesTableLayoutPanel.Controls.Add(recentMessageLabel02, 0, 1);
            recentMessagesTableLayoutPanel.Controls.Add(recentMessageLabel03, 0, 2);
            recentMessagesTableLayoutPanel.Controls.Add(recentMessageLabel04, 0, 3);
            recentMessagesTableLayoutPanel.Controls.Add(recentMessageLabel05, 0, 4);
            recentMessagesTableLayoutPanel.Controls.Add(recentMessageLabel06, 0, 5);
            recentMessagesTableLayoutPanel.Controls.Add(recentMessageLabel07, 0, 6);
            recentMessagesTableLayoutPanel.Controls.Add(recentMessageLabel08, 0, 7);
            recentMessagesTableLayoutPanel.Controls.Add(recentMessageLabel09, 0, 8);
            recentMessagesTableLayoutPanel.Controls.Add(recentMessageLabel10, 0, 9);
            recentMessagesTableLayoutPanel.Controls.Add(recentMessageLabel11, 0, 10);
            recentMessagesTableLayoutPanel.Controls.Add(recentMessageLabel12, 0, 11);
            recentMessagesTableLayoutPanel.Controls.Add(recentMessageLabel13, 0, 12);
            recentMessagesTableLayoutPanel.Controls.Add(recentMessageLabel14, 0, 13);
            recentMessagesTableLayoutPanel.Controls.Add(recentMessageLabel15, 0, 14);
            recentMessagesTableLayoutPanel.Controls.Add(recentMessageLabel16, 0, 15);
            recentMessagesTableLayoutPanel.Controls.Add(recentMessageLabel17, 0, 16);
            recentMessagesTableLayoutPanel.Controls.Add(recentMessageLabel18, 0, 17);
            recentMessagesTableLayoutPanel.Controls.Add(recentMessageLabel19, 0, 18);
            recentMessagesTableLayoutPanel.Controls.Add(recentMessageLabel20, 0, 19);
            recentMessagesTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            recentMessagesTableLayoutPanel.Location = new System.Drawing.Point(8, 22);
            recentMessagesTableLayoutPanel.Margin = new System.Windows.Forms.Padding(2);
            recentMessagesTableLayoutPanel.Name = "recentMessagesTableLayoutPanel";
            recentMessagesTableLayoutPanel.RowCount = 20;
            recentMessagesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            recentMessagesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            recentMessagesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            recentMessagesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            recentMessagesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            recentMessagesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            recentMessagesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            recentMessagesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            recentMessagesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            recentMessagesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            recentMessagesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            recentMessagesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            recentMessagesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            recentMessagesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            recentMessagesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            recentMessagesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            recentMessagesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            recentMessagesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            recentMessagesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            recentMessagesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            recentMessagesTableLayoutPanel.Size = new System.Drawing.Size(832, 304);
            recentMessagesTableLayoutPanel.TabIndex = 0;
            // 
            // recentMessageLabel01
            // 
            recentMessageLabel01.AutoEllipsis = true;
            recentMessageLabel01.Dock = System.Windows.Forms.DockStyle.Fill;
            recentMessageLabel01.Location = new System.Drawing.Point(2, 0);
            recentMessageLabel01.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            recentMessageLabel01.Name = "recentMessageLabel01";
            recentMessageLabel01.Size = new System.Drawing.Size(828, 15);
            recentMessageLabel01.TabIndex = 0;
            recentMessageLabel01.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // recentMessageLabel02
            // 
            recentMessageLabel02.AutoEllipsis = true;
            recentMessageLabel02.Dock = System.Windows.Forms.DockStyle.Fill;
            recentMessageLabel02.Location = new System.Drawing.Point(2, 15);
            recentMessageLabel02.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            recentMessageLabel02.Name = "recentMessageLabel02";
            recentMessageLabel02.Size = new System.Drawing.Size(828, 15);
            recentMessageLabel02.TabIndex = 1;
            recentMessageLabel02.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // recentMessageLabel03
            // 
            recentMessageLabel03.AutoEllipsis = true;
            recentMessageLabel03.Dock = System.Windows.Forms.DockStyle.Fill;
            recentMessageLabel03.Location = new System.Drawing.Point(2, 30);
            recentMessageLabel03.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            recentMessageLabel03.Name = "recentMessageLabel03";
            recentMessageLabel03.Size = new System.Drawing.Size(828, 15);
            recentMessageLabel03.TabIndex = 2;
            recentMessageLabel03.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // recentMessageLabel04
            // 
            recentMessageLabel04.AutoEllipsis = true;
            recentMessageLabel04.Dock = System.Windows.Forms.DockStyle.Fill;
            recentMessageLabel04.Location = new System.Drawing.Point(2, 45);
            recentMessageLabel04.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            recentMessageLabel04.Name = "recentMessageLabel04";
            recentMessageLabel04.Size = new System.Drawing.Size(828, 15);
            recentMessageLabel04.TabIndex = 3;
            recentMessageLabel04.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // recentMessageLabel05
            // 
            recentMessageLabel05.AutoEllipsis = true;
            recentMessageLabel05.Dock = System.Windows.Forms.DockStyle.Fill;
            recentMessageLabel05.Location = new System.Drawing.Point(2, 60);
            recentMessageLabel05.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            recentMessageLabel05.Name = "recentMessageLabel05";
            recentMessageLabel05.Size = new System.Drawing.Size(828, 15);
            recentMessageLabel05.TabIndex = 4;
            recentMessageLabel05.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // recentMessageLabel06
            // 
            recentMessageLabel06.AutoEllipsis = true;
            recentMessageLabel06.Dock = System.Windows.Forms.DockStyle.Fill;
            recentMessageLabel06.Location = new System.Drawing.Point(2, 75);
            recentMessageLabel06.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            recentMessageLabel06.Name = "recentMessageLabel06";
            recentMessageLabel06.Size = new System.Drawing.Size(828, 15);
            recentMessageLabel06.TabIndex = 5;
            recentMessageLabel06.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // recentMessageLabel07
            // 
            recentMessageLabel07.AutoEllipsis = true;
            recentMessageLabel07.Dock = System.Windows.Forms.DockStyle.Fill;
            recentMessageLabel07.Location = new System.Drawing.Point(2, 90);
            recentMessageLabel07.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            recentMessageLabel07.Name = "recentMessageLabel07";
            recentMessageLabel07.Size = new System.Drawing.Size(828, 15);
            recentMessageLabel07.TabIndex = 6;
            recentMessageLabel07.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // recentMessageLabel08
            // 
            recentMessageLabel08.AutoEllipsis = true;
            recentMessageLabel08.Dock = System.Windows.Forms.DockStyle.Fill;
            recentMessageLabel08.Location = new System.Drawing.Point(2, 105);
            recentMessageLabel08.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            recentMessageLabel08.Name = "recentMessageLabel08";
            recentMessageLabel08.Size = new System.Drawing.Size(828, 15);
            recentMessageLabel08.TabIndex = 7;
            recentMessageLabel08.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // recentMessageLabel09
            // 
            recentMessageLabel09.AutoEllipsis = true;
            recentMessageLabel09.Dock = System.Windows.Forms.DockStyle.Fill;
            recentMessageLabel09.Location = new System.Drawing.Point(2, 120);
            recentMessageLabel09.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            recentMessageLabel09.Name = "recentMessageLabel09";
            recentMessageLabel09.Size = new System.Drawing.Size(828, 15);
            recentMessageLabel09.TabIndex = 8;
            recentMessageLabel09.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // recentMessageLabel10
            // 
            recentMessageLabel10.AutoEllipsis = true;
            recentMessageLabel10.Dock = System.Windows.Forms.DockStyle.Fill;
            recentMessageLabel10.Location = new System.Drawing.Point(2, 135);
            recentMessageLabel10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            recentMessageLabel10.Name = "recentMessageLabel10";
            recentMessageLabel10.Size = new System.Drawing.Size(828, 15);
            recentMessageLabel10.TabIndex = 9;
            recentMessageLabel10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // recentMessageLabel11
            // 
            recentMessageLabel11.AutoEllipsis = true;
            recentMessageLabel11.Dock = System.Windows.Forms.DockStyle.Fill;
            recentMessageLabel11.Location = new System.Drawing.Point(2, 150);
            recentMessageLabel11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            recentMessageLabel11.Name = "recentMessageLabel11";
            recentMessageLabel11.Size = new System.Drawing.Size(828, 15);
            recentMessageLabel11.TabIndex = 10;
            recentMessageLabel11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // recentMessageLabel12
            // 
            recentMessageLabel12.AutoEllipsis = true;
            recentMessageLabel12.Dock = System.Windows.Forms.DockStyle.Fill;
            recentMessageLabel12.Location = new System.Drawing.Point(2, 165);
            recentMessageLabel12.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            recentMessageLabel12.Name = "recentMessageLabel12";
            recentMessageLabel12.Size = new System.Drawing.Size(828, 15);
            recentMessageLabel12.TabIndex = 11;
            recentMessageLabel12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // recentMessageLabel13
            // 
            recentMessageLabel13.AutoEllipsis = true;
            recentMessageLabel13.Dock = System.Windows.Forms.DockStyle.Fill;
            recentMessageLabel13.Location = new System.Drawing.Point(2, 180);
            recentMessageLabel13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            recentMessageLabel13.Name = "recentMessageLabel13";
            recentMessageLabel13.Size = new System.Drawing.Size(828, 15);
            recentMessageLabel13.TabIndex = 12;
            recentMessageLabel13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // recentMessageLabel14
            // 
            recentMessageLabel14.AutoEllipsis = true;
            recentMessageLabel14.Dock = System.Windows.Forms.DockStyle.Fill;
            recentMessageLabel14.Location = new System.Drawing.Point(2, 195);
            recentMessageLabel14.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            recentMessageLabel14.Name = "recentMessageLabel14";
            recentMessageLabel14.Size = new System.Drawing.Size(828, 15);
            recentMessageLabel14.TabIndex = 13;
            recentMessageLabel14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // recentMessageLabel15
            // 
            recentMessageLabel15.AutoEllipsis = true;
            recentMessageLabel15.Dock = System.Windows.Forms.DockStyle.Fill;
            recentMessageLabel15.Location = new System.Drawing.Point(2, 210);
            recentMessageLabel15.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            recentMessageLabel15.Name = "recentMessageLabel15";
            recentMessageLabel15.Size = new System.Drawing.Size(828, 15);
            recentMessageLabel15.TabIndex = 14;
            recentMessageLabel15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // recentMessageLabel16
            // 
            recentMessageLabel16.AutoEllipsis = true;
            recentMessageLabel16.Dock = System.Windows.Forms.DockStyle.Fill;
            recentMessageLabel16.Location = new System.Drawing.Point(2, 225);
            recentMessageLabel16.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            recentMessageLabel16.Name = "recentMessageLabel16";
            recentMessageLabel16.Size = new System.Drawing.Size(828, 15);
            recentMessageLabel16.TabIndex = 15;
            recentMessageLabel16.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // recentMessageLabel17
            // 
            recentMessageLabel17.AutoEllipsis = true;
            recentMessageLabel17.Dock = System.Windows.Forms.DockStyle.Fill;
            recentMessageLabel17.Location = new System.Drawing.Point(2, 240);
            recentMessageLabel17.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            recentMessageLabel17.Name = "recentMessageLabel17";
            recentMessageLabel17.Size = new System.Drawing.Size(828, 15);
            recentMessageLabel17.TabIndex = 16;
            recentMessageLabel17.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // recentMessageLabel18
            // 
            recentMessageLabel18.AutoEllipsis = true;
            recentMessageLabel18.Dock = System.Windows.Forms.DockStyle.Fill;
            recentMessageLabel18.Location = new System.Drawing.Point(2, 255);
            recentMessageLabel18.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            recentMessageLabel18.Name = "recentMessageLabel18";
            recentMessageLabel18.Size = new System.Drawing.Size(828, 15);
            recentMessageLabel18.TabIndex = 17;
            recentMessageLabel18.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // recentMessageLabel19
            // 
            recentMessageLabel19.AutoEllipsis = true;
            recentMessageLabel19.Dock = System.Windows.Forms.DockStyle.Fill;
            recentMessageLabel19.Location = new System.Drawing.Point(2, 270);
            recentMessageLabel19.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            recentMessageLabel19.Name = "recentMessageLabel19";
            recentMessageLabel19.Size = new System.Drawing.Size(828, 15);
            recentMessageLabel19.TabIndex = 18;
            recentMessageLabel19.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // recentMessageLabel20
            // 
            recentMessageLabel20.AutoEllipsis = true;
            recentMessageLabel20.Dock = System.Windows.Forms.DockStyle.Fill;
            recentMessageLabel20.Location = new System.Drawing.Point(2, 285);
            recentMessageLabel20.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            recentMessageLabel20.Name = "recentMessageLabel20";
            recentMessageLabel20.Size = new System.Drawing.Size(828, 19);
            recentMessageLabel20.TabIndex = 19;
            recentMessageLabel20.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(867, 422);
            Controls.Add(messagesGroupBox);
            Controls.Add(ignoreMove);
            Controls.Add(radio64bit);
            Controls.Add(radio32bit);
            Controls.Add(keyboardButton);
            Controls.Add(mouseButton);
            Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            Name = "Form1";
            Text = "Form1";
            messagesGroupBox.ResumeLayout(false);
            recentMessagesTableLayoutPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button mouseButton;
        private System.Windows.Forms.Button keyboardButton;
        private System.Windows.Forms.RadioButton radio32bit;
        private System.Windows.Forms.RadioButton radio64bit;
        private System.Windows.Forms.CheckBox ignoreMove;
        private System.Windows.Forms.GroupBox messagesGroupBox;
        private System.Windows.Forms.TableLayoutPanel recentMessagesTableLayoutPanel;
        private System.Windows.Forms.Label recentMessageLabel01;
        private System.Windows.Forms.Label recentMessageLabel02;
        private System.Windows.Forms.Label recentMessageLabel03;
        private System.Windows.Forms.Label recentMessageLabel04;
        private System.Windows.Forms.Label recentMessageLabel05;
        private System.Windows.Forms.Label recentMessageLabel06;
        private System.Windows.Forms.Label recentMessageLabel07;
        private System.Windows.Forms.Label recentMessageLabel08;
        private System.Windows.Forms.Label recentMessageLabel09;
        private System.Windows.Forms.Label recentMessageLabel10;
        private System.Windows.Forms.Label recentMessageLabel11;
        private System.Windows.Forms.Label recentMessageLabel12;
        private System.Windows.Forms.Label recentMessageLabel13;
        private System.Windows.Forms.Label recentMessageLabel14;
        private System.Windows.Forms.Label recentMessageLabel15;
        private System.Windows.Forms.Label recentMessageLabel16;
        private System.Windows.Forms.Label recentMessageLabel17;
        private System.Windows.Forms.Label recentMessageLabel18;
        private System.Windows.Forms.Label recentMessageLabel19;
        private System.Windows.Forms.Label recentMessageLabel20;
    }
}
