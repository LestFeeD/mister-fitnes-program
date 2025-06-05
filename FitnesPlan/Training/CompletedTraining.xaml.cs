using System;
using System.Collections;
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
using System.Dynamic;

namespace FitnesPlan
{
    /// <summary>
    /// Логика взаимодействия для CompletedTraining.xaml
    /// </summary>
    public partial class CompletedTraining : Window
    {
        private int idUser;
        private int idTrain;

        public CompletedTraining(int idUser, int idTrain)
        {
            InitializeComponent();
            this.idUser = idUser;
            this.idTrain = idTrain;
            LoadTrainingCompletedExercise();
        }

        private void LoadTrainingCompletedExercise()
        {
            List<ExpandoObject> exercises = new List<ExpandoObject>();
            string query = @"
SELECT 

    упр.название AS NameExer,
    повт.вес as вес, 
    повт.повторение as повторение, 
    вып.ид_выполнение_упражнения
FROM выполнение_упражнения вып
JOIN упражнение упр ON вып.ид_упражнение = упр.ид_упражнение
LEFT JOIN повторение_упражнения повт ON повт.ид_выполнение_упражнения = вып.ид_выполнение_упражнения
WHERE вып.ид_выполнение_тренировки = @trainId
ORDER BY вып.ид_выполнение_упражнения;
";

            string queryTrain = @"
SELECT т.название, вт.средний_отдых, вт.общие_время_тренировки  FROM тренировка т JOIN выполнение_тренировки вт ON т.ид_тренировка    = вт.ид_тренировка    WHERE вт.ид_выполнение_тренировки = @idTrainVip";
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@trainId", idTrain);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dynamic exercise = new ExpandoObject();
                        exercise.NameExer = reader["NameExer"].ToString();
                        exercise.вес = reader["вес"] == DBNull.Value ? "0" : reader["вес"].ToString();
                        exercise.повторение = reader["повторение"] == DBNull.Value ? "0" : reader["повторение"].ToString();
                        exercise.ид_выполнение_упражнения = Convert.ToInt32(reader["ид_выполнение_упражнения"]);

                        exercises.Add(exercise);
                    }
                }

                SqlCommand com = new SqlCommand(queryTrain, connection);
                com.Parameters.AddWithValue("@idTrainVip", idTrain);

                using (SqlDataReader reader = com.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string name = reader["название"].ToString();
                        string avgTime = reader["средний_отдых"].ToString();
                        string timePerform = reader["общие_время_тренировки"].ToString();

                        txtNameTrain.Text = name;
                        txtTimeTrain.Text = timePerform;
                        txtAvgTime.Text = avgTime;
                    }
                }
            }

            ExercisesIC.ItemsSource = exercises;
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
    }
}
