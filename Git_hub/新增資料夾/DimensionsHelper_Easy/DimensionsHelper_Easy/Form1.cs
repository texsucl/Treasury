using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DimensionsHelper_Easy
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.textBox1.Text = $@"FAS_src\Fubon.FGL\";
            this.textBox2.Text = $@"D:\FBL_FAS\FAS_src\Fubon.FGL\";
            this.textBox3.Text = $@"10.240.3.87";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open Text File";
            //theDialog.Filter = "TXT files|*.txt";
            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    List<string> org = new List<string>();
                    Stream myStream = null;
                    if ((myStream = theDialog.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            using (StreamReader sr = new StreamReader(myStream))
                            {                               
                                string line;
                                while ((line = sr.ReadLine()) != null)
                                {
                                    line = line?.Trim() ?? string.Empty;
                                    if(!string.IsNullOrWhiteSpace(line))
                                        org.Add(line);
                                }
                            }
                        }
                        var IPs = this.textBox3.Text.Split(';');
                        foreach (var IP in IPs)
                        {
                            foreach (var item in org)
                            {
                                var str1 = string.Empty;
                                var str2 = string.Empty;
                                var pi = item.LastIndexOf('\\');
                                if (pi > -1)
                                {
                                    str1 = item.Substring(0, pi + 1);
                                    str2 = item.Substring(pi + 1, item.Length - ((pi + 1)));
                                }
                                else
                                {
                                    str2 = item;
                                }
                                sb.AppendLine($@"""MOD"",""DAT"",""{this.textBox1.Text}{str1}"",""{str2}"",""{IP}"",""{this.textBox2.Text}{str1}"",""{str2}""");
                            }
                            sb.AppendLine(string.Empty);
                        }
                        SaveFileDialog save = new SaveFileDialog();
                        save.FileName = "DEPLOY.txt";
                        if (save.ShowDialog() == DialogResult.OK)
                        {
                            StreamWriter writer = new StreamWriter(save.OpenFile());
                            writer.WriteLine(sb.ToString());
                            writer.Dispose();
                            writer.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
            Console.ReadLine();
        }
    }
}
