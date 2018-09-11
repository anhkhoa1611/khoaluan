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
using OfficeOpenXml;
using System.IO;


namespace HPSTD.Controllers
{
    [Authorize]
    public class BranchController : CustomController
    {
        // GET: Branch
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listDV = dbConn.Select<Branch>("Select distinct ma_don_vi From Branch ");
                }
                return View("Branch");
            }
            return RedirectToAction("NoAccessRights", "Error");
        }

        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new List<Branch>();
                if (accessDetail.xem)
                {
                    if (request.Filters.Any())
                    {
                        var where = KendoApplyFilter.ApplyFilter(request.Filters[0]);
                        data = dbConn.Select<Branch>(where);
                    }
                    else
                    {
                        data = dbConn.Select<Branch>();
                    }
                }
                return Json(data.ToDataSourceResult(request));
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.Branch> items)
        {
            if (accessDetail.them)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            var exist = dbConn.SingleOrDefault<Branch>("ma_chi_nhanh={0}", item.ma_chi_nhanh);
                            if (exist != null)
                            {
                                ModelState.AddModelError("", "Mã chi nhánh phòng giao dịch này đã tồn tại.");
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

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.Branch> items)
        {
            if (accessDetail.sua)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            var exist = dbConn.SingleOrDefault<Branch>("id={0}", item.id);
                            if (item.ma_chi_nhanh != exist.ma_chi_nhanh)
                            {
                                var checkDup = dbConn.SingleOrDefault<Branch>("ma_chi_nhanh={0}", item.ma_chi_nhanh);
                                if (checkDup != null)
                                {
                                    ModelState.AddModelError("", "Mã chi nhánh phòng giao dịch này đã tồn tại.");
                                    return Json(items.ToDataSourceResult(request, ModelState));
                                }
                                else
                                {
                                    exist.ma_chi_nhanh = item.ma_chi_nhanh;
                                }
                            }
                            exist.ten_chi_nhanh = item.ten_chi_nhanh;
                            exist.dien_thoai_lien_he = item.dien_thoai_lien_he;
                            exist.dia_chi = item.dia_chi;
                            exist.email_lien_he = item.email_lien_he;
                            exist.fax = item.fax;
                            exist.ten_nguoi_nhan_hang = item.ten_nguoi_nhan_hang;
                            exist.dt_nguoi_nhan_hang = item.dt_nguoi_nhan_hang;
                            exist.dia_chi_nhan_hang = item.dia_chi_nhan_hang;
                            exist.ghi_chu = item.ghi_chu;
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
                        var existbranch = dbConn.SingleOrDefault<Branch>("id={0}", item);
                        var checkexist = dbConn.Select<PRequestHeader>(s => s.ma_don_vi == existbranch.ma_chi_nhanh);
                        if (checkexist != null && checkexist.Count > 0)
                        {
                            return Json(new { success = false, error = "Bạn không có thể xóa chi nhánh đang được sử dụng" });
                        }
                        else
                        {
                            dbConn.Delete<Core.Entities.UsersBranch>("ma_chi_nhanh={0}", existbranch.ma_chi_nhanh);
                            dbConn.Delete<Core.Entities.Branch>("id={0}", item);
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

        [HttpPost]
        public ActionResult Excel_Export(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);

            return File(fileContents, contentType, fileName);
        }
        public ActionResult Import()
        {
            var file = Request.Files["FileUpload"];
            try
            {
                if (file == null || file.ContentLength == 0) return Json(new { success = false, message = "File rỗng." });
                var fileExtension = System.IO.Path.GetExtension(file.FileName);
                if (fileExtension != ".xlsx" && fileExtension != ".xls") return Json(new { success = false, message = "File không không đúng định dạng excel *.xlsx,*.xls" });
                string datetime = DateTime.Now.ToString("yyyyMMddHHmmss");
                string fileLocation = string.Format("{0}/{1}", Server.MapPath("~/ExcelImport"), "[" + currentUser.ma_nguoi_dung + "-" + datetime + file.FileName);
                string errorFileLocation = string.Format("{0}/{1}", Server.MapPath("~/ExcelImport"), "[" + currentUser.ma_nguoi_dung + "-" + datetime + "-Error]" + file.FileName);
                string linkerror = "[" + currentUser.ma_nguoi_dung + "-" + datetime + "-Error]" + file.FileName;
                if (!System.IO.Directory.Exists(Server.MapPath("~/ExcelImport")))
                {
                    System.IO.Directory.CreateDirectory(fileLocation);
                }
                if (System.IO.File.Exists(fileLocation)) System.IO.File.Delete(fileLocation);

                file.SaveAs(fileLocation);

                var rownumber = 2;
                var total = 0;
                var error = 0;
                FileInfo fileInfo = new FileInfo(fileLocation);
                var excelPkg = new ExcelPackage(fileInfo);
                FileInfo template = new FileInfo(Server.MapPath(@"~\ExportExcelFile\Branch.xlsx"));
                template.CopyTo(errorFileLocation);
                FileInfo _fileInfo = new FileInfo(errorFileLocation);
                var _excelPkg = new ExcelPackage(_fileInfo);
                ExcelWorksheet oSheet = excelPkg.Workbook.Worksheets["Data"];
                ExcelWorksheet eSheet = _excelPkg.Workbook.Worksheets["Data"];
                int totalRows = oSheet.Dimension.End.Row;
                using (var dbConn = OrmliteConnection.openConn())
                {
                    for (int i = 2; i <= totalRows; i++)
                    {
                        string ma_chi_nhanh = oSheet.Cells[i, 1].Value != null ? oSheet.Cells[i, 1].Value.ToString().Trim() : "";
                        string ten_chi_nhanh = oSheet.Cells[i, 2].Value != null ? oSheet.Cells[i, 2].Value.ToString().Trim() : "";
                        string dia_chi = oSheet.Cells[i, 3].Value != null ? oSheet.Cells[i, 3].Value.ToString().Trim() : "";
                        string dien_thoai = oSheet.Cells[i, 4].Value != null ? oSheet.Cells[i, 4].Value.ToString().Trim() : "";
                        string fax = oSheet.Cells[i, 5].Value != null ? oSheet.Cells[i, 5].Value.ToString().Trim() : "";
                        string email = oSheet.Cells[i, 6].Value != null ? oSheet.Cells[i, 6].Value.ToString().Trim() : "";
                        string ten_nguoi_nhan_hang = oSheet.Cells[i, 7].Value != null ? oSheet.Cells[i, 7].Value.ToString().Trim() : "";
                        string dt_nguoi_nhan_hang = oSheet.Cells[i, 8].Value != null ? oSheet.Cells[i, 8].Value.ToString().Trim() : "";
                        string dia_chi_nhan_hang = oSheet.Cells[i, 9].Value != null ? oSheet.Cells[i, 9].Value.ToString().Trim() : "";
                        string trang_thai = "true";
                        if (oSheet.Cells[i, 10].Value.ToString() == "Hoạt động")
                        {
                            trang_thai = "true";
                        }
                        else if (oSheet.Cells[i, 10].Value.ToString() == "Ngưng hoạt động")
                        {
                            trang_thai = "false";
                        }
                        string ghi_chu = oSheet.Cells[i, 11].Value != null ? oSheet.Cells[i, 11].Value.ToString().Trim() : "";

                        try
                        {
                            var item = new Branch();
                            item.ma_chi_nhanh = ma_chi_nhanh;
                            item.ten_chi_nhanh = ten_chi_nhanh;
                            item.dia_chi = dia_chi;
                            item.dien_thoai_lien_he = dien_thoai;
                            item.fax = fax;
                            item.email_lien_he = email;
                            item.ten_nguoi_nhan_hang = ten_nguoi_nhan_hang;
                            item.dt_nguoi_nhan_hang = dt_nguoi_nhan_hang;
                            item.dia_chi_nhan_hang = dia_chi_nhan_hang;
                            item.trang_thai = trang_thai;
                            item.ghi_chu = ghi_chu;
                            item.ngay_tao = DateTime.Now;
                            item.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            item.nguoi_tao = currentUser.ma_nguoi_dung;
                            item.nguoi_cap_nhat = "";
                            dbConn.Insert<Branch>(item);

                            total++;
                            rownumber++;
                        }
                        catch (Exception e)
                        {
                            eSheet.Cells[rownumber, 1].Value = ma_chi_nhanh;
                            eSheet.Cells[rownumber, 2].Value = ten_chi_nhanh;
                            eSheet.Cells[rownumber, 3].Value = dia_chi;
                            eSheet.Cells[rownumber, 4].Value = dien_thoai;
                            eSheet.Cells[rownumber, 5].Value = fax;
                            eSheet.Cells[rownumber, 6].Value = email;
                            eSheet.Cells[rownumber, 7].Value = ten_nguoi_nhan_hang;
                            eSheet.Cells[rownumber, 8].Value = dt_nguoi_nhan_hang;
                            eSheet.Cells[rownumber, 9].Value = dia_chi_nhan_hang;
                            eSheet.Cells[rownumber, 10].Value = trang_thai;
                            eSheet.Cells[rownumber, 11].Value = ghi_chu;
                            eSheet.Cells[rownumber, 12].Value = e.Message;
                            rownumber++;
                            error++;
                            continue;
                        }
                    }
                }
                _excelPkg.Save();
                return Json(new { success = true, total = total, totalError = error, link = linkerror });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public ActionResult ExportTeamplate()
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                FileInfo fileInfo = new FileInfo(Server.MapPath(@"~\ExportExcelFile\Branch_Template.xlsx"));
                var excelPkg = new ExcelPackage(fileInfo);
                string fileName = "Branch_Import_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                MemoryStream output = new MemoryStream();
                excelPkg.SaveAs(output);
                output.Position = 0;
                return File(output.ToArray(), contentType, fileName);
            }
        }
    }

}