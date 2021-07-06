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

namespace DimensionsHelper
{
    public partial class Form1 : Form
    {    
        public Form1()
        {
            InitializeComponent();
            //預設排除的資料夾&附檔名
            this.listBox1.Items.Add(".git");
            this.listBox1.Items.Add(".vs");
            this.listBox1.Items.Add(".dm");
            this.listBox1.Items.Add("BUILDOPT");
            this.listBox1.Items.Add("DEPLOY.DIM");

            //this.textBox3.Text = @"D:\Dimensions\GLSI\201904240243_547";
            //this.textBox8.Text = "_f";
            this.textBox9.Text = "bin";
            this.textBox10.Text = ".cs;.aspx";
            //this.textBox5.Text = "report";

            this.checkBox1.Checked = true;
            //this.checkBox2.Checked = true;
        }

        /// <summary>
        /// 新增排除比對的副檔名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(this.textBox2.Text))
                this.listBox1.Items.Add(this.textBox2.Text?.Trim());
        }

        /// <summary>
        /// 刪除排除比對的副檔名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedItem != null)           
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            else
                MessageBox.Show("無選擇之檔案!");
        }

        /// <summary>
        /// 選擇產生明細資料夾
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.ShowDialog();
            this.textBox3.Text = folder.SelectedPath;
        }

        /// <summary>
        /// 選擇比對明細資料夾
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.ShowDialog();
            this.textBox4.Text = folder.SelectedPath;
        }

        /// <summary>
        /// 產生檔案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.listBox2.Items.Count == 0 && radioButton1.Checked)
                    MessageBox.Show("無寫入IP");
                else if (this.listBox2.SelectedItem == null && radioButton1.Checked)
                    MessageBox.Show("請選擇一IP為Source存放之IP");
                else if (string.IsNullOrWhiteSpace(this.textBox3.Text))
                    MessageBox.Show("無選擇產生明細資料夾");
                else
                {
                    SaveFileDialog save = new SaveFileDialog();
                    var fileName = "DEPLOY.DIM";
                    if (radioButton1.Checked)
                        fileName = "DEPLOY.DIM";
                    else if(radioButton2.Checked)
                        fileName = "Detail.txt";
                    save.FileName = fileName;
                    save.Filter = "所有檔案 (*.*)|*.*";
                    if (save.ShowDialog() == DialogResult.OK)
                    {
                        StreamWriter writer = new StreamWriter(save.OpenFile());
                        if(radioButton1.Checked)
                            writer.WriteLine(setDimFile("DIM"));
                        else if(radioButton2.Checked)
                            writer.WriteLine(setDimFile("Detail"));
                        writer.Dispose();
                        writer.Close();
                        MessageBox.Show("產生檔案成功!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }       
        }

        /// <summary>
        /// 新增寫入IP位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            var ip = this.textBox8.Text?.Trim()?.ToUpper();
            if(!string.IsNullOrWhiteSpace(this.textBox8.Text))
                if (ip == "_FRT" || ip == "_F")
                {
                    this.textBox3.ReadOnly = false;
                    this.textBox4.ReadOnly = false;
                    this.listBox2.Items.Add("10.240.3.87");
                    this.listBox2.Items.Add("10.240.68.36");
                    this.listBox2.Items.Add("10.240.68.38");
                    this.listBox2.Items.Add("10.240.1.80");
                    this.listBox2.Items.Add("10.240.1.81");
                    this.textBox6.Text = @"FAS_src\Fubon.FGL\";
                    this.textBox7.Text = @"D:\FBL_FAS\FAS_src\Fubon.FGL\";
                    this.textBox1.Text = @"D:\";
                }
                else if (ip == "_FPS")
                {
                    this.textBox3.ReadOnly = false;
                    this.textBox4.ReadOnly = false;
                    this.listBox2.Items.Add("10.240.3.87");
                    this.listBox2.Items.Add("10.240.68.36");
                    this.listBox2.Items.Add("10.240.68.38");
                    this.listBox2.Items.Add("10.240.1.80");
                    this.listBox2.Items.Add("10.240.1.81");
                    this.textBox3.Text = @"C:\Users\B0561\Desktop\Dimensions\GLSI\XXXXXXXXXXXX_XXX\FAS_src\FPS";
                    this.textBox6.Text = @"FAS_src\FPS\";
                    this.textBox7.Text = @"D:\FBL_FAS\FAS_src\FPS\";
                    this.textBox1.Text = @"D:\FPS\";
                }
                else if (ip == "_WANPIE" || ip == "_W")
                {
                    this.textBox3.ReadOnly = false;
                    this.textBox4.ReadOnly = false;
                    this.listBox2.Items.Add("10.240.3.87");
                    this.listBox2.Items.Add("10.42.70.54");
                    this.listBox2.Items.Add("10.42.1.98");
                    this.listBox2.Items.Add("10.42.1.99");
                    this.listBox2.Items.Add("10.42.1.49");
                    this.listBox2.Items.Add("10.42.52.191");
                    this.listBox2.Items.Add("10.42.71.11");
                    this.textBox3.Text = @"C:\Users\B0561\Desktop\Dimensions\GLSI\XXXXXXXXXXXX_XXX\FAS_src\GLSI_Source";
                    this.textBox6.Text = @"FAS_src\GLSI_Source\";
                    this.textBox7.Text = @"D:\FBL_FAS\FAS_src\GLSI_Source\";
                    this.textBox1.Text = @"D:\GLSI\Software\";
                }
                else if (ip == "_T")
                {
                    this.textBox3.ReadOnly = false;
                    this.textBox4.ReadOnly = false;
                    this.listBox2.Items.Add("10.240.3.87");
                    this.listBox2.Items.Add("10.240.3.127");
                    this.listBox2.Items.Add("10.240.3.128");
                    this.listBox2.Items.Add("10.240.68.36");
                    this.listBox2.Items.Add("10.240.68.38");
                    this.textBox6.Text = @"FAS_src\Fubon.Treasury\";
                    this.textBox7.Text = @"D:\FBL_FAS\FAS_src\Fubon.Treasury\";
                    this.textBox1.Text = @"D:\";
                }
                else
                {
                    this.listBox2.Items.Add(this.textBox8.Text?.Trim());
                }
            else
                MessageBox.Show("無新增之位置!");
        }

        /// <summary>
        /// 刪除寫入IP位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            if (this.listBox2.SelectedItem != null)
                listBox2.Items.RemoveAt(listBox2.SelectedIndex);
            else
                MessageBox.Show("無選擇之檔案!");
        }

        private string setDimFile(string type)
        {
            StringBuilder sb = new StringBuilder();
            string buildFile = this.textBox9.Text ?? string.Empty; //BuildFile
            List<string> excludes = new List<string>(); // 排除的檔案
            foreach (var i in listBox1.Items)
            {
                excludes.Add(i?.ToString() ?? string.Empty);
            }
            List<string> copyFileName = dirSearch(this.textBox3.Text, excludes).OrderBy(x => x).ToList();
            List<string> compareFileName = dirSearch(this.textBox4.Text, excludes).OrderBy(x => x).ToList();
            var _Excepts = copyFileName.Except(compareFileName).OrderBy(x => x).ToList(); //Add
            var _Intersect = copyFileName.Intersect(compareFileName).OrderBy(x => x).ToList(); //Update  
            if (type == "DIM")
            {
                string orgLocation = this.textBox6.Text; //(檔案來源位置)客製前綴位置
                string sourceLocation = this.textBox7.Text; //(檔案存放位置)客製前綴位置
                string runLocation = this.textBox1.Text; //(檔案執行位置)客製前綴位置
                string sourceIP = listBox2.GetItemText(listBox2.SelectedItem); //SourceIP
                bool csFlag = this.checkBox1.Checked; //檔案執行位置排除附檔名為 .cs 的檔案
                bool reportFlag = this.checkBox2.Checked; //位於設定資料夾附檔名為 .cs 的檔案不排除
                List<string> files = new List<string>(); //設定資料夾底下的檔案不排除
                files.AddRange((this.textBox5.Text ?? string.Empty).Split(';').Select(x=>x.ToUpper()).ToList());
                List<string> filenameExtensions = new List<string>(); //欲排除設定的附檔名
                filenameExtensions.AddRange((this.textBox10.Text ?? string.Empty).Split(';')
                    .Where(x => x.IndexOf('.') > -1).Select(x => x.Split('.')[1]).ToList());
                List<string> IPs = new List<string>(); // All IP
                foreach (var i in listBox2.Items)
                    IPs.Add(i?.ToString() ?? string.Empty);                  
                foreach (var str in _Excepts)
                    //sb = setValue(csFlag, reportFlag, files, filenameExtensions, "ADD", sb, sourceIP, str, orgLocation, sourceLocation);
                    sb = setValue(csFlag, reportFlag, files, filenameExtensions, "MOD", sb, sourceIP, str, orgLocation, sourceLocation);
                foreach (var str in _Intersect)
                    sb = setValue(csFlag, reportFlag, files, filenameExtensions, "MOD", sb, sourceIP, str, orgLocation, sourceLocation);
                sb.AppendLine(string.Empty);
                foreach (var ip in IPs.Where(x => !x.Contains(sourceIP)).OrderBy(x => x))
                {
                    foreach (var str in _Excepts)
                        //sb = setValue(csFlag, reportFlag, files, filenameExtensions, "ADD", sb, ip, str, orgLocation, runLocation, buildFile);
                        sb = setValue(csFlag, reportFlag, files, filenameExtensions, "MOD", sb, ip, str, orgLocation, runLocation, buildFile);
                    foreach (var str in _Intersect)
                        sb = setValue(csFlag, reportFlag, files, filenameExtensions, "MOD", sb, ip, str, orgLocation, runLocation, buildFile);
                    sb.AppendLine(string.Empty);
                }
            }
            else if (type == "Detail")
            {
                sb.AppendLine("ADD");
                var i = 1;
                _Excepts.ForEach(str =>
                {
                    sb.AppendLine($@"{i}. " + string.Join("\\", str.Split('\\').SkipWhile(x => x == string.Empty)));
                    i += 1;
                });                            
                sb.AppendLine(string.Empty);
                sb.AppendLine("MOD");
                i = 1;
                _Intersect.ForEach(str =>
                {
                    sb.AppendLine($@"{i}. " + string.Join("\\", str.Split('\\').SkipWhile(x => x == string.Empty)));
                    i += 1;
                });
            }
            return sb.ToString();
        }

        private StringBuilder setValue(
            bool csFlag,
            bool reportFlag,
            List<string> files, 
            List<string> filenameExtensions, 
            string firstValue, 
            StringBuilder sb, 
            string ip, 
            string value, 
            string sourceLocation, 
            string copyLocation, 
            string buildFile = null)
        {
            value = value ?? string.Empty;
            List<string> vals = value.Split('\\').ToList();
            string value2 = vals.Last();
            string value1 = string.Join("\\", vals.SkipWhile(x => x == string.Empty)).Replace(value2, string.Empty);
            if (buildFile == null)  //source          
                sb.AppendLine($"\"{firstValue}\",\"DAT\",\"{sourceLocation}{value1}\",\"{value2}\",\"{ip}\",\"{copyLocation}{value1}\",\"{value2}\"");           
            else //run
            {
                buildFile = buildFile?.Trim();
                if (!string.IsNullOrWhiteSpace(buildFile)) //example : bin
                {
                    var i = value.ToUpper().IndexOf(buildFile.ToUpper());
                    if (i > -1 )
                        sb.AppendLine($"\"{firstValue}\",\"DAT\",\"{sourceLocation}{value1}\",\"{value2}\",\"{ip}\",\"{copyLocation}{value.Substring(i  + buildFile.Length + 1).Replace(value2, string.Empty)}\",\"{value2}\"");
                } //all
                else
                    if((!csFlag || (reportFlag && vals.Any(x => files.Contains(x?.ToUpper()))) || (filenameExtensions.Any(z => value2.ToUpper().IndexOf(z.ToUpper()) == -1))))
                        sb.AppendLine($"\"{firstValue}\",\"DAT\",\"{sourceLocation}{value1}\",\"{value2}\",\"{ip}\",\"{copyLocation}{value1}\",\"{value2}\"");
            }               
            return sb;
        }

        private List<string> dirSearch(string sDir,List<string> excludes,string repStr = null)
        {
            List<string> result = new List<string>();
            if (string.IsNullOrWhiteSpace(sDir))
                return result;
            if (repStr == null)
                repStr = sDir;
            try
            {
                foreach (var _f in Directory.GetFiles(sDir))
                {
                    var info = new FileInfo(_f);
                    var tFileName = info.Name?.ToString()?.Trim();//取得檔名
                    if (!excludes.Contains(tFileName))
                        result.Add(_f.Replace(repStr, string.Empty));
                }
                foreach (var f in Directory.GetDirectories(sDir))
                {
                    var info = new FileInfo(f);
                    var tFileName = info.Name?.ToString()?.Trim();//取得檔名
                    if (!excludes.Contains(tFileName))
                        result.AddRange(dirSearch(f, excludes, repStr));                   
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return result;
        }
    }
}
