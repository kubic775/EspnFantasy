namespace NBAFantasy
{
    partial class DraftPicksForm
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
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.clear_button = new System.Windows.Forms.Button();
            this.draftPos_textBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.numOfTeams_textBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.calc_button = new System.Windows.Forms.Button();
            this.copy_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTextBox
            // 
            this.richTextBox.BackColor = System.Drawing.SystemColors.Info;
            this.richTextBox.Location = new System.Drawing.Point(7, 90);
            this.richTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.Size = new System.Drawing.Size(363, 113);
            this.richTextBox.TabIndex = 14;
            this.richTextBox.Text = "";
            // 
            // clear_button
            // 
            this.clear_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.clear_button.Location = new System.Drawing.Point(149, 48);
            this.clear_button.Margin = new System.Windows.Forms.Padding(2);
            this.clear_button.Name = "clear_button";
            this.clear_button.Size = new System.Drawing.Size(77, 26);
            this.clear_button.TabIndex = 13;
            this.clear_button.Text = "Clear";
            this.clear_button.UseVisualStyleBackColor = true;
            this.clear_button.Click += new System.EventHandler(this.clear_button_Click);
            // 
            // draftPos_textBox
            // 
            this.draftPos_textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.draftPos_textBox.Location = new System.Drawing.Point(302, 4);
            this.draftPos_textBox.Margin = new System.Windows.Forms.Padding(2);
            this.draftPos_textBox.Name = "draftPos_textBox";
            this.draftPos_textBox.Size = new System.Drawing.Size(68, 23);
            this.draftPos_textBox.TabIndex = 12;
            this.draftPos_textBox.Text = "2";
            this.draftPos_textBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(208, 5);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 17);
            this.label2.TabIndex = 11;
            this.label2.Text = "Draft Position:";
            // 
            // numOfTeams_textBox
            // 
            this.numOfTeams_textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numOfTeams_textBox.Location = new System.Drawing.Point(131, 4);
            this.numOfTeams_textBox.Margin = new System.Windows.Forms.Padding(2);
            this.numOfTeams_textBox.Name = "numOfTeams_textBox";
            this.numOfTeams_textBox.Size = new System.Drawing.Size(68, 23);
            this.numOfTeams_textBox.TabIndex = 10;
            this.numOfTeams_textBox.Text = "14";
            this.numOfTeams_textBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(5, 5);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(128, 17);
            this.label1.TabIndex = 9;
            this.label1.Text = "Number Of Teams:";
            // 
            // calc_button
            // 
            this.calc_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.calc_button.Location = new System.Drawing.Point(36, 48);
            this.calc_button.Margin = new System.Windows.Forms.Padding(2);
            this.calc_button.Name = "calc_button";
            this.calc_button.Size = new System.Drawing.Size(77, 26);
            this.calc_button.TabIndex = 8;
            this.calc_button.Text = "Calc";
            this.calc_button.UseVisualStyleBackColor = true;
            this.calc_button.Click += new System.EventHandler(this.calc_button_Click);
            // 
            // copy_button
            // 
            this.copy_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.copy_button.Location = new System.Drawing.Point(262, 48);
            this.copy_button.Margin = new System.Windows.Forms.Padding(2);
            this.copy_button.Name = "copy_button";
            this.copy_button.Size = new System.Drawing.Size(77, 26);
            this.copy_button.TabIndex = 15;
            this.copy_button.Text = "Copy";
            this.copy_button.UseVisualStyleBackColor = true;
            this.copy_button.Click += new System.EventHandler(this.copy_button_Click);
            // 
            // DraftPicksForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(381, 211);
            this.Controls.Add(this.copy_button);
            this.Controls.Add(this.richTextBox);
            this.Controls.Add(this.clear_button);
            this.Controls.Add(this.draftPos_textBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numOfTeams_textBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.calc_button);
            this.Name = "DraftPicksForm";
            this.Text = "DraftPicksForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox;
        private System.Windows.Forms.Button clear_button;
        private System.Windows.Forms.TextBox draftPos_textBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox numOfTeams_textBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button calc_button;
        private System.Windows.Forms.Button copy_button;
    }
}