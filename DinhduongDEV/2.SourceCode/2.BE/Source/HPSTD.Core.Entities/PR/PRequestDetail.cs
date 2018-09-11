using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class PRequestDetail : BaseEntity
    {
        public string ma_phieu { get; set; }
        public string ma_san_pham { get; set; }
        public int so_luong { get; set; }
        public int so_luong_duyet { get; set; }
        public string thong_so_ky_thuat { get; set; }
        public string chuc_danh_nguoi_su_dung { get; set; }
        public string don_vi_tinh { get; set; }
        public double don_gia_vat { get; set; }
        public double don_gia { get; set; }
        public double thue_vat { get; set; }
        public Boolean ke_hoach_nam { get; set; }
        public string ma_to_trinh { get; set; }
        public string ma_nha_cung_cap { get; set; }
        [Ignore]
        public string ten_san_pham { get; set; }
        [Ignore]
        public string ma_don_vi { get; set; }
        [Ignore]
        public string ma_chi_nhanh { get; set; }
        [Ignore]
        public double thanh_tien { get; set; }
        public string ma_chinh_sach_gia { get; set; }

        public string noi_dung_truong_don_vi_xac_nhan { get; set; }
        public string noi_dung_xac_nhan_ton_kho { get; set; }
        public string noi_dung_xac_nhan_cap_3 { get; set; }
        public string ma_san_pham_thay_the { get; set; }
    }
}
