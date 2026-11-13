using System;
using System.Data;
using BilliardsManagement.DAL;
using BilliardsManagement.DTO;

namespace BilliardsManagement.BUS
{
    public class HoaDonBUS
    {
        private readonly HoaDonDAL dal = new HoaDonDAL();

        public string MoBan(int maBan)
        {
            if (maBan <= 0) return "Vui lòng chọn bàn trống trên sơ đồ.";
            try
            {
                int maHD = dal.MoBanMoi(maBan);
                return $"Mở bàn thành công! Đã tạo Hóa đơn #{maHD} và bắt đầu tính giờ.";
            }
            catch (Exception ex)
            {
                return "Lỗi mở bàn: " + ex.Message;
            }
        }

        public HoaDonDTO GetHoaDonDangChoi(int maBan) => dal.GetHoaDonDangChoiTheoBan(maBan);

        public DataTable GetChiTietDisconnected(int maHD) => dal.GetChiTietHoaDon_Disconnected(maHD);

        public void LuuChiTietDisconnected(DataTable dtChiTiet) => dal.CapNhatChiTietHoaDon_Disconnected(dtChiTiet);

        // Tính giờ chơi tự động (làm tròn theo block 15 phút, tối thiểu 15 phút = 0.25 giờ)
        public (int TongPhut, decimal SoGioLamTron, decimal TienGioChoi) TinhTienGioChoi(DateTime gioBatDau, DateTime gioKetThuc, decimal giaTheoGio)
        {
            double phutThucTe = Math.Max(1, (gioKetThuc - gioBatDau).TotalMinutes);
            int soBlock15Phut = Math.Max(1, (int)Math.Ceiling(phutThucTe / 15.0));
            decimal soGioLamTron = soBlock15Phut * 0.25m;
            decimal tienGio = Math.Round(soGioLamTron * giaTheoGio, 0);
            return ((int)Math.Round(phutThucTe), soGioLamTron, tienGio);
        }

        public string ThanhToan(int maHD, int maBan, DateTime gioKetThuc, decimal tienGioChoi, decimal tienDichVu)
        {
            try
            {
                dal.ThanhToanHoaDon(maHD, maBan, gioKetThuc, tienGioChoi, tienDichVu);
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public DataTable GetLichSuHoaDon() => dal.GetLichSuHoaDon();
    }
}