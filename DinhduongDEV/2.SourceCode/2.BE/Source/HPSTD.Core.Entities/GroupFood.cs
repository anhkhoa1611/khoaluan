using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPSTD.Core.Entities
{
    public class GroupFood : BaseEntity
    {
        public string ma_nhom_thuc_pham { get; set; }
        public string ten_nhom_thuc_pham { get; set; }
        public string ghi_chu { get; set; }
    }
}
