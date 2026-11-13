using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using BilliardsManagement.DTO;

namespace BilliardsManagement.DAL
{
    public class BanBidaDAL : DBConnect
    {
        public List<BanBidaDTO> GetDanhSachBan()
        {
            var list = new List<BanBidaDTO>();
            using (SqlConnection conn = GetConnection())
            {
                string query = "SELECT MaBan, TenBan, LoaiBan, GiaTheoGio, TrangThai FROM BanBida ORDER BY MaBan";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new BanBidaDTO
                        {
                            MaBan = reader.GetInt32(0),
                            TenBan = reader.GetString(1),
                            LoaiBan = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            GiaTheoGio = reader.GetDecimal(3),
                            TrangThai = reader.GetString(4)
                        });
                    }
                }
            }
            return list;
        }

        public DataTable GetAllBanTable()
        {
            using (SqlConnection conn = GetConnection())
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT MaBan, TenBan, LoaiBan, GiaTheoGio, TrangThai FROM BanBida", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public bool Insert(BanBidaDTO ban)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = "INSERT INTO BanBida (TenBan, LoaiBan, GiaTheoGio, TrangThai) VALUES (@TenBan, @LoaiBan, @GiaTheoGio, N'Trống')";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TenBan", ban.TenBan);
                cmd.Parameters.AddWithValue("@LoaiBan", ban.LoaiBan);
                cmd.Parameters.AddWithValue("@GiaTheoGio", ban.GiaTheoGio);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Update(BanBidaDTO ban)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = "UPDATE BanBida SET TenBan = @TenBan, LoaiBan = @LoaiBan, GiaTheoGio = @GiaTheoGio WHERE MaBan = @MaBan";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaBan", ban.MaBan);
                cmd.Parameters.AddWithValue("@TenBan", ban.TenBan);
                cmd.Parameters.AddWithValue("@LoaiBan", ban.LoaiBan);
                cmd.Parameters.AddWithValue("@GiaTheoGio", ban.GiaTheoGio);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(int maBan)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = "DELETE FROM BanBida WHERE MaBan = @MaBan AND TrangThai = N'Trống'";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaBan", maBan);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}