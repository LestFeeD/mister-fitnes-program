using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FitnesPlan
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private int failedAttempts = 0;
        private const int MaxCaptchaAttempts = 3;
        public MainWindow()
        {
            InitializeComponent();
        }
        public int AuthenticateUser(string mail, string password)
        {
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                string query = @"SELECT ид_пользователь 
                             FROM пользователь 
                             WHERE почта = @login AND пароль = @password";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@login", mail);
                command.Parameters.AddWithValue("@password", password);

                connection.Open();
             
                object result = command.ExecuteScalar();

                if (result != null && int.TryParse(result.ToString(), out int id))
                {
                    return id;
                }
                else
                {
                    MessageBox.Show("Не удалось найти информацию о пользователе. Проверьте ввод данных.");
                }
                return failedAttempts;
            }
        }

        public string FindRoleUser(string mail, string password)
        {
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                string query = @"SELECT r.наименование
                             FROM пользователь ap
                             JOIN роль_пользователя r ON ap.ид_роль_пользователя  = r.ид_роль_пользователя 
                             WHERE ap.почта = @login AND ap.пароль = @password";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@login", mail);
                command.Parameters.AddWithValue("@password", password);

                connection.Open();
                string role = (string)command.ExecuteScalar();
                return role;
            }
        }


        private void Autho_Click(object sender, RoutedEventArgs e)
        {
            string mail = txtUsername.Text;
            string password = txtPassword.Password;

            int id = AuthenticateUser(mail, password);
            string role = FindRoleUser(mail, password);


            if (!string.IsNullOrEmpty(role)) 
            {

                if (role != "администратор")
                {
                    MessageBox.Show("Успешная авторизация!");

                    Profile labWindow = new Profile(id);
                    labWindow.Show();
                    this.Close();
                } else
                {
                    MessageBox.Show("Успешная авторизация!");
                    GuideProducts guideProducts = new GuideProducts(id);
                    guideProducts.Show();
                    this.Close();
                }

            }
            else
            {
                failedAttempts++;

                if (failedAttempts >= MaxCaptchaAttempts)
                {
                    Captha captha = new Captha();
                    captha.CaptchaVerificationFailed += BlockWindow; 
                    bool? result = captha.ShowDialog();

                    if (result == true) 
                    {
                        failedAttempts = 0; 
                    }
                    else
                    {
                        MessageBox.Show("Неправильная капча. Попробуйте снова.");
                    }
                }
                else
                {

                }
            }
        }
        private void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            watermarkUsername.Visibility = string.IsNullOrWhiteSpace(txtUsername.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void txtUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            watermarkUsername.Visibility = string.IsNullOrWhiteSpace(txtUsername.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void txtUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            watermarkUsername.Visibility = string.IsNullOrWhiteSpace(txtUsername.Text) ? Visibility.Visible : Visibility.Collapsed;
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
        private async void BlockWindow()
        {
            MessageBox.Show("Вы ввели неправильную капчу 3 раза. Окно заблокировано на 10 секунд.", "Blocked", MessageBoxButton.OK, MessageBoxImage.Warning);
            Autho.IsEnabled = false;
            await Task.Delay(10000);
            Autho.IsEnabled = true;
            failedAttempts = 0;
        }

        private void Autho_Click_1(object sender, RoutedEventArgs e)
        {
            Registration registration = new Registration();
            registration.Show();
            this.Close();
        }
    }
}
