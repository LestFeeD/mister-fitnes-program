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
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Diagnostics;
using WpfAnimatedGif;
using System.IO;

namespace FitnesPlan
{
    /// <summary>
    /// Логика взаимодействия для DetailsExerInPlan.xaml
    /// </summary>
    public partial class DetailsExerInPlan : Window
    {
        private List<int> ExerciseIds = new List<int>();
        private int currentExerciseIndex = 0;

        private int idExer;
        private int idTrain;
        private int idUser;
        private int idExercisePerformance;

        private DispatcherTimer _timer;
        private Stopwatch _stopwatch;

        private ObservableCollection<string> sets = new ObservableCollection<string>();

        private DispatcherTimer restTimer;
        private DateTime restStartTime;
        private bool isResting = false;
        private TimeSpan currentRestDuration = TimeSpan.Zero;
        private List<TimeSpan> restDurations = new List<TimeSpan>();
        public DetailsExerInPlan(List<int> exerciseIds, int currentIndex, int idTrain, int idExercisePerformance, int idUser)
        {
            InitializeComponent();
            this.ExerciseIds = exerciseIds;
            this.currentExerciseIndex = currentIndex;
            this.idExer = exerciseIds[currentIndex];
            this.idTrain = idTrain;
            this.idUser = idUser;
            this.idExercisePerformance = idExercisePerformance;
            TrainPlanNov.DataContext = sets;

            UpdateRepeatCounter();

            restTimer = new DispatcherTimer();
            restTimer.Interval = TimeSpan.FromSeconds(1);
            restTimer.Tick += RestTimer_Tick;

            LoadExercise(idExer);
            LoadExerciseSets(idExer, idUser);

        }
    

        private void AddSetButton_Click(object sender, RoutedEventArgs e)
        {
            string kg = KgTextBox.Text;
            string reps = RepTextBox.Text;

            int kgInt = int.TryParse(KgTextBox.Text, out int resultKg) ? resultKg : 0;
            int repsInt = int.TryParse(RepTextBox.Text, out int resultReps) ? resultReps : 0;

            if (string.IsNullOrWhiteSpace(kg) || string.IsNullOrWhiteSpace(reps) || kgInt <= 0 || repsInt <= 0)
            {
                MessageBox.Show("Заполните оба поля или введите корректное значение.");
                return;
            }
            DateTime now = DateTime.Now;
            TimeSpan timeOnly = now.TimeOfDay;

            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();
                string query = "INSERT INTO повторение_упражнения (вес, повторение, время_повторения, ид_выполнение_упражнения) " +
                               "VALUES (@вес, @повторение, @время_повторения, @ид_выполнение)";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@вес", kg);
                    cmd.Parameters.AddWithValue("@повторение", reps);
                    cmd.Parameters.AddWithValue("@время_повторения", timeOnly);
                    cmd.Parameters.AddWithValue("@ид_выполнение", idExercisePerformance);

                    cmd.ExecuteNonQuery();
                }
                using (SqlCommand cmd = new SqlCommand("UPDATE выполнение_упражнения SET  дата_выполнения = @дата_выполнения WHERE ид_выполнение_упражнения = @ид", connection))
                {
                    cmd.Parameters.AddWithValue("@ид", idExercisePerformance);
                    cmd.Parameters.AddWithValue("@дата_выполнения", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }
            UpdateRepeatCounter();
            LoadExerciseSets(idExer, idUser);

        }
        private void StartRestButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isResting)
            {
                // старт отдыха
                restStartTime = DateTime.Now;
                currentRestDuration = TimeSpan.Zero;
                restTimer.Start();
                isResting = true;
            }
            else
            {
                // Пауза отдыха
                restTimer.Stop();
                currentRestDuration = DateTime.Now - restStartTime;

                // Сохраняем в коллекц
                restDurations.Add(currentRestDuration);
                isResting = false;

            }
        }

        private void RestTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsed = DateTime.Now - restStartTime;
            RestTimerText.Text = $"Отдых: {elapsed.ToString(@"hh\:mm\:ss")}";
        }
       
        private void LoadExercise(int idExer)
        {
            DataTable exerciseTable = new DataTable();

            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                string query = @"SELECT название, видео FROM упражнение WHERE ид_упражнение = @id";

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
                string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "videos", videoFile);

                if (File.Exists(fullPath))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(fullPath, UriKind.Absolute);
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();

                    ImageBehavior.SetAnimatedSource(ExerciseGif, image);
                }

                var data = new { Название = name };
                this.DataContext = data;
            }
        }
        private void LoadExerciseSets(int exerciseId, int userId)
        {
            var data = new Dictionary<string, List<string>>();

            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();

                string query = @"
          SELECT 
    CONVERT(VARCHAR, ву.дата_выполнения, 104) AS дата,
    п.вес + ' кг x ' + п.повторение + ' раз (' + CONVERT(VARCHAR(8), п.время_повторения, 108) + ')' AS подход
FROM выполнение_упражнения ву
LEFT JOIN повторение_упражнения п 
    ON п.ид_выполнение_упражнения = ву.ид_выполнение_упражнения
JOIN выполнение_тренировки вт 
    ON вт.ид_выполнение_тренировки = ву.ид_выполнение_тренировки
WHERE 
    ву.ид_упражнение = @ид_упражнение
    AND вт.ид_пользователь = @ид_пользователь
ORDER BY ву.дата_выполнения;
        ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ид_упражнение", exerciseId);
                cmd.Parameters.AddWithValue("@ид_пользователь", userId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string date = reader["дата"].ToString();
                        string setInfo = reader["подход"].ToString();

                        if (!data.ContainsKey(date))
                            data[date] = new List<string>();

                        if (!string.IsNullOrEmpty(setInfo)) 
                        {
                            data[date].Add(setInfo);
                        }
                    }
                }
            }

            var itemsSource = data.Where(d => d.Value.Any()) 
                                   .Select(d => new
                                   {
                                       Date = d.Key,
                                       Sets = d.Value
                                   })
                                   .Reverse()  
                                   .ToList();

            TrainPlanNov.ItemsSource = itemsSource;
        }



     
        private void Timer_Tick(object sender, EventArgs e)
        {
            TimerText.Text = _stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _stopwatch = new Stopwatch();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;

            _stopwatch.Start();
            _timer.Start();

        }
      
        public int GetRepetitionCount(int idExercisePerformance)
        {
            int count = 0;

            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                string query = "SELECT COUNT(*) FROM повторение_упражнения WHERE ид_выполнение_упражнения = @id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", idExercisePerformance);
                    connection.Open();
                    count = (int)command.ExecuteScalar();
                }
            }

            return count;
        }

        private void UpdateRepeatCounter()
        {
            int count = GetRepetitionCount(idExercisePerformance);
            RepeatCounterTextBlock.Text = $"Количество повторений: {count}";
        }

        private void ReadyExer_Click(object sender, RoutedEventArgs e)
        {
            _stopwatch.Stop();
            _timer.Stop();
            if (isResting)
            {
                restTimer.Stop();
                currentRestDuration = DateTime.Now - restStartTime;

                restDurations.Add(currentRestDuration);
                isResting = false;
            }
            TimeSpan duration = _stopwatch.Elapsed;
            string totalTime = duration.ToString(@"hh\:mm\:ss");

            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("UPDATE выполнение_упражнения SET количество_времени = @время, дата_выполнения = @дата_выполнения WHERE ид_выполнение_упражнения = @ид", conn);
                cmd.Parameters.AddWithValue("@время", totalTime);
                cmd.Parameters.AddWithValue("@ид", idExercisePerformance);
                cmd.Parameters.AddWithValue("@дата_выполнения", DateTime.Now);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Упражнение завершено, время записано!");

            if (currentExerciseIndex + 1 < ExerciseIds.Count)
            {
                int nextIndex = currentExerciseIndex + 1;
                int nextExerId = ExerciseIds[nextIndex];

                int nextExercisePerformanceId = GetNextExercisePerformanceId(nextExerId);

                var nextWindow = new DetailsExerInPlan(ExerciseIds, nextIndex, idTrain, nextExercisePerformanceId, idUser);
                nextWindow.Show();
                this.Close();
            }
            else
            {
                CompleteTraining();
                MessageBox.Show("Тренировка завершена!");
                this.Close();
            }
        }

        private int GetNextExercisePerformanceId(int idExer)
        {
            int newId = 0;

            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO выполнение_упражнения (ид_упражнение, ид_выполнение_тренировки) OUTPUT INSERTED.ид_выполнение_упражнения VALUES (@ид_упражнение, @ид_выполнение_тренировки)", conn);
                cmd.Parameters.AddWithValue("@ид_упражнение", idExer);
                cmd.Parameters.AddWithValue("@ид_выполнение_тренировки", idTrain);
                newId = (int)cmd.ExecuteScalar();
            }

            return newId;
        }

        private void CompleteTraining()
        {
            TimeSpan totalDuration = TimeSpan.Zero;
            TimeSpan totalRestTime = TimeSpan.Zero;
            TimeSpan avgRestTime = TimeSpan.Zero;

            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();

                string query = @"
            SELECT ид_выполнение_упражнения, количество_времени
            FROM выполнение_упражнения
            WHERE ид_выполнение_тренировки = @ид_выполнение_тренировки";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ид_выполнение_тренировки", idTrain);

                List<int> exercisePerformanceIds = new List<int>();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        totalDuration += (TimeSpan)reader["количество_времени"];

                        exercisePerformanceIds.Add((int)reader["ид_выполнение_упражнения"]);
                    }
                }


                if (restDurations.Count > 0) { 

                    avgRestTime = new TimeSpan((long)restDurations.Average(r => r.Ticks));
                totalRestTime = new TimeSpan(restDurations.Sum(r => r.Ticks));
            }

                SqlCommand updateCmd = new SqlCommand(@"
            UPDATE выполнение_тренировки
            SET средний_отдых = @avgRest, общие_время_тренировки = @totalTime 
            WHERE ид_выполнение_тренировки = @ид", conn);

                updateCmd.Parameters.AddWithValue("@avgRest", avgRestTime.ToString(@"hh\:mm\:ss"));
                updateCmd.Parameters.AddWithValue("@totalTime", totalDuration);
                updateCmd.Parameters.AddWithValue("@ид", idTrain);

                updateCmd.ExecuteNonQuery();
            }
            CompletedTraining completedTraining = new CompletedTraining(idUser, idTrain);
            completedTraining.Show();
            this.Close();
        }

         //if (restDurations.Count > 0)
         //       {
         //           avgRestTime = new TimeSpan((long) restDurations.Average(r => r.Ticks));
         //           totalRestTime = new TimeSpan(restDurations.Sum(r => r.Ticks));
         //       }

}
}
