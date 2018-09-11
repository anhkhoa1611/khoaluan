using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class ReportStatistical
    {
        public string so_po { get; set; }
        public DateTime? ngay_tao_po { get; set; }
        public string don_vi_yeu_cau { get; set; }
        public string ma_san_pham { get; set; }
        public string ten_san_pham { get; set; }
        public int? so_luong { get; set; }
        public double chi_phi_thuc_te_vat { get; set; }
        public string ghi_chu { get; set; }

    }
}
