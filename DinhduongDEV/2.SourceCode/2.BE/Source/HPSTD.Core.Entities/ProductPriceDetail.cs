using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class ProductPriceDetail : BaseEntity
    {
        public string ma_chinh_sach_gia { get; set; }
        public string ma_vat_tu { get; set; }
        public double gia_bao { get; set; }
        public double thue_vat { get; set; }
        public double gia_bao_gom_vat { get; set; }
        public DateTime ngay_ap_dung { get; set; }
        public DateTime ngay_ket_thuc { get; set; }
        public string don_vi_tinh { get; set; }
        public double sl_min { get; set; }
        public double sl_max { get; set; }
        [Ignore]
        public string ten_san_pham { get; set; }

    }
    public class ProductPriceReport : ProductPriceDetail
    {
        [Ignore]
        public string nha_cung_cap_id { get; set; }
        [Ignore]
        public string ten_nha_cung_Cap { get; set; }
        [Ignore]
        public string dien_thoai { get; set; }
        [Ignore]
        public string so_hop_dong { get; set; }
        [Ignore]
        public String email { get; set; }
        [Ignore]
        public string ghi_chu { get; set; }
    }
}
