using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class ProductPriceHeader : BaseEntity
    {
        public string ma_chinh_sach_gia { get; set; }
        public string nha_cung_cap_id { get; set; }
        public DateTime ngay_ap_dung { get; set; }
        public DateTime ngay_ket_thuc { get; set; }
        public DateTime ngay_bao_gia { get; set; }
        public DateTime ngay_ky_hop_dong { get; set; }
        public string ghi_chu { get; set; }
        public string so_hop_dong { get; set; }
        public string file_hop_dong { get; set; }
        public string file_bao_gia { get; set; }
        [Ignore]
        public string ten_nha_cung_Cap { get; set; }
        [Ignore]
        public string dien_thoai { get; set; }
        [Ignore]
        public string email { get; set; }
        [Ignore]
        public string dia_chi { get; set; }
        [Ignore]
        public string website { get; set; }
    }
}
