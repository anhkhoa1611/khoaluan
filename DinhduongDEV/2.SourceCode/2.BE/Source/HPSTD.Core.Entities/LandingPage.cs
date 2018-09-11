using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPSTD.Core.Entities
{
    public class LandingPage : BaseEntity
    {

        public string loai { get; set; }
        public string ten_loai { get; set; }
        public string tieu_de { get; set; }
        public string noi_dung { get; set; }
        public string ten_file_anh { get; set; }
        public string duong_dan_anh { get; set; }       
        public string ghi_chu { get; set; }
       
    }
}
