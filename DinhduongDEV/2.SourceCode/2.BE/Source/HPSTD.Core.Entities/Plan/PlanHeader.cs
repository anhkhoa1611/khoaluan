using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class PlanHeader : BaseEntity
    { 
        public int nam_ke_hoach { get; set; }
        public string ten_ke_hoach { get; set; }
        public string don_vi_phu_trach { get; set; }
        public string nguoi_duyet { get; set; }
        public DateTime ngay_duyet { get; set; }
        public string ghi_chu { get; set; }    
    }
}
