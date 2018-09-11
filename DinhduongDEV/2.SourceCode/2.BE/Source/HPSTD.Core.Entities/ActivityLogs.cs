using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPSTD.Core.Entities
{
    public class ActivityLogs : BaseEntity
    {
        public string ma_nguoi_dung { get; set; }
        public string ma_man_hinh { get; set; }
        public string thao_tac { get; set; }
        public string chi_tiet { get; set; }
        [Ignore]
        public string ten_nguoi_dung { get; set; }
        [Ignore]
        public string ten_man_hinh { get; set; }
    }
}
