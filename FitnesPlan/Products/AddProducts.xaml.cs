using FitnesPlan.Guide;
using System;
using System.Collections.Generic;
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

namespace FitnesPlan.Products
{
    /// <summary>
    /// Логика взаимодействия для AddProducts.xaml
    /// </summary>
    public partial class AddProducts : Window
    {
        private int idUser;
        private byte[] selectedImageBytes;

        public AddProducts(int idUser)
        {
            InitializeComponent();
            this.idUser = idUser;
            LoadTypes();
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

        private void LoadTypes()
        {
            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                string query = "SELECT ид_тип_питания, наименование FROM тип_питания";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                TypeComboBox.ItemsSource = dt.DefaultView;
                TypeComboBox.DisplayMemberPath = "наименование";
                TypeComboBox.SelectedValuePath = "ид_тип_питания";
            }
        }

        private void AddProd_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text;
            string desc = txtDesc.Text;
            int? typeId = TypeComboBox.SelectedValue as int?;
            int userId = idUser;
            int newId;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(desc) || typeId == null || selectedImageBytes == null)
            {
                MessageBox.Show("Заполните все поля и выберите изображение.");
                return;
            }

            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();

                string query = "INSERT INTO питание (название, описание, фото, ид_пользователь, ид_тип_питания) " +
                               "VALUES (@name, @desc, @image, @userId, @typeId) SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@desc", desc);
                    cmd.Parameters.AddWithValue("@image", selectedImageBytes);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@typeId", typeId);
                    object result = cmd.ExecuteScalar(); 
                    newId = Convert.ToInt32(result);
/*                    cmd.ExecuteNonQuery();
*/                }
            }

            MessageBox.Show("Продукт успешно добавлен!");

            GuideDetailsProduct detailsProduct = new GuideDetailsProduct(newId, idUser);
            detailsProduct.Show();
            this.Close();
        }

        private void ProfileBut_Click(object sender, RoutedEventArgs e)
        {
            Profile profile = new Profile(idUser);
            profile.Show();
            this.Close();
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
    }
}
