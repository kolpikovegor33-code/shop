using shop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace shop
{
 
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            string login = tbLogin.Text.Trim();
            if(login == "")
            {
                MessageBox.Show("Введите логин");
                return;
            }
            AppDbContext db = new AppDbContext();
            UsersImport user = db.UsersImports.FirstOrDefault(u => u.Login == login);
            if (user == null)
            {
                MessageBox.Show("Пользователь с таким логином не найден");
                return;
            }
            User.CurrentUser = user;
            MainWindow main = new MainWindow(user);
            main.Show();
            this.Close();
        }
        private void btnGuest_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow(null);
            main.Show();
            this.Close();
        }

    }
}
