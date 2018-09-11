using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;
using HPSTD.Core.Entities;

namespace HPSTD.Core.Entities
{
    public class UsersBranch
    {
        [AutoIncrement]
        [PrimaryKey]
        public int id { get; set; }
        public int id_nguoi_dung { get; set; }
        public DateTime? ngay_tao { get; set; }
        public string nguoi_tao { get; set; }
        public DateTime? ngay_cap_nhat { get; set; }
        public string nguoi_cap_nhat { get; set; }
        public string ma_nguoi_dung { get; set; }
        public string ma_chi_nhanh { get; set; }

        [Ignore]
        public string ten_chi_nhanh { get; set; }

        [Ignore]
        public string ten_nguoi_dung { get; set; }

        [Ignore]
        public string ten_phong_ban { get; set; }

        [Ignore]
        public string ten_chuc_vu { get; set; }
        
    }
}
