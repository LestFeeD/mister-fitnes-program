using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace FitnesPlan
{
    /// <summary>
    /// Логика взаимодействия для DetailsExersice.xaml
    /// </summary>
    public partial class DetailsExersice : Window
    {
        private int idExer;
        private int idTrain;
        private int typeWin;
        private int idUser;
        private int idTypeExer;
        private bool isTube = false;
        private bool isCharacteristicsVisible = false;

        public DetailsExersice(int idExer, int idTrain, int typeWin, int idUser)
        {
            InitializeComponent();
            this.idExer = idExer;
            this.idTrain = idTrain;
            this.typeWin = typeWin;
            this.idUser = idUser;
            LoadExercise( idExer);
        }
    
        public DetailsExersice(int idExer, int typeWin, int idUser)
        {
            InitializeComponent();
            this.idExer = idExer;
            this.idUser = idUser;
            this.typeWin =typeWin;
            LoadExercise(idExer);

        }


        private void LoadExercise(int idExer)
        {
            DataTable exerciseTable = new DataTable();

            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                string query = @"SELECT 
                            название, 
                            видео, 
                            калорийность,
                            сложность_упражнения.наименование AS Сложность,
                            ссылка_техники
                         FROM упражнение
                         JOIN сложность_упражнения 
                             ON упражнение.ид_сложность_упражнения = сложность_упражнения.ид_сложность_упражнения
                         WHERE ид_упражнение = @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idExer);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(exerciseTable);
                    }
                }
            }

            if (exerciseTable.Rows.Count > 0)
            {
                var row = exerciseTable.Rows[0];

                string name = row["название"].ToString();
                string videoFile = row["видео"].ToString();
                int calories = Convert.ToInt32(row["калорийность"]);
                string difficulty = row["Сложность"].ToString();
                string videoLink = row["ссылка_техники"].ToString();


                string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "videos", videoFile);

                var data = new
                {
                    Название = name,
                    Видео = new Uri(fullPath, UriKind.Absolute),
                    Калорийность = calories,
                    Сложность = difficulty,
                    rutubeView = videoLink

                };

                this.DataContext = data;
                if (System.IO.Path.GetExtension(videoFile).ToLower() == ".gif")
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(fullPath, UriKind.Absolute);
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();

                    ImageBehavior.SetAnimatedSource(ExerciseGif, image);
                    ImageBehavior.SetRepeatBehavior(ExerciseGif, System.Windows.Media.Animation.RepeatBehavior.Forever);
                }
            }
        }

        private  void  Window_Loaded(object sender, RoutedEventArgs e)
        {

            if (idTrain > 0)
            {
                if(LoadRoleUser() == "администратор")
                {
                    AddExercise.Visibility = Visibility.Visible;

                }
                 else if (VerifyReadfyTrainPlan(idTrain) || typeWin == 6)
                {
                    AddExercise.Visibility = Visibility.Collapsed;
                }

                if (LoadRoleUser() == "администратор")
                {
                    DeleteExercise.Visibility = Visibility.Visible;

                }
                else if (VerifyReadfyTrainPlan(idTrain) || typeWin == 6)
                {
                    DeleteExercise.Visibility = Visibility.Collapsed;
                }
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
        private void AddExercise_Click(object sender, RoutedEventArgs e)
            {
                SqlConnection connection = DbConnectionHelper.GetConnection();
           
                if (idTrain > 0) {
                if (LoadRoleUser() == "администратор")
                {
                    if (!ExistTraining(idTrain))
                    {
                        AddExerciseToTraining(idTrain, idExer);
                        MessageBox.Show("Упражнение добавлено в тренировку");
                    }
                    else
                    {
                        MessageBox.Show("Упражнение уже добавлено в тренировку");
                    }
                }
                else
                {

                    if (!ExistPersonalTraining(idTrain))
                    {
                        AddExerciseToTraining(idTrain, idExer);
                        MessageBox.Show("Упражнение добавлено в тренировку");
                    }
                    else
                    {
                        MessageBox.Show("Упражнение уже добавлено в тренировку");
                    }
                }
                } else
                {

                    if (!App.SelectedExercises.Contains(idExer) && !ExistPersonalTraining(idTrain))
                    {
                        MessageBox.Show("Упражнение добавлено в тренировку");
                        App.SelectedExercises.Add(idExer);
                    }
                }
            }
        private bool AddExerciseToTraining(int idTrain, int idExer)
        {
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(
                        "INSERT INTO упражнение_тренировки (ид_тренировка, ид_упражнение) VALUES (@idTrain, @idExer)",
                        connection);
                    cmd.Parameters.AddWithValue("@idTrain", idTrain);
                    cmd.Parameters.AddWithValue("@idExer", idExer);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Ошибка при добавлении упражнения в тренировку: " + ex.Message);
                    return false;
                }
            }
        }

        private void DeleteExercise_Click(object sender, RoutedEventArgs e)
        {
            SqlConnection connection = DbConnectionHelper.GetConnection();
            if (idTrain > 0)
            {
            if(LoadRoleUser() == "администратор")
                {
                    if (!ExistTraining(idTrain))
                    {
                        MessageBox.Show("Упражнение не добавлено в тренировку");

                    }
                    else if (!LeftTraining(idTrain))
                    {
                        MessageBox.Show("Упражнение удалено из  тренировки");
                        DeleteExerciseFromTraining(idTrain, idExer);

                    }
                    else
                    {
                        MessageBox.Show("Минимальное количество упражнений в тренировке: 1.");
                    }
                }
                else if (!ExistPersonalTraining(idTrain))
                {
                    MessageBox.Show("Упражнение не добавлено в тренировку");

                }
                else if(!LeftPersonalTraining(idTrain))
                {
                    MessageBox.Show("Упражнение удалено из  тренировки");
                    DeleteExerciseFromTraining(idTrain, idExer);

                } else
                {
                    MessageBox.Show("Минимальное количество упражнений в тренировке: 1.");
                }
               
            } else
            {
                if (App.SelectedExercises.Contains(idExer))
                {
                    MessageBox.Show("Упражнение удалено из создание тренировки");
                    App.SelectedExercises.Remove(idExer);
                } else
                {
                    MessageBox.Show("Упражнение не в списке создание тренировки");

                }
            }
        }

        private bool DeleteExerciseFromTraining(int idTrain, int idExer)
        {
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(
                        "DELETE FROM упражнение_тренировки WHERE ид_тренировка = @idTrain AND ид_упражнение = @idExer",
                        connection);
                    cmd.Parameters.AddWithValue("@idTrain", idTrain);
                    cmd.Parameters.AddWithValue("@idExer", idExer);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении упражнения из тренировки: " + ex.Message);
                    return false;
                }
            }
        }

      
        bool ExistPersonalTraining(int trainingId)
        {
            using (SqlConnection conn = DbConnectionHelper.GetConnection()) {
                conn.Open();
                string query = "SELECT COUNT(*) FROM тренировка пт " +
                "JOIN упражнение_тренировки упт ON упт.ид_тренировка   = пт.ид_тренировка   " +
                "JOIN упражнение у ON у.ид_упражнение  = упт.ид_упражнение  " +
                "WHERE пт.ид_тренировка   = @id AND  у.ид_упражнение = @idExersice AND пт.ид_тип_тренировки = 1";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", trainingId);
                cmd.Parameters.AddWithValue("@idExersice", idExer);

                int count = (int)cmd.ExecuteScalar();

                return count > 0;
            }
        }

            }

        bool ExistTraining(int trainingId)
        {
            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM тренировка пт " +
                "JOIN упражнение_тренировки упт ON упт.ид_тренировка   = пт.ид_тренировка   " +
                "JOIN упражнение у ON у.ид_упражнение  = упт.ид_упражнение  " +
                "WHERE пт.ид_тренировка   = @id AND  у.ид_упражнение = @idExersice AND пт.ид_тип_тренировки = 2";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", trainingId);
                    cmd.Parameters.AddWithValue("@idExersice", idExer);

                    int count = (int)cmd.ExecuteScalar();

                    return count > 0;
                }
            }

        }

        private bool VerifyReadfyTrainPlan(int trainingId)
        {
            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM тренировка пт " +
               " JOIN тренировки_пользователя тп ON тп.ид_тренировка = пт.ид_тренировка " +
                " WHERE тп.ид_тренировка   = @id AND  тп.ид_пользователь  = @idUser ";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", trainingId);
                    cmd.Parameters.AddWithValue("@idUser", idUser);

                    int count = (int)cmd.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        bool LeftPersonalTraining(int idTrain)
        {
            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                string query = " SELECT COUNT(*)   FROM упражнение_тренировки упт INNER JOIN тренировка т ON т.ид_тренировка = упт.ид_тренировка WHERE т.ид_тренировка = @idTrain AND т.ид_тип_тренировки = 1";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idTrain", idTrain);

                    int count = (int)cmd.ExecuteScalar();

                    return count <= 1;
                }
            }
        }

        bool LeftTraining(int idTrain)
        {
            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                string query = " SELECT COUNT(*)   FROM упражнение_тренировки упт INNER JOIN тренировка т ON т.ид_тренировка = упт.ид_тренировка WHERE т.ид_тренировка = @idTrain AND т.ид_тип_тренировки = 2";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idTrain", idTrain);

                    int count = (int)cmd.ExecuteScalar();

                    return count <= 1;
                }
            }
        }
        private void TransitionBut_Click(object sender, RoutedEventArgs e)
        {

            if (typeWin == 2)
            {
                DetailsTrain createTrainPersonal = new DetailsTrain(idTrain, idUser);
                createTrainPersonal.Show();
                this.Close();
            } else if(typeWin == 1) {
                ListExer createTrainPersonal = new ListExer( typeWin, idUser);
                createTrainPersonal.Show();
                this.Close();

            } else if (typeWin == 6)
            {
                DetailsTrain details = new DetailsTrain(idTrain, idUser);
                details.Show();
                this.Close();
            }
            else if (typeWin == 7)
            {
                ListExer details = new ListExer(idTrain, idUser, typeWin);
                details.Show();
                this.Close();
            }
            else if(typeWin == 5)
            {
                GuideProducts guideProducts = new GuideProducts(idUser);
                guideProducts.Show();
                this.Close();
            }
          
            else if(typeWin == 0)
            {
                CreateTrainPersonal createTrainPersonal = new CreateTrainPersonal(idUser);
                createTrainPersonal.Show();
                this.Close();
            }

        }

        private async void OpenStackTube_Click(object sender, RoutedEventArgs e)
        {
            if (isTube)
            {
                ComplExer.Visibility = Visibility.Collapsed;
                ArrowRotateCharacteristics.Angle = 0;
            }
            else
            {
                ComplExer.Visibility = Visibility.Visible;
                ArrowRotateCharacteristics.Angle = 90;

                var data = this.DataContext as dynamic;
                string videoUrl = data?.rutubeView;

                if (!string.IsNullOrWhiteSpace(videoUrl) && Uri.IsWellFormedUriString(videoUrl, UriKind.Absolute))
                {
                    try
                    {
                        await rutubeView.EnsureCoreWebView2Async();
                        rutubeView.Source = new Uri(videoUrl);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ошибка загрузки видео: " + ex.Message);
                        rutubeView.Source = null;
                    }
                }
                else
                {
                    rutubeView.Source = null;
                }
            }

            isTube = !isTube;
        }


        private void OpenStackChar_Click(object sender, RoutedEventArgs e)
        {
            if (isCharacteristicsVisible)
            {
                ComplChar.Visibility = Visibility.Collapsed;
                ArrowRotateCharacteristic.Angle = 0;
            }
            else
            {
                ComplChar.Visibility = Visibility.Visible;
                ArrowRotateCharacteristic.Angle = 90;
            }
            isCharacteristicsVisible = !isCharacteristicsVisible;
        }
    }
}
