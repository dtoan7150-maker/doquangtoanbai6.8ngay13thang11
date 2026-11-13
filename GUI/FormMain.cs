using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BilliardsManagement.BUS;
using BilliardsManagement.DTO;

namespace BilliardsManagement.GUI
{
    public class FormMain : Form
    {
        private readonly BanBidaBUS banBus = new BanBidaBUS();
        private readonly MenuDichVuBUS dvBus = new MenuDichVuBUS();
        private readonly HoaDonBUS hdBus = new HoaDonBUS();

        // Tab 1: Sơ đồ bàn trực quan & Gọi dịch vụ / Thanh toán
        private FlowLayoutPanel flpSoDoBan;
        private Label lblBanDangChon, lblTrangThaiBan, lblGioBatDau, lblThoiGianChoi, lblTienGio, lblTienDichVu, lblTongCong;
        private Button btnMoBan, btnThemMon, btnXoaMon, btnLuuDataTable, btnThanhToan;
        private ComboBox cboDichVu;
        private NumericUpDown numSoLuongMon;
        private DataGridView dgvDichVuBan;
        private RichTextBox rtbHoaDon;

        private BanBidaDTO banHienTai = null;
        private HoaDonDTO hoaDonHienTai = null;
        private DataTable dtChiTietDisconnected = null;
        private DataTable dtMenuCache = null;
        private System.Windows.Forms.Timer timerTinhGio;

        // Tab 2: Quản lý Bàn, Menu Dịch vụ & Lịch sử Hóa đơn
        private DataGridView dgvQLBan, dgvQLMenu, dgvLichSuHD;
        private TextBox txtMaBan, txtTenBan, txtMaDV, txtTenDV;
        private ComboBox cboLoaiBan;
        private NumericUpDown numGiaGio, numGiaDV, numTonKhoDV;

        public FormMain()
        {
            InitUI();
            try
            {
                LoadAllData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối SQL Server: " + ex.Message, "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitUI()
        {
            this.Text = "Hệ Thống Quản Lý Bàn Bida & Dịch Vụ Giải Trí (BilliardsDB)";
            this.Font = new Font("Segoe UI", 9.5F);
            this.Size = new Size(1180, 740);
            this.StartPosition = FormStartPosition.CenterScreen;

            TabControl tabControl = new TabControl { Dock = DockStyle.Fill };
            tabControl.TabPages.Add(CreateTabSoDoVaTinhTien());
            tabControl.TabPages.Add(CreateTabQuanLyDanhMucVaLichSu());

            this.Controls.Add(tabControl);

            // Timer cập nhật đồng hồ tính giờ tự động mỗi 10 giây
            timerTinhGio = new System.Windows.Forms.Timer { Interval = 10000 };
            timerTinhGio.Tick += (s, e) => CapNhatThongTinTinhTien();
            timerTinhGio.Start();
        }

        // ==================== TAB 1: SƠ ĐỒ BÀN & GỌI MÓN / THANH TOÁN ====================
        private TabPage CreateTabSoDoVaTinhTien()
        {
            TabPage tab = new TabPage("1. Sơ Đồ Bàn Trực Quan & Tính Tiền");

            // --- Cột trái: Sơ đồ bàn bida (Visual Grid Layout) ---
            GroupBox grpSoDo = new GroupBox {
                Text = "Sơ Đồ Bàn Bida (Xanh lá: Trống | Cam đỏ: Đang chơi)",
                Location = new Point(10, 10), Size = new Size(460, 650)
            };

            flpSoDoBan = new FlowLayoutPanel {
                Location = new Point(12, 28), Size = new Size(436, 555),
                AutoScroll = true, BackColor = Color.WhiteSmoke, BorderStyle = BorderStyle.FixedSingle
            };

            btnMoBan = new Button {
                Text = "MỞ BÀN & BẮT ĐẦU TÍNH GIỜ",
                Location = new Point(12, 595), Width = 436, Height = 42,
                BackColor = Color.SeaGreen, ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold), Enabled = false
            };
            btnMoBan.Click += (s, e) => {
                if (banHienTai == null) return;
                string msg = hdBus.MoBan(banHienTai.MaBan);
                MessageBox.Show(msg, "Mở bàn Bida", MessageBoxButtons.OK, MessageBoxIcon.Information);
                int maBanVuaMo = banHienTai.MaBan;
                LoadSoDoBan();
                ChonBan(maBanVuaMo);
            };

            grpSoDo.Controls.AddRange(new Control[] { flpSoDoBan, btnMoBan });

            // --- Cột giữa: Gọi đồ uống / Thuê phụ kiện (Disconnected DataTable) ---
            GroupBox grpDichVu = new GroupBox {
                Text = "Gọi Đồ Uống & Thuê Gậy (Mô hình ngắt kết nối DataTable)",
                Location = new Point(480, 10), Size = new Size(380, 650)
            };

            lblBanDangChon = new Label {
                Text = "Bàn đang chọn: (Chưa chọn bàn)",
                Location = new Point(12, 28), AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.Navy
            };
            lblTrangThaiBan = new Label { Text = "Trạng thái: ---", Location = new Point(12, 55), AutoSize = true };
            lblGioBatDau = new Label { Text = "Giờ bắt đầu: ---", Location = new Point(12, 80), AutoSize = true };
            lblThoiGianChoi = new Label { Text = "Thời gian chơi: 0 phút (0.00 giờ)", Location = new Point(12, 105), AutoSize = true, ForeColor = Color.DarkGreen };

            grpDichVu.Controls.Add(new Label { Text = "Chọn dịch vụ:", Location = new Point(12, 142), AutoSize = true });
            cboDichVu = new ComboBox { Location = new Point(105, 139), Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };

            grpDichVu.Controls.Add(new Label { Text = "Số lượng:", Location = new Point(12, 178), AutoSize = true });
            numSoLuongMon = new NumericUpDown { Location = new Point(105, 175), Width = 75, Minimum = 1, Maximum = 100, Value = 1 };

            btnThemMon = new Button {
                Text = "+ Thêm / Tăng SL", Location = new Point(190, 173), Width = 175, Height = 30,
                BackColor = Color.DodgerBlue, ForeColor = Color.White, Enabled = false
            };
            btnThemMon.Click += BtnThemMon_Click;

            dgvDichVuBan = new DataGridView {
                Location = new Point(12, 215), Size = new Size(355, 250),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false
            };
            dgvDichVuBan.CellEndEdit += (s, e) => {
                if (dtChiTietDisconnected == null || e.RowIndex < 0) return;
                DataRow row = dtChiTietDisconnected.Rows[e.RowIndex];
                if (row.RowState != DataRowState.Deleted)
                {
                    int sl = Math.Max(1, Convert.ToInt32(row["SoLuong"]));
                    decimal dg = Convert.ToDecimal(row["DonGia"]);
                    row["SoLuong"] = sl;
                    row["ThanhTien"] = sl * dg;
                    CapNhatThongTinTinhTien();
                }
            };

            btnXoaMon = new Button { Text = "Xóa món chọn", Location = new Point(12, 475), Width = 135, Height = 30, Enabled = false };
            btnXoaMon.Click += (s, e) => {
                if (dgvDichVuBan.CurrentRow == null || dtChiTietDisconnected == null) return;
                int idx = dgvDichVuBan.CurrentRow.Index;
                if (idx >= 0 && idx < dgvDichVuBan.Rows.Count)
                {
                    DataRowView drv = (DataRowView)dgvDichVuBan.Rows[idx].DataBoundItem;
                    drv.Row.Delete();
                    CapNhatThongTinTinhTien();
                }
            };

            btnLuuDataTable = new Button {
                Text = "Lưu DataTable (DataAdapter)", Location = new Point(155, 475), Width = 212, Height = 30,
                BackColor = Color.Khaki, Enabled = false
            };
            btnLuuDataTable.Click += (s, e) => {
                if (dtChiTietDisconnected == null || hoaDonHienTai == null) return;
                hdBus.LuuChiTietDisconnected(dtChiTietDisconnected);
                dtChiTietDisconnected = hdBus.GetChiTietDisconnected(hoaDonHienTai.MaHD);
                dgvDichVuBan.DataSource = dtChiTietDisconnected;
                MessageBox.Show("Đã đồng bộ dữ liệu dịch vụ từ DataTable xuống CSDL!", "Disconnected Model");
                CapNhatThongTinTinhTien();
            };

            lblTienGio = new Label { Text = "Tiền giờ chơi: 0 VNĐ", Location = new Point(12, 520), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            lblTienDichVu = new Label { Text = "Tiền dịch vụ : 0 VNĐ", Location = new Point(12, 548), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            lblTongCong = new Label { Text = "TỔNG CỘNG    : 0 VNĐ", Location = new Point(12, 580), AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.DarkRed };

            grpDichVu.Controls.AddRange(new Control[] {
                lblBanDangChon, lblTrangThaiBan, lblGioBatDau, lblThoiGianChoi,
                cboDichVu, numSoLuongMon, btnThemMon, dgvDichVuBan,
                btnXoaMon, btnLuuDataTable, lblTienGio, lblTienDichVu, lblTongCong
            });

            // --- Cột phải: Thanh toán & In Hóa đơn ---
            GroupBox grpThanhToan = new GroupBox {
                Text = "Hóa Đơn Chi Tiết & Thanh Toán (SqlTransaction)",
                Location = new Point(870, 10), Size = new Size(280, 650)
            };

            rtbHoaDon = new RichTextBox {
                Location = new Point(10, 28), Size = new Size(260, 555),
                Font = new Font("Consolas", 8.8F), ReadOnly = true, BackColor = Color.White
            };

            btnThanhToan = new Button {
                Text = "THANH TOÁN & TRẢ BÀN",
                Location = new Point(10, 595), Width = 260, Height = 42,
                BackColor = Color.OrangeRed, ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold), Enabled = false
            };
            btnThanhToan.Click += BtnThanhToan_Click;

            grpThanhToan.Controls.AddRange(new Control[] { rtbHoaDon, btnThanhToan });

            tab.Controls.AddRange(new Control[] { grpSoDo, grpDichVu, grpThanhToan });
            return tab;
        }

        // ==================== TAB 2: QUẢN LÝ DANH MỤC & LỊCH SỬ ====================
        private TabPage CreateTabQuanLyDanhMucVaLichSu()
        {
            TabPage tab = new TabPage("2. Quản Lý Bàn, Menu Dịch Vụ & Lịch Sử Hóa Đơn");

            // Quản lý Bàn Bida
            GroupBox grpBan = new GroupBox { Text = "Quản lý Danh sách Bàn Bida", Location = new Point(10, 10), Size = new Size(555, 315) };
            grpBan.Controls.Add(new Label { Text = "Mã:", Location = new Point(12, 28), AutoSize = true });
            txtMaBan = new TextBox { Location = new Point(45, 25), Width = 45, ReadOnly = true };

            grpBan.Controls.Add(new Label { Text = "Tên bàn:", Location = new Point(100, 28), AutoSize = true });
            txtTenBan = new TextBox { Location = new Point(160, 25), Width = 140 };

            grpBan.Controls.Add(new Label { Text = "Loại:", Location = new Point(310, 28), AutoSize = true });
            cboLoaiBan = new ComboBox { Location = new Point(350, 25), Width = 190, DropDownStyle = ComboBoxStyle.DropDown };
            cboLoaiBan.Items.AddRange(new object[] { "Pool (Lỗ)", "Carom (Phăng)", "Snooker" });
            cboLoaiBan.SelectedIndex = 0;

            grpBan.Controls.Add(new Label { Text = "Giá/Giờ:", Location = new Point(12, 65), AutoSize = true });
            numGiaGio = new NumericUpDown { Location = new Point(75, 62), Width = 120, Maximum = 1000000, Increment = 5000, Value = 60000, ThousandsSeparator = true };

            Button btnThemBan = new Button { Text = "Thêm Bàn", Location = new Point(215, 60), Width = 95 };
            Button btnSuaBan = new Button { Text = "Sửa Bàn", Location = new Point(320, 60), Width = 95 };
            Button btnXoaBan = new Button { Text = "Xóa Bàn", Location = new Point(425, 60), Width = 95 };

            btnThemBan.Click += (s, e) => {
                MessageBox.Show(banBus.ThemBan(new BanBidaDTO { TenBan = txtTenBan.Text, LoaiBan = cboLoaiBan.Text, GiaTheoGio = numGiaGio.Value }));
                LoadAllData();
            };
            btnSuaBan.Click += (s, e) => {
                if (!int.TryParse(txtMaBan.Text, out int id)) return;
                MessageBox.Show(banBus.SuaBan(new BanBidaDTO { MaBan = id, TenBan = txtTenBan.Text, LoaiBan = cboLoaiBan.Text, GiaTheoGio = numGiaGio.Value }));
                LoadAllData();
            };
            btnXoaBan.Click += (s, e) => {
                if (!int.TryParse(txtMaBan.Text, out int id)) return;
                MessageBox.Show(banBus.XoaBan(id));
                LoadAllData();
            };

            dgvQLBan = new DataGridView {
                Location = new Point(12, 100), Size = new Size(530, 200),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true, AllowUserToAddRows = false
            };
            dgvQLBan.CellClick += (s, e) => {
                if (e.RowIndex < 0) return;
                var r = dgvQLBan.Rows[e.RowIndex];
                if (r.Cells["MaBan"].Value == null) return;
                txtMaBan.Text = r.Cells["MaBan"].Value.ToString();
                txtTenBan.Text = r.Cells["TenBan"].Value.ToString();
                cboLoaiBan.Text = r.Cells["LoaiBan"].Value.ToString();
                numGiaGio.Value = Convert.ToDecimal(r.Cells["GiaTheoGio"].Value);
            };
            grpBan.Controls.AddRange(new Control[] { txtMaBan, txtTenBan, cboLoaiBan, numGiaGio, btnThemBan, btnSuaBan, btnXoaBan, dgvQLBan });

            // Quản lý Menu Dịch vụ
            GroupBox grpMenu = new GroupBox { Text = "Quản lý Menu Đồ Uống & Thuê Gậy (Tồn kho = -1: Không trừ kho)", Location = new Point(575, 10), Size = new Size(575, 315) };
            grpMenu.Controls.Add(new Label { Text = "Mã:", Location = new Point(12, 28), AutoSize = true });
            txtMaDV = new TextBox { Location = new Point(45, 25), Width = 45, ReadOnly = true };

            grpMenu.Controls.Add(new Label { Text = "Tên DV:", Location = new Point(100, 28), AutoSize = true });
            txtTenDV = new TextBox { Location = new Point(160, 25), Width = 200 };

            grpMenu.Controls.Add(new Label { Text = "Đơn giá:", Location = new Point(370, 28), AutoSize = true });
            numGiaDV = new NumericUpDown { Location = new Point(430, 25), Width = 125, Maximum = 5000000, Increment = 5000, Value = 15000, ThousandsSeparator = true };

            grpMenu.Controls.Add(new Label { Text = "Tồn kho:", Location = new Point(12, 65), AutoSize = true });
            numTonKhoDV = new NumericUpDown { Location = new Point(75, 62), Width = 90, Minimum = -1, Maximum = 10000, Value = 50 };

            Button btnThemDV = new Button { Text = "Thêm DV", Location = new Point(185, 60), Width = 105 };
            Button btnSuaDV = new Button { Text = "Cập Nhật DV", Location = new Point(300, 60), Width = 115 };
            Button btnXoaDV = new Button { Text = "Xóa DV", Location = new Point(425, 60), Width = 105 };

            btnThemDV.Click += (s, e) => {
                MessageBox.Show(dvBus.ThemDichVu(new MenuDichVuDTO { TenDV = txtTenDV.Text, DonGia = numGiaDV.Value, TonKho = (int)numTonKhoDV.Value }));
                LoadAllData();
            };
            btnSuaDV.Click += (s, e) => {
                if (!int.TryParse(txtMaDV.Text, out int id)) return;
                MessageBox.Show(dvBus.SuaDichVu(new MenuDichVuDTO { MaDV = id, TenDV = txtTenDV.Text, DonGia = numGiaDV.Value, TonKho = (int)numTonKhoDV.Value }));
                LoadAllData();
            };
            btnXoaDV.Click += (s, e) => {
                if (!int.TryParse(txtMaDV.Text, out int id)) return;
                MessageBox.Show(dvBus.XoaDichVu(id));
                LoadAllData();
            };

            dgvQLMenu = new DataGridView {
                Location = new Point(12, 100), Size = new Size(550, 200),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true, AllowUserToAddRows = false
            };
            dgvQLMenu.CellClick += (s, e) => {
                if (e.RowIndex < 0) return;
                var r = dgvQLMenu.Rows[e.RowIndex];
                if (r.Cells["MaDV"].Value == null) return;
                txtMaDV.Text = r.Cells["MaDV"].Value.ToString();
                txtTenDV.Text = r.Cells["TenDV"].Value.ToString();
                numGiaDV.Value = Convert.ToDecimal(r.Cells["DonGia"].Value);
                numTonKhoDV.Value = Convert.ToDecimal(r.Cells["TonKho"].Value);
            };
            grpMenu.Controls.AddRange(new Control[] { txtMaDV, txtTenDV, numGiaDV, numTonKhoDV, btnThemDV, btnSuaDV, btnXoaDV, dgvQLMenu });

            // Lịch sử Hóa đơn đã thanh toán
            GroupBox grpLichSu = new GroupBox { Text = "Lịch Sử Hóa Đơn Đã Thanh Toán", Location = new Point(10, 335), Size = new Size(1140, 325) };
            dgvLichSuHD = new DataGridView {
                Location = new Point(12, 28), Size = new Size(1115, 282),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false
            };
            grpLichSu.Controls.Add(dgvLichSuHD);

            tab.Controls.AddRange(new Control[] { grpBan, grpMenu, grpLichSu });
            return tab;
        }

        // ==================== XỬ LÝ SƠ ĐỒ BÀN & NGHIỆP VỤ ====================
        private void LoadSoDoBan()
        {
            flpSoDoBan.Controls.Clear();
            List<BanBidaDTO> dsBan = banBus.GetDanhSachBan();

            foreach (var ban in dsBan)
            {
                bool dangChoi = ban.TrangThai == "Đang chơi";
                Button btnBan = new Button
                {
                    Width = 132,
                    Height = 105,
                    Margin = new Padding(6),
                    BackColor = dangChoi ? Color.OrangeRed : Color.MediumSeaGreen,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    Text = $"{ban.TenBan}\n({ban.LoaiBan})\n{ban.GiaTheoGio:N0}đ/h\n[{ban.TrangThai.ToUpper()}]",
                    Tag = ban.MaBan
                };
                btnBan.Click += (s, e) => ChonBan((int)((Button)s).Tag);
                flpSoDoBan.Controls.Add(btnBan);
            }
        }

        private void ChonBan(int maBan)
        {
            var dsBan = banBus.GetDanhSachBan();
            banHienTai = dsBan.Find(b => b.MaBan == maBan);
            if (banHienTai == null) return;

            lblBanDangChon.Text = $"Bàn đang chọn: {banHienTai.TenBan} ({banHienTai.LoaiBan})";
            lblTrangThaiBan.Text = $"Trạng thái: {banHienTai.TrangThai} | Giá: {banHienTai.GiaTheoGio:N0} VNĐ/giờ";

            if (banHienTai.TrangThai == "Trống")
            {
                hoaDonHienTai = null;
                dtChiTietDisconnected = null;
                dgvDichVuBan.DataSource = null;

                lblGioBatDau.Text = "Giờ bắt đầu: (Chưa mở bàn)";
                lblThoiGianChoi.Text = "Thời gian chơi: 0 phút (0.00 giờ)";
                lblTienGio.Text = "Tiền giờ chơi: 0 VNĐ";
                lblTienDichVu.Text = "Tiền dịch vụ : 0 VNĐ";
                lblTongCong.Text = "TỔNG CỘNG    : 0 VNĐ";
                rtbHoaDon.Text = $"BÀN {banHienTai.TenBan.ToUpper()} ĐANG TRỐNG.\n\nNhấn nút 'MỞ BÀN & BẮT ĐẦU TÍNH GIỜ' ở góc dưới bên trái để nhận khách.";

                btnMoBan.Enabled = true;
                btnThemMon.Enabled = false;
                btnXoaMon.Enabled = false;
                btnLuuDataTable.Enabled = false;
                btnThanhToan.Enabled = false;
            }
            else
            {
                hoaDonHienTai = hdBus.GetHoaDonDangChoi(banHienTai.MaBan);
                if (hoaDonHienTai == null) return;

                lblGioBatDau.Text = $"Giờ bắt đầu: {hoaDonHienTai.GioBatDau:HH:mm:ss dd/MM/yyyy} (HĐ #{hoaDonHienTai.MaHD})";

                // Nạp dữ liệu theo mô hình ngắt kết nối (Disconnected DataTable)
                dtChiTietDisconnected = hdBus.GetChiTietDisconnected(hoaDonHienTai.MaHD);
                dgvDichVuBan.DataSource = dtChiTietDisconnected;
                if (dgvDichVuBan.Columns["MaHD"] != null) dgvDichVuBan.Columns["MaHD"].Visible = false;
                if (dgvDichVuBan.Columns["MaDV"] != null) dgvDichVuBan.Columns["MaDV"].ReadOnly = true;
                if (dgvDichVuBan.Columns["TenDV"] != null) dgvDichVuBan.Columns["TenDV"].ReadOnly = true;
                if (dgvDichVuBan.Columns["DonGia"] != null) dgvDichVuBan.Columns["DonGia"].ReadOnly = true;
                if (dgvDichVuBan.Columns["ThanhTien"] != null) dgvDichVuBan.Columns["ThanhTien"].ReadOnly = true;

                btnMoBan.Enabled = false;
                btnThemMon.Enabled = true;
                btnXoaMon.Enabled = true;
                btnLuuDataTable.Enabled = true;
                btnThanhToan.Enabled = true;

                CapNhatThongTinTinhTien();
            }
        }

        private void BtnThemMon_Click(object sender, EventArgs e)
        {
            if (hoaDonHienTai == null || dtChiTietDisconnected == null || cboDichVu.SelectedValue == null) return;

            int maDV = Convert.ToInt32(cboDichVu.SelectedValue);
            int soLuongThem = (int)numSoLuongMon.Value;
            DataRow[] menuRows = dtMenuCache.Select($"MaDV = {maDV}");
            if (menuRows.Length == 0) return;

            string tenDV = menuRows[0]["TenDV"].ToString();
            decimal donGia = Convert.ToDecimal(menuRows[0]["DonGia"]);

            // Thao tác trực tiếp trên DataTable ngắt kết nối (Disconnected Model)
            DataRow existingRow = dtChiTietDisconnected.Rows.Find(new object[] { hoaDonHienTai.MaHD, maDV });
            if (existingRow != null)
            {
                int slCu = Convert.ToInt32(existingRow["SoLuong"]);
                int slMoi = slCu + soLuongThem;
                existingRow["SoLuong"] = slMoi;
                existingRow["ThanhTien"] = slMoi * donGia;
            }
            else
            {
                DataRow newRow = dtChiTietDisconnected.NewRow();
                newRow["MaHD"] = hoaDonHienTai.MaHD;
                newRow["MaDV"] = maDV;
                newRow["TenDV"] = tenDV;
                newRow["SoLuong"] = soLuongThem;
                newRow["DonGia"] = donGia;
                newRow["ThanhTien"] = soLuongThem * donGia;
                dtChiTietDisconnected.Rows.Add(newRow);
            }

            // Đồng bộ DataTable xuống SQL Server qua SqlDataAdapter
            hdBus.LuuChiTietDisconnected(dtChiTietDisconnected);
            CapNhatThongTinTinhTien();
        }

        private void CapNhatThongTinTinhTien()
        {
            if (banHienTai == null || hoaDonHienTai == null || banHienTai.TrangThai != "Đang chơi") return;

            DateTime gioHienTai = DateTime.Now;
            var (tongPhut, soGioLamTron, tienGio) = hdBus.TinhTienGioChoi(hoaDonHienTai.GioBatDau, gioHienTai, banHienTai.GiaTheoGio);

            decimal tienDV = 0;
            if (dtChiTietDisconnected != null)
            {
                foreach (DataRow r in dtChiTietDisconnected.Rows)
                {
                    if (r.RowState != DataRowState.Deleted)
                        tienDV += Convert.ToDecimal(r["ThanhTien"]);
                }
            }

            decimal tongCong = tienGio + tienDV;
            lblThoiGianChoi.Text = $"Thời gian chơi: {tongPhut} phút (Làm tròn: {soGioLamTron:0.##} giờ)";
            lblTienGio.Text = $"Tiền giờ chơi: {tienGio:N0} VNĐ";
            lblTienDichVu.Text = $"Tiền dịch vụ : {tienDV:N0} VNĐ";
            lblTongCong.Text = $"TỔNG CỘNG    : {tongCong:N0} VNĐ";

            // Hiển thị hóa đơn tạm tính trực tiếp trên khung phải
            rtbHoaDon.Text = TaoNoiDungHoaDon(hoaDonHienTai.MaHD, banHienTai.TenBan, hoaDonHienTai.GioBatDau, gioHienTai, soGioLamTron, tienGio, tienDV, "CHƯA THANH TOÁN (TẠM TÍNH)");
        }

        private void BtnThanhToan_Click(object sender, EventArgs e)
        {
            if (banHienTai == null || hoaDonHienTai == null) return;

            // Lưu các thay đổi cuối cùng trên DataTable nếu người dùng vừa sửa trực tiếp trên lưới
            if (dtChiTietDisconnected != null && dtChiTietDisconnected.GetChanges() != null)
                hdBus.LuuChiTietDisconnected(dtChiTietDisconnected);

            DateTime gioKetThuc = DateTime.Now;
            var (_, soGioLamTron, tienGio) = hdBus.TinhTienGioChoi(hoaDonHienTai.GioBatDau, gioKetThuc, banHienTai.GiaTheoGio);

            decimal tienDV = 0;
            if (dtChiTietDisconnected != null)
            {
                foreach (DataRow r in dtChiTietDisconnected.Rows)
                {
                    if (r.RowState != DataRowState.Deleted)
                        tienDV += Convert.ToDecimal(r["ThanhTien"]);
                }
            }

            string ketQua = hdBus.ThanhToan(hoaDonHienTai.MaHD, banHienTai.MaBan, gioKetThuc, tienGio, tienDV);
            if (ketQua == "OK")
            {
                string hoaDonIn = TaoNoiDungHoaDon(hoaDonHienTai.MaHD, banHienTai.TenBan, hoaDonHienTai.GioBatDau, gioKetThuc, soGioLamTron, tienGio, tienDV, "ĐÃ THANH TOÁN");
                rtbHoaDon.Text = hoaDonIn;

                MessageBox.Show($"Thanh toán thành công Hóa đơn #{hoaDonHienTai.MaHD}!\nTổng tiền thu: {(tienGio + tienDV):N0} VNĐ\nBàn {banHienTai.TenBan} đã chuyển về trạng thái Trống.",
                    "Thanh toán hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadAllData();
                btnThemMon.Enabled = false;
                btnXoaMon.Enabled = false;
                btnLuuDataTable.Enabled = false;
                btnThanhToan.Enabled = false;
                btnMoBan.Enabled = false;
            }
            else
            {
                MessageBox.Show("CẢNH BÁO GIAO DỊCH BỊ HỦY:\n" + ketQua, "Lỗi Thanh Toán (Rollback)", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private string TaoNoiDungHoaDon(int maHD, string tenBan, DateTime batDau, DateTime ketThuc, decimal soGio, decimal tienGio, decimal tienDV, string trangThai)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("==============================");
            sb.AppendLine("    CLB BIDA GIẢI TRÍ PRO     ");
            sb.AppendLine("     HÓA ĐƠN TÍNH TIỀN        ");
            sb.AppendLine("==============================");
            sb.AppendLine($"Số HĐ   : #{maHD}");
            sb.AppendLine($"Bàn chơi: {tenBan}");
            sb.AppendLine($"Vào lúc : {batDau:HH:mm dd/MM/yyyy}");
            sb.AppendLine($"Ra lúc  : {ketThuc:HH:mm dd/MM/yyyy}");
            sb.AppendLine($"Số giờ  : {soGio:0.##} giờ");
            sb.AppendLine("------------------------------");
            sb.AppendLine("Dịch vụ gọi thêm:");

            if (dtChiTietDisconnected != null && dtChiTietDisconnected.Rows.Count > 0)
            {
                foreach (DataRow r in dtChiTietDisconnected.Rows)
                {
                    if (r.RowState == DataRowState.Deleted) continue;
                    string ten = r["TenDV"].ToString();
                    int sl = Convert.ToInt32(r["SoLuong"]);
                    decimal tt = Convert.ToDecimal(r["ThanhTien"]);
                    sb.AppendLine($"- {ten} x{sl}: {tt:N0}đ");
                }
            }
            else
            {
                sb.AppendLine("  (Không gọi dịch vụ)");
            }

            sb.AppendLine("------------------------------");
            sb.AppendLine($"Tiền giờ chơi : {tienGio,12:N0}đ");
            sb.AppendLine($"Tiền dịch vụ  : {tienDV,12:N0}đ");
            sb.AppendLine($"TỔNG CỘNG     : {(tienGio + tienDV),12:N0}đ");
            sb.AppendLine("------------------------------");
            sb.AppendLine($"[{trangThai}]");
            sb.AppendLine("==============================");
            return sb.ToString();
        }

        private void LoadAllData()
        {
            LoadSoDoBan();

            dtMenuCache = dvBus.GetAllMenu();
            DataTable dtCombo = dtMenuCache.Copy();
            dtCombo.Columns.Add("HienThi", typeof(string), "TenDV + ' - ' + DonGia + 'đ'");
            cboDichVu.DataSource = dtCombo;
            cboDichVu.DisplayMember = "HienThi";
            cboDichVu.ValueMember = "MaDV";

            dgvQLBan.DataSource = banBus.GetAllBanTable();
            dgvQLMenu.DataSource = dtMenuCache;
            dgvLichSuHD.DataSource = hdBus.GetLichSuHoaDon();
        }
    }
}