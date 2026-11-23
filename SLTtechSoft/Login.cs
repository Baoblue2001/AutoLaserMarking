using Microsoft.VisualBasic;
using Newtonsoft.Json;
using SLTtechSoft;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    

    public partial class Login : Form
    {
      
        public Login()
        {
            InitializeComponent();
            UserManager.LoadUsers();
            if (!File.Exists(UserManager.PathUserDb))
                UserManager.SaveUsers(new List<User> { new User { UserName = "admin", Password = "admin123", Role = "Admin" } });
            txtPassword.PasswordChar = '●';
            this.AcceptButton = btnLogin;
           
        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string user = txtUserName.Text.Trim();
            string pass = txtPassword.Text.Trim();
            var users = UserManager.LoadUsers();
            string hashInput = HashPassword(txtPassword.Text);
            txtPassword.Clear();
            var found = users.FirstOrDefault(u => u.UserName == user && u.Password == pass);
            if (found == null)
            { 
                Program.IsLoggedIn = false;
                MessageBox.Show("Sai tài khoản hoặc mật khẩu!");
                return;
            }
            Program.IsLoggedIn = true;
            
            // Lưu user vào biến toàn cục, hoặc truyền vào MainForm
            Program.CurrentUser = found;
            this.Hide();
          

        }
        public class User
        {
            public string UserName { get; set; }
            public string Password { get; set; } // Ở thực tế nên mã hóa/hash
            public string Role { get; set; } // "Admin", "Tech", "CongNhan"
        }
        public static class UserManager
        {
            public static string PathUserDb = "Users.json";

            public static List<User> LoadUsers()
            {
                if (!File.Exists(PathUserDb)) return new List<User>();
                var json = File.ReadAllText(PathUserDb);
                return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
            }

            public static void SaveUsers(List<User> users)
            {
                var json = JsonConvert.SerializeObject(users, Formatting.Indented);
                File.WriteAllText(PathUserDb, json);
            }
        }
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        private void cbShowPass_CheckedChanged(object sender, EventArgs e)
        {
            if(cbShowPass.Checked)
            {
                string pass = Interaction.InputBox("Nhập mật khẩu:");
                if (pass != "2468")
                {
                    cbShowPass.Checked = false;
                    MessageBox.Show("Sai mật khẩu");
                   
                }
                else
                {
                    txtPassword.PasswordChar = cbShowPass.Checked ? '\0' : '●';
                }
            }
            else
            {
                txtPassword.PasswordChar = cbShowPass.Checked ? '\0' : '●';
            }
           
            
        }
    }
  
}
