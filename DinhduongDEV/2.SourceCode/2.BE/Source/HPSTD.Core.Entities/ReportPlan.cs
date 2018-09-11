using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPSTD.Core.Entities
{
    public class ReportPlan
    {
        public int nam_ke_hoach { get; set; }
        public string ten_chi_nhanh { get; set; }
        public int so_luong_du_kien { get; set; }
        public string ten_san_pham { get; set; }
        public int total_tien_du_kien { get; set; }
        public int ke_hoach_nam_truoc { get; set; }
        public int thuc_hien_nam_truoc { get; set; }
        public int chech_lech { get; set; }
    }
}
