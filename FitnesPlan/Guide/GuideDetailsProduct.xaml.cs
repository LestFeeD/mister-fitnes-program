using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
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

namespace FitnesPlan.Guide
{
    /// <summary>
    /// Логика взаимодействия для GuideDetailsProduct.xaml
    /// </summary>
    public partial class GuideDetailsProduct : Window
    {
        private int idProduct;
        private int idTypeProduct;
        private int idUser;

        public GuideDetailsProduct(int idProduct, int idUser, int idTypeProduct)
        {
            InitializeComponent();
            this.idProduct = idProduct;
            this.idTypeProduct = idTypeProduct;
            this.idUser = idUser;
            LoadProductDetails();
        }

        public GuideDetailsProduct(int idProduct, int idUser)
        {
            InitializeComponent();
            this.idProduct = idProduct;
            this.idUser = idUser;
            LoadProductDetails();
        }

        private void LoadProductDetails()
        {
            DataTable tranPlanTable = new DataTable();
            using (SqlConnection connection = DbConnectionHelper.GetConnection())
            {
                connection.Open();
                string query = @"SELECT гт.ид_питание  , гт.название, гт.описание, гт.фото 
            FROM питание гт
            JOIN тип_питания тт ON гт.ид_тип_питания   = тт.ид_тип_питания  
            WHERE гт.ид_питание = @typeId";


                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@typeId", idProduct);


                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(tranPlanTable);
                    }
                }
            }
            var items = new List<ExpandoObject>();
            foreach (DataRow rowPh in tranPlanTable.Rows)
            {
                byte[] imageData = rowPh["фото"] as byte[];
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
                var row = tranPlanTable.Rows[0];

                string name = row["название"].ToString();
                string description = row["описание"].ToString();

                var data = new
                {
                    NameProduct = name,
                    Image = image,
                    DescriptionProduct = description

                };

                this.DataContext = data;
            }

        }
      
        private void TransitionBut_Click(object sender, RoutedEventArgs e)
        {
            
                GuideProducts guideProducts = new GuideProducts(idUser);
                guideProducts.Show();
                this.Close();
            
        }

    }

}

