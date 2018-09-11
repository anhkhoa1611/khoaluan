using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class EmailContent
    {
        [AutoIncrement]
        public int id { get; set; }
        public string mau_email { get; set; }
        public string mailTo { get; set; }
        public string mailCc { get; set; }
        public string mailBcc { get; set; }
        public string tieu_de { get; set; }
        public string noi_dung { get; set; }
        public string nguoi_tao { get; set; }
        public DateTime ngay_tao { get; set; }
        public string nguoi_cap_nhat { get; set; }
        public DateTime ngay_cap_nhat { get; set; }
    }
}
