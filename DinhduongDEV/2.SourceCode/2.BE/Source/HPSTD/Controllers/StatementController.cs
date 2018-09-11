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
using OfficeOpenXml;

namespace HPSTD.Controllers
{
    [Authorize]
    public class StatementController : CustomController
    {
        // GET: Statement
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {                   
                    return View();
                }
            }
            return RedirectToAction("NoAccessRights", "Error");
        }

        public ActionResult Create()
        {
            if (accessDetail.them)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listProduct = dbConn.Select<Product>();
                    ViewBag.listDonVi = dbConn.Select<Branch>();
                    ViewBag.PORequestHeader = dbConn.Select<PRequestHeader>();
                    ViewBag.Vendor = dbConn.Select<Vendor>();
                    //dbConn.Delete<SelectedPO>(s => s.nguoi_tao == currentUser.ma_nguoi_dung);
                    return View();
                }
            }
            return RedirectToAction("NoAccessRights", "Error");
        }

        public ActionResult Edit(int id)
        {
            if (accessDetail.sua)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listProduct = dbConn.Select<Product>();
                    ViewBag.listDonVi = dbConn.Select<Branch>();
                    ViewBag.StatementHeader = dbConn.FirstOrDefault<StatementHeader>(s => s.id == id);
                    ViewBag.PORequestHeader = dbConn.Select<PRequestHeader>();
                    ViewBag.Vendor = dbConn.Select<Vendor>();
                    var data = dbConn.FirstOrDefault<StatementHeader>(s => s.id == id);
                    return View(data);
                }
            }
            else
                return RedirectToAction("NoAccess", "Error");
        }

        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                if (accessDetail.xem)
                {
                    var data = new DataSourceResult();
                    data = KendoApplyFilter.KendoData<StatementHeader>(request);
                    return Json(data);
                }
                return RedirectToAction("NoAccessRights", "Error");
            }
        }

        public ActionResult ReadPYC([DataSourceRequest]DataSourceRequest request)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                if (accessDetail.xem)
                {
                    var data = new DataSourceResult();
                    var strsql = @" SELECT * FROM (SELECT pod.*, h.ma_don_vi, h.ma_chi_nhanh, pod.so_luong_duyet*pod.don_gia_vat as thanh_tien
                                                FROM PRequestHeader h
                                                INNER JOIN PRequestDetail pod
                                                ON h.ma_phieu = pod.ma_phieu
                                                LEFT JOIN dbo.Branch d
                                                ON h.ma_chi_nhanh = d.ma_chi_nhanh
                                                WHERE ISNULL(pod.ma_to_trinh,'') = ''
                                                 AND h.trang_thai={0}
                                    ) data WHERE 1 = 1 ".Params(AllConstant.TRANGTHAI_DA_DUYET);

                    string whereCondition = "";

                    if (request.Filters != null)
                        if (request.Filters.Count != 0)
                            whereCondition = KendoApplyFilter.ApplyFilter(request.Filters[0], "");

                    //data = KendoApplyFilter.KendoDataByQuery<PRequestDetail>(request, strsql, whereCondition);
                    //var data = dbConn.SqlList<PORequestDetail>("EXEC p_get_PO_CreateStatement @WhereCondition", new { WhereCondition =  whereCondition });
                    var lstResult = dbConn.SqlList<PRequestDetail>(strsql + whereCondition);
                    lstResult.ForEach(s => {
                        if (!string.IsNullOrEmpty(s.ma_san_pham_thay_the))
                            s.ma_san_pham = s.ma_san_pham_thay_the;
                    });
                    return Json(lstResult.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
                return RedirectToAction("NoAccessRights", "Error");
            }
        }

        public ActionResult ReadDetail([DataSourceRequest]DataSourceRequest request, string ma_phieu_header)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                if (accessDetail.xem)
                {
                    var data = new DataSourceResult();
                    data = KendoApplyFilter.KendoData<StatementDetail>(request, "ma_phieu_header={0}".Params(ma_phieu_header));
                    return Json(data);
                }
                return RedirectToAction("NoAccessRights", "Error");
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
                        var checkStatus = dbConn.FirstOrDefault<StatementHeader>(s => s.id == int.Parse(item) && s.trang_thai != "MOI");
                        if (checkStatus != null)
                        {
                            return Json(new { success = false, error = "Bạn không thể xóa tờ trình đã duyệt" });
                        }
                        else
                        {
                            checkStatus = dbConn.FirstOrDefault<StatementHeader>(s => s.id == int.Parse(item));
                            dbConn.Delete<Core.Entities.StatementHeader>("ma_phieu={0}", checkStatus.ma_phieu);
                            dbConn.Delete<Core.Entities.StatementDetail>(s => s.ma_phieu_header == checkStatus.ma_phieu);
                            string sql = "update PRequestDetail set ma_to_trinh = null where ma_to_trinh={0}".Params(checkStatus.ma_phieu);
                            SqlHelperAsync.ExecuteNonQuery(dbConn.ConnectionString, System.Data.CommandType.Text, sql);
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
                        var checkStatus = dbConn.FirstOrDefault<StatementHeader>(s => s.id == id && s.trang_thai != "MOI");
                        if (checkStatus != null)
                        {
                            return Json(new { success = false, error = "Bạn không thể xóa tờ trình đã duyệt" });
                        }
                        else
                        {
                            dbConn.Delete<StatementHeader>("id={0}", id);
                            dbConn.Delete<StatementDetail>("ma_phieu_header = {0}", checkStatus.ma_phieu);

                            string sql = "update PRequestDetail set ma_to_trinh = null where ma_to_trinh = {0} ".Params(checkStatus.ma_phieu);
                            SqlHelperAsync.ExecuteNonQuery(dbConn.ConnectionString, System.Data.CommandType.Text, sql);
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
        public ActionResult DeleteDetail(int id)
        {
            if (accessDetail.xoa)
            {
                try
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        var statemenedetail = dbConn.FirstOrDefault<StatementDetail>(s => s.id == id);
                        dbConn.Update<PRequestDetail>(set: "ma_to_trinh = null", where: "ma_to_trinh = {0} AND ma_san_pham = {1} AND ma_phieu_header = {2}".Params(statemenedetail.ma_phieu_header, statemenedetail.ma_san_pham, statemenedetail.ma_pyc_header));
                        dbConn.Delete<StatementDetail>("id = {0}", id);
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
        public ActionResult ExportData([DataSourceRequest]DataSourceRequest request, string ma_phieu_header)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                //using (ExcelPackage excelPkg = new ExcelPackage())
                FileInfo fileInfo = new FileInfo(Server.MapPath(@"~\ExportExcelFile\StatementEx.xlsx"));
                var excelPkg = new ExcelPackage(fileInfo);
                string fileName = "StatementEx_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                var data = new List<StatementDetail>();
                if (accessDetail.xuat)
                {
                    if (request.Filters.Any())
                    {
                        var where = KendoApplyFilter.ApplyFilter(request.Filters[0], "");
                        data = dbConn.Select<StatementDetail>(where).ToList();
                    }
                    else
                    {
                        data = dbConn.Select<StatementDetail>().ToList();
                    }
                }
                ExcelWorksheet Sheet = excelPkg.Workbook.Worksheets["BTH"];
                ViewBag.listitem = dbConn.Select<StatementDetail>(@"
                                       SELECT DISTINCT detail.*, pr.ten_san_pham AS ten_san_pham, d.ten_chi_nhanh as ten_don_vi,
                                        detail.don_gia, detail.don_gia_vat, p.gia_tri as don_vi_tinh,
                                        detail.don_gia_vat * detail.so_luong as thanh_tien,
                                        v.ten_nha_cung_cap,
										--prd.ma_phieu as ma_phieu_pr,
                                        CASE WHEN pph.so_hop_dong is null THEN CONVERT(VARCHAR(10), pph.ngay_bao_gia, 101)
										ELSE pph.so_hop_dong + ' - ' + CONVERT(VARCHAR(10), pph.ngay_ky_hop_dong, 101) END AS thong_tin_hop_dong
                                        FROM StatementDetail detail
                                        LEFT JOIN dbo.Product pr
                                        ON detail.ma_san_pham=pr.ma_san_pham
                                        LEFT JOIN dbo.Branch d
                                        ON detail.ma_chi_nhanh = d.ma_chi_nhanh
                                        INNER JOIN Vendor v
                                        ON detail.ma_nha_cung_cap = v.nha_cung_cap_id
                                        LEFT JOIN Parameters p
										ON detail.don_vi_tinh = P.ma_tham_so
	                                    INNER JOIN PRequestDetail prd
										ON detail.ma_phieu_header = prd.ma_to_trinh
										and detail.ma_san_pham = CASE WHEN ISNULL(prd.ma_san_pham_thay_the,'')='' THEN prd.ma_san_pham ELSE prd.ma_san_pham_thay_the END
                                        LEFT JOIN ProductPriceHeader pph
										ON detail.ma_chinh_sach_gia = pph.ma_chinh_sach_gia
                                        WHERE detail.ma_phieu_header={0}
                                        order by d.ten_chi_nhanh
	                                    ".Params(ma_phieu_header));
                int rowData = 11;
                int stt = 0;
                foreach (var item in ViewBag.listitem)
                {
                    int i = 1;
                    stt++;
                    rowData++;
                    Sheet.Cells[rowData, i++].Value = stt;
                    Sheet.Cells[rowData, i++].Value = item.ten_don_vi;
                    Sheet.Cells[rowData, i++].Value = item.ma_pyc_header;
                    Sheet.Cells[rowData, i++].Value = item.ten_san_pham;
                    Sheet.Cells[rowData, i++].Value = item.don_vi_tinh;
                    Sheet.Cells[rowData, i++].Value = item.so_luong;
                    Sheet.Cells[rowData, i++].Value = item.don_gia;
                    Sheet.Cells[rowData, i++].Value = item.don_gia_vat;
                    Sheet.Cells[rowData, i++].Value = item.thanh_tien;
                    Sheet.Cells[rowData, i++].Value = item.ten_nha_cung_cap;
                    Sheet.Cells[rowData, i++].Value = item.thong_tin_hop_dong;
                }
                MemoryStream output = new MemoryStream();
                excelPkg.SaveAs(output);
                output.Position = 0;
                return File(output.ToArray(), contentType, fileName);
            }
        }
        public ActionResult ExportPrint(int Id = 0, bool isView = true, string listPO = "")
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                //string[] separators = { "@@" };
                //var listID = listPO.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                //var strID = "";
                //foreach (var item in listID)
                //{
                //    strID += "'" + item + "',";
                //}
                //strID = strID.Substring(0, strID.Length - 1);
                ViewBag.listitem = dbConn.Select<StatementDetail>(@"
                                       SELECT DISTINCT detail.*, pr.ten_san_pham AS ten_san_pham, d.ten_chi_nhanh as ten_don_vi,
                                        detail.don_gia, detail.don_gia_vat, p.gia_tri as don_vi_tinh,
                                        detail.don_gia_vat * detail.so_luong as thanh_tien,
                                        v.ten_nha_cung_cap,
										--prd.ma_phieu as ma_phieu_pr,
                                        CASE WHEN pph.so_hop_dong is null THEN CONVERT(VARCHAR(10), pph.ngay_bao_gia, 101)
										ELSE pph.so_hop_dong + ' - ' + CONVERT(VARCHAR(10), pph.ngay_ky_hop_dong, 101) END AS thong_tin_hop_dong
                                        FROM StatementDetail detail
                                        LEFT JOIN dbo.Product pr
                                        ON detail.ma_san_pham=pr.ma_san_pham
                                        LEFT JOIN dbo.Branch d
                                        ON detail.ma_chi_nhanh = d.ma_chi_nhanh
                                        INNER JOIN Vendor v
                                        ON detail.ma_nha_cung_cap = v.nha_cung_cap_id
                                        LEFT JOIN Parameters p
										ON detail.don_vi_tinh = P.ma_tham_so
	                                    INNER JOIN PRequestDetail prd
										ON detail.ma_phieu_header = prd.ma_to_trinh
										and detail.ma_san_pham = CASE WHEN ISNULL(prd.ma_san_pham_thay_the,'')='' THEN prd.ma_san_pham ELSE prd.ma_san_pham_thay_the END
                                        LEFT JOIN ProductPriceHeader pph
										ON detail.ma_chinh_sach_gia = pph.ma_chinh_sach_gia
                                        WHERE detail.ma_phieu_header={0}
                                        order by d.ten_chi_nhanh
	                                    ".Params(listPO));
                ViewBag.ItemHeader = dbConn.FirstOrDefault<StatementHeader>(s => s.ma_phieu == listPO);
                var subtemplate = "_template_product_table_to_trinh";
                ViewBag.subWiewName = subtemplate;
                string viewName = "_template_export_to_trinh";
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

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateUpdateNew(StatementHeader data, List<StatementDetail> details)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    int id = 0;
                    if (accessDetail.them)
                    {
                        string ma_phieu = "";
                        var loai = "BTH";
                        //var ma_don_vi = currentUser.ma_don_vi;
                        var yyMMdd = DateTime.Now.ToString("yyMMdd");
                        var existLast = dbConn.SingleOrDefault<StatementHeader>("SELECT TOP 1 * FROM StatementHeader ORDER BY id DESC");
                        var nextNo = 0;
                        var yearNow = int.Parse(DateTime.Now.Year.ToString().Substring(2, 2));
                        if (existLast != null)
                        {
                            nextNo = int.Parse(existLast.ma_phieu.Substring(9, existLast.ma_phieu.Length - 9)) + 1;
                            var yearOld = int.Parse(existLast.ma_phieu.Substring(3, 2));
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
                        data.ten_phieu = data.ma_phieu;
                        data.ngay_tao_yeu_cau = DateTime.Now;
                        data.ngay_cap_thiet_bi = DateTime.Now;
                        data.ngay_tao = DateTime.Now;
                        data.nguoi_tao = currentUser.ma_nguoi_dung;
                        data.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                        data.nguoi_cap_nhat = "";
                        data.trang_thai = "MOI";
                        dbConn.Insert(data);
                        id = (int)dbConn.GetLastInsertId();


                        foreach (var item in details)
                        {
                            StatementDetail newdata = new StatementDetail();
                            newdata.ma_phieu_header = data.ma_phieu;
                            newdata.ma_san_pham = item.ma_san_pham;
                            newdata.so_luong = item.so_luong;
                            newdata.thong_so_ky_thuat = item.thong_so_ky_thuat;
                            newdata.muc_dich_su_dung = "";
                            newdata.nguoi_tao = currentUser.ma_nguoi_dung;
                            newdata.ma_pyc_header = item.ma_pyc_header;
                            newdata.ma_nha_cung_cap = item.ma_nha_cung_cap;
                            newdata.don_gia_vat = item.don_gia_vat;
                            newdata.don_gia = item.don_gia;
                            newdata.thue_vat = item.thue_vat;
                            newdata.don_vi_tinh = item.don_vi_tinh;
                            newdata.ma_chinh_sach_gia = item.ma_chinh_sach_gia;
                            newdata.ma_chi_nhanh = item.ma_chi_nhanh;
                            newdata.ma_don_vi = item.ma_don_vi;
                            newdata.noi_dung_xac_nhan_ton_kho = item.noi_dung_xac_nhan_ton_kho;
                            newdata.noi_dung_xac_nhan_cap_3 = item.noi_dung_xac_nhan_cap_3;
                            newdata.ngay_tao = DateTime.Now;
                            newdata.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            newdata.nguoi_cap_nhat = "";
                            newdata.trang_thai = "";
                            dbConn.Insert<StatementDetail>(newdata);
                            //PRequestDetail detail = dbConn.FirstOrDefault<PRequestDetail>(s => s.ma_phieu_header == item.ma_pyc_header && s.ma_san_pham == item.ma_san_pham && s.ma_nha_cung_cap == item.ma_nha_cung_cap);
                            PRequestDetail detail = dbConn.FirstOrDefault<PRequestDetail>(s => s.id == item.id);
                            detail.ma_to_trinh = ma_phieu;
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

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateNew(StatementHeader data, List<StatementDetail> details)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    if (accessDetail.sua)
                    {
                        var exist = dbConn.SingleOrDefault<StatementHeader>("id={0}", data.id);
                        exist.ngay_cap_thiet_bi = data.ngay_cap_thiet_bi;
                        exist.ten_phieu = data.ten_phieu;
                        exist.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                        exist.ngay_cap_nhat = DateTime.Now;
                        dbConn.Update(exist);
                        foreach (var item in details)
                        {
                            var detail = dbConn.FirstOrDefault<StatementDetail>(s => s.id == item.id);
                            detail.don_gia_vat = item.don_gia_vat;
                            detail.don_gia = item.don_gia;
                            detail.thue_vat = item.thue_vat;
                            detail.don_vi_tinh = item.don_vi_tinh;
                            detail.ma_chinh_sach_gia = item.ma_chinh_sach_gia;
                            detail.ma_nha_cung_cap = item.ma_nha_cung_cap;
                            detail.thong_so_ky_thuat = item.thong_so_ky_thuat;
                            detail.muc_dich_su_dung = item.muc_dich_su_dung;
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

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Adddetail(string ma_phieu_header, List<StatementDetail> details)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    if (accessDetail.sua)
                    {

                        foreach (var item in details)
                        {
                            StatementDetail newdata = new StatementDetail();
                            newdata.ma_phieu_header = ma_phieu_header;
                            newdata.ma_san_pham = item.ma_san_pham;
                            newdata.so_luong = item.so_luong;
                            newdata.thong_so_ky_thuat = item.thong_so_ky_thuat;
                            newdata.muc_dich_su_dung = "";
                            newdata.nguoi_tao = currentUser.ma_nguoi_dung;
                            newdata.ma_pyc_header = item.ma_pyc_header;
                            newdata.ma_nha_cung_cap = item.ma_nha_cung_cap;
                            newdata.don_gia_vat = item.don_gia_vat;
                            newdata.don_gia = item.don_gia;
                            newdata.thue_vat = item.thue_vat;
                            newdata.don_vi_tinh = item.don_vi_tinh;
                            newdata.ma_chinh_sach_gia = item.ma_chinh_sach_gia;
                            newdata.ma_don_vi = item.ma_don_vi;
                            newdata.ngay_tao = DateTime.Now;
                            newdata.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            newdata.nguoi_cap_nhat = "";
                            newdata.trang_thai = "";
                            newdata.noi_dung_xac_nhan_ton_kho = item.noi_dung_xac_nhan_ton_kho;
                            newdata.noi_dung_xac_nhan_cap_3 = item.noi_dung_xac_nhan_cap_3;
                            newdata.ma_chi_nhanh = item.ma_chi_nhanh;
                            dbConn.Insert<StatementDetail>(newdata);
                            //PRequestDetail detail = dbConn.FirstOrDefault<PRequestDetail>(s => s.ma_phieu_header == item.ma_pyc_header && s.ma_san_pham == item.ma_san_pham && s.ma_nha_cung_cap == item.ma_nha_cung_cap);
                            PRequestDetail detail = dbConn.FirstOrDefault<PRequestDetail>(s => s.id == item.id);
                            detail.ma_to_trinh = ma_phieu_header;
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


    }
}