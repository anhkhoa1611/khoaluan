using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPSTD.Core.Entities
{
    public class Company: BaseEntity
    {
        public string ten_cong_ty { get; set; }
        public string so_dien_thoai { get; set; }
        public string dia_chi { get; set; }
        public string email { get; set; }
        public string fax { get; set; }
        public string website { get; set; }
        public string ghi_chu { get; set; }
    }
}
