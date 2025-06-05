using FitnesPlan.Products;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
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

namespace FitnesPlan
{
    /// <summary>
    /// Логика взаимодействия для ListTranPlan.xaml
    /// </summary>
    public partial class ListTranPlan : Window
    {

        private int idUser;
        public ListTranPlan(int idUser)
        {
            InitializeComponent();
            this.idUser = idUser;
            LoadBreastDelt();
           
        }

        private void LoadBreastDelt()
        {
            DataTable tranPlanTable = new DataTable();
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();
                string query = @"SELECT гт.ид_тренировка , гт.название, гт.фото
            FROM тренировка гт
            JOIN тип_тренировки тт ON гт.ид_тип_тренировки  = тт.ид_тип_тренировки 
            WHERE тт.ид_тип_тренировки = 2";


                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(tranPlanTable);

                }
            }
            var items = new List<ExpandoObject>();
            foreach (DataRow row in tranPlanTable.Rows)
            {

                byte[] imageData = row["фото"] as byte[];
                BitmapImage image = null;

                if (imageData != null)
                {
                    using (var ms = new MemoryStream(imageData))
                    {
                        image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = ms;
                        image.EndInit();
                    }
                }
             

                dynamic item = new ExpandoObject();
                item.TrainId = Convert.ToInt32(row["ид_тренировка"]);
                item.NameTrain = row["название"].ToString();
                item.Image = image;

                items.Add(item);
            }

            GrydDelt.ItemsSource = items;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           
            if (LoadRoleUser() != "администратор")
            {

                FormNewPlanBut.Visibility = Visibility.Visible;
                ReadyTrainPlanBut.Visibility = Visibility.Visible;
                AddNewPlanBut.Visibility = Visibility.Collapsed;
                ListExercise.Visibility = Visibility.Collapsed;
                AddNewExercise.Visibility = Visibility.Collapsed;

            }
            else
            {
                ProfileBut.Visibility = Visibility.Collapsed;
                HideAddTrainButtons();
            }
        }
        private void HideAddTrainButtons()
        {
            foreach (var item in GrydDelt.Items)
            {
                var container = GrydDelt.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                if (container != null)
                {
                    var button = FindChild<Button>(container, "AddTrainPlan");
                    if (button != null)
                    {
                        button.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T childType)
                {
                    if (child is FrameworkElement fe && fe.Name == childName)
                        return childType;
                }

                var foundChild = FindChild<T>(child, childName);
                if (foundChild != null)
                    return foundChild;
            }

            return null;
        }
        //     var items = new List<dynamic>();
        //    foreach (DataRow row in table.Rows)
        //    {
        //        items.Add(new
        //        {
        //            TrainId = row["ид_готовая_тренировка"],
        //            NameTrain = row["название"].ToString(),
        //            Description = row["описание"].ToString()
        //        });
        //    }

        //    TrainPlanNov.ItemsSource = items;






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

        private void Train_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border clickedButton = sender as Border;
            /*            int trainId = (int)clickedBorder.Tag;
            */
            var grid = clickedButton?.Parent as Grid;
            var trainId = grid.Tag;
            int idTrain = Convert.ToInt32(trainId);
            DetailsTrain train = new DetailsTrain(idTrain, idUser);
            train.Show();
            this.Close();
        }

        private void AddTrainPlan_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            var grid = clickedButton?.Parent as Grid;
            var trainId = grid.Tag;
            int idTrain = Convert.ToInt32(trainId);

            if (ExistTrainUser(idUser, idTrain))
            {
                MessageBox.Show("Данная тренировка уже добавлена");
                return;
            }
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();

                string insertQuery = @"INSERT INTO тренировки_пользователя (ид_пользователь, ид_тренировка)
                               VALUES (@userId, @trainingId);";

                using (SqlCommand cmd = new SqlCommand(insertQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", idUser);
                    cmd.Parameters.AddWithValue("@trainingId", trainId);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Тренировка успешно назначена пользователю!");
            }
        }
        private void AddNewPlanBut_Click(object sender, RoutedEventArgs e)
        {
          
                AddProducts create = new AddProducts(idUser);
                create.Show();
                this.Close();
           
        }

        private bool ExistTrainUser(int idUser, int idTrain)
        {
            int count;
            string query = "SELECT COUNT(*) FROM тренировки_пользователя WHERE ид_пользователь = @ид_пользователь AND ид_тренировка = @ид_тренировка ";
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@ид_пользователь", idUser);
                    cmd.Parameters.AddWithValue("@ид_тренировка", idTrain);
                    count = (int)cmd.ExecuteScalar();

                }
            }
            return count > 0;
        }

        private void AddNewExercise_Click(object sender, RoutedEventArgs e)
        {
            AddExerExercise add = new AddExerExercise(idUser);
            add.Show();
            this.Close();
        }
        private void ListExercise_Click(object sender, RoutedEventArgs e)
        {
            ListExer list = new ListExer(4, idUser);
            list.Show();
            this.Close();
        }
    }
}
