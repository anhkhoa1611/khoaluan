using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class ReportPendingPR
    {
        public string ma_phieu_pr { get; set; }
        public DateTime? ngay_tao_pr { get; set; }
        public string don_vi_yeu_cau { get; set; }
        public string ma_san_pham { get; set; }
        public string ten_san_pham { get; set; }
        public int? so_luong { get; set; }
        public DateTime khoi_cntt_nhdt_xac_nhan { get; set; }
        public DateTime phong_qldv_khnq_xac_nhan { get; set; }
        public DateTime phong_ptml_xdcb_xac_nhan { get; set; }
        public DateTime phong_mkt_pr_xac_nhan { get; set; }
        public DateTime phong_ban_khac { get; set; }
        public DateTime bang_tong_hop_chi_mua_sam { get; set; }
        public DateTime po_don_dat_hang { get; set; }
        public DateTime ngay_nhan_hang { get; set; }
    }
}
