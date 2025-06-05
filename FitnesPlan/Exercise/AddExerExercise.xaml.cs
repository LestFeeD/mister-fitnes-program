using FitnesPlan.Guide;
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
using System.IO;
using FitnesPlan.Products;

namespace FitnesPlan
{
    /// <summary>
    /// Логика взаимодействия для AddExerExercise.xaml
    /// </summary>
    public partial class AddExerExercise : Window
    {
        private int idUser;
        private byte[] selectedImageBytes;
        private string selectedGifPath;


        public AddExerExercise(int idUser)
        {
            InitializeComponent();
            this.idUser = idUser;
            LoadTypes();
            DifficultExerLoad();
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
                string query = "SELECT ид_тип_упражнения, наименование FROM тип_упражнения";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                TypeComboBox.ItemsSource = dt.DefaultView;
                TypeComboBox.DisplayMemberPath = "наименование";
                TypeComboBox.SelectedValuePath = "ид_тип_упражнения";
            }
        }

        private void DifficultExerLoad()
        {
            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                string query = "SELECT ид_сложность_упражнения, наименование FROM сложность_упражнения";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                DifficultComboBox.ItemsSource = dt.DefaultView;
                DifficultComboBox.DisplayMemberPath = "наименование";
                DifficultComboBox.SelectedValuePath = "ид_сложность_упражнения";
            }
        }

        private void AddProd_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text;
            int? typeId = TypeComboBox.SelectedValue as int?;
            int? difId = DifficultComboBox.SelectedValue as int?;
            string link = txtLink.Text;
            int userId = idUser;
            int newId;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(link) || typeId == null || selectedImageBytes == null || difId == null
                || string.IsNullOrWhiteSpace(txtCalor.Text) || string.IsNullOrWhiteSpace(selectedGifPath))
            {
                MessageBox.Show("Заполните все поля и выберите изображение.");
                return;
            }
            if (!int.TryParse(txtCalor.Text, out int calcalories))
            {
                MessageBox.Show("Введите корректное числовое значение калорийности.");
                return;
            }
            if (!Uri.IsWellFormedUriString(link, UriKind.Absolute) ||
    !(link.StartsWith("http://") || link.StartsWith("https://")))
            {
                MessageBox.Show("Введите корректную ссылку на видео (должна начинаться с http:// или https://).");
                return;
            }
            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();

                string query = "INSERT INTO упражнение (название, видео, фото, ид_тип_упражнения, калорийность, ид_сложность_упражнения,  ссылка_техники) " +
                               "VALUES (@name, @video, @image, @idTypeExer, @calor, @idDiffExer, @link) SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@video", selectedGifPath);
                    cmd.Parameters.AddWithValue("@image", selectedImageBytes);
                    cmd.Parameters.AddWithValue("@idTypeExer", typeId);
                    cmd.Parameters.AddWithValue("@calor", calcalories);
                    cmd.Parameters.AddWithValue("@idDiffExer", difId);
                    cmd.Parameters.AddWithValue("@link", link);

                    object result = cmd.ExecuteScalar();
                    newId = Convert.ToInt32(result);
                 
                }
            }

            MessageBox.Show("Упражнение успешно добавлен!");

            DetailsExersice detailsProduct = new DetailsExersice(newId, 5, idUser);
            detailsProduct.Show();
            this.Close();
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

        private void ListExercise_Click(object sender, RoutedEventArgs e)
        {
            ListExer list = new ListExer(4, idUser);
            list.Show();
            this.Close();
        }

        private void ReadyTrainPlanBut_Click(object sender, RoutedEventArgs e)
        {
            {
                ListTranPlan list = new ListTranPlan(idUser);
                list.Show();
                this.Close();
            }
        }
        private void CreateNewPlan_Click(object sender, RoutedEventArgs e)
        {
            CreateTrainPersonal create = new CreateTrainPersonal(idUser);
            create.Show();
            this.Close();
        }
    }
}
