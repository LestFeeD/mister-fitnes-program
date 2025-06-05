using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnesPlan
{
    public static class DbConnectionHelper
    {
        //MS SQL Server
        private const string connectionString = "Server=DESKTOP-8PMNR1A;Database=фитнес_бд;Trusted_Connection=True;";


        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
