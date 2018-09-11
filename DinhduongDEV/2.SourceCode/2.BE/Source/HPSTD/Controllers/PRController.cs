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
using Kendo.Mvc.Extensions;
using HPSTD.Models;

namespace HPSTD.Controllers
{
    [Authorize]
    public class PRController : CustomController
    {
        // GET: PO
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listProduct = dbConn.Select<Product>("select id,ma_san_pham,ten_san_pham from Product where ma_nhom_san_pham in (select ma_phan_cap from UserProductCategory where ma_nguoi_dung ={0})", currentUser.ma_nguoi_dung);
                    ViewBag.listVender = dbConn.Select<VendorModel>(@"SELECT nha_cung_cap_id,ISNULL(ten_thuong_goi,ten_nha_cung_cap) AS ten_nha_cung_cap FROM Vendor WHERE trang_thai='true' ");
                    ViewBag.listBranch = dbConn.Select<Branch>("Select * From Branch Where ma_chi_nhanh in (Select ma_chi_nhanh From UsersBranch Where ma_nguoi_dung = '" + currentUser.ma_nguoi_dung + "')");
                    ViewBag.listTrangThai = dbConn.Select<Parameters>("loai_tham_so='TRANGTHAI'");
                    return View("PR");
                }
            }
            return RedirectToAction("NoAccessRights", "Error");
        }
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new List<PRequestHeader>();
                if (accessDetail.xem)
                {
                    if (request.Filters.Any())
                    {
                        var where = KendoApplyFilter.ApplyFilter(request.Filters[0], "a.");
                        data = dbConn.Select<PRequestHeader>(@"Select distinct a.* From PRequestHeader a
                                                               Inner Join UsersBranch b on b.ma_chi_nhanh = a.ma_chi_nhanh
                                                               Where b.ma_nguoi_dung = '" + currentUser.ma_nguoi_dung + "' and " + where);
                    }
                    else
                    {
                        data = dbConn.Select<PRequestHeader>(@"Select distinct a.* From PRequestHeader a
                                                               Inner Join UsersBranch b on b.ma_chi_nhanh = a.ma_chi_nhanh
                                                               Where b.ma_nguoi_dung = '" + currentUser.ma_nguoi_dung + "'");

                    }
                }
                else
                {
                    return RedirectToAction("NoAccessRights", "Error");
                }
                return Json(data.ToDataSourceResult(request));
            }
        }
        public ActionResult ReadDetail([DataSourceRequest]DataSourceRequest request, string ma_phieu_header)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                if (accessDetail.xem)
                {
                    var data = new List<PRequestDetail>();
                    var masanpham = dbConn.Select<Product>("select ma_san_pham from Product where ma_nhom_san_pham in (select ma_phan_cap from UserProductCategory where ma_nguoi_dung ={0})", currentUser.ma_nguoi_dung);
                    if (masanpham.Count() > 0)
                    {
                        string inmasanpham = string.Empty;
                        inmasanpham = " AND ma_san_pham IN (" + String.Join(",", masanpham.Select(s => "'" + s.ma_san_pham + "'")) + ")";
                        data = dbConn.Select<PRequestDetail>("ma_phieu='" + ma_phieu_header + "'" + inmasanpham);
                        data.ForEach(s =>
                           s.thanh_tien = s.so_luong * s.don_gia_vat
                        );
                    }
                    else
                    {
                        data = new List<PRequestDetail>();
                    }
                    return Json(data.ToDataSourceResult(request));
                }
                return RedirectToAction("NoAccessRights", "Error");
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateUpdate(PRequestHeader data)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    var ma_phieu = "";
                    if (data.id > 0)
                    {
                        if (accessDetail.sua)
                        {
                            var exist = dbConn.SingleOrDefault<PRequestHeader>("id={0} ", data.id);
                            data.id = exist.id;
                            data.ma_phieu = exist.ma_phieu;
                            data.ma_don_vi = exist.ma_don_vi;
                            //data.ma_chi_nhanh = exist.ma_chi_nhanh;
                            data.file_chu_ki_TDV = exist.file_chu_ki_TDV;
                            data.ngay_tao_yeu_cau = !string.IsNullOrEmpty(Request["ngay_tao_yeu_cau"]) ? DateTime.Parse(DateTime.ParseExact(Request["ngay_tao_yeu_cau"], "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")) : DateTime.Parse("1900-01-01");
                            data.ngay_tao = exist.ngay_tao;
                            data.ngay_giao = DateTime.Now;
                            data.ten_nguoi_lap_de_nghi = exist.ten_nguoi_lap_de_nghi;
                            data.ngay_cap_nhat = DateTime.Now;
                            data.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            data.trang_thai = exist.trang_thai;
                            dbConn.Update(data);
                            ma_phieu = data.ma_phieu;
                        }
                        else
                        {
                            return Json(new { success = false, error = "Bạn không có quyền cập nhật dữ liệu" });
                        }
                    }
                    else
                    {
                        if (accessDetail.them)
                        {
                           
                            var branch = dbConn.SingleOrDefault<Branch>("ma_chi_nhanh={0}", data.ma_chi_nhanh);
                            if (branch == null)
                            {
                                return Json(new { success = false, error = "Bạn phải chọn Phòng ban/Chi nhánh" });
                            }
                            var ma_don_vi = branch.ma_don_vi;
                            var ma_chi_nhanh = branch.ma_chi_nhanh;
                            var yyMMdd = DateTime.Now.ToString("yyMMdd");
                            var existLast = dbConn.SingleOrDefault<PRequestHeader>("SELECT TOP 1 * FROM PRequestHeader WHERE ma_don_vi={0} ORDER BY id DESC".Params(ma_don_vi));
                            var nextNo = 0;
                            var yearNow = int.Parse(DateTime.Now.Year.ToString().Substring(2, 2));
                            
                            
                            if (existLast != null)
                            {
                                nextNo = int.Parse(existLast.ma_phieu.Substring(11, existLast.ma_phieu.Length - 11)) + 1;
                                var yearOld = int.Parse(existLast.ma_phieu.Substring(5, 2));
                                if (yearOld == yearNow)
                                {
                                    ma_phieu = ma_don_vi + "PR" + yyMMdd + String.Format("{0:00000}", nextNo);
                                }
                                else
                                {
                                    ma_phieu = ma_don_vi + "PR" + yyMMdd + "00001";
                                }
                            }
                            else
                            {
                                ma_phieu = ma_don_vi + "PR" + yyMMdd + "00001";
                            }
                            data.ma_phieu = ma_phieu;
                            data.ma_don_vi = ma_don_vi;
                            data.ma_chi_nhanh = ma_chi_nhanh;
                            data.ngay_tao_yeu_cau = !string.IsNullOrEmpty(Request["ngay_tao_yeu_cau"]) ? DateTime.Parse(DateTime.ParseExact(Request["ngay_tao_yeu_cau"], "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")) : DateTime.Parse("1900-01-01");
                            data.ngay_giao = !string.IsNullOrEmpty(Request["ngay_giao"]) ? DateTime.Parse(DateTime.ParseExact(Request["ngay_giao"], "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")) : DateTime.Parse("1900-01-01");
                            data.ngay_tao = DateTime.Now;
                            data.ten_nguoi_lap_de_nghi = currentUser.ten_nguoi_dung;
                            data.so_dt_lien_lac_nguoi_lap = currentUser.dien_thoai;
                            data.nguoi_tao = currentUser.ma_nguoi_dung;
                            data.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            data.nguoi_cap_nhat = "";
                            data.trang_thai = "MOI";
                            data.y_kien_cua_don_vi = !string.IsNullOrEmpty(data.y_kien_cua_don_vi) ? data.y_kien_cua_don_vi : "";
                            data.y_kien_HCQT = !string.IsNullOrEmpty(data.y_kien_HCQT) ? data.y_kien_HCQT : "";
                            data.y_kien_khac_HO = !string.IsNullOrEmpty(data.y_kien_khac_HO) ? data.y_kien_khac_HO : "";
                            data.y_kien_QLDVKH_NQT = !string.IsNullOrEmpty(data.y_kien_QLDVKH_NQT) ? data.y_kien_QLDVKH_NQT : "";
                            data.y_kien_TTCNTT_NHDT = !string.IsNullOrEmpty(data.y_kien_TTCNTT_NHDT) ? data.y_kien_TTCNTT_NHDT : "";
                            dbConn.Insert(data);
                            data.id = (int)dbConn.GetLastInsertId();
                        }
                        else
                        {
                            return Json(new { success = false, error = "Bạn không có quyền thêm dữ liệu" });
                        }
                    }
                    return Json(new { success = true, ma_phieu = ma_phieu, id = data.id });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = ex.Message });
                }
            }
        }
        public ActionResult DeleteHeader(string data)
        {
            if (accessDetail.xoa)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    string[] separators = { "@@" };
                    var listItem = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in listItem)
                    {
                        var checkStatus = dbConn.Select<PRequestHeader>(s => s.id == int.Parse(item) && (s.trang_thai != AllConstant.TRANGTHAI_MOI && s.trang_thai != "TDV_DA_XAC_NHAN"));
                        if (checkStatus != null && checkStatus.Count > 0)
                        {
                            return Json(new { success = false, error = "Bạn không thể xóa phiếu đã duyệt" });
                        }
                        else
                        {
                            var ma_phieu = dbConn.FirstOrDefault<PRequestHeader>("id={0}", item);
                            if (ma_phieu != null)
                            {
                                dbConn.Delete<Core.Entities.PRequestHeader>("id={0}", ma_phieu.id);
                                dbConn.Delete<Core.Entities.PRequestDetail>(s => s.ma_phieu == ma_phieu.ma_phieu);
                            }
                        }
                    }
                    return Json(new { success = true });
                }
            }
            else
            {
                return Json(new { success = false, error = "Bạn không có quyền xóa dữ liệu" });
            }
        }
        public ActionResult Delete(int id)
        {
            if (accessDetail.xoa)
            {
                try
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        var ma_phieu = dbConn.FirstOrDefault<PRequestHeader>("id={0}", id);
                        if (ma_phieu != null)
                        {
                            dbConn.Delete<Core.Entities.PRequestHeader>("id={0}", ma_phieu.id);
                            dbConn.Delete<Core.Entities.PRequestDetail>(s => s.ma_phieu == ma_phieu.ma_phieu);
                        }
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
        public ActionResult DeleteDetail(string id)
        {
            if (accessDetail.xoa)
            {
                try
                {
                    string[] separators = { "@@" };
                    var listdata = id.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in listdata)
                        {
                            dbConn.Delete<PRequestDetail>("id={0}", item);
                        }
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
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult InsertUpdate([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<PRequestDetail> items, string ma_phieu)
        {
            if (accessDetail.sua || accessDetail.them)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var row in items)
                            if (!string.IsNullOrEmpty(row.ma_san_pham))
                            {
                                var checkID = dbConn.FirstOrDefault<PRequestDetail>("ma_phieu={0} AND id={1}", ma_phieu, row.id);
                                if (checkID != null)
                                {
                                    row.id = checkID.id;
                                    row.ma_san_pham_thay_the = !string.IsNullOrEmpty(row.ma_san_pham_thay_the) ? row.ma_san_pham_thay_the : "";
                                    row.so_luong_duyet = row.so_luong;
                                    row.nguoi_tao = checkID.nguoi_tao;
                                    row.ngay_tao = checkID.ngay_tao;
                                    row.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                                    row.ngay_cap_nhat = DateTime.Now;
                                    row.trang_thai = checkID.trang_thai;
                                    dbConn.Update(row);
                                }
                                else
                                {
                                    row.ma_phieu = ma_phieu;
                                    row.ma_san_pham_thay_the = !string.IsNullOrEmpty(row.ma_san_pham_thay_the) ? row.ma_san_pham_thay_the : "";
                                    row.so_luong_duyet = row.so_luong;
                                    row.nguoi_tao = currentUser.ma_nguoi_dung;
                                    row.ngay_tao = DateTime.Now;
                                    row.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                                    row.ngay_cap_nhat = DateTime.Now;
                                    row.trang_thai = "MOI";
                                    dbConn.Insert(row);
                                }
                            }
                    }
                    return Json(new { success = true, ma_phieu = ma_phieu, error = "" });
                }
            }
            else
            {
                ModelState.AddModelError("", "Bạn không có quyền cập nhật");
            }
            return Json(items.ToDataSourceResult(request, ModelState));
        }

        public ActionResult ExportPrint(int Id = 0, bool isView = true, string ma_phieu = "")
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                ViewBag.listitem = dbConn.Select<PRequestDetail>(@"
                                        SELECT detail.*, pr.ten_san_pham AS ten_san_pham  FROM PRequestDetail detail
                                            LEFT JOIN dbo.Product pr
                                            ON detail.ma_san_pham=pr.ma_san_pham
                                            WHERE detail.ma_phieu={0}
	                                      ".Params(ma_phieu));

                //ViewBag.ItemHeader = dbConn.FirstOrDefault<PRequestHeader>(@"
                //                    SELECT head.*, 
                //                        unit.ten_chi_nhanh AS ten_chi_nhanh,
                //                        unit.ten_nguoi_nhan_hang,
                //                        unit.dt_nguoi_nhan_hang,
                //                        unit.dia_chi_nhan_hang,
                //                        us.ten_nguoi_dung
                //                        FROM PRequestHeader head, [User] us,dbo.Branch unit
                //                    WHERE head.ma_chi_nhanh=unit.ma_chi_nhanh AND head.nguoi_duyet_HCQT = us.ma_nguoi_dung
                //                    OR  head.nguoi_duyet_TTCNTT_NHDT = us.ma_nguoi_dung
                //                    OR  head.nguoi_duyet_QLDVKH_NQT = us.ma_nguoi_dung
                //                    OR  head.nguoi_duyet_khac_HO = us.ma_nguoi_dung AND
                //                     head.ma_phieu=" + "'".Params(ma_phieu) + "'");

                var data = dbConn.SqlList<PRequestHeader>("EXEC ExportPrintHeaderPR @ma_phieu", new { ma_phieu = ma_phieu }).FirstOrDefault();
                ViewBag.ItemHeader = data;

                var subtemplate = "_template_product_table_pyc";
                ViewBag.subWiewName = subtemplate;
                string viewName = "_template_export_pyc";
                string html = RenderPartialViewToString(viewName);
                return View(viewName);

            }
        }
        protected string RenderPartialViewToString(string viewName, object model = null)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");
            ViewData.Model = model;
            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                return sw.GetStringBuilder().ToString();
            }
        }

        public ActionResult GetVendorByProduct(string ma_san_pham)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = dbConn.SqlList<VendorModelView>(@"
                                SELECT DISTINCT data.nha_cung_cap_id,ISNULL(data.ten_thuong_goi,data.ten_nha_cung_cap) AS ten_nha_cung_cap 
                                FROM 
                                (SELECT a.nha_cung_cap_id,b.ten_nha_cung_Cap,b.ten_thuong_goi 
                                FROM dbo.VendorProductCategory a LEFT JOIN dbo.Vendor b ON b.nha_cung_cap_id = a.nha_cung_cap_id
                                WHERE b.trang_thai ='true' AND a.ma_phan_cap=
                                	(SELECT TOP 1 ISNULL(ma_nhom_san_pham,'') 
                                			FROM dbo.Product 
                                			WHERE ma_san_pham={0})
                                			) data 
                                		LEFT JOIN	dbo.ProductPriceHeader a ON data.nha_cung_cap_id = a.nha_cung_cap_id
                                		LEFT JOIN dbo.ProductPriceDetail b ON b.id = a.id
                                WHERE a.[trang_thai] ='DANG_HOAT_DONG'".Params(ma_san_pham));
                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetPriceByCGSVendor(string ma_san_pham, int so_luong, string ma_nha_cung_cap)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = dbConn.FirstOrDefault<ProductPriceDetail>(@"
                                SELECT b.*
                                FROM 
                                (SELECT a.nha_cung_cap_id,ISNULL(b.ten_thuong_goi,b.ten_nha_cung_cap) AS ten_nha_cung_cap 
                                FROM dbo.VendorProductCategory a INNER JOIN dbo.Vendor b ON b.nha_cung_cap_id = a.nha_cung_cap_id
                                WHERE a.ma_phan_cap=
	                                (SELECT TOP 1 ISNULL(ma_nhom_san_pham,'') 
			                                FROM dbo.Product 
			                                WHERE ma_san_pham={0})
			                                ) data 
		                                INNER JOIN dbo.ProductPriceHeader a ON data.nha_cung_cap_id = a.nha_cung_cap_id
		                                INNER JOIN dbo.ProductPriceDetail b ON b.ma_chinh_sach_gia = a.ma_chinh_sach_gia
                                WHERE a.[trang_thai] ='DANG_HOAT_DONG' --AND b.sl_min <= {1} AND b.sl_max >= {1}
                                AND a.nha_cung_cap_id={2} AND b.ma_vat_tu={0}
                                AND (CONVERT(DATE,b.ngay_ap_dung,101) <= convert(DATE,getdate(),101) AND 
                                CONVERT(DATE,b.ngay_ket_thuc,101)  >= convert(DATE,getdate(),101))".Params(ma_san_pham, so_luong, ma_nha_cung_cap));

                return Json(new { success = true, data = data ?? new ProductPriceDetail() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UploadFile(string ma_phieu)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var file = Request.Files["FileSignature"];
                if (file != null)
                {
                    string destinationPath = Helpers.Upload.UploadFile("PR", file);
                    dbConn.Update<PRequestHeader>(set: "file_chu_ki_TDV = {0}".Params(destinationPath), where: "ma_phieu = {0}".Params(ma_phieu));
                }
                var data = dbConn.FirstOrDefault<PRequestHeader>("ma_phieu = {0}".Params(ma_phieu));
                return Json(new { success = true, error = "", data = data }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}