using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class Product : BaseEntity
    {
        public string ma_san_pham { get; set; }
        public string ten_san_pham { get; set; }
        public string ma_don_vi_tinh { get; set; }
        public string ma_nhom_san_pham { get; set; }
        public string thong_so_ky_thuat { get; set; }
        public string loai_hang_hoa { get; set; }
        [Ignore]
        public double gia_bao_gom_vat { get; set; }
        [Ignore]
        public double thue_vat { get; set; }
        [Ignore]
        public double don_gia { get; set; }
        [Ignore]
        public string don_vi_tinh { get; set; }
    }
}
