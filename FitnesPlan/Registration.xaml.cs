using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FitnesPlan
{
    /// <summary>
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {

        public Registration()
        {
            InitializeComponent();
        }

        private void Reg_Click(object sender, RoutedEventArgs e)
        {
            string password = txtPassword.Password;
            string mail = txtEmail.Text;

            
            bool mailExist = VerifyExistUser(mail);
            if(mailExist != false )
            {
                MessageBox.Show("Пользователь с такой почтой уже зарегистрирован");
                return;
            }
            
            using(SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();
                string query = "INSERT INTO пользователь ( пароль, почта ,ид_роль_пользователя ) VALUES ( @password, @mail, @roleId); SELECT SCOPE_IDENTITY();";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@password", password);
                    command.Parameters.AddWithValue("@mail", mail);
                    command.Parameters.AddWithValue("@roleId", 1);

                    object result = command.ExecuteScalar();
                    int newUserId = Convert.ToInt32(result);

                    if (newUserId > 0)
                    {
                        MessageBox.Show("Пользователь успешно зарегистрирован");
                        Profile profile = new Profile(newUserId);
                        profile.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при регистрации");
                    }
                }
            }
        }
        private void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            watermarkUsername.Visibility = string.IsNullOrWhiteSpace(txtEmail.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void txtUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            watermarkUsername.Visibility = string.IsNullOrWhiteSpace(txtEmail.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void txtUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            watermarkUsername.Visibility = string.IsNullOrWhiteSpace(txtEmail.Text) ? Visibility.Visible : Visibility.Collapsed;

            TxtEmail_LostFocus(sender, e);
        }

        private void TxtEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            Regex regex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");

            if (!regex.IsMatch(email))
            {
                MessageBox.Show("Введите корректный адрес электронной почты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            watermarkPassword.Visibility = string.IsNullOrWhiteSpace(txtPassword.Password) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void txtPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            watermarkPassword.Visibility = string.IsNullOrWhiteSpace(txtPassword.Password) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void txtPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            watermarkPassword.Visibility = string.IsNullOrWhiteSpace(txtPassword.Password) ? Visibility.Visible : Visibility.Collapsed;
        }
        public bool VerifyExistUser(string mail)
        {
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM пользователь WHERE почта = @mail";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@mail", mail);

                    int userCount = (int)command.ExecuteScalar();

                    return userCount > 0;
                }
            }
        }

        private void Log_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }
    }
}
