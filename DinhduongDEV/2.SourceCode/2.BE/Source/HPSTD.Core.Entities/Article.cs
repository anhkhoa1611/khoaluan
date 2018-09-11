
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class Article : BaseEntity
    {
        public string tieu_de { get; set; }
        public string noi_dung { get; set; }
        public string file_dinh_kem { get; set; }
        public string loai_tin { get; set; }
        public Boolean xem_truoc { get; set; }
        public string mo_ta { get; set; }
        [Ignore]
        public List<string> ma_chi_nhanh { get; set; }
        [Ignore]
        public List<string> danh_sach_chi_nhanh { get; set; }
        [Ignore]
        public string tin_tuc { get; set; }
    }
    public class ArticleBranch
    {
        [AutoIncrement]
        public int id { get; set; }
        public int ma_tin_id { get; set; }
        public string ma_chi_nhanh { get; set; }
        public string nguoi_tao { get; set; }
        public DateTime ngay_tao { get; set; }
    }
}