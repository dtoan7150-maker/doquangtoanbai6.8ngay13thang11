using System;
using System.Data;
using Microsoft.Data.SqlClient;
using BilliardsManagement.DTO;

namespace BilliardsManagement.DAL
{
    public class HoaDonDAL : DBConnect
    {
        // 1. Mở bàn mới (Tạo HoaDon + Đổi trạng thái BanBida -> "Đang chơi")
        public int MoBanMoi(int maBan)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    string sqlCheck = "SELECT TrangThai FROM BanBida WHERE MaBan = @MaBan";
                    SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn, trans);
                    cmdCheck.Parameters.AddWithValue("@MaBan", maBan);
                    string trangThai = cmdCheck.ExecuteScalar()?.ToString();

                    if (trangThai != "Trống")
                        throw new Exception("Bàn này hiện đang có khách chơi!");

                    string sqlHD = @"INSERT INTO HoaDon (MaBan, GioBatDau, TienGioChoi, TienDichVu, TrangThai)
                                     OUTPUT INSERTED.MaHD
                                     VALUES (@MaBan, GETDATE(), 0, 0, 0)";
                    SqlCommand cmdHD = new SqlCommand(sqlHD, conn, trans);
                    cmdHD.Parameters.AddWithValue("@MaBan", maBan);
                    int maHDMoi = (int)cmdHD.ExecuteScalar();

                    string sqlBan = "UPDATE BanBida SET TrangThai = N'Đang chơi' WHERE MaBan = @MaBan";
                    SqlCommand cmdBan = new SqlCommand(sqlBan, conn, trans);
                    cmdBan.Parameters.AddWithValue("@MaBan", maBan);
                    cmdBan.ExecuteNonQuery();

                    trans.Commit();
                    return maHDMoi;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        // Lấy hóa đơn chưa thanh toán của bàn đang chơi
        public HoaDonDTO GetHoaDonDangChoiTheoBan(int maBan)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = @"SELECT TOP 1 hd.MaHD, hd.MaBan, b.TenBan, hd.GioBatDau, hd.TienGioChoi, hd.TienDichVu, hd.TrangThai
                                 FROM HoaDon hd
                                 INNER JOIN BanBida b ON hd.MaBan = b.MaBan
                                 WHERE hd.MaBan = @MaBan AND hd.TrangThai = 0
                                 ORDER BY hd.MaHD DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaBan", maBan);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new HoaDonDTO
                        {
                            MaHD = reader.GetInt32(0),
                            MaBan = reader.GetInt32(1),
                            TenBan = reader.GetString(2),
                            GioBatDau = reader.GetDateTime(3),
                            TienGioChoi = reader.GetDecimal(4),
                            TienDichVu = reader.GetDecimal(5),
                            TrangThai = reader.GetBoolean(6)
                        };
                    }
                }
            }
            return null;
        }

        // 2. Mô hình ngắt kết nối (Disconnected Model): Nạp ChiTietHoaDon vào DataTable qua SqlDataAdapter
        public DataTable GetChiTietHoaDon_Disconnected(int maHD)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = @"SELECT ct.MaHD, ct.MaDV, dv.TenDV, ct.SoLuong, ct.DonGia,
                                        (ct.SoLuong * ct.DonGia) AS ThanhTien
                                 FROM ChiTietHoaDon ct
                                 INNER JOIN MenuDichVu dv ON ct.MaDV = dv.MaDV
                                 WHERE ct.MaHD = @MaHD";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@MaHD", maHD);

                DataTable dt = new DataTable();
                da.Fill(dt);
                dt.PrimaryKey = new DataColumn[] { dt.Columns["MaHD"], dt.Columns["MaDV"] };
                dt.Columns["TenDV"].ReadOnly = false;
                dt.Columns["ThanhTien"].ReadOnly = false;
                return dt;
            }
        }

        // Đồng bộ thay đổi từ DataTable (Added / Modified / Deleted) xuống SQL Server bằng SqlDataAdapter.Update()
        public void CapNhatChiTietHoaDon_Disconnected(DataTable dtChiTiet)
        {
            using (SqlConnection conn = GetConnection())
            {
                SqlDataAdapter da = new SqlDataAdapter();

                da.InsertCommand = new SqlCommand(
                    "INSERT INTO ChiTietHoaDon (MaHD, MaDV, SoLuong, DonGia) VALUES (@MaHD, @MaDV, @SoLuong, @DonGia)", conn);
                da.InsertCommand.Parameters.Add("@MaHD", SqlDbType.Int, 4, "MaHD");
                da.InsertCommand.Parameters.Add("@MaDV", SqlDbType.Int, 4, "MaDV");
                da.InsertCommand.Parameters.Add("@SoLuong", SqlDbType.Int, 4, "SoLuong");
                da.InsertCommand.Parameters.Add("@DonGia", SqlDbType.Decimal, 18, "DonGia");

                da.UpdateCommand = new SqlCommand(
                    "UPDATE ChiTietHoaDon SET SoLuong = @SoLuong, DonGia = @DonGia WHERE MaHD = @MaHD AND MaDV = @MaDV", conn);
                da.UpdateCommand.Parameters.Add("@SoLuong", SqlDbType.Int, 4, "SoLuong");
                da.UpdateCommand.Parameters.Add("@DonGia", SqlDbType.Decimal, 18, "DonGia");
                da.UpdateCommand.Parameters.Add("@MaHD", SqlDbType.Int, 4, "MaHD").SourceVersion = DataRowVersion.Original;
                da.UpdateCommand.Parameters.Add("@MaDV", SqlDbType.Int, 4, "MaDV").SourceVersion = DataRowVersion.Original;

                da.DeleteCommand = new SqlCommand(
                    "DELETE FROM ChiTietHoaDon WHERE MaHD = @MaHD AND MaDV = @MaDV", conn);
                da.DeleteCommand.Parameters.Add("@MaHD", SqlDbType.Int, 4, "MaHD").SourceVersion = DataRowVersion.Original;
                da.DeleteCommand.Parameters.Add("@MaDV", SqlDbType.Int, 4, "MaDV").SourceVersion = DataRowVersion.Original;

                da.Update(dtChiTiet);
            }
        }

        // 3. Thanh toán hóa đơn bằng SqlTransaction
        public bool ThanhToanHoaDon(int maHD, int maBan, DateTime gioKetThuc, decimal tienGioChoi, decimal tienDichVu)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    // Bước 1: Cập nhật GioKetThuc, TienGioChoi, TienDichVu, TrangThai = 1 cho HoaDon
                    string sqlUpdateHD = @"UPDATE HoaDon
                                           SET GioKetThuc = @GioKetThuc,
                                               TienGioChoi = @TienGioChoi,
                                               TienDichVu = @TienDichVu,
                                               TrangThai = 1
                                           WHERE MaHD = @MaHD AND TrangThai = 0";
                    SqlCommand cmdHD = new SqlCommand(sqlUpdateHD, conn, trans);
                    cmdHD.Parameters.AddWithValue("@GioKetThuc", gioKetThuc);
                    cmdHD.Parameters.AddWithValue("@TienGioChoi", tienGioChoi);
                    cmdHD.Parameters.AddWithValue("@TienDichVu", tienDichVu);
                    cmdHD.Parameters.AddWithValue("@MaHD", maHD);

                    if (cmdHD.ExecuteNonQuery() == 0)
                        throw new Exception("Hóa đơn không tồn tại hoặc đã được thanh toán trước đó.");

                    // Bước 2: Đọc danh sách dịch vụ đã gọi để trừ TonKho (Bỏ qua dịch vụ thuê gậy)
                    var items = new System.Collections.Generic.List<(int MaDV, string TenDV, int SoLuong, int TonKho)>();
                    string sqlGetCT = @"SELECT ct.MaDV, dv.TenDV, ct.SoLuong, dv.TonKho
                                        FROM ChiTietHoaDon ct
                                        INNER JOIN MenuDichVu dv ON ct.MaDV = dv.MaDV
                                        WHERE ct.MaHD = @MaHD";
                    SqlCommand cmdGetCT = new SqlCommand(sqlGetCT, conn, trans);
                    cmdGetCT.Parameters.AddWithValue("@MaHD", maHD);

                    using (SqlDataReader reader = cmdGetCT.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add((
                                reader.GetInt32(0),
                                reader.GetString(1),
                                reader.GetInt32(2),
                                reader.IsDBNull(3) ? -1 : reader.GetInt32(3)
                            ));
                        }
                    }

                    foreach (var item in items)
                    {
                        // Bỏ qua nếu là dịch vụ thuê gậy (TonKho < 0 hoặc tên chứa "Thuê")
                        bool laDichVuThue = item.TonKho < 0 || item.TenDV.Contains("Thuê", StringComparison.OrdinalIgnoreCase);
                        if (laDichVuThue) continue;

                        if (item.TonKho < item.SoLuong)
                            throw new Exception($"Mặt hàng '{item.TenDV}' không đủ tồn kho để thanh toán (Cần: {item.SoLuong}, Tồn: {item.TonKho}). Đã Rollback giao dịch!");

                        string sqlTruKho = "UPDATE MenuDichVu SET TonKho = TonKho - @SoLuong WHERE MaDV = @MaDV";
                        SqlCommand cmdTruKho = new SqlCommand(sqlTruKho, conn, trans);
                        cmdTruKho.Parameters.AddWithValue("@SoLuong", item.SoLuong);
                        cmdTruKho.Parameters.AddWithValue("@MaDV", item.MaDV);
                        cmdTruKho.ExecuteNonQuery();
                    }

                    // Bước 3: Cập nhật trạng thái BanBida về "Trống"
                    string sqlUpdateBan = "UPDATE BanBida SET TrangThai = N'Trống' WHERE MaBan = @MaBan";
                    SqlCommand cmdBan = new SqlCommand(sqlUpdateBan, conn, trans);
                    cmdBan.Parameters.AddWithValue("@MaBan", maBan);
                    cmdBan.ExecuteNonQuery();

                    // Bước 4: Xác nhận giao dịch
                    trans.Commit();
                    return true;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        // Lịch sử hóa đơn đã thanh toán
        public DataTable GetLichSuHoaDon()
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = @"SELECT hd.MaHD, b.TenBan, hd.GioBatDau, hd.GioKetThuc,
                                        hd.TienGioChoi, hd.TienDichVu,
                                        (hd.TienGioChoi + hd.TienDichVu) AS TongThanhToan
                                 FROM HoaDon hd
                                 INNER JOIN BanBida b ON hd.MaBan = b.MaBan
                                 WHERE hd.TrangThai = 1
                                 ORDER BY hd.GioKetThuc DESC";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }
}