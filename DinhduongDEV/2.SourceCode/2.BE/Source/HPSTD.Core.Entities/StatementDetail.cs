using System;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class StatementDetail : BaseEntity
    {
        public string ma_phieu_header { get; set; }
        public string ma_san_pham { get; set; }
        public int so_luong { get; set; }
        //public int ma_pyc { get; set; }
        public double don_gia_vat { get; set; }
        public string don_vi_tinh { get; set; }
        public double don_gia { get; set; }
        public double thue_vat { get; set; }
        public string thong_so_ky_thuat { get; set; }
        public string muc_dich_su_dung { get; set; }
        public string ma_nha_cung_cap { get; set; }
        public string ma_don_vi { get; set; }
        public string ma_don_dat_hang { get; set; }
        public string ma_chinh_sach_gia { get; set; }

        [Ignore]
        public string ma_phieu_pr { get; set; }
        [Ignore]
        public string ten_san_pham { get; set; }
        public string ma_pyc_header { get; set; }
        [Ignore]
        public string ten_don_vi { get; set; }
        [Ignore]
        public double thanh_tien { get; set; }
        [Ignore]
        public string ten_nha_cung_cap { get; set; }
        [Ignore]
        public string dia_chi_don_vi { get; set; }
        public string ma_chi_nhanh { get; set; }
        public string noi_dung_xac_nhan_ton_kho { get; set; }
        public string noi_dung_xac_nhan_cap_3 { get; set; }
        [Ignore]
        public string thong_tin_hop_dong { get; set; }
        [Ignore]
        public string thong_tin_noi_bo { get; set; }
    }
}
