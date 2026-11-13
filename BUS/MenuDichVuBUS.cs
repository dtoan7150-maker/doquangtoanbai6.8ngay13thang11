using System;
using System.Data;
using BilliardsManagement.DAL;
using BilliardsManagement.DTO;

namespace BilliardsManagement.BUS
{
    public class MenuDichVuBUS
    {
        private readonly MenuDichVuDAL dal = new MenuDichVuDAL();

        public DataTable GetAllMenu() => dal.GetAllMenu();

        public string ThemDichVu(MenuDichVuDTO dv)
        {
            if (string.IsNullOrWhiteSpace(dv.TenDV)) return "Tên dịch vụ không được để trống.";
            if (dv.DonGia < 0) return "Đơn giá không hợp lệ.";
            return dal.Insert(dv) ? "Thêm dịch vụ mới thành công!" : "Thêm thất bại.";
        }

        public string SuaDichVu(MenuDichVuDTO dv)
        {
            if (dv.MaDV <= 0) return "Vui lòng chọn dịch vụ cần sửa.";
            return dal.Update(dv) ? "Cập nhật dịch vụ thành công!" : "Cập nhật thất bại.";
        }

        public string XoaDichVu(int maDV)
        {
            if (maDV <= 0) return "Vui lòng chọn dịch vụ cần xóa.";
            try
            {
                return dal.Delete(maDV) ? "Xóa dịch vụ thành công!" : "Xóa thất bại.";
            }
            catch (Exception)
            {
                return "Không thể xóa dịch vụ này vì đã tồn tại trong hóa đơn!";
            }
        }
    }
}