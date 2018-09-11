using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPSTD.Core.Entities
{
    public class Parameters : BaseEntity
    {
        public string ma_tham_so { get; set; }
        public string loai_tham_so { get; set; }
        public string gia_tri { get; set; }
        public string mo_ta { get; set; }
    }
}
