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

namespace HPSTD.Controllers
{
    [Authorize]
    public class ReportQualityServiceController : CustomController
    {
        // GET: PO
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    return View("ReportQualityService");
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
                    var data = dbConn.SqlList<ReportQualityService>("EXEC p_Get_ReportQualityService '1','200',@WhereCondition", new { WhereCondition = WhereCondition });
                    return Json(data.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
                return RedirectToAction("NoAccessRights", "Error");
            }
        }
        public ActionResult ExportData(string startdate, string enddate)
        {
            try
            {

                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    FileInfo fileInfo = new FileInfo(Server.MapPath(@"~\ExportExcelFile\Bao_Cao_Thuc_Hien_Cam_Ket_CLDV_SLA_Template.xlsx"));
                    var excelPkg = new ExcelPackage(fileInfo);
                    string fileName = "Bao_Cao_Thuc_Hien_Cam_Ket_CLDV _SLA" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                    string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    var WhereCondition = " AND ([ngay_tao_pr]>='" + startdate + "' And [ngay_tao_pr]<='" + enddate + "' ) ".Params(startdate, enddate);
                    var data = dbConn.SqlList<ReportQualityService>("EXEC p_Get_ReportQualityService '1','100000',@WhereCondition", new { WhereCondition = WhereCondition });
                    int rowData = 7;
                    ExcelWorksheet Sheet = excelPkg.Workbook.Worksheets["Data"];
                    int i = 0;
                    int stt = 0;
                    var daterange = "TỪ NGÀY " + DateTime.Parse(startdate).ToString("dd/MM/yyyy") + " ĐẾN NGÀY " + DateTime.Parse(enddate).ToString("dd/MM/yyyy");
                    Sheet.Cells[2, 7].Value = daterange;

                    foreach (var item in data)
                    {
                        //int i = 2;
                        rowData = rowData + 1;
                        i = 1;
                        stt++;
                        Sheet.Cells[rowData, i++].Value = stt;
                        Sheet.Cells[rowData, i++].Value = item.ma_phieu_pr;
                        Sheet.Cells[rowData, i++].Value = item.ngay_tao_pr;
                        Sheet.Cells[rowData, i++].Value = item.don_vi_yeu_cau;
                        Sheet.Cells[rowData, i++].Value = item.ma_san_pham;
                        Sheet.Cells[rowData, i++].Value = item.ten_san_pham;
                        Sheet.Cells[rowData, i++].Value = item.so_luong;
                        Sheet.Cells[rowData, i++].Value = item.khoi_cntt_nhdt_xac_nhan != new DateTime() ? item.khoi_cntt_nhdt_xac_nhan.ToString("dd/MM/yyyy") : "";
                        Sheet.Cells[rowData, i++].Value = item.phong_qldv_khnq_xac_nhan != new DateTime() ? item.phong_qldv_khnq_xac_nhan.ToString("dd/MM/yyyy") : "";
                        Sheet.Cells[rowData, i++].Value = item.phong_ptml_xdcb_xac_nhan != new DateTime() ? item.phong_ptml_xdcb_xac_nhan.ToString("dd/MM/yyyy") : "";
                        Sheet.Cells[rowData, i++].Value = item.phong_mkt_pr_xac_nhan != new DateTime() ? item.phong_mkt_pr_xac_nhan.ToString("dd/MM/yyyy") : ""; ;
                        Sheet.Cells[rowData, i++].Value = item.phong_ban_khac != new DateTime() ? item.phong_ban_khac.ToString("dd/MM/yyyy") : ""; ;
                        Sheet.Cells[rowData, i++].Value = item.bang_tong_hop_chi_mua_sam != new DateTime() ? item.bang_tong_hop_chi_mua_sam.ToString("dd/MM/yyyy") : "";
                        Sheet.Cells[rowData, i++].Value = item.po_don_dat_hang != new DateTime() ? item.po_don_dat_hang.ToString("dd/MM/yyyy") : "";
                        //Sheet.Cells[rowData, i++].Value = item.ngay_nhan_hang != null ? item.khoi_cntt_nhdt_xac_nhan.ToString("dd/MM/yyyy") : ""; 


                    }
                    MemoryStream output = new MemoryStream();
                    excelPkg.SaveAs(output);
                    output.Position = 0;
                    return File(output.ToArray(), contentType, fileName);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Cố lỗi xảy ra khi xuất" });
            }
        }
    }
}