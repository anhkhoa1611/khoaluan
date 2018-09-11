using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Collections;
using System.IO;
using NPOI.HSSF.UserModel;
using System.Data.OleDb;
using System.Data;
using System.Dynamic;
using Kendo.Mvc;
using ServiceStack.OrmLite;
using HPSTD.Models;
using HPSTD.Core.Entities;
using HPSTD.Helpers;

namespace HPSTD.Controllers
{
    public class HomeController : CustomController
    {
        [Authorize]
        // GET: Home
        public ActionResult Index()
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                ViewBag.listBranch = dbConn.Select<Branch>("Select * From Branch Where ma_chi_nhanh in (Select ma_chi_nhanh From UsersBranch Where ma_nguoi_dung = '" + currentUser.ma_nguoi_dung + "')");
                ViewBag.listTrangThai = dbConn.Select<Parameters>("loai_tham_so='TRANGTHAI'");
                ViewBag.listProduct = dbConn.Select<Product>();
                ViewBag.listVendor = dbConn.Select<Vendor>();
            }
            return View();
        }
        public ActionResult Articles_Read([DataSourceRequest] DataSourceRequest request)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = dbConn.Select<Article>("select a.*, isnull(p.gia_tri, '') as tin_tuc from Article a left join Parameters p on a.loai_tin = p.ma_tham_so where a.xem_truoc = 1");

                return Json(data.ToDataSourceResult(request));
            }
        }

        public ActionResult Link_Read([DataSourceRequest] DataSourceRequest request)
        {
            var dbConn = Helpers.OrmliteConnection.openConn();
            var data = dbConn.Select<Link>();
            return Json(data.ToDataSourceResult(request));

        }
        //public ActionResult Chart()
        //{
        //    var data = SpendingByMonth.GetAllSpedingbyMonth().ToList();
        //    return Json(data);
        //}
        //public ActionResult Articles_Read([DataSourceRequest] DataSourceRequest request)
        //{
        //    var data = DC_Article.GetAllDC_Articles().OrderByDescending(a => a.ArticleId).ToList();
        //    return Json(data.ToDataSourceResult(request));
        //}
        [HttpPost]
        public ActionResult getUserInfo(string username)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                EmployeeInfo ei = dbConn.SingleOrDefault<EmployeeInfo>("UserName = {0}", username);
                return PartialView("_UserInfoTooltip", ei);
            }
        }
        //[HttpPost]
        //public ActionResult getListFollowerUser()
        //{
        //    var result = Deca_RT_Follower.GetListUser();
        //    return Json(new { success = true, list = result.Select(x => x.UserName).ToArray() });
        //}
        public ActionResult SaveXliteCode(string XliteCode)
        {
            try
            {
                Session["XliteID"] = XliteCode;
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    var currentEmployyeeInfo = dbConn.FirstOrDefault<EmployeeInfo>("UserName={0}", currentUser.ma_nguoi_dung);
                    currentEmployyeeInfo.XLiteID = XliteCode;
                    currentEmployyeeInfo.LastUpdatedDateTime = DateTime.Now;
                    currentEmployyeeInfo.LastUpdatedUser = currentUser.ma_nguoi_dung;
                    dbConn.Update(currentEmployyeeInfo);
                }
                return Json(new { success = true, message = "" });
            }
            catch
            {
                return Json(new { success = false, message = "Xảy ra lỗi khi cập nhật extension. Vui lòng liên lạc hỗ trợ kĩ thuật." });
            }
        }

        public ActionResult ReadPR([DataSourceRequest]DataSourceRequest request)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new List<PRequestHeader>();
                if (accessDetail.xem)
                {

                    data = dbConn.Select<PRequestHeader>(@"Select distinct a.* From PRequestHeader a
                                                        Inner Join UsersBranch b on b.ma_chi_nhanh = a.ma_chi_nhanh
                                                        Where b.ma_nguoi_dung = {0} AND a.trang_thai={1}".Params(currentUser.ma_nguoi_dung, AllConstant.TRANGTHAI_MOI));
                }
                else
                {
                    return RedirectToAction("NoAccessRights", "Error");
                }
                return Json(data.ToDataSourceResult(request));
            }
        }

        public ActionResult ReadPO([DataSourceRequest]DataSourceRequest request)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new List<POHeader>();
                if (accessDetail.xem)
                {
                    data = dbConn.Select<POHeader>();
                    return Json(data.ToDataSourceResult(request));
                }
                return RedirectToAction("NoAccessRights", "Error");
            }
        }
    }
}