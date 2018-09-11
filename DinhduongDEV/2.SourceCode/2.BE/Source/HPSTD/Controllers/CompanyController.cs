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
    public class CompanyController : CustomController
    {
        // GET: Company
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                }
                return View("Company");
            }
            return RedirectToAction("NoAccessRights", "Error");
        }

        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new List<Company>();
                if (accessDetail.xem)
                {
                    if (request.Filters.Any())
                    {
                        var where = KendoApplyFilter.ApplyFilter(request.Filters[0]);
                        data = dbConn.Select<Company>(where);
                    }
                    else
                    {
                        data = dbConn.Select<Company>();
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
                        var item = dbConn.FirstOrDefault<Company>("Id={0}", id);
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
                        var item = dbConn.FirstOrDefault<Company>("Id={0}", id);
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

        [HttpPost]
        public ActionResult Excel_Export(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);

            return File(fileContents, contentType, fileName);
        }

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
                        dbConn.Delete<Core.Entities.Company>("id={0}", item);
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
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.Company> items)
        {
            if (accessDetail.sua)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            var exist = dbConn.SingleOrDefault<Company>("id={0}", item.id);
                            exist.ten_cong_ty = item.ten_cong_ty;
                            exist.so_dien_thoai = item.so_dien_thoai;
                            exist.dia_chi = item.dia_chi;
                            exist.email = item.email;
                            exist.fax = item.fax;
                            exist.website = item.website;
                            exist.ghi_chu = item.ghi_chu;
                            exist.trang_thai = item.trang_thai;
                            exist.ngay_cap_nhat = DateTime.Now;
                            exist.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            dbConn.Update(exist, s => s.id == exist.id);
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
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.Company> items)
        {
            if (accessDetail.them)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
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
            else
            {
                ModelState.AddModelError("", "Bạn không có quyền thêm. Vui lòng liên hệ với ban quản trị để cập nhật quyền.");
            }
            return Json(items.ToDataSourceResult(request, ModelState));
        }
    }
}