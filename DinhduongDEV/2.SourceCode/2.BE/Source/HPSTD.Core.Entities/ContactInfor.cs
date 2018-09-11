using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPSTD.Core.Entities
{
    public class ContactInfor:BaseEntity
    {
        public string ten_nguoi_lien_he { get; set; }
        public string email { get; set; }
        public string so_dien_thoai { get; set; }
        public string nha_cung_cap_id { get; set; }
        public Boolean nguoi_lien_he_chinh { get; set; }
    }
}
