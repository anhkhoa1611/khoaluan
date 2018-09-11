using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPSTD.Core.Entities
{
    public class OrderDetail:BaseEntity
    {
        public int order_id_header { get; set; }
        public int ma_san_pham { get; set; }
        public string ma_don_vi_tinh { get; set; }
        public int so_luong { get; set; }
        public double don_gia_chua_VAT { get; set; }
        public double don_gia_da_VAT { get; set; }
        public string dia_chi_giao_hang { get; set; }
        public string thong_tin_noi_bo { get; set; }
    }
}
