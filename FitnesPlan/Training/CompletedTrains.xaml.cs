using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
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
    /// Логика взаимодействия для CompletedTrains.xaml
    /// </summary>
    public partial class CompletedTrains : Window
    {
        private DateTime trainingDate;
        private int idUser;
        public CompletedTrains(int idUser, DateTime trainingDate)
        {
            InitializeComponent();
            this.idUser = idUser;
            this.trainingDate = trainingDate;
            LoadTrainingName();
        }

        private void LoadTrainingName()
        {
            var trainingList = new List<object>();

            string query = @"
        SELECT t.название, vt.ид_выполнение_тренировки 
        FROM выполнение_тренировки vt
        JOIN тренировка t ON vt.ид_тренировка = t.ид_тренировка
        WHERE vt.дата_выполнения = @date AND vt.ид_пользователь = @userId";
            int count = 1;
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@date", trainingDate.Date);
                command.Parameters.AddWithValue("@userId", idUser);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        int idTran = reader.GetInt32(1);
                        trainingList.Add(new { Name = name, IdTran = idTran, NumberText = $"Тренировка {count}"
                    });
                        count++;
                    }
                }
            }
            AllExer.ItemsSource = trainingList;
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Border clickedBorder = sender as Border;
            int typeExerciseId = (int)clickedBorder.Tag;

            CompletedTraining listSpecExer = new CompletedTraining(idUser, typeExerciseId);
            listSpecExer.Show();
            this.Close();


        }

        private void TransitionBut_Click(object sender, RoutedEventArgs e)
        {
            Profile profile = new Profile(idUser);
            profile.Show();
            this.Close();

        }


    }
}

