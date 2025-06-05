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
    /// Логика взаимодействия для UserSettings.xaml
    /// </summary>
    public partial class UserSettings : Window
    {
        private int idUser;
        public UserSettings(int idUser)
        {
            InitializeComponent();
            this.idUser = idUser;
            LoadUser();
        }
        private void TxtEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            string email = txtMail.Text.Trim();
            Regex regex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");

            if (!regex.IsMatch(email))
            {
                MessageBox.Show("Введите корректный адрес электронной почты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUser()
        {
            string query = "SELECT TOP 1  п.пароль, п.почта, вп.вес FROM пользователь п " +
                " LEFT JOIN вес_пользователя вп ON вп.ид_пользователь = п.ид_пользователь WHERE п.ид_пользователь = @idUser " +
                " ORDER BY вп.дата_взвешивания DESC";
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@idUser", idUser);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string mail = reader["почта"].ToString();
                            string password = reader["пароль"].ToString();
                            string weight = reader["вес"].ToString();

                            txtPassword.Text = password;
                            txtMail.Text = mail;
                            txtWeight.Text = weight;

                        }
                    }
                }
            }
        }


        private void PasswordEd_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 3)
            {
                MessageBox.Show("Пароль не может быть пустым и должен содержать минимум 3 символа.");
                return;
            }

            string query = "UPDATE пользователь SET пароль = @newPassword WHERE ид_пользователь = @idUser";

            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@newPassword", newPassword);
                    cmd.Parameters.AddWithValue("@idUser", idUser);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Пароль обновлён!");
        }

        private void EmailEd_Click(object sender, RoutedEventArgs e)
        {
            string newEmail = txtMail.Text.Trim();

            if (string.IsNullOrEmpty(newEmail) || newEmail.Length < 3)
            {
                MessageBox.Show("Почта не может быть пустой и должна содержать минимум 3 символа.");
                return;
            }

            string query = "UPDATE пользователь SET почта = @newEmail WHERE ид_пользователь = @idUser";

            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@newEmail", newEmail);
                    cmd.Parameters.AddWithValue("@idUser", idUser);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Почта обновлена!");
        }

        private void ProfileBut_Click(object sender, RoutedEventArgs e)
        {
            Profile profile = new Profile(idUser);
            profile.Show();
            this.Close();
        }

        private void ReadyTrainPlanBut_Click(object sender, RoutedEventArgs e)
        {
          
           
                ListTranPlan list = new ListTranPlan(idUser);
                list.Show();
                this.Close();
            
        }

        private void FormNewPlanBut_Click(object sender, RoutedEventArgs e)
        {
         
           
                CreateTrainPersonal create = new CreateTrainPersonal(idUser);
                create.Show();
                this.Close();
            
        }

        private void GuideProductsBut_Click(object sender, RoutedEventArgs e)
        {
            GuideProducts guideProducts = new GuideProducts(idUser);
            guideProducts.Show();
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (LoadRoleUser() != "администратор")
            {

                LabWeight.Visibility = Visibility.Visible;
                BorWeight.Visibility = Visibility.Visible;
                WeightEd.Visibility = Visibility.Visible;

            }
            else
            {
                LabWeight.Visibility = Visibility.Collapsed;
                BorWeight.Visibility = Visibility.Collapsed;
                WeightEd.Visibility = Visibility.Collapsed;


            }


        }

        private string LoadRoleUser()
        {
            string query = "SELECT рп.наименование  FROM роль_пользователя рп  JOIN пользователь п ON п.ид_роль_пользователя = рп.ид_роль_пользователя " +
                "  WHERE п.ид_пользователь = @idUser";
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@idUser", idUser);

                    object result = cmd.ExecuteScalar();

                    return result != null ? result.ToString() : null;
                }
            }
        }

        private void WeightEd_Click(object sender, RoutedEventArgs e)
        {
            string newWeightText = txtWeight.Text.Trim();
            int newWeight;
            if (!int.TryParse(newWeightText, out newWeight))
            {
                MessageBox.Show("Введите корректный вес (целое число).");
                return;
            }

            if (newWeight > 500 || newWeight < 20)
            {
                MessageBox.Show("Введите корректное значение.");
                return;
            }
            string weight = VerifyDateWeight();
            if (weight != null)
            {
                string query = "UPDATE вес_пользователя SET вес = @newWeight WHERE ид_пользователь = @idUser AND дата_взвешивания = @date";

                using (SqlConnection connection = DbConnectionHelper.GetConnection())
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@newWeight", newWeightText);
                        cmd.Parameters.AddWithValue("@idUser", idUser);
                        cmd.Parameters.AddWithValue("@date", DateTime.Today);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Вес обновлена!");
            }
            else
            {
                using (SqlConnection connection = DbConnectionHelper.GetConnection())
                {
                    connection.Open();

                    string query = "INSERT INTO вес_пользователя (вес,  ид_пользователь  ) VALUES ( @weight, @user);";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@weight", newWeight);
                        command.Parameters.AddWithValue("@user", idUser);


                        command.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Вес добавлен!");

            }
        }
       
        private string VerifyDateWeight()
        {
            string query = " SELECT вп.вес FROM пользователь п JOIN вес_пользователя вп ON вп.ид_пользователь = п.ид_пользователь " +
                " WHERE п.ид_пользователь = @idUser AND вп.дата_взвешивания = @date";
              
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@idUser", idUser);
                    cmd.Parameters.AddWithValue("@date", DateTime.Today);


                    object result = cmd.ExecuteScalar();

                    return result != null ? result.ToString() : null;
                }
            }
        }
    }
}
