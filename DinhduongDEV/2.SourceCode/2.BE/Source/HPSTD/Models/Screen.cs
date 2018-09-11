using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.DataAnnotations;

namespace HPSTD.Models
{
    public class Screen
    {
        [AutoIncrement]
        public int id { get; set; }
        public string ma_man_hinh { get; set; }
        public string ten_man_hinh { get; set; }
        public string ghi_chu { get; set; }
        public string trang_thai { get; set; }
        public DateTime ngay_tao { get; set; }
        public string nguoi_tao { get; set; }
        public DateTime ngay_cap_nhat { get; set; }
        public string nguoi_cap_nhat { get; set; }
    }
}