using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPSTD.Core.Entities
{
    public class ReportPR : BaseEntity
    {
        public string ma_phieu { get; set; }
        public string ten_phieu { get; set; }
        public string ma_don_vi { get; set; }
        public string ma_chi_nhanh { get; set; }
        public DateTime ngay_tao_yeu_cau { get; set; }
        public DateTime ngay_giao { get; set; }
        public bool mua_moi { get; set; }
        public bool thay_the { get; set; }
        public string tai_san_cu_1 { get; set; }
        public string tai_san_cu_2 { get; set; }
        public string tai_san_cu_3 { get; set; }
        public string ly_do_thay_the { get; set; }
        public int so_luong_dinh_bien_nhan_su { get; set; }
        public int so_luong_dinh_muc { get; set; }
        public string ly_do_mua_them { get; set; }

        public string y_kien_cua_don_vi { get; set; }
        public DateTime? ngay_duyet_TDV { get; set; }
        public string nguoi_duyet_TDV { get; set; }

        public string ten_nguoi_lap_de_nghi { get; set; }
        public string so_dt_lien_lac_nguoi_lap { get; set; }

        public string y_kien_HCQT { get; set; }
        public DateTime? ngay_duyet_HCQT { get; set; }
        public string nguoi_duyet_HCQT { get; set; }

        public string y_kien_TTCNTT_NHDT { get; set; }
        public DateTime? ngay_duyet_TTCNTT_NHDT { get; set; }
        public string nguoi_duyet_TTCNTT_NHDT { get; set; }

        public string y_kien_QLDVKH_NQT { get; set; }
        public DateTime? ngay_duyet_QLDVKH_NQT { get; set; }
        public string nguoi_duyet_QLDVKH_NQT { get; set; }

        public string y_kien_khac_HO { get; set; }
        public DateTime? ngay_duyet_khac_HO { get; set; }
        public string nguoi_duyet_khac_HO { get; set; }

        public string y_kien_GDTC { get; set; }
        public DateTime? ngay_duyet_GDTC { get; set; }

        public string y_kien_TGD { get; set; }
        public DateTime? ngay_duyet_TGD { get; set; }

        public string trang_thai_GDTC { get; set; }
        public string trang_thai_TGD { get; set; }

        [Ignore]
        public string ten_chi_nhanh { get; set; }
        [Ignore]
        public string ten_phan_cap { get; set; }
        [Ignore]
        public string dt_nguoi_nhan_hang { get; set; }
        [Ignore]
        public string ten_nguoi_nhan_hang { get; set; }
        [Ignore]
        public string dia_chi_nhan_hang { get; set; }
        public string ma_san_pham { get; set; }
        public int so_luong { get; set; }
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
    }
}
