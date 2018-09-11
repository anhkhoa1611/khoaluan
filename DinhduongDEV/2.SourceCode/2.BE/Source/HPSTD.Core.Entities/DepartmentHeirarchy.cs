using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPSTD.Core.Entities
{
    public class DepartmentHeirarchy : BaseEntity
    {      
        public string ma_phan_cap { get; set; }
        public string ten_phan_cap { get; set; }
        public int cap { get; set; }      
        public string loai_phan_cap { get; set; }
        public string ten_loai_phan_cap { get; set; }
        public string ma_phan_cap_cha { get; set; }
        public int thu_tu { get; set; }     
    }
}
