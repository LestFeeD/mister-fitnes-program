using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
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

namespace FitnesPlan.Products
{
    /// <summary>
    /// Логика взаимодействия для EditExercise.xaml
    /// </summary>
    public partial class EditExercise : Window
    {
        private int exerciseId;
        private byte[] photoData;
        private int idUser;
        private string selectedGifPath;
        private byte[] selectedImageBytes;

        public EditExercise(int exerciseId, int idUser)
        {
            InitializeComponent();
            this.exerciseId = exerciseId;
            this.idUser = idUser;
            LoadExerciseTypes();
            LoadDifExer();
            LoadExercise();
        }
        private void SelectPhoto_Click(object sender, RoutedEventArgs e)
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
        private void LoadExerciseTypes()
        {
            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT ид_тип_упражнения, наименование FROM тип_упражнения", conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ExerciseTypeComboBox.ItemsSource = dt.DefaultView;
            }
        }

        private void LoadDifExer()
        {
            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT ид_сложность_упражнения, наименование FROM сложность_упражнения", conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                DifficultyComboBox.ItemsSource = dt.DefaultView;
            }
        }

        private void LoadExercise()
        {
            using (var connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM упражнение   WHERE ид_упражнение = @id", connection);
                command.Parameters.AddWithValue("@id", exerciseId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        TitleTextBox.Text = reader["название"].ToString();
                        var gifPath = reader["видео"].ToString();

                        if (File.Exists(gifPath))
                        {
                            ExerciseGif.Source = new BitmapImage(new Uri(gifPath, UriKind.Absolute));
                        }
                        CaloriesTextBox.Text = reader["калорийность"].ToString();
                        TechniqueLinkTextBox.Text = reader["ссылка_техники"].ToString();

                        ExerciseGif.Source = new BitmapImage(new Uri(reader["видео"].ToString(), UriKind.RelativeOrAbsolute));

                        ExerciseTypeComboBox.SelectedValue = Convert.ToInt32(reader["ид_тип_упражнения"]);
                        DifficultyComboBox.SelectedValue = Convert.ToInt32(reader["ид_сложность_упражнения"]);
                    }
                }
            }
        }
        private void SelectGif_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "GIF-анимации (*.gif)|*.gif"
            };

            if (openFileDialog.ShowDialog() == true)
            {

                selectedGifPath = openFileDialog.FileName;
                MessageBox.Show("GIF-файл выбран: " + selectedGifPath);
            }
        }
      
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = DbConnectionHelper.GetConnection())
                {
                    conn.Open();

                    List<string> setClauses = new List<string>();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    if (string.IsNullOrEmpty(TitleTextBox.Text))
                    {
                        MessageBox.Show("Название не может быть пустым.");
                        return;
                    }
                  
                    setClauses.Add("название = @name");
                    cmd.Parameters.AddWithValue("@name", TitleTextBox.Text.Trim());

                
                    if (!string.IsNullOrEmpty(selectedGifPath))
                    {
                        setClauses.Add("видео = @video");
                        cmd.Parameters.AddWithValue("@video", selectedGifPath);
                    }

          
                    if (selectedImageBytes != null)
                    {
                        setClauses.Add("фото = @photo");
                        cmd.Parameters.AddWithValue("@photo", selectedImageBytes);
                    }

               
                    if (ExerciseTypeComboBox.SelectedValue != null)
                    {
                        setClauses.Add("ид_тип_упражнения = @type");
                        cmd.Parameters.AddWithValue("@type", ExerciseTypeComboBox.SelectedValue);
                    }

                 
                    if (int.TryParse(CaloriesTextBox.Text, out int cal))
                    {
                        setClauses.Add("калорийность = @calories");
                        cmd.Parameters.AddWithValue("@calories", cal);
                    }

                
                    if (DifficultyComboBox.SelectedValue != null)
                    {
                        setClauses.Add("ид_сложность_упражнения = @difficulty");
                        cmd.Parameters.AddWithValue("@difficulty", DifficultyComboBox.SelectedValue);
                    }

                    if (!string.IsNullOrWhiteSpace(TechniqueLinkTextBox.Text))
                    {
                        if (!Uri.IsWellFormedUriString(TechniqueLinkTextBox.Text, UriKind.Absolute) ||
    !(TechniqueLinkTextBox.Text.StartsWith("http://") || TechniqueLinkTextBox.Text.StartsWith("https://")))
                        {
                            MessageBox.Show("Введите корректную ссылку на видео (должна начинаться с http:// или https://).");
                            return;
                        }
                        setClauses.Add("ссылка_техники = @link");
                        cmd.Parameters.AddWithValue("@link", TechniqueLinkTextBox.Text.Trim());
                    }

                    
                    if (setClauses.Count == 0)
                    {
                        MessageBox.Show("Нет данных для обновления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    cmd.Parameters.AddWithValue("@id", exerciseId);

                    string sql = $"UPDATE упражнение SET {string.Join(", ", setClauses)} WHERE ид_упражнение = @id";
                    cmd.CommandText = sql;

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Упражнение обновлено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadExercise();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private BitmapImage ByteArrayToImage(byte[] byteArray)
        {
            using (var stream = new MemoryStream(byteArray))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }
        }

        private void ListExercise_Click(object sender, RoutedEventArgs e)
        {
            ListExer list = new ListExer(4, idUser);
            list.Show();
            this.Close();
        }

        private void FormNewPlanBut_Click(object sender, RoutedEventArgs e)
        {
            AddProducts create = new AddProducts(idUser);
            create.Show();
            this.Close();
        }

        private void GuideProductsBut_Click(object sender, RoutedEventArgs e)
        {
            GuideProducts guideProducts = new GuideProducts(idUser);
            guideProducts.Show();
            this.Close();
        }
        private void AddNewExercise_Click(object sender, RoutedEventArgs e)
        {
            AddExerExercise add = new AddExerExercise(idUser);
            add.Show();
            this.Close();
        }

        private void ReadyTrainPlanBut_Click(object sender, RoutedEventArgs e)
        {
            
                ListTranPlan list = new ListTranPlan(idUser);
                list.Show();
                this.Close();
            
        }
        private void CreateNewPlan_Click(object sender, RoutedEventArgs e)
        {
            CreateTrainPersonal create = new CreateTrainPersonal(idUser);
            create.Show();
            this.Close();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    List<int> affectedTrainingIds = new List<int>();

                    string selectTrainings = "SELECT ид_тренировка FROM упражнение_тренировки WHERE ид_упражнение = @exerciseId";
                    using (SqlCommand cmd = new SqlCommand(selectTrainings, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@exerciseId", exerciseId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                affectedTrainingIds.Add(reader.GetInt32(0));
                            }
                        }
                    }

                    var commands = new List<string>
            {
                @"DELETE FROM повторение_упражнения 
                  WHERE ид_выполнение_упражнения IN (
                      SELECT ид_выполнение_упражнения FROM выполнение_упражнения 
                      WHERE ид_упражнение = @exerciseId
                  )",

                @"DELETE FROM выполнение_упражнения WHERE ид_упражнение = @exerciseId",

                @"DELETE FROM упражнение_тренировки WHERE ид_упражнение = @exerciseId",

                @"DELETE FROM упражнение WHERE ид_упражнение = @exerciseId"
            };

                    foreach (var sql in commands)
                    {
                        using (SqlCommand cmd = new SqlCommand(sql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@exerciseId", exerciseId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    foreach (int trainId in affectedTrainingIds)
                    {
                        string checkExerciseCount = "SELECT COUNT(*) FROM упражнение_тренировки WHERE ид_тренировка = @trainId";
                        using (SqlCommand cmd = new SqlCommand(checkExerciseCount, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@trainId", trainId);
                            int count = (int)cmd.ExecuteScalar();

                            if (count == 0)
                            {
                                var trainDeleteCommands = new List<string>
                        {
                            @"DELETE FROM повторение_упражнения WHERE ид_выполнение_упражнения IN (
                                SELECT вуп.ид_выполнение_упражнения
                                FROM выполнение_упражнения вуп
                                JOIN выполнение_тренировки вт ON вт.ид_выполнение_тренировки = вуп.ид_выполнение_тренировки
                                WHERE вт.ид_тренировка = @trainId
                            )",
                            @"DELETE FROM выполнение_упражнения WHERE ид_выполнение_тренировки IN (
                                SELECT ид_выполнение_тренировки FROM выполнение_тренировки
                                WHERE ид_тренировка = @trainId
                            )",
                            @"DELETE FROM выполнение_тренировки WHERE ид_тренировка = @trainId",
                            @"DELETE FROM тренировки_пользователя WHERE ид_тренировка = @trainId",
                            @"DELETE FROM тренировка WHERE ид_тренировка = @trainId"
                        };

                                foreach (var sqlDel in trainDeleteCommands)
                                {
                                    using (SqlCommand cmdDel = new SqlCommand(sqlDel, connection, transaction))
                                    {
                                        cmdDel.Parameters.AddWithValue("@trainId", trainId);
                                        cmdDel.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }

                    transaction.Commit();
                    MessageBox.Show("Упражнение удалено.");
                    ListExer list = new ListExer(4, idUser);
                    list.Show();
                    this.Close();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка при удалении упражнения: " + ex.Message);
                }
            }
        }


    }
        }


