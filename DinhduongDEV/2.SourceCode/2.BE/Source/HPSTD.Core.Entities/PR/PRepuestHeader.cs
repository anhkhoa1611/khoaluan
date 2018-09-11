using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class PRequestHeader : BaseEntity
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

        public string file_chu_ki_TDV { get; set; }

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
        [Ignore]
        public string ten_nguoi_tao { get; set; }
        [Ignore]
        public string ten_nguoi_duyet_TDV { get; set; }
        [Ignore]
        public string ten_nguoi_duyet_HCQT { get; set; }
        [Ignore]
        public string ten_nguoi_duyet_TTCNTT_NHDT { get; set; }
        [Ignore]
        public string ten_nguoi_duyet_QLDVKH_NQT { get; set; }
        [Ignore]
        public string ten_nguoi_duyet_khac_HO { get; set; }

    }
}
