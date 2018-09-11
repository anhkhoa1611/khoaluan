using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPSTD.Core.Entities
{
    public class POHeader : BaseEntity
    {
        public string ma_phieu { get; set; }
        //public string ten_phieu { get; set; }
        public string ghi_chu { get; set; }
        public string ma_nha_cung_cap { get; set; }
        public DateTime? ngay_duyet { get; set; }
        [Ignore]
        public string ListStatement { get; set; }

    }
}
