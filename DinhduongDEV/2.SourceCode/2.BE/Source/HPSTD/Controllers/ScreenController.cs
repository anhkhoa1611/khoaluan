using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HPSTD.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using HPSTD.Helpers;
using System.Data;

namespace HPSTD.Controllers
{
    [Authorize]
    public class ScreenController : CustomController
    {
        // GET: Screen
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                return View();
            }
            return RedirectToAction("NoAccessRights", "Error");
        }
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new DataSourceResult();
                data = KendoApplyFilter.KendoData<Screen>(request);
                return Json(data);
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Screen> items)
        {
            if (accessDetail.sua)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            item.ngay_cap_nhat = DateTime.Now;
                            item.nguoi_cap_nhat = User.Identity.Name;
                            dbConn.Update(item);
                        }
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Bạn không có quyền cập nhật");
            }

            return Json(items.ToDataSourceResult(request, ModelState));
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
                        dbConn.Delete<Screen>("id={0}", item);
                    }
                    return Json(new { success = true });
                }
            }
            else
            {
                return Json(new { success = false, error = "Bạn không có quyền xóa dữ liệu" });
            }
        }

    }
}