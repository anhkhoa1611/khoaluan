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
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace HPSTD.Controllers
{
    [Authorize]
    public class ReportStatisticalController : CustomController
    {
        // GET: PO
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listProduct = dbConn.Select<Product>("select id,ma_san_pham,ten_san_pham from Product where ma_nhom_san_pham in (select ma_phan_cap from UserProductCategory where ma_nguoi_dung ={0})", currentUser.ma_nguoi_dung);
                    ViewBag.listVender = dbConn.Select<VendorModel>(@"SELECT nha_cung_cap_id,ten_nha_cung_Cap FROM Vendor WHERE trang_thai='DANG_HOAT_DONG' ");
                    ViewBag.listDonVi = dbConn.Select<Branch>("Select * From Branch Where ma_chi_nhanh in (Select ma_chi_nhanh From UsersBranch Where ma_nguoi_dung = '" + currentUser.ma_nguoi_dung + "')");
                    ViewBag.listTrangThai = dbConn.Select<Parameters>("loai_tham_so='TRANGTHAI'");
                    return View("ReportStatistical");
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

                    var WhereCondition = "";
                    if (request.Filters != null)
                    {
                        if (request.Filters.Count > 0)
                        {
                            WhereCondition = " AND " + KendoApplyFilter.ApplyFilter(request.Filters[0], "");
                        }
                    }
                    var data = dbConn.SqlList<ReportStatistical>("EXEC p_Get_ReportStatistical '1','200',@WhereCondition", new { WhereCondition = WhereCondition });
                    return Json(data.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
                return RedirectToAction("NoAccessRights", "Error");
            }
        }
        public ActionResult ExportData(string startdate, string enddate)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                FileInfo fileInfo = new FileInfo(Server.MapPath(@"~\ExportExcelFile\Bao_Cao_Thong_Ke_CPMS_Template.xlsx"));
                var excelPkg = new ExcelPackage(fileInfo);
                string fileName = "Bao_Cao_Thong_Ke_CPMS_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var WhereCondition = " AND ([ngay_tao_po]>='" + startdate + "' And [ngay_tao_po]<='" + enddate + "' ) ".Params(startdate, enddate);
                var data = dbConn.SqlList<ReportStatistical>("EXEC p_Get_ReportStatistical '1','100000',@WhereCondition", new { WhereCondition = WhereCondition });
                int rowData = 7;
                ExcelWorksheet Sheet = excelPkg.Workbook.Worksheets["Data"];
                Sheet.Cells[8, 1, 10000, 9].Style.Font.SetFromFont(new System.Drawing.Font("Times New Roman", 12));
                int i = 0;
                int stt = 0;
                var daterange = "TỪ NGÀY " + DateTime.Parse(startdate).ToString("dd/MM/yyyy") + " ĐẾN NGÀY " + DateTime.Parse(enddate).ToString("dd/MM/yyyy");
                Sheet.Cells[2, 6].Value = daterange;
                double tong = 0;
                foreach (var item in data)
                {
                    //int i = 2;
                    rowData = rowData + 1;
                    i = 1;
                    stt++;
                    Sheet.Cells[rowData, i++].Value = stt;
                    Sheet.Cells[rowData, i++].Value = item.so_po;
                    Sheet.Cells[rowData, i++].Value = item.ngay_tao_po;
                    Sheet.Cells[rowData, i++].Value = item.don_vi_yeu_cau;
                    Sheet.Cells[rowData, i++].Value = item.ma_san_pham;
                    Sheet.Cells[rowData, i++].Value = item.ten_san_pham;
                    Sheet.Cells[rowData, i++].Value = item.so_luong;
                    Sheet.Cells[rowData, i++].Value = item.chi_phi_thuc_te_vat;
                    Sheet.Cells[rowData, i++].Value = item.ghi_chu;
                    tong = tong + item.chi_phi_thuc_te_vat;
                }
                rowData = rowData + 1;
                Sheet.Cells[rowData, 7].Value = "Tổng:";
                Sheet.Cells[rowData, 8].Value = tong;
                Sheet.Cells[rowData, 1, rowData, 9].Style.Font.Bold = true;
                Sheet.Cells[rowData, 1, rowData, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                Sheet.Cells[rowData, 1, rowData, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Green);
                MemoryStream output = new MemoryStream();
                excelPkg.SaveAs(output);
                output.Position = 0;
                return File(output.ToArray(), contentType, fileName);
            }
        }
    }
}