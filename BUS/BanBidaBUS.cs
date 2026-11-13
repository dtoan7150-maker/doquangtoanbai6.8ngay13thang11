using System.Collections.Generic;
using System.Data;
using BilliardsManagement.DAL;
using BilliardsManagement.DTO;

namespace BilliardsManagement.BUS
{
    public class BanBidaBUS
    {
        private readonly BanBidaDAL dal = new BanBidaDAL();

        public List<BanBidaDTO> GetDanhSachBan() => dal.GetDanhSachBan();
        public DataTable GetAllBanTable() => dal.GetAllBanTable();

        public string ThemBan(BanBidaDTO ban)
        {
            if (string.IsNullOrWhiteSpace(ban.TenBan)) return "Tên bàn không được để trống.";
            if (ban.GiaTheoGio <= 0) return "Giá theo giờ phải lớn hơn 0.";
            return dal.Insert(ban) ? "Thêm bàn mới thành công!" : "Thêm bàn thất bại.";
        }

        public string SuaBan(BanBidaDTO ban)
        {
            if (ban.MaBan <= 0) return "Vui lòng chọn bàn cần sửa.";
            if (ban.GiaTheoGio <= 0) return "Giá theo giờ không hợp lệ.";
            return dal.Update(ban) ? "Cập nhật thông tin bàn thành công!" : "Cập nhật thất bại.";
        }

        public string XoaBan(int maBan)
        {
            if (maBan <= 0) return "Vui lòng chọn bàn cần xóa.";
            return dal.Delete(maBan) ? "Xóa bàn thành công!" : "Chỉ được xóa bàn đang ở trạng thái Trống và chưa có hóa đơn!";
        }
    }
}