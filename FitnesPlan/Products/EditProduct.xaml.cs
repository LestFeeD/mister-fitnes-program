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
using Microsoft.Win32;

namespace FitnesPlan.Products
{
    /// <summary>
    /// Логика взаимодействия для EditProduct.xaml
    /// </summary>
    public partial class EditProduct : Window
    {
        private byte[] selectedImageBytes;
        private int idUser;
        private int idProduct;

        public EditProduct( int idUser, int idProduct)
        {
            InitializeComponent();
            this.idUser = idUser;
            this.idProduct = idProduct;
            LoadTypeProduct();

            LoadProduct();

        }

        private void LoadTypeProduct()
        {
            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT ид_тип_питания, наименование FROM тип_питания", conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                TypeProduct.ItemsSource = dt.DefaultView;
                TypeProduct.DisplayMemberPath = "наименование";
                TypeProduct.SelectedValuePath = "ид_тип_питания";
            }
        }

        private void LoadProduct()
        {
            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT название, описание, фото, ид_тип_питания FROM питание WHERE ид_питание = @id", conn);
                cmd.Parameters.AddWithValue("@id", idProduct);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    TitleTextBox.Text = reader["название"].ToString();
                    DescTextBox.Text = reader["описание"].ToString();
                    TypeProduct.SelectedValue = reader["ид_тип_питания"];

                    if (reader["фото"] != DBNull.Value)
                    {
                        byte[] imageBytes = (byte[])reader["фото"];
                        using (MemoryStream ms = new MemoryStream(imageBytes))
                        {
                            BitmapImage image = new BitmapImage();
                            image.BeginInit();
                            image.StreamSource = ms;
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.EndInit();
                            ExerciseGif.Source = image;
                        }
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TitleTextBox.Text) && string.IsNullOrEmpty(DescTextBox.Text))
            {
                MessageBox.Show("Название и описание не может быть пустым.");
                return; 
            }

            using (SqlConnection conn = DbConnectionHelper.GetConnection())
            {
                conn.Open();

                byte[] imageBytes = selectedImageBytes;
                if (ExerciseGif.Source != null)
                {
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)ExerciseGif.Source));
                    MemoryStream ms = new MemoryStream();
                    encoder.Save(ms);
                    imageBytes = ms.ToArray();
                }

                SqlCommand cmd = new SqlCommand("UPDATE питание SET название = @name, описание = @desc, фото = @img, ид_тип_питания = @type WHERE ид_питание = @id", conn);
                cmd.Parameters.AddWithValue("@name", TitleTextBox.Text);
                cmd.Parameters.AddWithValue("@desc", DescTextBox.Text);
                if (imageBytes != null)
                {
                    cmd.Parameters.Add("@img", SqlDbType.VarBinary).Value = imageBytes;
                }
                else
                {
                    
                    cmd.Parameters.Add("@img", SqlDbType.VarBinary).Value = DBNull.Value;
                }
                cmd.Parameters.AddWithValue("@type", TypeProduct.SelectedValue);
                cmd.Parameters.AddWithValue("@id", idProduct); 

                cmd.ExecuteNonQuery();
                MessageBox.Show("Данные сохранены.");
            }
        }

        private void SelectGif_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif";
            if (openFileDialog.ShowDialog() == true)
            {
                BitmapImage image = new BitmapImage(new Uri(openFileDialog.FileName));
                ExerciseGif.Source = image;

                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    selectedImageBytes = ms.ToArray();
                }
            }
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

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();
                    string deleteQuery = "DELETE FROM питание WHERE ид_питание = @nutritionId";

                    using (SqlCommand cmd = new SqlCommand(deleteQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@nutritionId", idProduct);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                            MessageBox.Show("Питание успешно удалено.");
                        else
                            MessageBox.Show("Питание с указанным ID не найдено.");
                    }
            }
        }
    }
}
