using System.Configuration;
using Microsoft.Data.SqlClient;

namespace BilliardsManagement.DAL
{
    public class DBConnect
    {
        protected static string connectionString =
            ConfigurationManager.ConnectionStrings["BilliardsDB"]?.ConnectionString
            ?? @"Data Source=.\SQLEXPRESS;Initial Catalog=BilliardsDB;Integrated Security=True;TrustServerCertificate=True";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}