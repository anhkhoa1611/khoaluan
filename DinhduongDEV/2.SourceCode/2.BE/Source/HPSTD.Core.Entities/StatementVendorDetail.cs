using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class StatementVendorDetail : BaseEntity
    {
        public string ma_to_trinh_ncc { get; set; }
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
}
