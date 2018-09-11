using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DevTrends.MvcDonutCaching;
using HPSTD.Core.Entities;
using HPSTD.Helpers;
using HPSTD.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using ServiceStack.OrmLite;


namespace HPSTD.Controllers
{
    [Authorize]
    public class WebsitesManageController : CustomController
    {
        // GET: WebsitesManage
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                }
                return View("WebsitesManage");
            }
            return RedirectToAction("NoAccessRights", "Error");
        }

        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new List<WebsitesManage>();
                if (accessDetail.xem)
                {
                    if (request.Filters.Any())
                    {
                        var where = KendoApplyFilter.ApplyFilter(request.Filters[0]);
                        data = dbConn.Select<WebsitesManage>(where);
                    }
                    else
                    {
                        data = dbConn.Select<WebsitesManage>();
                    }
                }
                return Json(data.ToDataSourceResult(request));
            }
        }
      
        public ActionResult Active(string data)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            using (var dbTrans = dbConn.OpenTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    int i = 0;
                    string[] separators = { "@@" };
                    var listid = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var id in listid)
                    {
                        var item = dbConn.FirstOrDefault<WebsitesManage>("Id={0}", id);
                        item.trang_thai = "DANG_HOAT_DONG";
                        item.ngay_cap_nhat = DateTime.Now;
                        item.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                        dbConn.Update(item);
                        i++;
                    }
                    dbTrans.Commit();
                    return Json(new { success = true, message = i });
                }
                catch (Exception e)
                {
                    dbTrans.Rollback();
                    return Json(new { success = false, error = e.Message });
                }
            }
        }

        public ActionResult Inactive(string data)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            using (var dbTrans = dbConn.OpenTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    int i = 0;
                    string[] separators = { "@@" };
                    var listid = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var id in listid)
                    {
                        var item = dbConn.FirstOrDefault<WebsitesManage>("Id={0}", id);
                        item.trang_thai = "KHONG_HOAT_DONG";
                        item.ngay_cap_nhat = DateTime.Now;
                        item.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                        dbConn.Update(item);
                        i++;
                    }
                    dbTrans.Commit();
                    return Json(new { success = true, message = i });
                }
                catch (Exception e)
                {
                    dbTrans.Rollback();
                    return Json(new { success = false, error = e.Message });
                }
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.WebsitesManage> items)
        {
            if (accessDetail.them)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        
                        foreach (var item in items)
                        {
                            if (item.ma_website == null || item.ten_website == null)
                            {
                                continue;
                            }
                            if(item.trang_thai == null)
                            {
                                continue;
                            }

                            else
                            {
                                item.ngay_tao = DateTime.Now;
                                item.nguoi_tao = currentUser.ma_nguoi_dung;
                                item.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                                item.nguoi_cap_nhat = "";
                                dbConn.Insert(item);
                            }
                        }
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Bạn không có quyền thêm. Vui lòng liên hệ với ban quản trị để cập nhật quyền.");
            }
            return Json(items.ToDataSourceResult(request, ModelState));
        }


        [HttpPost]

        public ActionResult DeleteList(string data)
        {
            if (accessDetail.xoa)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    string[] separators = { "@@" };
                    var listItem = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in listItem)
                    {
                        dbConn.Delete<Core.Entities.WebsitesManage>("id={0}", item);
                    }
                    return Json(new { success = true });
                }
            }
            else
            {
                return Json(new { success = false, error = "Bạn không có quyền xóa dữ liệu. Vui lòng liên hệ với ban quản trị để cập nhật quyền." });
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.WebsitesManage> items)
        {
            if (accessDetail.sua)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            if (item.ma_website == null || item.ten_website == null)
                            {
                                continue;
                            }
                            if (item.trang_thai == null)
                            {
                                continue;
                            }
                            else
                            {
                                var exist = dbConn.SingleOrDefault<WebsitesManage>("id={0}", item.id);
                                exist.ma_website = item.ma_website;
                                exist.ten_website = item.ten_website;
                                exist.trang_thai = item.trang_thai;
                                exist.ngay_cap_nhat = DateTime.Now;
                                exist.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                                dbConn.Update(exist, s => s.id == exist.id);
                            }
                        }
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Bạn không có quyền cập nhật. Vui lòng liên hệ với ban quản trị để cập nhật quyền.");
            }
            return Json(items.ToDataSourceResult(request, ModelState));
        }
       
    }
}