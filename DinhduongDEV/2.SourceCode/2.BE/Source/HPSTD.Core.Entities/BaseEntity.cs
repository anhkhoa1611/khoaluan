using ServiceStack.DataAnnotations;
using System;

namespace HPSTD.Core.Entities
{
    public class BaseEntity
    {
        [AutoIncrement]
        [PrimaryKey]
        public int id { get; set; }
        public string trang_thai { get; set; }
        public DateTime? ngay_tao { get; set; }
        public string nguoi_tao { get; set; }
        public DateTime? ngay_cap_nhat { get; set; }
        public string nguoi_cap_nhat { get; set; }
    }
}
