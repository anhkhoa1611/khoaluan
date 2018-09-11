using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPSTD.Core.Entities
{
    public class UserInGroup
    {
        [AutoIncrement]
        [PrimaryKey]
        public int id { get; set; }
        public string ma_nguoi_dung { get; set; }
        public int id_nhom_nguoi_dung { get; set; }
        public DateTime? ngay_tao { get; set; }
        public string nguoi_tao { get; set; }
        public DateTime? ngay_cap_nhat { get; set; }
        public string nguoi_cap_nhat { get; set; }


        [Ignore]
        public string ten_nguoi_dung { get; set; }
        [Ignore]
        public int ma_nhom { get; set; }
        [Ignore]
        public int thuoc_nhom { get; set; }
        [Ignore]
        public string email { get; set; }
        [Ignore]
        public string ma_chi_nhanh { get; set; }
        [Ignore]
        public string ten_chi_nhanh { get; set; }
    }
}

