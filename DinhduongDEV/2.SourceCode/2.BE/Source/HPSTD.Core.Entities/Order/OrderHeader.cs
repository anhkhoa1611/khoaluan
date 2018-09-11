using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPSTD.Core.Entities
{
    public class OrderHeader:BaseEntity
    {
        public string so_don_hang { get; set; }
        public string ten_don_hang { get; set; }
        public int ma_don_vi { get; set; }
        public DateTime ngay_to_trinh { get; set; }
    }
}
