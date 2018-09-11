using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HPSTD.Core.Entities
{
    public class UserProductCategory
    {
        [AutoIncrement]
        [PrimaryKey]
        public int id { get; set; }
        public int id_nguoi_dung { get; set; }
        public string ma_nguoi_dung { get; set; }
        public string ma_phan_cap { get; set; }
        public DateTime? ngay_tao { get; set; }
        public string nguoi_tao { get; set; }
        public DateTime? ngay_cap_nhat { get; set; }
        public string nguoi_cap_nhat { get; set; }
        [Ignore]
        public string ten_nguoi_dung { get; set; }
        [Ignore]
        public string ma_chi_nhanh { get; set; }
        [Ignore]
        public string ListProduct { get; set; }

        [Ignore]
        public List<string> lstma_phan_cap { get; set; }
    }
}
