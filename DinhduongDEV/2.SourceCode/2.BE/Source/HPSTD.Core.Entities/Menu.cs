using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.SqlServer;

namespace HPSTD.Core.Entities
{
    public class Menu : BaseEntity
    {         
        public string ten_chuc_nang { get; set; }
        public string ma_man_hinh { get; set; }
        public string thu_tu { get; set; }
        public int id_cha { get; set; }
        public int cap { get; set; }
        public string icon { get; set; }
    }
}
