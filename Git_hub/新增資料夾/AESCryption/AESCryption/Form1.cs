using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AESCryption
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ASE 加密 (Encrypt)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string encryptStr = textBox1.Text;
                string key = textBox4.Text;
                if (string.IsNullOrWhiteSpace(encryptStr))
                {
                    MessageBox.Show("加密字串不能為空");
                }
                else if (string.IsNullOrWhiteSpace(key))
                {
                    MessageBox.Show("加解密Key值不能為空");
                }
                else
                {
                    byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key.Substring(0,(key.Length > 32) ? 32 : key.Length).PadLeft(32,'0'));
                    byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(encryptStr);
                    RijndaelManaged rDel = new RijndaelManaged();
                    rDel.Key = keyArray;
                    rDel.Mode = CipherMode.ECB;
                    rDel.Padding = PaddingMode.PKCS7;
                    ICryptoTransform cTransform = rDel.CreateEncryptor();
                    byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                    textBox3.Text = Convert.ToBase64String(resultArray, 0, resultArray.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// ASE 解密 (Decrypt)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                string decryptStr = textBox2.Text;
                string key = textBox4.Text;
                if (string.IsNullOrWhiteSpace(decryptStr))
                {
                    MessageBox.Show("解密字串不能為空");
                }
                else if (string.IsNullOrWhiteSpace(key))
                {
                    MessageBox.Show("加解密Key值不能為空");
                }
                else
                {
                    byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key.Substring(0, (key.Length > 32) ? 32 : key.Length).PadLeft(32, '0'));
                    byte[] toDecryptArray = Convert.FromBase64String(decryptStr);
                    RijndaelManaged rDel = new RijndaelManaged();
                    rDel.Key = keyArray;
                    rDel.Mode = CipherMode.ECB;
                    rDel.Padding = PaddingMode.PKCS7;
                    ICryptoTransform cTransform = rDel.CreateDecryptor();
                    byte[] resultArray = cTransform.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);
                    textBox3.Text = UTF8Encoding.UTF8.GetString(resultArray);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
