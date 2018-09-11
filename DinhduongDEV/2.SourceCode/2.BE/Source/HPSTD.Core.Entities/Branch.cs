using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPSTD.Core.Entities
{
    public class Branch : BaseEntity
    {
        public string ma_don_vi { get; set; }
        public string ma_chi_nhanh { get; set; }
        public string ten_chi_nhanh { get; set; }
        public string dien_thoai_lien_he { get; set; }
        public string dia_chi { get; set; }
        public string email_lien_he { get; set; }
        public string fax { get; set; }
        public string ghi_chu { get; set; }
        public string dt_nguoi_nhan_hang { get; set; }
        public string ten_nguoi_nhan_hang { get; set; }
        public string dia_chi_nhan_hang { get; set; }
    }
}
