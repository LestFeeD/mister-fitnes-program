using FitnesPlan.Products;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
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
    /// Логика взаимодействия для CreateTrainPersonal.xaml
    /// </summary>
    public partial class CreateTrainPersonal : Window
    {
        int idUser;
        private byte[] selectedImageBytes;

        public CreateTrainPersonal(int idUser)
        {
            InitializeComponent();
            this.idUser = idUser;
            App.SelectedExercises.CollectionChanged += SelectedExercises_CollectionChanged;
            LoadExersice();
            LoadTypes();
        }

        private void ChooseExer_Click(object sender, RoutedEventArgs e)
        {
            ListExer listExer = new ListExer(1, idUser);
            listExer.Show();
            this.Close();
        }

        private void BorderExerUserEm_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border clickedButton = sender as Border;

            var detailsId = clickedButton.Tag;
            int idExers = Convert.ToInt32(detailsId);
            DetailsExersice detailsExersice = new DetailsExersice(idExers,0, idUser);
            detailsExersice.Show();
            this.Close();
        }


        private void SelectedExercises_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                ExerciseList.ItemsSource = null;
            }
            else
            {
                LoadExersice();
            }
        }

        private void LoadExersice()
        {
            var exerciseList = new List<object>();

            if (App.SelectedExercises == null || App.SelectedExercises.Count == 0)
                return;

            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();

                var ids = App.SelectedExercises;
                var parameters = ids.Select((id, index) => $"@id{index}").ToList();
                var query = $"SELECT ид_упражнение, название, фото FROM упражнение WHERE ид_упражнение IN ({string.Join(",", parameters)})";

                var cmd = new SqlCommand(query, conn);
                for (int i = 0; i < ids.Count; i++)
                {
                    cmd.Parameters.AddWithValue($"@id{i}", ids[i]);
                }

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    byte[] imageData = reader["фото"] as byte[];

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

                    exerciseList.Add(new
                    {
                        idExer = id,
                        NameExer = name,
                        Image = image
                    });
                }
            }

            ExerciseList.ItemsSource = exerciseList;
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
                TrainTypeComboBox.Visibility = Visibility.Collapsed;
                ChoicePhoto.Visibility = Visibility.Collapsed;
                GroupMuscl.Visibility = Visibility.Collapsed;
            }
            else
            {
                ProfileBut.Visibility = Visibility.Collapsed;

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
        private void Perform_Click(object sender, RoutedEventArgs e)
        {
            if (App.SelectedExercises == null || App.SelectedExercises.Count == 0)
            {
                MessageBox.Show("Вы не выбрали ни одного упражнения.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNameTrain.Text))
            {
                MessageBox.Show("Название тренировочного плана пустое.");
                return;
            }

            string role = LoadRoleUser();
            int? typeId = TrainTypeComboBox.SelectedValue as int?;

            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();

                try
                {
                    int newTrainId;
                    string insertTrainQuery;

                    if (role == "администратор")
                    {
                        insertTrainQuery = @"
                    INSERT INTO тренировка 
                    (название, описание, фото, ид_тип_тренировки, ид_вид_тренировки, ид_пользователь)
                    OUTPUT INSERTED.ид_тренировка
                    VALUES (@name, @desc, @photo, @typeTrain, @viewTrain, @userId)";
                    }
                    else
                    {
                        insertTrainQuery = @"
                    INSERT INTO тренировка 
                    (название, описание, ид_тип_тренировки, ид_пользователь)
                    OUTPUT INSERTED.ид_тренировка
                    VALUES (@name, @desc, @typeTrain, @userId)";
                    }

                    using (SqlCommand cmd = new SqlCommand(insertTrainQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", txtNameTrain.Text);
                        cmd.Parameters.AddWithValue("@desc", txtDescTrain.Text ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@userId", idUser);

                        if (role == "администратор")
                        {
                            if (selectedImageBytes == null)
                            {
                                MessageBox.Show("Администратор должен выбрать фото.");
                                return;
                            }

                            if (typeId == null)
                            {
                                MessageBox.Show("Выберите вид тренировки.");
                                return;
                            }

                            cmd.Parameters.AddWithValue("@photo", selectedImageBytes);
                            cmd.Parameters.AddWithValue("@typeTrain", 2); 
                            cmd.Parameters.AddWithValue("@viewTrain", typeId.Value); 
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@typeTrain", 1); 
                        }

                        newTrainId = (int)cmd.ExecuteScalar();
                    }

                    string insertExQuery = @"
                INSERT INTO упражнение_тренировки (ид_тренировка, ид_упражнение)
                VALUES (@trainId, @exId)";

                    foreach (int exId in App.SelectedExercises)
                    {
                        using (SqlCommand cmd = new SqlCommand(insertExQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@trainId", newTrainId);
                            cmd.Parameters.AddWithValue("@exId", exId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    txtNameTrain.Clear();
                    txtDescTrain.Clear();
                    App.SelectedExercises.Clear();

                    MessageBox.Show("Тренировка успешно создана!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при создании тренировки: " + ex.Message);
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

        private void Exer_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void DeleteTrain_Click(object sender, RoutedEventArgs e)
        {
            App.SelectedExercises.Clear();

        }

        private void LoadTypes()
        {
            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                string query = "SELECT ид_вид_тренировки, наименование FROM вид_тренировки";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                TrainTypeComboBox.ItemsSource = dt.DefaultView;
                TrainTypeComboBox.DisplayMemberPath = "наименование";
                TrainTypeComboBox.SelectedValuePath = "ид_вид_тренировки";
            }
        }

        private void ChoicePhoto_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedImageBytes = File.ReadAllBytes(openFileDialog.FileName);
                MessageBox.Show("Файл выбран");
            }
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
        private void AddNewPlanBut_Click(object sender, RoutedEventArgs e)
        {

            AddProducts create = new AddProducts(idUser);
            create.Show();
            this.Close();

        }
    }
}
