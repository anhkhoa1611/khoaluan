using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class PlanDetail : BaseEntity
    {
        public int nam_ke_hoach { get; set; }
        public int ma_san_pham { get; set; }
        public int ma_don_vi { get; set; }
        public string don_vi_phu_trach { get; set; }
        public string ma_don_vi_tinh { get; set; }
        public int so_luong_du_kien { get; set; }
        public double don_gia_du_kien_vat { get; set; }
        public double total_tien_du_kien { get; set; }
        public double ke_hoach_nam_truoc { get; set; }
        public double thuc_hien_nam_truoc { get; set; }
        public double chech_lech { get; set; }
        public string ghi_chu { get; set; }
        [Ignore]
        public int ma_nhom_san_pham { get; set; }
        [Ignore]
        public string ten_don_vi_tinh { get; set; }
        [Ignore]
        public string ten_san_pham { get; set; }
        

    }
}
