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
using System.Collections.ObjectModel;
using System.Dynamic;
using FitnesPlan.Products;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Controls.Primitives;
using ControlzEx.Standard;
using ControlzEx.Theming;

namespace FitnesPlan
{
    /// <summary>
    /// Логика взаимодействия для Profile.xaml
    /// </summary>
    public partial class Profile : Window
    {
        public SeriesCollection SeriesCollection { get; set; }

        private int idUser;
        ObservableCollection<DateTime> trainingExecutionDates = new ObservableCollection<DateTime>();
        private bool _isProcessing = false;
        public SeriesCollection WeightSeries { get; set; }
        public List<string> DateLabels { get; set; }
        public Profile(int idUser)
        {
            InitializeComponent();
            this.idUser = idUser;
            DataContext = this;
            LoadUser();
            LoadUsersTrainings();
            LoadCalend();
            WeightSeries = new SeriesCollection();
            DateLabels = new List<string>();
            LoadChartData();
            LoadWeightData();
        }
        private void LoadCalend()
        {
            string query = "SELECT DISTINCT вт.дата_выполнения FROM выполнение_тренировки вт " +
                " JOIN тренировка т ON т.ид_тренировка  = вт.ид_тренировка " +
                "LEFT JOIN тренировки_пользователя тп ON тп.ид_тренировка = т.ид_тренировка " +
                "WHERE вт.дата_выполнения BETWEEN '2025-01-01' AND '2026-12-31' AND (вт.ид_пользователь = @idUser  )";

            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@idUser", idUser);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            trainingExecutionDates.Add(reader.GetDateTime(0)); // дата выполненной тренировки
                        }
                    }
                }

            }

            foreach (var date in trainingExecutionDates)
            {

                TaskCalendar.SelectedDates.Add(date);

            }
        }
        private void TaskCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isProcessing) return;

            _isProcessing = true;

            try
            {

                var selectedDates = TaskCalendar.SelectedDates.ToList();
                foreach (var date in trainingExecutionDates)
                {
                    if (!selectedDates.Contains(date))
                        selectedDates.Add(date);
                }

                TaskCalendar.SelectedDates.Clear();
                foreach (var date in selectedDates)
                {
                    TaskCalendar.SelectedDates.Add(date);
                }

            }
            finally
            {
                _isProcessing = false;
            }
        }
        private void TaskCalendar_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement fe && fe.DataContext is DateTime clickedDate)
            {
                ShowCompletedTrainingWindow(clickedDate.Date);
            }
        }
        private void LoadWeightData()
        {
            var values = new ChartValues<int>();

            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                string query = @"SELECT вес, дата_взвешивания 
                             FROM вес_пользователя 
                             WHERE ид_пользователь = @id 
                             ORDER BY дата_взвешивания";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idUser);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int weight = reader.GetInt32(0);
                            DateTime date = reader.GetDateTime(1);

                            values.Add(weight);
                            DateLabels.Add(date.ToShortDateString());
                        }
                    }
                }
            }

            WeightSeries.Add(new LineSeries
            {
                Title = "Вес",
                Values = values,
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 10,
                FontSize = 18,
                Fill = Brushes.Transparent,
                Stroke = Brushes.Gray

            });
        }

        private void ShowCompletedTrainingWindow(DateTime date)
        {
            if (date == DateTime.MinValue)
            {
                MessageBox.Show("Дата не передана или передана некорректная дата.");
            }

            if (date >= (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue.Value &&
     trainingExecutionDates.Contains(date.Date))
            {
                var window = new CompletedTrains(idUser, date.Date);
                window.Show();
                this.Close();
            }
            else
            {
            }
        }

        private void LoadUser()
        {
            string query = "SELECT почта FROM пользователь WHERE ид_пользователь = @idUser";
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
                            string name = reader["почта"].ToString();
                            txtFamil.Text = name;
                        }
                    }
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

        private void LoadUsersTrainings()
        {
            DataTable readyPlanTable = new DataTable();
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();
                string queryReady = @"SELECT гт.ид_пользователь, т.ид_тренировка, т.название 
                              FROM  тренировка т 
                              LEFT JOIN тренировки_пользователя гт ON т.ид_тренировка  = гт.ид_тренировка 
                              LEFT JOIN пользователь тт ON гт.ид_пользователь = тт.ид_пользователь
                              WHERE тт.ид_пользователь = @idUser OR т.ид_пользователь = @idUser";



                using (SqlCommand cmdReady = new SqlCommand(queryReady, connection))
                {
                    cmdReady.Parameters.AddWithValue("@idUser", idUser);

                    new SqlDataAdapter(cmdReady).Fill(readyPlanTable);
                }


                var items = new List<ExpandoObject>();
                foreach (DataRow row in readyPlanTable.Rows)
                {
                    dynamic item = new ExpandoObject();
                    item.TrainId = Convert.ToInt32(row["ид_тренировка"]);
                    item.NameTrain = row["название"].ToString();

                    items.Add(item);
                }



                TrainPlanUser.ItemsSource = items;
            }
        }

        private void HighlightCompletedDates(int userId, int trainingId)
        {
            List<DateTime> completedDates = new List<DateTime>();

            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
            SELECT дата_планирования 
            FROM тренировки_пользователя 
            WHERE ид_пользователь = @userId 
              AND ид_готовая_тренировка = @trainingId 
              AND дата_планирования < CAST(GETDATE() AS DATE)", conn);

                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@trainingId", trainingId);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    completedDates.Add(reader.GetDateTime(0).Date);
                }
            }

            TaskCalendar.SelectedDates.Clear();
            foreach (var date in completedDates)
            {
                TaskCalendar.SelectedDates.Add(date);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (LoadRoleUser() != "администратор")
            {

                FormNewPlanBut.Visibility = Visibility.Visible;
                ReadyTrainPlanBut.Visibility = Visibility.Visible;
                Setting.Visibility = Visibility.Visible;
                TrainPl.Visibility = Visibility.Visible;

            }
            else
            {
                FormNewPlanBut.Visibility = Visibility.Collapsed;
                ReadyTrainPlanBut.Visibility = Visibility.Collapsed;
                Setting.Visibility = Visibility.Collapsed;
                TrainPl.Visibility = Visibility.Collapsed;
                Calend.Visibility = Visibility.Collapsed;
                Graph.Visibility = Visibility.Collapsed;
                GraphFiz.Visibility = Visibility.Collapsed;
                TaskCalendar.Visibility = Visibility.Collapsed;
            }

            if (LoadRoleUser() == "администратор")
            {

                AddNewPlanBut.Visibility = Visibility.Visible;
            }
            else
            {
                AddNewPlanBut.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadChartData()
        {
            SeriesCollection = new SeriesCollection();

            using (var connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand(@"
      ;WITH уникальные_повторы AS (
    SELECT DISTINCT пу.ид_повторение_упражнения, u.ид_тип_упражнения
    FROM повторение_упражнения пу
    JOIN выполнение_упражнения ву 
        ON ву.ид_выполнение_упражнения = пу.ид_выполнение_упражнения
    JOIN выполнение_тренировки vu 
        ON vu.ид_выполнение_тренировки = ву.ид_выполнение_тренировки
    JOIN упражнение u 
        ON u.ид_упражнение = ву.ид_упражнение
    WHERE 
        vu.ид_пользователь = @userId 
        AND vu.дата_выполнения IS NOT NULL
)

SELECT 
    tu.наименование AS тип_упражнения,
    COUNT(DISTINCT uv.ид_повторение_упражнения) AS количество
FROM уникальные_повторы uv
JOIN тип_упражнения tu 
    ON tu.ид_тип_упражнения = uv.ид_тип_упражнения
GROUP BY tu.наименование;


", connection);
                command.Parameters.AddWithValue("@userId", idUser);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string type = reader.GetString(0);
                        int count = reader.GetInt32(1);

                        SeriesCollection.Add(new PieSeries
                        {
                            Title = type,
                            Values = new ChartValues<int> { count },
                            DataLabels = true,
                            FontSize = 18,
                            LabelPoint = chartPoint => $"{(int)chartPoint.Y} раз(а)"
                        });
                    }
                }
            }
        }

        private void AddNewPlanBut_Click(object sender, RoutedEventArgs e)
        {
            if (LoadRoleUser() == "администратор")
            {
                AddProducts create = new AddProducts(idUser);
                create.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Вы не администратор");
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
            if (LoadRoleUser() == "администратор")
            {
                MessageBox.Show("Вы не клиент");
            }
            else
            {
                ListTranPlan list = new ListTranPlan(idUser);
                list.Show();
                this.Close();
            }
        }

        private void FormNewPlanBut_Click(object sender, RoutedEventArgs e)
        {
            if (LoadRoleUser() == "администратор")
            {
                MessageBox.Show("Вы не клиент");
            }
            else
            {
                CreateTrainPersonal create = new CreateTrainPersonal(idUser);
                create.Show();
                this.Close();
            }
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

            var trainId = clickedButton.Tag;
            int idTrain = Convert.ToInt32(trainId);
            DetailsTrain detailsTrain = new DetailsTrain(idTrain, idUser);
            detailsTrain.Show();
            this.Close();
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            if (LoadRoleUser() == "администратор")
            {
                MessageBox.Show("Вы не клиент");
            }
            else
            {
                UserSettings user = new UserSettings(idUser);
                user.Show();
                this.Close();
            }
        }

        private void ExistAcc_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }

        //                var command = new SqlCommand(@"
        //        SELECT
        // tu.наименование AS тип_упражнения,
        // COUNT(*) AS количество
        //FROM выполнение_тренировки vu
        //JOIN тренировка tr ON vu.ид_тренировка = tr.ид_тренировка
        //JOIN упражнение_тренировки ut ON ut.ид_тренировка = tr.ид_тренировка
        //JOIN упражнение u ON u.ид_упражнение = ut.ид_упражнение
        //JOIN тип_упражнения tu ON tu.ид_тип_упражнения = u.ид_тип_упражнения


        //left JOIN  тренировки_пользователя tp ON tp.ид_тренировка = tr.ид_тренировка


        //WHERE

        // (vu.ид_пользователь = @userId)
        //    AND vu.дата_выполнения IS NOT NULL -- Фильтруем завершенные тренировки
        //GROUP BY tu.наименование;", connection);
        //                command.Parameters.AddWithValue("@userId", idUser);
    }
}
