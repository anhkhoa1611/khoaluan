using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPSTD.Core.Entities
{
    public class Group : BaseEntity
    {
        public string ma_nhom { get; set; }
        public string ten_nhom { get; set; }
        public string ghi_chu { get; set; }
        [Ignore]
        public List<AccessRightDetail> listAsset { get; set; }
    }
}
