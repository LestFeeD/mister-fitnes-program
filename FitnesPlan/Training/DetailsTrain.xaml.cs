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
using System.Collections;
using System.ComponentModel.Design;
using FitnesPlan.Products;

namespace FitnesPlan
{
    /// <summary>
    /// Логика взаимодействия для DetailsTrain.xaml
    /// </summary>
    public partial class DetailsTrain : Window
    {
        int idTrain;
        int idUser;
        private string lastName = string.Empty;  // Храним предыдущее значение для названия
        private string lastDescription = string.Empty;  // Храним предыдущее значение для описания
        public DetailsTrain(int idTrain, int idUser)
        {
            InitializeComponent();
            this.idTrain = idTrain;
            this.idUser = idUser;
            LoadExerTraining();
            UpdateTextBoxStates(idTrain);
        }

        private void BorderExerUserEm_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int typeWin = 2;
            if(LoadRoleUser() == "администратор")
            {
                typeWin = 6;
            }

            Border clickedButton = sender as Border;

            var detailsId = clickedButton.Tag;
            int idExers = Convert.ToInt32(detailsId);
            DetailsExersice detailsExersice = new DetailsExersice(idExers, idTrain, typeWin, idUser);
            detailsExersice.Show();
            this.Close();
        }

        private void LoadExerTraining()
        {
            DataTable tranPlanTable = new DataTable();
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();
                string query = @"SELECT гт.ид_упражнение , гт.название, гт.фото, тт.название as название_тренировки, тт.описание
            FROM упражнение гт
            JOIN упражнение_тренировки угт ON угт.ид_упражнение  = гт.ид_упражнение 
            JOIN тренировка тт ON угт.ид_тренировка   = тт.ид_тренировка  

            WHERE тт.ид_тренировка = @idTrain";


                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("idTrain", idTrain);


                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(tranPlanTable);
                    }
                }
            }
            var items = new List<dynamic>();
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

                items.Add(new
                {
                    idExer = row["ид_упражнение"],
                    NameExerTrain = row["название"].ToString(),
                    Image = image
                });

                string name = row["название_тренировки"].ToString();
                string desc = row["описание"].ToString();

                var data = new
                {
                    Название = name,
                    Описание = desc
                };

                this.DataContext = data;
            }


            

            TrainPlanNov.ItemsSource = items;
        }

        private void UpdateTextBoxStates(int trainingId)
        {
            bool isPersonal = IsPersonalTraining(trainingId);
            
            txtFamil.IsReadOnly = !isPersonal;
            txtDescTrain.IsReadOnly = !isPersonal;

            if(LoadRoleUser() == "администратор")
            {
                txtFamil.IsReadOnly = false;
                txtDescTrain.IsReadOnly = false;
            }
            else if (isPersonal)
            {
                txtFamil.IsReadOnly = false;
                txtDescTrain.IsReadOnly = false;
            }
            else
            {
                txtFamil.IsReadOnly = true;
                txtDescTrain.IsReadOnly = true;
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (LoadRoleUser() != "администратор")
            {
                ListExercise.Visibility = Visibility.Collapsed;
                AddNewExercise.Visibility = Visibility.Collapsed;
                AddNewPlanBut.Visibility = Visibility.Collapsed;
            }
            if (LoadRoleUser() == "администратор")
            {
                AddTrain.Visibility = Visibility.Visible;
                ProfileBut.Visibility = Visibility.Collapsed;
                GoTrainBut.Visibility = Visibility.Collapsed;

            }
            else if (!IsPersonalTraining(idTrain))
            {
                AddTrain.Visibility = Visibility.Collapsed;
               
            }
        }

        private void TxtFamil_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string currentName = txtFamil.Text.Trim();
                if (currentName != lastName)  // Если текст изменился
                {
                    lastName = currentName;  // Обновляем предыдущее значение
                    UpdateTrainingIfNeeded();  // Вызываем метод обновления
                }
            }
        }

        private void TxtDescTrain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string currentDescription = txtDescTrain.Text.Trim();
                if (currentDescription != lastDescription)  
                {
                    lastDescription = currentDescription; 
                    UpdateTrainingIfNeeded();  
                }
            }
        }

        private void UpdateTrainingIfNeeded()
        {
            string newName = txtFamil.Text.Trim();
            string newDescription = txtDescTrain.Text.Trim();

            if (newName != lastName || newDescription != lastDescription)
            {
                UpdateTraining(idTrain, newName, newDescription);
            }
        }

        private void UpdateTraining(int trainingId, string name, string description)
        {
            string query = @"
    UPDATE тренировка
    SET название = @название, описание = @описание
    WHERE ид_тренировка = @ид_тренировка";

            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@название", name);
                    cmd.Parameters.AddWithValue("@описание", description);
                    cmd.Parameters.AddWithValue("@ид_тренировка", trainingId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void AddTrain_Click(object sender, RoutedEventArgs e)
        {
     
                    ListExer listExer = new ListExer(idTrain, idUser, 7);
                    listExer.Show();
                    this.Close();
                
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

        private void DeleteTrain_Click(object sender, RoutedEventArgs e)
        {
            int userId = idUser;
            int trainingId = idTrain;

            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    if (LoadRoleUser() == "администратор")
                    {
                        string deleteRepeat = @"
                DELETE FROM повторение_упражнения
                WHERE ид_выполнение_упражнения IN (
                    SELECT вуп.ид_выполнение_упражнения
                    FROM выполнение_упражнения вуп
                    JOIN выполнение_тренировки вт ON вт.ид_выполнение_тренировки = вуп.ид_выполнение_тренировки
                    WHERE вт.ид_тренировка = @trainingId
                )";

                        string deleteExerciseExecution = @"
                DELETE FROM выполнение_упражнения
                WHERE ид_выполнение_тренировки IN (
                    SELECT ид_выполнение_тренировки
                    FROM выполнение_тренировки
                    WHERE ид_тренировка = @trainingId
                )";

                        string deleteExecution = "DELETE FROM выполнение_тренировки WHERE ид_тренировка = @trainingId";

                        string deleteExerciseLink = "DELETE FROM упражнение_тренировки WHERE ид_тренировка = @trainingId";

                        string deleteTrainingUserLink = "DELETE FROM тренировки_пользователя WHERE ид_тренировка = @trainingId";

                        string deleteTraining = "DELETE FROM тренировка WHERE ид_тренировка = @trainingId";

                        var commands = new List<SqlCommand>
                {
                    new SqlCommand(deleteRepeat, connection, transaction),
                    new SqlCommand(deleteExerciseExecution, connection, transaction),
                    new SqlCommand(deleteExecution, connection, transaction),
                    new SqlCommand(deleteExerciseLink, connection, transaction),
                    new SqlCommand(deleteTrainingUserLink, connection, transaction),
                    new SqlCommand(deleteTraining, connection, transaction)
                };

                        foreach (var cmd in commands)
                        {
                            cmd.Parameters.AddWithValue("@trainingId", trainingId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        txtFamil.Clear();
                        txtDescTrain.Clear();
                        App.SelectedExercises.Clear();
                        LoadExerTraining();
                        MessageBox.Show("Тренировка и все связанные данные успешно удалены.");
                    }
                    else
                    {
                        if (!IsPersonalTraining(trainingId))
                        {
                            string deleteFromUserTrainings = "DELETE FROM тренировки_пользователя WHERE ид_пользователь = @userId AND ид_тренировка = @trainingId";
                            using (SqlCommand cmd = new SqlCommand(deleteFromUserTrainings, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@userId", userId);
                                cmd.Parameters.AddWithValue("@trainingId", trainingId);

                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    transaction.Commit();
                                    MessageBox.Show("Тренировка удалена.");
                                }
                                else
                                {
                                    transaction.Rollback();
                                    MessageBox.Show("Тренировка не найдена или уже удалена.");
                                }
                            }
                        }
                        else
                        {
                            string deleteRepeat = @"
                    DELETE FROM повторение_упражнения
                    WHERE ид_выполнение_упражнения IN (
                        SELECT вуп.ид_выполнение_упражнения
                        FROM выполнение_упражнения вуп
                        JOIN выполнение_тренировки вт ON вт.ид_выполнение_тренировки = вуп.ид_выполнение_тренировки
                        WHERE вт.ид_тренировка = @trainingId
                    )";

                            string deleteExerciseExecution = @"
                    DELETE FROM выполнение_упражнения
                    WHERE ид_выполнение_тренировки IN (
                        SELECT ид_выполнение_тренировки
                        FROM выполнение_тренировки
                        WHERE ид_тренировка = @trainingId
                    )";

                            string deleteExecution = "DELETE FROM выполнение_тренировки WHERE ид_тренировка = @trainingId";

                            string deleteExerciseLink = "DELETE FROM упражнение_тренировки WHERE ид_тренировка = @trainingId";

                            string deleteTraining = "DELETE FROM тренировка WHERE ид_пользователь = @userId AND ид_тренировка = @trainingId";

                            var commands = new List<SqlCommand>
                {
                    new SqlCommand(deleteRepeat, connection, transaction),
                    new SqlCommand(deleteExerciseExecution, connection, transaction),
                    new SqlCommand(deleteExecution, connection, transaction),
                    new SqlCommand(deleteExerciseLink, connection, transaction),
                    new SqlCommand(deleteTraining, connection, transaction)
                };

                            foreach (var cmd in commands)
                            {
                                cmd.Parameters.AddWithValue("@trainingId", trainingId);
                                if (cmd.CommandText.Contains("@userId"))
                                    cmd.Parameters.AddWithValue("@userId", userId);

                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            txtFamil.Clear();
                            txtDescTrain.Clear();
                            App.SelectedExercises.Clear();
                            LoadExerTraining();
                            MessageBox.Show("Тренировка удалена.");
                        }
                    }
                }

                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка при удалении тренировки: " + ex.Message);
                }
            }
        }




        bool IsPersonalTraining(int trainingId)
        {
            string query = "SELECT COUNT(*) FROM тренировка WHERE ид_тренировка  = @id AND ид_тип_тренировки = 1";
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", trainingId);
                    int count = (int)cmd.ExecuteScalar();

                    return count > 0;
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

        private void GoTrainBut_Click(object sender, RoutedEventArgs e)
        {
            int newTrainId = 0;
           
            if (!ExistPerfTrainUser(idUser, idTrain) && !ExistIndivTrainUser(idUser, idTrain))
            {
                MessageBox.Show("Тренировка не добавлена");
            } else if(!ExistExerciseInTrain())
            {
                MessageBox.Show("У данной тренировки нет упражнений");
            }
           
            else
            {

                using (SqlConnection connection = DbConnectionHelper.GetConnection())
                {
                    connection.Open();

                    string query = "INSERT INTO выполнение_тренировки (ид_тренировка,  ид_пользователь  ) VALUES ( @train, @user); SELECT SCOPE_IDENTITY();";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@train", idTrain);
                        command.Parameters.AddWithValue("@user", idUser);


                        object result = command.ExecuteScalar();
                        newTrainId = Convert.ToInt32(result);
                    }


                    string queryEx = @"SELECT ид_упражнение FROM упражнение_тренировки WHERE ид_тренировка = @idTrain";
                    List<int> exerciseIds = new List<int>();
                    using (SqlCommand cmd = new SqlCommand(queryEx, connection))
                    {
                        cmd.Parameters.AddWithValue("@idTrain", idTrain);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                exerciseIds.Add(reader.GetInt32(0));
                            }
                        }
                    }
                    if (exerciseIds.Count > 0)
                    {
                        DetailsExerInPlan details = new DetailsExerInPlan(
                            exerciseIds,
                            0,            
                            newTrainId,    
                            GetExercisePerformanceId(newTrainId),       
                            idUser          
                        );
                        details.Show();
                        this.Close();
                    }

                }
            }
        }

        private int FindExercise()
        {
            int newId = 0;

            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                string query = "SELECT TOP 1 у.ид_упражнение  FROM упражнение у JOIN упражнение_тренировки ут ON ут.ид_упражнение = у.ид_упражнение " +
                    " WHERE  ут.ид_тренировка = @ид_тренировка ";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ид_тренировка", idTrain);
                newId = (int)cmd.ExecuteScalar();

            }
            return newId;
        }

            private int GetExercisePerformanceId(int idTrain)
        {
            int newId = 0;

            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO выполнение_упражнения (ид_упражнение, ид_выполнение_тренировки) OUTPUT INSERTED.ид_выполнение_упражнения VALUES (@ид_упражнение, @ид_выполнение_тренировки)", conn);
                cmd.Parameters.AddWithValue("@ид_упражнение", FindExercise());
                cmd.Parameters.AddWithValue("@ид_выполнение_тренировки", idTrain);
                newId = (int)cmd.ExecuteScalar();
            }

            return newId;
        }

        private bool ExistExerciseInTrain()
        {
            int? count = FindExercise();
            if (count == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private bool ExistPerfTrainUser(int idUser, int idTrain)
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

        private bool ExistIndivTrainUser(int idUser, int idTrain)
        {
            int count;
            string query = "SELECT COUNT(*) FROM тренировка WHERE ид_пользователь = @ид_пользователь AND ид_тренировка = @ид_тренировка ";
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
        private void AddNewPlanBut_Click(object sender, RoutedEventArgs e)
        {
            AddProducts create = new AddProducts(idUser);
            create.Show();
            this.Close();
        }


    }



    //        int userId = idUser;
    //        int trainingId = idTrain;

    //            using (SqlConnection connection = new SqlConnection(connectionString))
    //            {
    //                string query = "INSERT INTO тренировки_пользователя (ид_пользователь, ид_готовая_тренировка, дата_планирования) VALUES (@userId, @trainingId, @planningDate)";
    //    SqlCommand command = new SqlCommand(query, connection);
    //    command.Parameters.AddWithValue("@userId", userId);
    //                command.Parameters.AddWithValue("@trainingId", trainingId);
    //                command.Parameters.AddWithValue("@planningDate", null);

    //                connection.Open();
    //                int rows = command.ExecuteNonQuery();

    //                if (rows > 0)
    //                {
    //                    MessageBox.Show("Тренировка добавлена.");
    //                }
    //                else
    //{
    //    MessageBox.Show("Ошибка при добавлении.");
    //}
    //            }
}
        
    
