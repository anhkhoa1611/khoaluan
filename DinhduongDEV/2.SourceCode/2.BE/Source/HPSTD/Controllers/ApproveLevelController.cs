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
using System.IO;
using HPSTD.Core.Entities;
using OfficeOpenXml;

namespace HPSTD.Controllers
{
    [Authorize]
    public class ApproveLevelController : CustomController
    {
        //
        // GET: /Users/
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listCapDuyet = dbConn.Select<Parameters>(p => p.loai_tham_so == "CAPDUYET");
                    ViewBag.listChuyenMon = dbConn.Select<Parameters>("loai_tham_so = 'NHOMCHUYENMON' And trang_thai ='true'");
                    ViewBag.listChucVu = dbConn.Select<Parameters>("loai_tham_so ='CHUCVU' And trang_thai ='true'");
                    ViewBag.listNguoiDung = dbConn.Select<User>("chuc_vu = 'CHUCVU09' And trang_thai ='true'");
                    ViewBag.listDonVi = dbConn.Select<Branch>("trang_thai ='true'");
                }
                return View("ApproveLevel");
            }
            else
            {
                return RedirectToAction("NoAccessRights", "Error");
            }
        }

        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {

                if (accessDetail.xem)
                {

                    var WhereCondition = "";
                    if (request.Filters != null)
                    {
                        if (request.Filters.Count > 0)
                        {
                            WhereCondition = " AND " + KendoApplyFilter.ApplyFilter(request.Filters[0], "");
                        }
                    }
                    var data = dbConn.SqlList<ApproveLevel>("EXEC p_Get_UserApprovel @WhereCondition", new { WhereCondition = WhereCondition });
                    return Json(data.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
                return RedirectToAction("NoAccessRights", "Error");
            }
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
                        dbConn.Delete<Core.Entities.ApproveLevelUsers>("id_cap_duyet={0}", item);
                        dbConn.Delete<Core.Entities.ApproveLevel>("id={0}", item);
                    }
                    return Json(new { success = true });
                }
            }
            else
            {
                return Json(new { success = false, error = "Bạn không có quyền xóa dữ liệu. Vui lòng liên hệ với ban quản trị để cập nhật quyền." });
            }
        }
        public ActionResult Create([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.ApproveLevel> items)
        {
            if (accessDetail.them)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            if (string.IsNullOrEmpty(item.cap_duyet))
                            {
                                ModelState.AddModelError("", "Vui lòng chọn cấp duyệt.");
                                return Json(items.ToDataSourceResult(request, ModelState));

                            }
                            if (string.IsNullOrEmpty(item.ten_cap_duyet))
                            {
                                ModelState.AddModelError("", "Vui lòng nhập tên cấp duyệt.");
                                return Json(items.ToDataSourceResult(request, ModelState));
                            }
                            if (string.IsNullOrEmpty(item.ma_nhom_chuyen_mon))
                            {
                                ModelState.AddModelError("", "Vui lòng chọn nhóm chuyên môn.");
                                return Json(items.ToDataSourceResult(request, ModelState));
                            }
                            var exist = dbConn.SingleOrDefault<ApproveLevel>("cap_duyet = {0} And ma_nhom_chuyen_mon = {1}", item.cap_duyet, item.ma_nhom_chuyen_mon);
                            if (exist != null)
                            {
                                ModelState.AddModelError("", "Nhóm chuyên môn này đã tồn tại cấp duyệt. Vui lòng kiểm tra lại thông tin trước khi lưu.");
                                return Json(items.ToDataSourceResult(request, ModelState));
                            }
                            item.ngay_tao = DateTime.Now;
                            item.nguoi_tao = User.Identity.Name;
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
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.ApproveLevel> items)
        {
            if (accessDetail.sua)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            var check = dbConn.FirstOrDefault<Core.Entities.ApproveLevel>(s => s.id == item.id);
                            if (check != null)
                            {
                                item.ngay_cap_nhat = DateTime.Now;
                                item.nguoi_cap_nhat = User.Identity.Name;
                                dbConn.Update(item, s => s.id == check.id);
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
        public ActionResult GetUserApprovelevelByID(int id_cap_duyet)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = dbConn.GetFirstColumn<string>("SELECT ma_nguoi_dung FROM ApproveLevelUsers where id_cap_duyet =" + id_cap_duyet);
                return Json(new { success = true, data = data, JsonRequestBehavior.AllowGet });
            }
        }

        public ActionResult GetUserByBranch(string ma_don_vi)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var selectquery = "SELECT ma_nguoi_dung, ten_nguoi_dung FROM [User] where ma_chi_nhanh='" + ma_don_vi + "'";
                var data = dbConn.Select<UserBaseInfo>(selectquery);
                return Json(new { success = true, data = data, JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public ActionResult CreateUpdate(ApproveLevel data)
        {
            try
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    foreach (string manguoidung in data.lstnguoi_dung)
                    {
                        var exist = dbConn.SingleOrDefault<ApproveLevelUsers>("id_cap_duyet={0} AND ma_nguoi_dung={1}", data.id, manguoidung);
                        if (exist == null)
                        {
                            var newdata = new ApproveLevelUsers();
                            newdata.id_cap_duyet = data.id;
                            newdata.ma_nhom_chuyen_mon = data.ma_nhom_chuyen_mon;
                            newdata.ma_nguoi_dung = manguoidung;
                            newdata.ngay_tao = DateTime.Now;
                            newdata.nguoi_tao = currentUser.ma_nguoi_dung;
                            newdata.ngay_cap_nhat = DateTime.Now;
                            newdata.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            dbConn.Insert(newdata);
                        }
                    }
                    dbConn.Delete<ApproveLevelUsers>("id_cap_duyet={0} AND ma_nguoi_dung NOT IN(" + String.Join(",", data.lstnguoi_dung.Select(s => ("'" + s + "'"))) + ")", data.id);
                }
                return Json(new { success = true, data = data });
            }
            catch (Exception e)
            {
                return Json(new { success = false, error = e.Message });
            }
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult GetDataFilter(List<String> listChucVu, List<String> listDonVi)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = dbConn.Select<User>();

                var removeList = new List<string>() { "" };

                if (listChucVu != null && listChucVu.Count > 0)
                {
                    listChucVu.RemoveAll(r => removeList.Any(a => a == r));
                    if (listChucVu.Count > 0)
                        data = data.Where(s => listChucVu.Any(p => p == s.chuc_vu)).ToList();
                }

                if (listDonVi != null && listDonVi.Count > 0)
                {
                    listDonVi.RemoveAll(r => removeList.Any(a => a == r));
                    if (listDonVi.Count > 0)
                    {
                        var lstdataDonVi = dbConn.Select<UsersBranch>("ma_chi_nhanh IN(" + String.Join(",", listDonVi.Select(s => ("'" + s + "'"))) + ")");
                        data = data.Where(s => lstdataDonVi.Any(p => p.ma_nguoi_dung == s.ma_nguoi_dung)).ToList();
                    }
                }

                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}