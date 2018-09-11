using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class ApproveLevel : BaseEntity
    {
        public string cap_duyet { get; set; }
        public string ten_cap_duyet { get; set; }           
        public string ma_nhom_chuyen_mon  { get; set; }      
        public string ghi_chu { get; set; }      
        [Ignore]
        public string danh_sach_nguoi_dung { get; set; }
        [Ignore]
        public List<string> lstnguoi_dung { get; set; }
    }
}
