using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class VendorProductCategory
    {
        [AutoIncrement]
        public int id { get; set; }
        public string nha_cung_cap_id { get; set; }
        public string ma_phan_cap { get; set; }
        public string nguoi_tao { get; set; }
        public DateTime ngay_tao { get; set; }
    }
}
