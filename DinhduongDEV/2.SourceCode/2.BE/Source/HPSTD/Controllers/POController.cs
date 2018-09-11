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

namespace HPSTD.Controllers
{
    [Authorize]
    public class POController : CustomController
    {
        // GET: PO
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listProduct = dbConn.Select<Product>();
                    //ViewBag.listDonVi = dbConn.Select<DepartmentHeirarchy>("cap=3");
                    ViewBag.listVendor = dbConn.Select<Vendor>("select * from Vendor where nha_cung_cap_id in (select distinct ma_nha_cung_cap from POHeader)");
                    return View();
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
                    var data = new DataSourceResult();
                    data = KendoApplyFilter.KendoData<POHeader>(request);
                    return Json(data);
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
                    ViewBag.listDonVi = dbConn.Select<Branch>("Select ma_chi_nhanh, ten_chi_nhanh from Branch");
                    ViewBag.StatementHeader = dbConn.Select<StatementHeader>();
                    ViewBag.Vendor = dbConn.Select<Vendor>(@"select * from Vendor
                                                        where nha_cung_cap_id in 
                                                        (
                                                            select distinct d.ma_nha_cung_cap
                                                            FROM StatementHeader h
                                                            INNER JOIN StatementDetail d
                                                            ON h.ma_phieu = d.ma_phieu_header
                                                            WHERE ISNULL(d.ma_don_dat_hang, '') = ''
                                                        )");
                    return View();
                }
            }
            return RedirectToAction("NoAccessRights", "Error");
        }

        public ActionResult ReadStatement([DataSourceRequest]DataSourceRequest request)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                if (accessDetail.xem)
                {
                    var data = new DataSourceResult();
                    var strsql = @" SELECT * FROM (SELECT d.*, d.don_gia_vat * d.so_luong AS thanh_tien, b.dia_chi as dia_chi_don_vi
                                                    FROM StatementHeader h
                                                    INNER JOIN StatementDetail d
                                                    ON h.ma_phieu = d.ma_phieu_header
                                                    LEFT JOIN dbo.Vendor v
                                                    ON  d.ma_nha_cung_cap = v.nha_cung_cap_id
                                                    INNER JOIN Branch b
													on d.ma_chi_nhanh=b.ma_chi_nhanh
													and d.ma_don_vi=b.ma_don_vi
                                                    WHERE ISNULL(d.ma_don_dat_hang,'') = ''
                                    ) data ";

                    string whereCondition = "";

                    if (request.Filters != null)
                        if (request.Filters.Count != 0)
                            whereCondition = KendoApplyFilter.ApplyFilter(request.Filters[0], "");
                        else
                            whereCondition = " 1 = 0 ";
                    else
                        whereCondition = " 1 = 0 ";

                    data = KendoApplyFilter.KendoDataByQuery<StatementDetail>(request, strsql, whereCondition);
                    return Json(data);
                }
                return RedirectToAction("NoAccessRights", "Error");
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateUpdate(POHeader data, List<PODetail> details)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    int id = 0;
                    if (accessDetail.them)
                    {
                        string ma_phieu = "";
                        var loai = "PO";
                        //var ma_don_vi = currentUser.ma_don_vi;
                        var yyMMdd = DateTime.Now.ToString("yyMMdd");
                        var existLast = dbConn.SingleOrDefault<POHeader>("SELECT TOP 1 * FROM POHeader ORDER BY id DESC");
                        var nextNo = 0;
                        var yearNow = int.Parse(DateTime.Now.Year.ToString().Substring(2, 2));
                        if (existLast != null)
                        {
                            nextNo = int.Parse(existLast.ma_phieu.Substring(8, existLast.ma_phieu.Length - 8)) + 1;
                            var yearOld = int.Parse(existLast.ma_phieu.Substring(2, 2));
                            if (yearOld == yearNow)
                            {
                                ma_phieu = loai + yyMMdd + String.Format("{0:00000}", nextNo);
                            }
                            else
                            {
                                ma_phieu = loai + yyMMdd + "00001";
                            }
                        }
                        else
                        {
                            ma_phieu = loai + yyMMdd + "00001";
                        }

                        data.ma_phieu = ma_phieu;
                        //data.ngay_tao_yeu_cau = !string.IsNullOrEmpty(Request["ngay_tao_yeu_cau"]) ? DateTime.Parse(DateTime.ParseExact(Request["ngay_tao_yeu_cau"], "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")) : DateTime.Parse("1900-01-01");
                        //data.ngay_cap_thiet_bi = !string.IsNullOrEmpty(Request["ngay_cap_thiet_bi"]) ? DateTime.Parse(DateTime.ParseExact(Request["ngay_cap_thiet_bi"], "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")) : DateTime.Parse("1900-01-01");
                        //data.ten_phieu = data.ma_phieu;
                        data.ngay_tao = DateTime.Now;
                        data.nguoi_tao = currentUser.ma_nguoi_dung;
                        data.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                        data.nguoi_cap_nhat = "";
                        data.trang_thai = "MOI";
                        dbConn.Insert(data);
                        id = (int)dbConn.GetLastInsertId();


                        foreach (var item in details)
                        {
                            PODetail newdata = new PODetail();
                            newdata.ma_phieu_header = data.ma_phieu;
                            newdata.ma_san_pham = item.ma_san_pham;
                            newdata.so_luong = item.so_luong;
                            newdata.thong_so_ky_thuat = item.thong_so_ky_thuat;
                            newdata.muc_dich_su_dung = "";
                            newdata.nguoi_tao = currentUser.ma_nguoi_dung;
                            newdata.ma_to_trinh = item.ma_to_trinh;
                            newdata.id_StatementDetail = item.id_StatementDetail;
                            newdata.don_gia_vat = item.don_gia_vat;
                            newdata.don_gia = item.don_gia;
                            newdata.thue_vat = item.thue_vat;
                            newdata.don_vi_tinh = item.don_vi_tinh;
                            newdata.chi_phi = item.chi_phi;
                            newdata.ma_don_vi = item.ma_don_vi;
                            newdata.ma_chi_nhanh = item.ma_chi_nhanh;
                            newdata.thong_tin_noi_bo = item.thong_tin_noi_bo;
                            newdata.ngay_tao = DateTime.Now;
                            newdata.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            newdata.nguoi_cap_nhat = "";
                            newdata.trang_thai = "";
                            dbConn.Insert<PODetail>(newdata);
                            StatementDetail detail = dbConn.FirstOrDefault<StatementDetail>(s => s.id == item.id_StatementDetail);
                            detail.ma_don_dat_hang = ma_phieu;
                            dbConn.Update(detail);
                        }
                    }
                    else
                    {
                        return Json(new { success = false, error = "Bạn không có quyền thêm dữ liệu" });
                    }
                    return Json(new { success = true, id = id });

                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = ex.Message });
                }
            }

        }

        public ActionResult Edit(int id)
        {
            if (accessDetail.sua)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listProduct = dbConn.Select<Product>();
                    ViewBag.listDonVi = dbConn.Select<Branch>("Select ma_chi_nhanh, ten_chi_nhanh from Branch");
                    ViewBag.POHeader = dbConn.FirstOrDefault<POHeader>(s => s.id == id);
                    ViewBag.StatementHeader = dbConn.Select<StatementHeader>();
                    ViewBag.Vendor = dbConn.Select<Vendor>();
                    var data = dbConn.FirstOrDefault<POHeader>(s => s.id == id);
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
                    var strsql = @"SELECT * FROM (SELECT d.*, b.dia_chi as dia_chi_don_vi
                                                    FROM PODetail d
                                                    INNER JOIN Branch b
													on d.ma_chi_nhanh=b.ma_chi_nhanh
													and d.ma_don_vi=b.ma_don_vi
                                    ) data  ";

                    data = KendoApplyFilter.KendoDataByQuery<PODetail>(request, strsql, "ma_phieu_header = {0}".Params(ma_phieu_header));
                    return Json(data);
                }
                return RedirectToAction("NoAccessRights", "Error");
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(POHeader data, List<PODetail> details)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    if (accessDetail.sua)
                    {
                        var exist = dbConn.SingleOrDefault<POHeader>("id={0}", data.id);
                        //exist.ten_phieu = data.ten_phieu;
                        exist.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                        exist.ngay_cap_nhat = DateTime.Now;
                        dbConn.Update(exist);
                        foreach (var item in details)
                        {
                            var detail = dbConn.FirstOrDefault<PODetail>(s => s.id == item.id);
                            detail.thong_so_ky_thuat = item.thong_so_ky_thuat;
                            detail.muc_dich_su_dung = item.muc_dich_su_dung;
                            detail.thong_tin_noi_bo = item.thong_tin_noi_bo;
                            detail.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            detail.ngay_cap_nhat = DateTime.Now;
                            dbConn.Update(detail);
                        }
                    }
                    else
                    {
                        return Json(new { success = false, error = "Bạn không có quyền sửa dữ liệu" });
                    }
                    return Json(new { success = true, id = data.id });

                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = ex.Message });
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
                        var podetail = dbConn.FirstOrDefault<PODetail>(s => s.id == id);
                        dbConn.Update<StatementDetail>(set: "ma_don_dat_hang = null", where: "id = {0} ".Params(podetail.id_StatementDetail));
                        dbConn.Delete<PODetail>("id = {0}", id);
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
        public ActionResult Adddetail(string ma_phieu_header, List<PODetail> details)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    if (accessDetail.sua)
                    {

                        foreach (var item in details)
                        {
                            PODetail newdata = new PODetail();
                            newdata.ma_phieu_header = ma_phieu_header;
                            newdata.ma_san_pham = item.ma_san_pham;
                            newdata.so_luong = item.so_luong;
                            newdata.thong_so_ky_thuat = item.thong_so_ky_thuat;
                            newdata.muc_dich_su_dung = "";
                            newdata.nguoi_tao = currentUser.ma_nguoi_dung;
                            newdata.ma_to_trinh = item.ma_to_trinh;
                            newdata.id_StatementDetail = item.id_StatementDetail;
                            newdata.don_gia_vat = item.don_gia_vat;
                            newdata.don_gia = item.don_gia;
                            newdata.thue_vat = item.thue_vat;
                            newdata.don_vi_tinh = item.don_vi_tinh;
                            newdata.chi_phi = item.chi_phi;
                            newdata.ma_don_vi = item.ma_don_vi;
                            newdata.ma_chi_nhanh = item.ma_chi_nhanh;
                            newdata.thong_tin_noi_bo = item.thong_tin_noi_bo;
                            newdata.ngay_tao = DateTime.Now;
                            newdata.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            newdata.nguoi_cap_nhat = "";
                            newdata.trang_thai = "";
                            dbConn.Insert<PODetail>(newdata);
                            StatementDetail detail = dbConn.FirstOrDefault<StatementDetail>(s => s.id == item.id_StatementDetail);
                            detail.ma_don_dat_hang = ma_phieu_header;
                            dbConn.Update(detail);
                        }
                    }
                    else
                    {
                        return Json(new { success = false, error = "Bạn không có quyền sửa dữ liệu" });
                    }
                    return Json(new { success = true, ma_phieu_header = ma_phieu_header });

                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = ex.Message });
                }
            }
        }

        public ActionResult ExportPrint(int Id = 0, bool isView = true, string listPO = "")
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {

                ViewBag.listitem = dbConn.Select<PODetail>(@"
                                        SELECT detail.*, pr.ten_san_pham AS ten_san_pham, d.ten_chi_nhanh as ten_don_vi,d.dia_chi as dia_chi_dv,
                                        detail.don_gia * detail.so_luong as thanh_tien, std.ngay_tao as ngay_to_trinh, p.gia_tri as ten_don_vi_tinh,detail.ma_phieu_header,std.ma_pyc_header as ma_phieu_PR
                                        FROM PODetail detail
                                        LEFT JOIN dbo.Product pr
                                        ON detail.ma_san_pham=pr.ma_san_pham
                                        LEFT JOIN Parameters p ON pr.ma_don_vi_tinh = p.ma_tham_so
                                        LEFT JOIN Branch d
                                        ON detail.ma_don_vi = d.ma_don_vi
                                        INNER JOIN StatementDetail std
                                        ON detail.id_StatementDetail = std.id
                                        WHERE detail.ma_phieu_header={0}
                                        order by d.ten_chi_nhanh
	                                    ".Params(listPO));

                //string[] listSP = dbConn.FirstOrDefault<PODetail>(@"select p.ma_san_pham
                //                                        from PODetail p
                //                                        where p.ma_phieu_header=".Params(listPO));
                //foreach (var item in listSP)
                //{
                //    ViewBag.listSTM = dbConn.Select<PODetail>(@"select DISTINCT po.ma_to_trinh 
                //                                                from PODetail po, StatementDetail st
                //                                                where st.ma_phieu_header=po.ma_to_trinh and st.ma_san_pham={0}".Params(item.ma_san_pham));
                //}


                //POHeader itemHeader = dbConn.FirstOrDefault<POHeader>(s => s.ma_phieu == listPO);
                POHeader itemHeader = dbConn.SqlList<POHeader>("EXEC ExportPrintPOHeader @WhereCondition", new { WhereCondition = " AND ma_phieu = {0}".Params(listPO) }).FirstOrDefault();
                ViewBag.ItemHeader = itemHeader;
                ViewBag.Vendor = dbConn.FirstOrDefault<Vendor>(s => s.nha_cung_cap_id == itemHeader.ma_nha_cung_cap);

                var subtemplate = "_template_product_table_po";
                ViewBag.subWiewName = subtemplate;
                string viewName = "_template_export_po";
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
                        var checkStatus = dbConn.FirstOrDefault<POHeader>(s => s.id == int.Parse(item) && s.trang_thai != "MOI");
                        if (checkStatus != null)
                        {
                            return Json(new { success = false, error = "Bạn không thể xóa đơn đặt hàng đã duyệt" });
                        }
                        else
                        {
                            checkStatus = dbConn.FirstOrDefault<POHeader>(s => s.id == int.Parse(item));
                            dbConn.Delete<Core.Entities.POHeader>("ma_phieu={0}", checkStatus.ma_phieu);
                            dbConn.Delete<Core.Entities.PODetail>(s => s.ma_phieu_header == checkStatus.ma_phieu);
                            dbConn.Update<StatementDetail>(set: "ma_don_dat_hang = null", where: "ma_don_dat_hang = {0}".Params(checkStatus.ma_phieu));
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

        public ActionResult Approve(string data)
        {
            if (accessDetail.sua)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    string[] separators = { "@@" };
                    var listItem = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var itemid in listItem)
                    {

                        var poheader = dbConn.FirstOrDefault<POHeader>(s => s.id == int.Parse(itemid));
                        var podetails = dbConn.Select<PODetail>(s => s.ma_phieu_header == poheader.ma_phieu);

                        //var lstStockIn = podetail.GroupBy(
                        //                p => p.ma_chi_nhanh,
                        //                (key, g) => new { ma_chi_nhanh = key, Details = g.ToList() });
                        //foreach (var stockin in lstStockIn)
                        //{
                        StockInHeader stockinheader = new StockInHeader();
                        string ma_phieu = "";
                        var loai = "SI";
                        //var ma_don_vi = currentUser.ma_don_vi;
                        var yyMMdd = DateTime.Now.ToString("yyMMdd");
                        var existLast = dbConn.SingleOrDefault<StockInHeader>("SELECT TOP 1 * FROM StockInHeader ORDER BY id DESC");
                        var nextNo = 0;
                        var yearNow = int.Parse(DateTime.Now.Year.ToString().Substring(2, 2));
                        if (existLast != null)
                        {
                            nextNo = int.Parse(existLast.ma_phieu.Substring(8, existLast.ma_phieu.Length - 8)) + 1;
                            var yearOld = int.Parse(existLast.ma_phieu.Substring(2, 2));
                            if (yearOld == yearNow)
                            {
                                ma_phieu = loai + yyMMdd + String.Format("{0:00000}", nextNo);
                            }
                            else
                            {
                                ma_phieu = loai + yyMMdd + "00001";
                            }
                        }
                        else
                        {
                            ma_phieu = loai + yyMMdd + "00001";
                        }

                        stockinheader.ma_phieu = ma_phieu;

                        //stockinheader.ten_phieu_nhap_kho = stockinheader.ma_phieu_nhap_kho;
                        //stockinheader.ma_don_vi = stockin.ma_chi_nhanh;
                        //stockinheader.ma_phieu_po = poheader.ma_phieu;
                        stockinheader.ma_nha_cung_cap = poheader.ma_nha_cung_cap;
                        stockinheader.ghi_chu = poheader.ghi_chu;
                        stockinheader.ngay_tao = DateTime.Now;
                        stockinheader.nguoi_tao = currentUser.ma_nguoi_dung;
                        stockinheader.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                        stockinheader.nguoi_cap_nhat = "";
                        stockinheader.trang_thai = "MOI";
                        dbConn.Insert(stockinheader);

                        foreach (var item in podetails)
                        {
                            StockInDetail newdata = new StockInDetail();
                            newdata.ma_phieu_header = stockinheader.ma_phieu;
                            newdata.ma_san_pham = item.ma_san_pham;
                            newdata.so_luong = item.so_luong;
                            newdata.so_luong_da_nhap = 0;
                            newdata.so_luong_con_lai = item.so_luong;
                            newdata.id_po_detail = item.id;
                            newdata.thong_so_ky_thuat = item.thong_so_ky_thuat;
                            newdata.muc_dich_su_dung = "";
                            newdata.nguoi_tao = currentUser.ma_nguoi_dung;
                            newdata.ma_to_trinh = item.ma_to_trinh;
                            newdata.id_StatementDetail = item.id_StatementDetail;
                            newdata.don_gia_vat = item.don_gia_vat;
                            newdata.don_gia = item.don_gia;
                            newdata.thue_vat = item.thue_vat;
                            newdata.don_vi_tinh = item.don_vi_tinh;
                            newdata.chi_phi = item.chi_phi;
                            newdata.ma_don_vi = item.ma_don_vi;
                            newdata.ma_chi_nhanh = item.ma_chi_nhanh;
                            newdata.thong_tin_noi_bo = item.thong_tin_noi_bo;
                            newdata.ngay_tao = DateTime.Now;
                            newdata.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            newdata.nguoi_cap_nhat = "";
                            newdata.trang_thai = "";
                            dbConn.Insert<StockInDetail>(newdata);
                        }
                        dbConn.Update<POHeader>(set: "trang_thai = {0}, ngay_duyet = {1}".Params(AllConstant.TRANGTHAI_DA_DUYET, DateTime.Now), where: "id = {0}".Params(int.Parse(itemid)));
                    }

                    //}
                    return Json(new { success = true });
                }
            }
            else
            {
                return Json(new { success = false, error = "Bạn không có quyền duyệt đơn đặt hàng" });
            }
        }
    }
}