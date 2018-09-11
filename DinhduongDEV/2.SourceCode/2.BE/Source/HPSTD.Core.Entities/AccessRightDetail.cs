using ServiceStack.DataAnnotations;
using System;

namespace HPSTD.Core.Entities
{
    public class AccessRightDetail
    {
        [AutoIncrement]
        public int id { get; set; }
        public int ma_nhom { get; set; }
        public string ma_man_hinh { get; set; }
        public bool xem { get; set; }
        public bool them { get; set; }
        public bool xoa { get; set; }
        public bool sua { get; set; }
        public bool xuat { get; set; }
        public bool nhap { get; set; }
        public bool phan_quyen { get; set; }
        public bool xuat_pdf { get; set; }
        public DateTime ngay_tao { get; set; }
        public string nguoi_tao { get; set; }
        public DateTime ngay_cap_nhat { get; set; }
        public string nguoi_cap_nhat { get; set; }
        [Ignore]
        public int id_cha { get; set; }
        [Ignore]
        public int id_menu { get; set; }
        [Ignore]
        public string thu_tu { get; set; }
    }
}
