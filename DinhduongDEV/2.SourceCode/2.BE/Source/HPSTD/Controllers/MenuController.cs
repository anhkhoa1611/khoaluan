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
    public class MenuController : CustomController
    {
        // GET: Menu
        public ActionResult Index()
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                if (accessDetail.xem)
                {
                    ViewBag.listScreen = dbConn.Select<Screen>();
                    ViewBag.listMenu = dbConn.Select<DropListDownInt>("Select id as Value , ten_chuc_nang as Text from Menu");
                    return View("Menu");
                }
                else
                {
                    return RedirectToAction("NoAccessRights", "Error");
                }
            }
        }
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new DataSourceResult();
                data = KendoApplyFilter.KendoData<Core.Entities.Menu>(request, "id_cha = -1");
                return Json(data);
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.Menu> items)
        {
            if (accessDetail.them)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            if (string.IsNullOrEmpty(item.thu_tu))
                            {
                                ModelState.AddModelError("", "Vui lòng nhập thứ tự của chức năng này.");
                                return Json(items.ToDataSourceResult(request, ModelState));
                            }
                            item.ngay_tao = DateTime.Now;
                            item.nguoi_tao = User.Identity.Name;
                            item.cap = 0;
                            item.id_cha = -1;
                            item.ma_man_hinh = !string.IsNullOrEmpty(item.ma_man_hinh) ? item.ma_man_hinh : "";
                            dbConn.Insert(item);
                        }
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Bạn không có quyền thêm. Vui lòng liên hệ ban quản trị để cập nhật quyền.");
            }
            return Json(items.ToDataSourceResult(request, ModelState));
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.Menu> items)
        {
            if (accessDetail.sua)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            var exist = dbConn.SingleOrDefault<Core.Entities.Menu>("id={0}", item.id);

                            exist.ten_chuc_nang = item.ten_chuc_nang;
                            exist.ma_man_hinh = item.ma_man_hinh != null ? item.ma_man_hinh : "";                         
                            exist.icon = item.icon;
                            exist.thu_tu = item.thu_tu;
                            exist.ngay_cap_nhat = DateTime.Now;
                            exist.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            dbConn.Update(exist, s => s.id == exist.id);              
                        }
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Bạn không có quyền cập nhật. Vui lòng liên hệ ban quản trị để cập nhật quyền.");
            }
            return Json(items.ToDataSourceResult(request, ModelState));
        }
        public ActionResult Delete(string data)
        {
            if (accessDetail.xoa)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    string[] separators = { "@@" };
                    var listItem = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in listItem)
                    {
                        dbConn.Delete<Core.Entities.Menu>("id={0}", item);
                        dbConn.Delete<Core.Entities.Menu>("id_cha={0}", item);
                    }
                    return Json(new { success = true });
                }
            }
            else
            {
                return Json(new { success = false, error = "Bạn không có quyền xóa dữ liệu" });
            }
        }

        List<DropListDownInt> GetMenu(int id_cha, int currentlevel)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                List<DropListDownInt> lstResult = new List<DropListDownInt>();
                List<Core.Entities.Menu> lstdata = dbConn.Select<Core.Entities.Menu>(s => s.id_cha == id_cha);
                foreach (var item in lstdata)
                {
                    if (item.cap == currentlevel - 1)
                    {
                        lstResult.Add(new DropListDownInt { Value = item.id, Text = item.ten_chuc_nang });
                    }
                    else
                    {
                        List<DropListDownInt> lstAdd = GetMenu(item.id, currentlevel);
                        if (lstAdd.Count > 0)
                            lstResult.AddRange(lstAdd);
                    }
                }
                return lstResult;
            }
        }


        public ActionResult GetAllMenu(int id, int currentlevel)
        {
            try
            {
                using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
                {
                    List<DropListDownInt> lstResult = GetMenu(id, currentlevel);
                    if (currentlevel == 1)
                    {
                        var data = dbConn.FirstOrDefault<DropListDownInt>("Select id as Value , ten_chuc_nang as Text from Menu where id={0}".Params(id));
                        lstResult.Insert(0, data);
                    }
                    return Json(new { success = true, data = lstResult }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("NoAccess", "Error");
            }
        }

        List<Core.Entities.Menu> GetMenuGrid(int id_cha, int level)
        {
            if (level > 3)
                return new List<Core.Entities.Menu>();
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                List<Core.Entities.Menu> lstResult = new List<Core.Entities.Menu>();
                List<Core.Entities.Menu> lstdata = dbConn.Select<Core.Entities.Menu>(s => s.id_cha == id_cha);
                foreach (var item in lstdata)
                {
                    lstResult.Add(item);
                    List<Core.Entities.Menu> lstAdd = GetMenuGrid(item.id, level + 1);
                    if (lstAdd.Count > 0)
                        lstResult.AddRange(lstAdd);
                }
                return lstResult;
            }
        }

        public ActionResult ReadDetail([DataSourceRequest]DataSourceRequest request, int id)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {           
                var data = GetMenuGrid(id, 2);
                return Json(data.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult CreateDetail([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.Menu> items, int id)
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
                            item.nguoi_tao = User.Identity.Name;      
                            item.ma_man_hinh = !string.IsNullOrEmpty(item.ma_man_hinh) ? item.ma_man_hinh : "";
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
        public ActionResult UpdateDetail([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.Menu> items)
        {
            if (accessDetail.sua)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            var check = dbConn.FirstOrDefault<Core.Entities.Menu>(s => s.id == item.id);
                            if (check != null)
                            {
                                check.ngay_cap_nhat = DateTime.Now;
                                check.nguoi_cap_nhat = User.Identity.Name;
                                check.thu_tu = item.thu_tu;
                                check.cap = item.cap;
                                check.ma_man_hinh = !string.IsNullOrEmpty(item.ma_man_hinh) ? item.ma_man_hinh : "";
                                check.id_cha = item.id_cha;
                                check.icon = !string.IsNullOrEmpty(item.icon) ? item.icon : "";
                                check.trang_thai = !string.IsNullOrEmpty(item.trang_thai) ? item.trang_thai : "";
                                check.ten_chuc_nang = !string.IsNullOrEmpty(item.ten_chuc_nang) ? item.ten_chuc_nang : "";
                                dbConn.Update(check, s => s.id == check.id);
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
        public ActionResult DeleteDetail(string data)
        {
            if (accessDetail.xoa)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    string[] separators = { "@@" };
                    var listItem = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in listItem)
                    {
                        dbConn.Delete<Core.Entities.Menu>("id={0}", item);
                    }
                    return Json(new { success = true });
                }
            }
            else
            {
                return Json(new { success = false, error = "Bạn không có quyền xóa dữ liệu. Vui lòng liên hệ với ban quản trị để cập nhật quyền." });
            }
        }
    }
}