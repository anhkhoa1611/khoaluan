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
    public class StatementVendorController : CustomController
    {
        // GET: StatementVendor
        public ActionResult Index()
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                ViewBag.listDonVi = dbConn.Select<DepartmentHeirarchy>("cap=3");
                ViewBag.listDonViTinh = dbConn.Select<Parameters>("loai_tham_so ='DONVITINH'");
                ViewBag.listVendor = dbConn.Select<Vendor>(p => p.trang_thai == "true");
                ViewBag.listProduct = dbConn.Select<Product>();
                ViewBag.listTrangThai = dbConn.Select<Parameters>("loai_tham_so='TRANGTHAI'");
            }
            return View("StatementVendor");
        }
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new DataSourceResult();
                var query = @"SELECT P.id, P.ma_to_trinh_ncc, P.ten_to_trinh_ncc, P.nha_cung_cap_id, V.ten_nha_cung_Cap, V.dien_thoai, P.so_hop_dong,
                                    V.email, V.dia_chi, V.website, P.ngay_ap_dung, P.ngay_ket_thuc, P.trang_thai, P.ghi_chu, P.ngay_bao_gia
                                    , p.file_hop_dong, p.file_bao_gia, p.nguoi_tao, p.ngay_tao, p.nguoi_cap_nhat, p.ngay_cap_nhat
                            FROM StatementVendorHeader P LEFT JOIN Vendor V ON P.nha_cung_cap_id = V.nha_cung_cap_id";
                if (accessDetail.xem)
                {
                    if (request.Filters.Any())
                    {
                        var where = KendoApplyFilter.ApplyFilter(request.Filters[0]);
                        data = KendoApplyFilter.KendoDataByQuery<StatementVendorHeader>(request, query, where);
                    }
                    else
                    {
                        data = KendoApplyFilter.KendoDataByQuery<StatementVendorHeader>(request, query, "");
                    }


                }
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ReadDetail([DataSourceRequest]DataSourceRequest request, string ma_to_trinh_ncc = "")
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                if (accessDetail.xem)
                {
                    var data = new DataSourceResult();
                    data = KendoApplyFilter.KendoData<StatementVendorDetail>(request, "ma_to_trinh_ncc={0}".Params(ma_to_trinh_ncc));
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                return RedirectToAction("NoAccessRights", "Error");
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<StatementVendorDetail> items, StatementVendorHeader inputHead)
        {
            if (accessDetail.sua || accessDetail.them)
            {
                var Ma_TTNCC = "";
                var IdHead = 0;
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        //insert header
                        var checkHead = dbConn.FirstOrDefault<StatementVendorHeader>(s => s.id == inputHead.id);
                        if (checkHead != null)
                        {
                            checkHead.ghi_chu = !string.IsNullOrEmpty(inputHead.ghi_chu) ? inputHead.ghi_chu : "";
                            checkHead.ngay_bao_gia = inputHead.ngay_bao_gia;
                            checkHead.ngay_tao = checkHead.ngay_tao;
                            checkHead.nguoi_tao = checkHead.nguoi_tao;
                            checkHead.ngay_cap_nhat = DateTime.Now;
                            checkHead.so_hop_dong = !string.IsNullOrEmpty(inputHead.so_hop_dong) ? inputHead.so_hop_dong : "";
                            checkHead.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            dbConn.Update(checkHead);

                            IdHead = checkHead.id;
                            Ma_TTNCC = checkHead.ma_to_trinh_ncc;
                        }
                        else
                        {
                            var prefix = "CSG";
                            var isExist = dbConn.SingleOrDefault<StatementVendorHeader>("select id,ma_to_trinh_ncc from StatementVendorHeader order by id desc");
                            if (isExist != null)
                            {
                                var nextNo = int.Parse(isExist.ma_to_trinh_ncc.Substring(9, isExist.ma_to_trinh_ncc.Length - 9)) + 1;
                                inputHead.ma_to_trinh_ncc = prefix + DateTime.Now.ToString("yyMMdd") + String.Format("{0:00}", nextNo);
                            }
                            else
                            {
                                inputHead.ma_to_trinh_ncc = prefix + DateTime.Now.ToString("ddMMyy") + "01";
                            }
                            inputHead.so_hop_dong = !string.IsNullOrEmpty(inputHead.so_hop_dong) ? inputHead.so_hop_dong : "";
                            inputHead.ghi_chu = !string.IsNullOrEmpty(inputHead.ghi_chu) ? inputHead.ghi_chu : "";
                            inputHead.trang_thai = AllConstant.TRANGTHAI_MOI;
                            inputHead.ngay_tao = DateTime.Now;
                            inputHead.nguoi_tao = User.Identity.Name;
                            inputHead.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            dbConn.Insert(inputHead);
                            IdHead = (int)dbConn.GetLastInsertId();
                            Ma_TTNCC = inputHead.ma_to_trinh_ncc;
                        }
                        //insert detail
                        foreach (var row in items)
                        {
                            if (string.IsNullOrEmpty(row.ma_vat_tu))
                            {
                                continue;
                            }
                            var checkDetail = dbConn.FirstOrDefault<StatementVendorDetail>(p => p.ma_to_trinh_ncc == Ma_TTNCC && p.ma_vat_tu == row.ma_vat_tu);
                            if (checkDetail != null)
                            {
                                checkDetail.gia_bao = row.gia_bao;
                                checkDetail.thue_vat = row.thue_vat;
                                checkDetail.gia_bao_gom_vat = row.gia_bao_gom_vat;
                                checkDetail.ngay_ap_dung = row.ngay_ap_dung;
                                checkDetail.ngay_ket_thuc = row.ngay_ket_thuc;
                                checkDetail.ngay_cap_nhat = DateTime.Now;
                                checkDetail.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                                checkDetail.don_vi_tinh = !string.IsNullOrEmpty(row.don_vi_tinh) ? row.don_vi_tinh : "";
                                checkDetail.sl_min = row.sl_min != 0 ? row.sl_min : 0;
                                checkDetail.sl_max = row.sl_max != 0 ? row.sl_max : 0;
                                dbConn.Update(checkDetail);
                            }
                            else
                            {
                                row.trang_thai = row.trang_thai;
                                row.ngay_tao = DateTime.Now;
                                row.nguoi_tao = User.Identity.Name;
                                row.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                                row.nguoi_cap_nhat = "";
                                row.ma_to_trinh_ncc = Ma_TTNCC;
                                row.don_vi_tinh = !string.IsNullOrEmpty(row.don_vi_tinh) ? row.don_vi_tinh : "";
                                row.sl_min = row.sl_min != 0 ? row.sl_min : 0;
                                row.sl_max = row.sl_max != 0 ? row.sl_max : 0;
                                dbConn.Insert(row);
                            }
                        }
                    }
                }
                return Json(new { success = true, ma_ttncc = Ma_TTNCC, error = "", IdHead = IdHead });
            }
            else
            {
                return Json(new { success = false, error = "Bạn không có quyền cập nhật" });
            }
        }

        public ActionResult DeleteList(string data)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            using (var dbTrans = dbConn.OpenTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    string[] separators = { "@@" };
                    var listid = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var id in listid)
                    {
                        dbConn.Delete<StatementVendorHeader>(p => p.ma_to_trinh_ncc == id);
                        dbConn.Delete<StatementVendorDetail>(p => p.ma_to_trinh_ncc == id);
                    }
                    dbTrans.Commit();
                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    dbTrans.Rollback();
                    return Json(new { success = false, error = e.Message });
                }
            }
        }

        public ActionResult ApproveList(string data)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            using (var dbTrans = dbConn.OpenTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    string[] separators = { "@@" };
                    var listid = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var id in listid)
                    {

                        var datavendor = dbConn.FirstOrDefault<StatementVendorHeader>("ma_to_trinh_ncc ={0}".Params(id));
                        var datavendordetail = dbConn.Select<StatementVendorDetail>("ma_to_trinh_ncc ={0}".Params(id));

                        var databg = new ProductPriceHeader
                        {
                            nha_cung_cap_id = datavendor.nha_cung_cap_id,
                            ngay_ap_dung = datavendor.ngay_ap_dung,
                            ngay_bao_gia = datavendor.ngay_ap_dung,
                            ngay_ket_thuc = datavendor.ngay_ket_thuc,
                            ghi_chu = datavendor.ghi_chu,
                            trang_thai = "DANG_HOAT_DONG",
                            nguoi_tao = currentUser.ma_nguoi_dung,
                            ngay_tao = DateTime.Now
                        };

                        var isExist = dbConn.SingleOrDefault<ProductPriceHeader>("select * from ProductPriceHeader order by id desc");
                        var prefix = "CSG";
                        if (isExist != null)
                        {
                            var nextNo = int.Parse(isExist.ma_chinh_sach_gia.Substring(9, isExist.ma_chinh_sach_gia.Length - 9)) + 1;
                            databg.ma_chinh_sach_gia = prefix + DateTime.Now.ToString("yyMMdd") + String.Format("{0:00}", nextNo);
                        }
                        else
                        {
                            databg.ma_chinh_sach_gia = prefix + DateTime.Now.ToString("ddMMyy") + "01";
                        }

                        dbConn.Insert(databg);

                        foreach (var item in datavendordetail)
                        {
                            var detail = new ProductPriceDetail {
                                ma_chinh_sach_gia=databg.ma_chinh_sach_gia,
                                ma_vat_tu = item.ma_vat_tu,
                                gia_bao = item.gia_bao,
                                thue_vat = item.thue_vat,
                                gia_bao_gom_vat = item.gia_bao_gom_vat,
                                ngay_ap_dung = item.ngay_ap_dung,
                                ngay_ket_thuc = item.ngay_ket_thuc,
                                don_vi_tinh = item.don_vi_tinh,
                                sl_min = item.sl_min,
                                sl_max = item.sl_max,
                                nguoi_tao = currentUser.ma_nguoi_dung,
                                ngay_tao = DateTime.Now
                            };
                            dbConn.Insert(detail);
                        }

                        dbConn.Update<StatementVendorHeader>(set: "trang_thai={0}".Params(AllConstant.TRANGTHAI_DA_DUYET), where: "ma_to_trinh_ncc={0}".Params(id));
                    }
                    dbTrans.Commit();
                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    dbTrans.Rollback();
                    return Json(new { success = false, error = e.Message });
                }
            }
        }

        public ActionResult DeleteDetail(int id)
        {
            if (accessDetail.xoa)
            {
                try
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        dbConn.Delete<StatementVendorDetail>("id={0}", id);
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = ex.Message });
                }
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = "Bạn không có quyền xóa dữ liệu" });
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UploadFile(string ma_ttncc)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var file = Request.Files["FileUploadBG"];
                if (file != null)
                {
                    string destinationPath = Helpers.Upload.UploadFile("BaoGia", file);
                    dbConn.Update<StatementVendorHeader>(set: "file_bao_gia = {0}".Params(destinationPath), where: "ma_to_trinh_ncc = {0}".Params(ma_ttncc));
                }
                var fileHD = Request.Files["FileUploadHD"];
                if (fileHD != null)
                {
                    string destinationPath = Helpers.Upload.UploadFile("HopDong", fileHD);
                    dbConn.Update<StatementVendorHeader>(set: "file_hop_dong = {0}".Params(destinationPath), where: "ma_to_trinh_ncc = {0}".Params(ma_ttncc));
                }
                var data = dbConn.FirstOrDefault<StatementVendorHeader>("ma_to_trinh_ncc = {0}".Params(ma_ttncc));
                return Json(new { success = true, error = "", data = data }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}