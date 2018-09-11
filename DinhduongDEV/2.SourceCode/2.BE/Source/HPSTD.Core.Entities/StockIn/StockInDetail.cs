using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPSTD.Core.Entities
{
    public class StockInDetail : BaseEntity
    {
        public string ma_phieu_header { get; set; }
        public string ma_san_pham { get; set; }
        public int so_luong { get; set; }
        public int so_luong_da_nhap { get; set; }
        public int so_luong_con_lai { get; set; }
        public string don_vi_tinh { get; set; }
        public double don_gia_vat { get; set; }
        public double don_gia { get; set; }
        public double thue_vat { get; set; }
        public double chi_phi { get; set; }
        public string thong_so_ky_thuat { get; set; }
        public string muc_dich_su_dung { get; set; }
        public string ma_don_vi { get; set; }
        public string ma_to_trinh { get; set; }
        public int id_StatementDetail { get; set; }

        public string ngay_nhap { get; set; }

        public int id_po_detail { get; set; }
        [Ignore]
        public double thanh_tien { get; set; }
        [Ignore]
        public string dia_chi_don_vi { get; set; }
        [Ignore]
        public DateTime ngay_to_trinh { get; set; }
        [Ignore]
        public string ten_don_vi { get; set; }
        [Ignore]
        public string ten_san_pham { get; set; }
        [Ignore]
        public string ten_don_vi_tinh { get; set; }

        public string ma_chi_nhanh { get; set; }
        public string thong_tin_noi_bo { get; set; }

        [Ignore]
        public int so_luong_nhap { get; set; }

        
    }
}
