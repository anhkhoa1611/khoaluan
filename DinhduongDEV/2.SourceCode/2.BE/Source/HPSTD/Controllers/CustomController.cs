using HPSTD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.SqlServer;
using System.Net;
using Microsoft.Owin.Security;
using System.Web.Security;
using HPSTD.Core.Entities;


namespace HPSTD.Controllers
{
    public class CustomController : Controller
    {
        protected User currentUser;
        //protected List<Groups> currentGroups;
        //protected Asset asset = new Asset() { View = false, Create = false, Update = false, Delete = false, Export = false, Import = false };
        protected Core.Entities.AccessRightDetail accessDetail = new Core.Entities.AccessRightDetail() { xem = false, them = false, sua = false, xoa = false, xuat = false, nhap = false };
        //protected User userCurrent;
        protected bool isAdmin;
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (this.User.Identity.IsAuthenticated)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    currentUser = dbConn.FirstOrDefault<User>("ma_nguoi_dung={0}", User.Identity.Name);
                    if (currentUser == null || currentUser.trang_thai == "false")
                    {
                        AuthenticationManager.SignOut();
                        FormsAuthentication.SignOut();
                    }
                    else
                    {
                        var exist = dbConn.FirstOrDefault<Screen>("ma_man_hinh={0}", this.GetType().Name.Substring(0, this.GetType().Name.IndexOf("Controller")));
                        if (exist == null)
                        {
                            var newAssets = new Screen();
                            newAssets.ma_man_hinh = this.GetType().Name.Substring(0, this.GetType().Name.IndexOf("Controller"));
                            newAssets.ten_man_hinh = "";
                            newAssets.ghi_chu = "";
                            newAssets.trang_thai = "";
                            newAssets.ngay_tao = DateTime.Now;
                            newAssets.nguoi_tao = "system";
                            newAssets.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            dbConn.Insert(newAssets);
                        }
                        //Phan quyen moi
                        var existAdminPermission = dbConn.SingleOrDefault<Core.Entities.AccessRightDetail>("ma_nhom = 1 AND ma_man_hinh={0}", this.GetType().Name.Substring(0, this.GetType().Name.IndexOf("Controller")));
                        if (existAdminPermission == null)
                        {
                            var accessDetail = new Core.Entities.AccessRightDetail();
                            accessDetail.them = true;
                            accessDetail.sua = true;
                            accessDetail.xoa = true;
                            accessDetail.xem = true;
                            accessDetail.nhap = true;
                            accessDetail.xuat = true;
                            accessDetail.xuat_pdf = true;
                            accessDetail.ma_man_hinh = this.GetType().Name.Substring(0, this.GetType().Name.IndexOf("Controller"));
                            accessDetail.ma_nhom = 1;
                            accessDetail.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            accessDetail.nguoi_tao = "system";
                            accessDetail.ngay_tao = DateTime.Now;
                            dbConn.Insert(accessDetail);
                        }
                        var listAccess = currentUser.listAccess.Where(s => s.ma_man_hinh == this.GetType().Name.Substring(0, this.GetType().Name.IndexOf("Controller")));
                        if (listAccess.Count() > 0)
                        {
                            foreach (var item in listAccess)
                            {
                                accessDetail.them = item.them;
                                accessDetail.sua = item.sua;
                                accessDetail.xoa = item.xoa;
                                accessDetail.xem = item.xem;
                                accessDetail.nhap = item.nhap;
                                accessDetail.xuat = item.xuat;
                                accessDetail.phan_quyen = item.phan_quyen;
                                accessDetail.xuat_pdf = item.xuat_pdf;
                            }
                        }
                        if (currentUser.ma_nguoi_dung == "administrator")
                        {
                            accessDetail.them = true;
                            accessDetail.sua = true;
                            accessDetail.xoa = true;
                            accessDetail.xem = true;
                            accessDetail.nhap = true;
                            accessDetail.xuat = true;
                            accessDetail.xuat_pdf = true;
                        }
                        ViewBag.accessDetail = accessDetail;
                    }
                }
            }
        }
    }
}