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
using FitnesPlan.Products;

namespace FitnesPlan
{
    /// <summary>
    /// Логика взаимодействия для ListExer.xaml
    /// </summary>
    public partial class ListExer : Window
    {
        private int idTrain = 0;
        private int idUser;
        private List<dynamic> allExercises = new List<dynamic>();
        private HashSet<int> selectedTypeIds = new HashSet<int>();

        private int typeWin = 0;
        public ListExer(int idTrain, int idUser,  int typeWin)
        {
            InitializeComponent();
            this.idTrain = idTrain;
            this.idUser = idUser;
            this.typeWin = typeWin;
            LoadGroupExerTrainings();
            LoadExerTrainings();
        }
        public ListExer(int typeWin, int idUser)
        {
            InitializeComponent();
            this.idUser = idUser;
            this.typeWin = typeWin;
            LoadExerTrainings();
            LoadGroupExerTrainings();
        }

        private void LoadGroupExerTrainings()
        {
            DataTable tranPlanTable = new DataTable();
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();
                string query = @"SELECT гт.ид_тип_упражнения   , гт.наименование 
            FROM тип_упражнения гт";


                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(tranPlanTable);
                }
            }
            var items = new List<dynamic>();
            foreach (DataRow row in tranPlanTable.Rows)
            {
              
                items.Add(new
                {
                    IdExer = row["ид_тип_упражнения"],
                    NameExerGroup = row["наименование"].ToString(),
                });
            }

            AllExer.ItemsSource = items;
        }

        private void LoadExerTrainings(string nameFilter = "", List<int> idTypeExerList = null)
        {
            DataTable tranPlanTable = new DataTable();
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();

                string query = @"
            SELECT у.ид_упражнение, у.название, у.фото
            FROM упражнение у
            LEFT JOIN тип_упражнения ту ON ту.ид_тип_упражнения = у.ид_тип_упражнения
            WHERE 1 = 1";

                if (!string.IsNullOrWhiteSpace(nameFilter))
                {
                    query += " AND LOWER(у.название) LIKE @nameFilter";
                }

                if (idTypeExerList != null && idTypeExerList.Count > 0)
                {
                    string ids = string.Join(",", idTypeExerList);
                    query += $" AND у.ид_тип_упражнения IN ({ids})";
                }

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    if (!string.IsNullOrWhiteSpace(nameFilter))
                    {
                        cmd.Parameters.AddWithValue("@nameFilter", $"%{nameFilter.ToLower()}%");
                    }

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
                    IdExer = row["ид_упражнение"],
                    NameExer = row["название"].ToString(),
                    Image = image,
                });
            }

            ExerPlanNov.ItemsSource = items;
        }

        private void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadExerTrainings(Filter.Text.Trim(), selectedTypeIds.ToList());

            watermarkUsername.Visibility = string.IsNullOrWhiteSpace(Filter.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            watermarkUsername.Visibility = string.IsNullOrWhiteSpace(Filter.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void txtUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            watermarkUsername.Visibility = string.IsNullOrWhiteSpace(Filter.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void txtUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            watermarkUsername.Visibility = string.IsNullOrWhiteSpace(Filter.Text) ? Visibility.Visible : Visibility.Collapsed;

        }

        private void SpecExer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border clickedBorder = sender as Border;
            int productId = (int)clickedBorder.Tag;
            if (idTrain > 0 && typeWin == 2)
            {
                DetailsExersice details = new DetailsExersice(productId, idTrain, typeWin, idUser);
                details.Show();
                this.Close();
            }
            else if (idTrain > 0 && typeWin == 7)
            {
                DetailsExersice details = new DetailsExersice(productId, idTrain, typeWin, idUser);
                details.Show();
                this.Close();
            }
            else if (typeWin == 1)
            {
                DetailsExersice details = new DetailsExersice(productId, typeWin, idUser);
                details.Show();
                this.Close();
            }
            else if (typeWin == 3)
            {
                DetailsExersice details = new DetailsExersice(productId, typeWin, idUser);
                details.Show();
                this.Close();
            }
            else if (typeWin == 4)
            {
                EditExercise details = new EditExercise(productId, idUser);
                details.Show();
                this.Close();
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

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Border clickedBorder = sender as Border;
            if (clickedBorder != null)
            {
                int typeExerciseId = (int)clickedBorder.Tag;

                if (selectedTypeIds.Contains(typeExerciseId))
                {
                    selectedTypeIds.Remove(typeExerciseId); 
                    clickedBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAF0E6"));
                }
                else
                {
                    selectedTypeIds.Add(typeExerciseId); 
                    clickedBorder.Background = new SolidColorBrush(Color.FromRgb(255, 220, 180)); 
                }

                LoadExerTrainings(Filter.Text.Trim(), selectedTypeIds.ToList());
            }
        }

        private void TransitionBut_Click(object sender, RoutedEventArgs e)
        {
          
             if (typeWin == 1)
            {
                CreateTrainPersonal createTrainPersonal = new CreateTrainPersonal(idUser);
                createTrainPersonal.Show();
                this.Close();
            } else if(typeWin == 4)
            {
                AddExerExercise guideProducts = new AddExerExercise(idUser);
                guideProducts.Show();
                this.Close();
            }
            
            else if (typeWin == 2)

            {
                DetailsTrain detailsTrain = new DetailsTrain(idTrain, idUser); 
                detailsTrain.Show();
                this.Close();
            }
            else if (typeWin == 7)

            {
                DetailsTrain detailsTrain = new DetailsTrain(idTrain, idUser);
                detailsTrain.Show();
                this.Close();
            }
        }


        private void FormNewPlanBut_Click(object sender, RoutedEventArgs e)
        {
            AddProducts create = new AddProducts(idUser);
            create.Show();
            this.Close();
        }
    }
}
