using HPSTD.Helpers;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ServiceStack.OrmLite;
using HPSTD.Core.Entities;
using System.Globalization;
using System.IO;
using Dapper;
using Kendo.Mvc.Extensions;

namespace HPSTD.Controllers
{
    [Authorize]
    public class InvoiceController : CustomController
    {
        // GET: PO
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listProduct = dbConn.Select<Product>();
                    ViewBag.listDonVi = dbConn.Select<Branch>();
                    ViewBag.listVendor = dbConn.Select<Vendor>();
                    ViewBag.listTrangThai = dbConn.Select<Parameters>("loai_tham_so='TRANGTHAI'");
                    return View("Invoice");
                }
            }
            return RedirectToAction("NoAccessRights", "Error");
        }

        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                if (accessDetail.xem)
                {
                    var query = @"SELECT S.*, B.ten_chi_nhanh AS ten_don_vi FROM InvoiceHeader S LEFT JOIN Branch B ON S.ma_don_vi=B.ma_chi_nhanh";
                    var data = new DataSourceResult();
                    if (request.Filters.Any())
                    {
                        var where = KendoApplyFilter.ApplyFilter(request.Filters[0]);
                        data = KendoApplyFilter.KendoDataByQuery<InvoiceHeader>(request, query, where);
                    }
                    else
                    {
                        data = KendoApplyFilter.KendoDataByQuery<InvoiceHeader>(request, query, "");
                    }
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                return RedirectToAction("NoAccessRights", "Error");
            }
        }

        public ActionResult Create()
        {
            if (accessDetail.them)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listProduct = dbConn.Select<Product>();
                    ViewBag.listDonVi = dbConn.Select<DepartmentHeirarchy>("cap=3");
                    ViewBag.StatementHeader = dbConn.Select<StatementHeader>();
                    ViewBag.Vendor = dbConn.Select<Vendor>();
                    return View();
                }
            }
            return RedirectToAction("NoAccessRights", "Error");
        }
        public ActionResult Edit(int id)
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listProduct = dbConn.Select<Product>();
                    //ViewBag.listDonVi = dbConn.Select<DepartmentUnit>("cap=3");
                    var query = @"SELECT S.*, B.ten_chi_nhanh AS ten_don_vi FROM InvoiceHeader S LEFT JOIN Branch B ON S.ma_don_vi=B.ma_chi_nhanh where S.id={0}";
                    ViewBag.InvoiceHeader = dbConn.FirstOrDefault<InvoiceHeader>(query.Params(id));
                    var data = dbConn.FirstOrDefault<InvoiceHeader>(s => s.id == id);
                    return View(data);
                }
            }
            else
                return RedirectToAction("NoAccess", "Error");
        }

        public ActionResult ReadDetail([DataSourceRequest]DataSourceRequest request, string ma_phieu_header)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                if (accessDetail.xem)
                {
                    var data = new DataSourceResult();
                    data = KendoApplyFilter.KendoData<InvoiceDetail>(request, "ma_phieu_header={0}".Params(ma_phieu_header));
                    return Json(data);
                }
                return RedirectToAction("NoAccessRights", "Error");
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateHeader(int id, string ten_phieu_nhap_kho, string ghi_chu)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    if (accessDetail.sua)
                    {
                        var exist = dbConn.SingleOrDefault<InvoiceHeader>("id={0}", id);
                        exist.ghi_chu = ghi_chu;
                        exist.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                        exist.ngay_cap_nhat = DateTime.Now;
                        dbConn.Update(exist);
                    }
                    else
                    {
                        return Json(new { success = false, error = "Bạn không có quyền sửa dữ liệu" });
                    }
                    return Json(new { success = true, id = id });

                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = ex.Message });
                }
            }

        }
        public ActionResult UpdateDetail([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")] IEnumerable<InvoiceDetail> list)
        {
            var dbConn = Helpers.OrmliteConnection.openConn();
            if (accessDetail.them || accessDetail.sua)
            {
                var headerId = "";
                if (list != null)
                {
                    try
                    {
                    //    foreach (var item in list)
                    //    {
                    //        var isExist = dbConn.FirstOrDefault<InvoiceDetail>(s => s.id == item.id);
                    //        if (isExist != null)
                    //        {
                    //            headerId = isExist.ma_phieu_nhap_kho_header;
                    //            if (item.so_luong == item.so_luong_da_nhap + item.so_luong_nhap)
                    //            {
                    //                isExist.trang_thai = "HOAN_THANH";
                    //            }
                    //            else if (item.so_luong_da_nhap>0)
                    //            {
                    //                isExist.trang_thai = "NHAP_MOT_PHAN";
                    //            }
                    //            else if(item.so_luong_da_nhap + item.so_luong_nhap == 0)
                    //            {
                    //                isExist.trang_thai = "MOI";
                    //            }
                    //            isExist.so_luong_da_nhap = item.so_luong_da_nhap + item.so_luong_nhap;
                    //            isExist.ngay_cap_nhat = DateTime.Now;
                    //            isExist.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                    //            dbConn.Update<InvoiceDetail>(isExist);
                    //        }
                            
                    //    }
                    //    var listDetail = dbConn.Select<InvoiceDetail>(p => p.ma_phieu_nhap_kho_header == headerId && p.trang_thai != "HOAN_THANH");
                    //    if(listDetail.Count==0)
                    //    {
                    //        var header = dbConn.Select<InvoiceHeader>(p => p.ma_phieu_nhap_kho == headerId).FirstOrDefault();
                    //        header.trang_thai = "HOAN_THANH";
                    //        header.ngay_cap_nhat = DateTime.Now;
                    //        dbConn.Update<InvoiceHeader>(header);
                    //    }
                    //    else
                    //    {
                    //        var listDetailnew = dbConn.Select<InvoiceDetail>(p => p.ma_phieu_nhap_kho_header == headerId && p.trang_thai == "NHAP_MOT_PHAN");
                    //        if(listDetail.Count>0)
                    //        {
                    //            var header = dbConn.Select<InvoiceHeader>(p => p.ma_phieu_nhap_kho == headerId).FirstOrDefault();
                    //            header.trang_thai = "NHAP_MOT_PHAN";
                    //            header.ngay_cap_nhat = DateTime.Now;
                    //            dbConn.Update<InvoiceHeader>(header);
                    //        }
                    //    }
                    //    ModelState.AddModelError("Thành công!", "Lưu thành công.");
                    //    return Json(new { sussess = true });
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("error", ex.Message);
                        return Json(list.ToDataSourceResult(request, ModelState));
                    }
                }
                return Json(new { sussess = true });
            }
            else
            {
                ModelState.AddModelError("error", "Bạn không có quyền cập nhật.");
                return Json(list.ToDataSourceResult(request, ModelState));
            }
        }
        public ActionResult ConfirmFinish(string data)
        {
            var dbConn = Helpers.OrmliteConnection.openConn();
            if (accessDetail.them || accessDetail.sua)
            {

                try
                {
                    string[] separators = { "@@" };
                    var listItem = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in listItem)
                    {
                        var header = dbConn.Select<InvoiceHeader>(p => p.id == int.Parse(item)).FirstOrDefault();
                        dbConn.ExecuteNonQuery("EXEC p_ConfirmFinish @HeaderID", new { HeaderID = header.id });
                    }
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false });
                }
               
            }
            else
            {
                ModelState.AddModelError("error", "Bạn không có quyền cập nhật.");
            }
            return Json(new { success = true });

        }
        public ActionResult CreateIn(InvoiceHeader data, List<InvoiceDetail> details)
        {

            if (accessDetail.them)
            {
                if (data.id > 0 && ModelState.IsValid)
                {
                    //    try
                    //    {
                    //        using (var dbConn = Helpers.OrmliteConnection.openConn())
                    //        {
                    //            string ma_hoa_don = "";
                    //            int so_luong_sp = 0;
                    //            var loai = "HD";
                    //            var yyMMdd = DateTime.Now.ToString("yyMMdd");
                    //            var existLast = dbConn.SingleOrDefault<InvoiceHeader>("SELECT TOP 1 * FROM InvoiceHeader ORDER BY id DESC");
                    //            var nextNo = 0;
                    //            var yearNow = int.Parse(DateTime.Now.Year.ToString().Substring(2, 2));
                    //            if (existLast != null)
                    //            {
                    //                nextNo = int.Parse(existLast.ma_hoa_don.Substring(9, existLast.ma_hoa_don.Length - 9)) + 1;
                    //                var yearOld = int.Parse(existLast.ma_hoa_don.Substring(2, 2));
                    //                if (yearOld == yearNow)
                    //                {
                    //                    ma_hoa_don = loai + yyMMdd + String.Format("{0:00000}", nextNo);
                    //                }
                    //                else
                    //                {
                    //                    ma_hoa_don = loai + yyMMdd + "00001";
                    //                }
                    //            }
                    //            else
                    //            {
                    //                ma_hoa_don = loai + yyMMdd + "00001";
                    //            }
                    //            var exist = dbConn.SingleOrDefault<InvoiceHeader>("so_hoa_don={0}", data.so_hoa_don);
                    //            if (exist != null)
                    //            {
                    //                return Json(new { success = false, error = "Số hóa đơn đã tồn tại." });
                    //            }
                    //            var isHaveProduct = false;

                    //            foreach (var item in details)
                    //            {
                    //                so_luong_sp++;
                    //                InvoiceDetail newdata = new InvoiceDetail();
                    //                newdata.ma_hoa_don = ma_hoa_don;
                    //                newdata.ma_san_pham = item.ma_san_pham;
                    //                newdata.so_luong = item.so_luong;
                    //                if (newdata.so_luong == 0)
                    //                {
                    //                    continue;
                    //                }
                    //                isHaveProduct = true;
                    //                newdata.chi_phi = item.chi_phi;
                    //                newdata.don_gia = item.don_gia;
                    //                newdata.don_gia_vat = item.don_gia_vat;
                    //                newdata.thue_vat = item.thue_vat;
                    //                newdata.don_vi_tinh = item.don_vi_tinh;
                    //                newdata.thong_so_ky_thuat = item.thong_so_ky_thuat;
                    //                newdata.muc_dich_su_dung = "";
                    //                newdata.trang_thai = "";
                    //                newdata.nguoi_tao = currentUser.ma_nguoi_dung;
                    //                newdata.ngay_tao = DateTime.Now;
                    //                newdata.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                    //                newdata.nguoi_cap_nhat = "";
                    //                dbConn.Insert<InvoiceDetail>(newdata);
                    //            }
                    //            if (isHaveProduct)
                    //            {
                    //                data.ma_hoa_don = ma_hoa_don;
                    //                data.so_hoa_don = data.so_hoa_don;
                    //                data.ma_don_vi = data.ma_don_vi;
                    //                data.ma_phieu_nhap_kho = data.ma_phieu_nhap_kho;
                    //                data.so_luong_san_pham = 10;
                    //                data.vat = 0;
                    //                data.tong_tien = data.tong_tien;
                    //                data.ghi_chu = data.ghi_chu;
                    //                data.thanh_tien_vat = data.tong_tien;
                    //                data.ngay_tao = DateTime.Now;
                    //                data.nguoi_tao = currentUser.ma_nguoi_dung;
                    //                data.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                    //                data.nguoi_cap_nhat = "";
                    //                data.trang_thai = "MOI";
                    //                dbConn.Insert(data);
                    //                data.id = (int)dbConn.GetLastInsertId();
                    //            }
                    //        }
                //}

                //    catch (Exception ex)
                //    {
                //        return Json(new { success = false, error = ex.Message });
                //    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Bạn không có quyền. Vui lòng liên hệ ban quản trị để cập nhật quyền.");
            }

            return Json(new { success = true });

        }

    }
}