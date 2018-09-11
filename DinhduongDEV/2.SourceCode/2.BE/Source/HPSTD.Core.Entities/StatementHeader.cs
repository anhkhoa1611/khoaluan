using System;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class StatementHeader : BaseEntity
    {
        public string ma_phieu { get; set; }
        public string ten_phieu { get; set; }
        public DateTime ngay_tao_yeu_cau { get; set; }
        public DateTime ngay_cap_thiet_bi { get; set; }
        public string ghi_chu { get; set; }
        [Ignore]
        public string don_vi { get; set; }
        [Ignore]
        public string ten_don_vi { get; set; }
    }

    public class SelectedPO
    {
        [AutoIncrement]
        [PrimaryKey]
        public int id { get; set; }
        public int ma_phieu_header { get; set; }
        public int ma_san_pham { get; set; }
        public string nguoi_tao { get; set; }
    }
}
