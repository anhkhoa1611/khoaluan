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
    public class WarehouseController : CustomController
    {
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.Company = dbConn.Select<Company>();
                    ViewBag.Branch = dbConn.Select<Branch>();
                }
                return View();
            }
            return RedirectToAction("NoAccessRights", "Error");
        }

        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new List<WareHouse>();
                if (accessDetail.xem)
                {
                    if (request.Filters.Any())
                    {
                        var where = KendoApplyFilter.ApplyFilter(request.Filters[0]);
                        data = dbConn.Select<WareHouse>(where);
                    }
                    else
                    {
                        data = dbConn.Select<WareHouse>();
                    }
                }
                return Json(data.ToDataSourceResult(request));
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateUpdate(WareHouse data)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    if (data.id > 0)
                    {
                        if (accessDetail.them && ModelState.IsValid)
                        {
                            var exist = dbConn.SingleOrDefault<WareHouse>("id ={0}", data.id);
                            if (data.loai_kho == "KhoCT")
                            {
                                exist.chi_nhanh_id = null;
                                exist.cong_ty_id = data.cong_ty_id;
                            }
                            else
                            {
                                exist.cong_ty_id = null;
                                exist.chi_nhanh_id = data.chi_nhanh_id;
                            }
                            exist.loai_kho = data.loai_kho;
                            exist.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            exist.ngay_cap_nhat = DateTime.Now;
                            exist.ten_kho = !string.IsNullOrEmpty(data.ten_kho) ? data.ten_kho : "";
                            exist.dia_chi = !string.IsNullOrEmpty(data.dia_chi) ? data.dia_chi : "";
                            exist.email = !string.IsNullOrEmpty(data.email) ? data.email : "";
                            exist.thu_kho = !string.IsNullOrEmpty(data.thu_kho) ? data.thu_kho : "";
                            exist.dien_thoai_thu_kho = !string.IsNullOrEmpty(data.dien_thoai_thu_kho) ? data.dien_thoai_thu_kho : "";
                            exist.ghi_chu = !string.IsNullOrEmpty(data.ghi_chu) ? data.ghi_chu : "";
                            exist.trang_thai = data.trang_thai;
                            exist.nguoi_cap_nhat = User.Identity.Name;
                            exist.ngay_cap_nhat = DateTime.Now;
                            dbConn.Update(exist);
                        }
                        else
                        {
                            return Json(new { success = false, error = "Bạn không có quyền cập nhập kho" });
                        }
                    }
                    else
                    {
                        if (accessDetail.them && ModelState.IsValid)
                        {
                            var isExist = dbConn.SingleOrDefault<WareHouse>("select * from WareHouse order by id desc");
                            var prefix = "WH";
                            if (isExist != null)
                            {
                                var nextNo = int.Parse(isExist.kho_id.Substring(2, isExist.kho_id.Length - 2)) + 1;
                                data.kho_id = prefix + String.Format("{0:00000000}", nextNo);
                            }
                            else
                            {
                                data.kho_id = prefix + "00000001";
                            }

                            var existName = dbConn.SingleOrDefault<WareHouse>("ten_kho={0}", data.ten_kho);
                            if (existName != null)
                            {
                                return Json(new { success = false, error = "Tên kho này đã tồn tại." });
                            }

                            if (data.loai_kho == "KhoCT")
                                data.chi_nhanh_id = null;
                            else
                                data.cong_ty_id = null;
                            data.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            data.ngay_cap_nhat = DateTime.Now;
                            data.ten_kho = !string.IsNullOrEmpty(data.ten_kho) ? data.ten_kho : "";
                            data.dia_chi = !string.IsNullOrEmpty(data.dia_chi) ? data.dia_chi : "";
                            data.email = !string.IsNullOrEmpty(data.email) ? data.email : "";
                            data.thu_kho = !string.IsNullOrEmpty(data.thu_kho) ? data.thu_kho : "";
                            data.dien_thoai_thu_kho = !string.IsNullOrEmpty(data.dien_thoai_thu_kho) ? data.dien_thoai_thu_kho : "";
                            data.ghi_chu = !string.IsNullOrEmpty(data.ghi_chu) ? data.ghi_chu : "";
                            data.trang_thai = data.trang_thai;
                            data.ngay_tao = DateTime.Now;
                            data.nguoi_tao = User.Identity.Name;
                            data.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            dbConn.Insert(data);
                        }
                        else
                        {
                            return Json(new { success = false, error = "Bạn không có quyền tạo chi nhánh" });
                        }

                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = ex.Message });
                }

                return Json(new { success = true });

            }
        }

        public ActionResult GetCompany()
        {
            using (var dbConn = OrmliteConnection.openConn())
            {
                var data = dbConn.Select<Company>();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetBranch()
        {
            using (var dbConn = OrmliteConnection.openConn())
            {
                var data = dbConn.Select<Branch>();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Active(string data)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    int i = 0;
                    string[] separators = { "@@" };
                    var listid = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var id in listid)
                    {
                        var item = dbConn.FirstOrDefault<WareHouse>("Id={0}", id);
                        item.trang_thai = "DANG_HOAT_DONG";
                        item.ngay_cap_nhat = DateTime.Now;
                        item.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                        dbConn.Update(item);
                        i++;
                    }
                    return Json(new { success = true, message = i });
                }
                catch (Exception e)
                {
                    return Json(new { success = false, error = e.Message });
                }
            }
        }

        public ActionResult Inactive(string data)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    int i = 0;
                    string[] separators = { "@@" };
                    var listid = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var id in listid)
                    {
                        var item = dbConn.FirstOrDefault<WareHouse>("Id={0}", id);
                        item.trang_thai = "KHONG_HOAT_DONG";
                        item.ngay_cap_nhat = DateTime.Now;
                        item.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                        dbConn.Update(item);
                        i++;
                    }
                    return Json(new { success = true, message = i });
                }
                catch (Exception e)
                {
                    return Json(new { success = false, error = e.Message });
                }
            }
        }
    }
}