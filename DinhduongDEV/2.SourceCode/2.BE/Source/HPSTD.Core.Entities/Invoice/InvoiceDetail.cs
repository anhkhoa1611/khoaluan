using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class InvoiceDetail : BaseEntity
    {
        public string ma_phieu_header { get; set; }
        public string ma_san_pham { get; set; }
        public int so_luong { get; set; }
        public string don_vi_tinh { get; set; }
        public double don_gia_vat { get; set; }
        public double don_gia { get; set; }
        public double thue_vat { get; set; }
        public string ma_phieu_nhap_kho { get; set; }
        public int id_nhap_kho { get; set; }
        [Ignore]
        public int so_luong_nhap { get; set; }
        public int so_luong_da_nhap { get; set; }
        public double chi_phi { get; set; }
        public string thong_so_ky_thuat { get; set; }
        public string muc_dich_su_dung { get; set; }
        [Ignore]
        public string chi_phi_nhap { get; set; }
    }
}
