using System;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class WareHouse : BaseEntity
    {
        public string kho_id { get; set; }
        public string loai_kho { get; set; }
        public string cong_ty_id { get; set; }
        public string chi_nhanh_id { get; set; }
        public string ten_kho { get; set; }
        public string dia_chi { get; set; }
        public string email { get; set; }
        public string thu_kho { get; set; }
        public string dien_thoai_thu_kho { get; set; }
        public string ghi_chu { get; set; }
    }
}
