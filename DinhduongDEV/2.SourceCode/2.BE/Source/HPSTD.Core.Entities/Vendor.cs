using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;

namespace HPSTD.Core.Entities
{
    public class Vendor : BaseEntity
    {
        public string nha_cung_cap_id { get; set; }
        public string ten_nha_cung_cap { get; set; }
        public string ten_thuong_goi { get; set; }
        public string ma_so_thue { get; set; }
        public string dien_thoai { get; set; }
        public double? von_dieu_le { get; set; }
        public string email { get; set; }
        public string quy_mo { get; set; }
        public string pham_vi_cung_ung { get; set; }
        public string bao_hanh { get; set; }
        public string thoi_gian_cung_ung { get; set; }
        public string khach_hang_cung_cap { get; set; }
        public string chung_loai_hang_hoa_ncc { get; set; }
        public string nha_cung_cap_cua_hdbank { get; set; }
        public int so_luong_tieu_chuan { get; set; }
        public string thoi_gian_giao_hang { get; set; }
        public string phuong_thuc_thanh_toan { get; set; }
        public string dieu_kien_thanh_toan { get; set; }
        public string website { get; set; }
        public string ghi_chu { get; set; }
        public string dia_chi { get; set; }
        public string file_dinh_kem { get; set; }
        public string thong_bao_giao_hang { get; set; }
        public string thoi_han_thanh_toan { get; set; }

        [Ignore]
        public List<string> ma_phan_cap { get; set; }
        [Ignore]
        public List<string> listMa_phan_cap { get; set; }

    }
    public class VendorModel : BaseEntity
    {
        public string nha_cung_cap_id { get; set; }
        public string ten_nha_cung_cap { get; set; } 
    }
    public class VendorModelView
    {
        public string nha_cung_cap_id { get; set; }
        public string ten_nha_cung_cap { get; set; }
        public string ten_thuong_goi { get; set; }
    }
}
