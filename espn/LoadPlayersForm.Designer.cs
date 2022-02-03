
namespace espn
{
    partial class LoadPlayersForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.nbaTeams_comboBox = new System.Windows.Forms.ComboBox();
            this.yahooTeams_comboBox = new System.Windows.Forms.ComboBox();
            this.ok_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(49, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "NBA Team";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(215, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Yahoo Team";
            // 
            // nbaTeams_comboBox
            // 
            this.nbaTeams_comboBox.FormattingEnabled = true;
            this.nbaTeams_comboBox.Location = new System.Drawing.Point(12, 28);
            this.nbaTeams_comboBox.Name = "nbaTeams_comboBox";
            this.nbaTeams_comboBox.Size = new System.Drawing.Size(163, 21);
            this.nbaTeams_comboBox.TabIndex = 2;
            // 
            // yahooTeams_comboBox
            // 
            this.yahooTeams_comboBox.FormattingEnabled = true;
            this.yahooTeams_comboBox.Location = new System.Drawing.Point(181, 28);
            this.yahooTeams_comboBox.Name = "yahooTeams_comboBox";
            this.yahooTeams_comboBox.Size = new System.Drawing.Size(163, 21);
            this.yahooTeams_comboBox.TabIndex = 3;
            // 
            // ok_button
            // 
            this.ok_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ok_button.Location = new System.Drawing.Point(141, 67);
            this.ok_button.Name = "ok_button";
            this.ok_button.Size = new System.Drawing.Size(75, 23);
            this.ok_button.TabIndex = 4;
            this.ok_button.Text = "OK";
            this.ok_button.UseVisualStyleBackColor = true;
            this.ok_button.Click += new System.EventHandler(this.ok_button_Click);
            // 
            // LoadPlayersForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(365, 102);
            this.Controls.Add(this.ok_button);
            this.Controls.Add(this.yahooTeams_comboBox);
            this.Controls.Add(this.nbaTeams_comboBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "LoadPlayersForm";
            this.Text = "LoadPlayersForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox nbaTeams_comboBox;
        private System.Windows.Forms.ComboBox yahooTeams_comboBox;
        private System.Windows.Forms.Button ok_button;
    }
}