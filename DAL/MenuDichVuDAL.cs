using System.Data;
using Microsoft.Data.SqlClient;
using BilliardsManagement.DTO;

namespace BilliardsManagement.DAL
{
    public class MenuDichVuDAL : DBConnect
    {
        public DataTable GetAllMenu()
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = "SELECT MaDV, TenDV, DonGia, TonKho FROM MenuDichVu";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public bool Insert(MenuDichVuDTO dv)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = "INSERT INTO MenuDichVu (TenDV, DonGia, TonKho) VALUES (@TenDV, @DonGia, @TonKho)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TenDV", dv.TenDV);
                cmd.Parameters.AddWithValue("@DonGia", dv.DonGia);
                cmd.Parameters.AddWithValue("@TonKho", dv.TonKho);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Update(MenuDichVuDTO dv)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = "UPDATE MenuDichVu SET TenDV = @TenDV, DonGia = @DonGia, TonKho = @TonKho WHERE MaDV = @MaDV";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaDV", dv.MaDV);
                cmd.Parameters.AddWithValue("@TenDV", dv.TenDV);
                cmd.Parameters.AddWithValue("@DonGia", dv.DonGia);
                cmd.Parameters.AddWithValue("@TonKho", dv.TonKho);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(int maDV)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = "DELETE FROM MenuDichVu WHERE MaDV = @MaDV";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaDV", maDV);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}