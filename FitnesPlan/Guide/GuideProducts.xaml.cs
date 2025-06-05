using FitnesPlan.Guide;
using FitnesPlan.Products;
using System;
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
    /// Логика взаимодействия для GuideProducts.xaml
    /// </summary>
    public partial class GuideProducts : Window
    {
        private int idUser;
        private int idProduct;
        private HashSet<int> selectedTypeProductIds = new HashSet<int>();

        public GuideProducts(int idUser)
        {
            InitializeComponent();
            this.idUser = idUser;
            LoadTypeProducts();
            LoadProduct();
        }

        private void LoadTypeProducts()
        {
            var typeProduct = new List<dynamic>();

            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT ид_тип_питания , наименование FROM тип_питания", conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    typeProduct.Add(new {
                        NameTypeProduct = reader.GetString(1),
                        TypeProductId = reader.GetInt32(0)
                    });
                }
            }

            TrainPlanNov.ItemsSource = typeProduct;
        }
        private void LoadProduct(string nameFilter = "", List<int> idTypeProducts = null)
        {
            DataTable tranPlanTable = new DataTable();
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();
                string query = @"
        SELECT гт.ид_питание, гт.название, гт.описание, гт.фото
        FROM питание гт
        LEFT JOIN тип_питания тт ON гт.ид_тип_питания = тт.ид_тип_питания
        WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(nameFilter))
                {
                    query += " AND LOWER(гт.название) LIKE @nameFilter";
                }

                if (idTypeProducts != null && idTypeProducts.Count > 0)
                {
                    string paramList = string.Join(",", idTypeProducts.Select((id, i) => $"@id{i}"));
                    query += $" AND гт.ид_тип_питания IN ({paramList})";
                }

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    if (!string.IsNullOrWhiteSpace(nameFilter))
                    {
                        cmd.Parameters.AddWithValue("@nameFilter", $"%{nameFilter.ToLower()}%");
                    }

                    if (idTypeProducts != null && idTypeProducts.Count > 0)
                    {
                        for (int i = 0; i < idTypeProducts.Count; i++)
                        {
                            cmd.Parameters.AddWithValue($"@id{i}", idTypeProducts[i]);
                        }
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(tranPlanTable);
                    }
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
                item.ProductId = Convert.ToInt32(row["ид_питание"]);
                item.NameProduct = row["название"].ToString();
                item.DescriptionProduct = row["описание"].ToString();
                item.Image = image;

                items.Add(item);
            }

            ExerPlanNov.ItemsSource = items;
        }


       
        private void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 1. Вызовите метод фильтрации (например, для поиска)
            LoadProduct(Filter.Text.Trim(), selectedTypeProductIds.ToList());

            // 2. Обновите видимость watermark в зависимости от текста в TextBox
            watermarkUsername.Visibility = string.IsNullOrWhiteSpace(Filter.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Управляем видимостью watermark в зависимости от содержимого в TextBox
            watermarkUsername.Visibility = string.IsNullOrWhiteSpace(Filter.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void txtUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            // Управляем видимостью watermark, когда поле получает фокус
            watermarkUsername.Visibility = string.IsNullOrWhiteSpace(Filter.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void txtUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            // Управляем видимостью watermark, когда поле теряет фокус
            watermarkUsername.Visibility = string.IsNullOrWhiteSpace(Filter.Text) ? Visibility.Visible : Visibility.Collapsed;

        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (LoadRoleUser() != "администратор")
            {
                Border clickedBorder = sender as Border;
                if (clickedBorder != null)
                {
                    int typeProductId = (int)clickedBorder.Tag;

                    GuideDetailsProduct guide = new GuideDetailsProduct(typeProductId, idUser);
                    guide.Show();
                    this.Close();
                }
            } else
            {
                Border clickedBorder = sender as Border;
                if (clickedBorder != null)
                {
                    int typeProductId = (int)clickedBorder.Tag;

                    EditProduct guide = new EditProduct(idUser, typeProductId);
                    guide.Show();
                    this.Close();
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (LoadRoleUser() != "администратор")
            {

                FormNewPlanBut.Visibility = Visibility.Visible;
                ReadyTrainPlanBut.Visibility = Visibility.Visible;
                ProfileBut.Visibility = Visibility.Visible;
                AddNewExercise.Visibility = Visibility.Collapsed;
                ListExercise.Visibility = Visibility.Collapsed;
                AddNewPlanBut.Visibility = Visibility.Collapsed;

            }
            else
            {
                ProfileBut.Visibility= Visibility.Collapsed;

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
        private void AddNewPlanBut_Click(object sender, RoutedEventArgs e)
        {
           
                AddProducts create = new AddProducts(idUser);
                create.Show();
                this.Close();
           
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

        private void TypeProduct_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border clickedBorder && clickedBorder.Tag is int typeProductId)
            {
                if (selectedTypeProductIds.Contains(typeProductId))
                {
                    selectedTypeProductIds.Remove(typeProductId); 
                    clickedBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAF0E6"));
                }
                else
                {
                    selectedTypeProductIds.Add(typeProductId); 
                    clickedBorder.Background = new SolidColorBrush(Color.FromRgb(255, 220, 180)); 
                }

                LoadProduct(Filter.Text.Trim(), selectedTypeProductIds.ToList());
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
    }
}
