using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;
using HPSTD.Core.Entities;

namespace HPSTD.Models
{
    public class User
    {
        [AutoIncrement]
        public int id { get; set; }
        public string ma_nguoi_dung { get; set; }
        public string ten_nguoi_dung { get; set; }
        public string mat_khau { get; set; }
        public string domain { get; set; }
      
        public string dia_chi { get; set; }
        public string dien_thoai { get; set; }
        public string email { get; set; }
        public string gioi_tinh { get; set; }
        [Ignore]
        public string gioi_tinh_value { get; set; }

        public string chuc_vu { get; set; }
        public string trang_thai { get; set; }
        public string avatar { get; set; }
        public string ghi_chu { get; set; }
        public DateTime ngay_tao { get; set; }
        public string nguoi_tao { get; set; }
        public DateTime ngay_cap_nhat { get; set; }
        public string nguoi_cap_nhat { get; set; }
        public string cap_bac { get; set; }
        //public string chuc_danh { get; set; }

        [Ignore]
        public List<int> groupId { get; set; }

        [Ignore]
        public List<string> lstbranch { get; set; }
      
        public static string Avatar()
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                string text = "SELECT TOP 1 avatar FROM [User] WHERE ma_nguoi_dung = '" + System.Web.HttpContext.Current.User.Identity.Name + "'";
                var imageUrl = dbConn.Scalar<string>(text);
                if (!String.IsNullOrEmpty(imageUrl))
                {
                    return imageUrl;
                }
                else
                {
                    return "default_avartar.png";
                }
            }

        }

        public static string GetBranchName()
        {
            string tenchinhanh = "";
            
            return tenchinhanh;

        }
        [Ignore]
        public List<AccessRightDetail> listAccess
        {
            get
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    var data = new List<AccessRightDetail>();
                    data = dbConn.Select<AccessRightDetail>("SELECT * FROM AccessRightDetail WHERE ma_nhom IN (SELECT id_nhom_nguoi_dung FROM UserInGroup where ma_nguoi_dung = '" + ma_nguoi_dung + "')");
                    return data;
                }
            }
        }
        [Ignore]
        public List<int> listGroup
        {
            get
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    return dbConn.GetFirstColumn<int>("SELECT id_nhom_nguoi_dung FROM UserInGroup where ma_nguoi_dung ={0}", ma_nguoi_dung);
                }
            }
        }

        [Ignore]
        public List<string> ListUserBranch
        {
            get
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    var data = new List<string>();
                    data = dbConn.GetFirstColumn<string>("SELECT ma_chi_nhanh FROM UsersBranch where id_nguoi_dung = {0}".Params(id));
                    return data;
                }
            }
        }

        public static string ListAsset()
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var user = dbConn.SingleOrDefault<HPSTD.Core.Entities.UserInGroup>("ma_nguoi_dung={0}", System.Web.HttpContext.Current.User.Identity.Name);
                if (user != null)
                {
                    if (user.id_nhom_nguoi_dung == 1)
                        return "All";
                    else
                    {
                        var listAsset = dbConn.Select<AccessRightDetail>("xem = 1 and ma_nhom={0}", user.id_nhom_nguoi_dung);
                        if (listAsset.Count() > 0)
                            return string.Join(",", listAsset.Select(s => s.ma_man_hinh));
                    }
                }
                return "";
            }
        }
    }
    public class UserBaseInfo
    {
        [AutoIncrement]
        public int id { get; set; }
        public string ma_nguoi_dung { get; set; }
        public string ten_nguoi_dung { get; set; }
    }
    public class GroupViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}