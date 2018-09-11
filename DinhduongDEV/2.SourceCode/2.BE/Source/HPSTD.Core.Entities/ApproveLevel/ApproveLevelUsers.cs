using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class ApproveLevelUsers : BaseEntity
    { 
        public int id_cap_duyet { get; set; }
        public string ma_nguoi_dung { get; set; }
        public string ma_nhom_chuyen_mon { get; set; }
        [Ignore]
        public string ten_nguoi_dung { get; set; }
       
   
      
    }
}
