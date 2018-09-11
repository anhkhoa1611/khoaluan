using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class ProductCategory: BaseEntity
    {
        public string ma_phan_cap { get; set; }
        public string ten_phan_cap { get; set; }
        public string ma_phan_cap_cha { get; set; }
        public int cap { get; set; }
    }
}
