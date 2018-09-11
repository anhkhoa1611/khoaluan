using HPSTD.Core.Entities;
using HPSTD.Helpers;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HPSTD.Controllers
{
    public class ParametersController : CustomController
    {
        // GET: Company
        public ActionResult Index()
        {
            return View("Parameters");
        }
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new List<Parameters>();
                if (accessDetail.xem)
                {
                    if (request.Filters.Any())
                    {
                        var where = KendoApplyFilter.ApplyFilter(request.Filters[0]);
                        data = dbConn.Select<Parameters>(where);
                    }
                    else
                    {
                        data = dbConn.Select<Parameters>();
                    }
                }
                return Json(data.ToDataSourceResult(request));
            }
        }
        public ActionResult Create([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")] IEnumerable<Parameters> items)
        {

            if (accessDetail.them)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            var exist = dbConn.SingleOrDefault<Parameters>("ma_tham_so={0}", item.ma_tham_so);
                            if (exist != null)
                            {
                                ModelState.AddModelError("", "Mã tham số này đã tồn tại.");
                                return Json(items.ToDataSourceResult(request, ModelState));
                            }
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

        public ActionResult Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")] IEnumerable<Parameters> items)
        {
            if (accessDetail.sua)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            var exist = dbConn.SingleOrDefault<Parameters>("id={0}", item.id);
                            if (item.ma_tham_so != exist.ma_tham_so)
                            {
                                var checkDup = dbConn.SingleOrDefault<Parameters>("ma_tham_so={0}", item.ma_tham_so);
                                if (checkDup != null)
                                {
                                    ModelState.AddModelError("", "Mã tham số này đã tồn tại.");
                                    return Json(items.ToDataSourceResult(request, ModelState));
                                }
                                else
                                {
                                    exist.ma_tham_so = item.ma_tham_so;
                                }
                            }
                            exist.loai_tham_so = item.loai_tham_so;
                            exist.gia_tri = item.gia_tri;
                            exist.mo_ta = item.mo_ta;
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

        public ActionResult DeleteItem(string data)
        {
            var dbConn = Helpers.OrmliteConnection.openConn();
            try
            {
                string[] separators = { "@@" };
                var listdata = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                var detail = new Parameters();
                foreach (var item in listdata)
                {
                    if (dbConn.Select<Parameters>(s => s.id == int.Parse(item)).Count() > 0)
                    {
                        var success = dbConn.Delete<Parameters>(where: "id = '" + item + "'") >= 1;
                        if (!success)
                        {
                            return Json(new { success = false, message = "Không thể lưu" });
                        }
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }

    }
}