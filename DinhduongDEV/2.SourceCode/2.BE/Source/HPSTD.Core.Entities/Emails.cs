using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPSTD.Core.Entities
{
    public class Emails : BaseEntity
    {       
        public string email { get; set; }
        public string password { get; set; }
        public string mail_server { get; set; }
        public bool enable_ssl { get; set; }
        public bool is_default { get; set; }     
        public int port { get; set; }
    }
}
