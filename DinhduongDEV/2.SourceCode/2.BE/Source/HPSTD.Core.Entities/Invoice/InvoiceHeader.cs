using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class InvoiceHeader : BaseEntity
    {
        public string ma_phieu { get; set; }
        public string ma_don_vi { get; set; }
        public string ma_phieu_nhap_kho { get; set; }
        public string ghi_chu { get; set; }
        public string ma_hoa_don { get; set; }
        public DateTime? ngay_hoa_don { get; set; }


        [Ignore]
        public string ten_don_vi { get; set; }
    }
}
