﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Windows.Forms;

namespace NBAFantasy
{
    public partial class FactorsForm : Form
    {
        public double Pts, Reb, Ast, Tpm, Stl, Blk, To;
        public double FtPer, FgPer, Fta, Fga;

        public FactorsForm()
        {
            InitializeComponent();
            InitFactors();
            AcceptButton = ok_button;
        }

        private void InitFactors()
        {
            FieldInfo[] fieldNames = typeof(FactorsForm).GetFields();
            foreach (FieldInfo fieldName in fieldNames)
            {
                fieldName.SetValue(this, double.Parse(ConfigurationManager.AppSettings[fieldName.Name]));
            }

            pts_textBox.Text = Pts.ToString();
            reb_textBox.Text = Reb.ToString();
            ast_textBox.Text = Ast.ToString();
            stl_textBox.Text = Stl.ToString();
            blk_textBox.Text = Blk.ToString();
            to_textBox.Text = To.ToString();
            tpm_textBox.Text = Tpm.ToString();
            ftPer_textBox.Text = FtPer.ToString();
            fgPer_textBox.Text = FgPer.ToString();
            ftVol_textBox.Text = Fta.ToString();
            fgVol_textBox.Text = Fga.ToString();
        }

        public static Dictionary<string, double> GetFactors()
        {
            FieldInfo[] fieldNames = typeof(FactorsForm).GetFields();
            Dictionary<string, double> factors = new Dictionary<string, double>();
            foreach (FieldInfo fieldName in fieldNames)
            {
                factors.Add(fieldName.Name, double.Parse(ConfigurationManager.AppSettings[fieldName.Name]));
            }
            return factors;
        }

        private void ok_button_Click(object sender, EventArgs e)
        {
            try
            {
                ParseFactors();
                Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Check Your Factors");
            }
        }
      
        private void ParseFactors()
        {
            Pts = double.Parse(pts_textBox.Text);
            Reb = double.Parse(reb_textBox.Text);
            Ast = double.Parse(ast_textBox.Text);
            Tpm = double.Parse(tpm_textBox.Text);
            Stl = double.Parse(stl_textBox.Text);
            Blk = double.Parse(blk_textBox.Text);
            To = double.Parse(to_textBox.Text);

            FtPer = double.Parse(ftPer_textBox.Text);
            Fta = double.Parse(ftVol_textBox.Text);
            FgPer = double.Parse(fgPer_textBox.Text);
            Fga = double.Parse(fgVol_textBox.Text);
        }

        private void save_button_Click(object sender, EventArgs e)
        {
            try
            {
                ParseFactors();
                SaveFactors();
                Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Cant Save Factors To File");

            }
        }

        private void SaveFactors()
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            FieldInfo[] fieldNames = typeof(FactorsForm).GetFields();
            foreach (FieldInfo fieldName in fieldNames)
            {
                configuration.AppSettings.Settings[fieldName.Name].Value = fieldName.GetValue(this).ToString();
                configuration.Save();
            }
            ConfigurationManager.RefreshSection("appSettings");
        }

    }
}
