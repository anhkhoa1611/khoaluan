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
using System.Globalization;
using OfficeOpenXml.Style;

namespace HPSTD.Controllers
{
    public class GroupFoodController : CustomController
    {
        // GET: GroupFood
        public ActionResult Index()
        {
            return View("GroupFood");
        }
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new List<GroupFood>();
                if (accessDetail.xem)
                {
                    if (request.Filters.Any())
                    {
                        var where = KendoApplyFilter.ApplyFilter(request.Filters[0]);
                        data = dbConn.Select<GroupFood>(where);
                    }
                    else
                    {
                        data = dbConn.Select<GroupFood>();
                    }
                }
                return Json(data.ToDataSourceResult(request));
            }
        }
        public ActionResult CreateUpdate(GroupFood data)
        {
            try
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    if (data.id > 0)
                    {
                        if (accessDetail.sua)
                        {
                            if (dbConn.SingleOrDefault<GroupFood>("ma_nhom_thuc_pham ={0} and id !={1} and trang_thai = 'true'", data.ma_nhom_thuc_pham, data.id) != null)
                            {
                                return Json(new { success = false, error = "Mã khu vực đã tồn tại. Vui lòng nhập mã khác" });
                            }
                            var exist = dbConn.SingleOrDefault<GroupFood>("id={0}", data.id);
                            exist.ma_nhom_thuc_pham = data.ma_nhom_thuc_pham;
                            exist.ten_nhom_thuc_pham = data.ten_nhom_thuc_pham;
                            exist.ghi_chu = !string.IsNullOrEmpty(data.ghi_chu) ? data.ghi_chu : "";
                            exist.trang_thai = data.trang_thai;
                            exist.ngay_cap_nhat = DateTime.Now;
                            exist.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            dbConn.Update(exist, s => s.id == exist.id);
                        }
                        else
                        {
                            return Json(new { success = true, error = "Bạn không có quyền cập nhật. Vui lòng liên hệ với ban quản trị để cập nhật quyền." });
                        }
                    }
                    else
                    {
                        if (accessDetail.them)
                        {
                            if (dbConn.SingleOrDefault<GroupFood>("ma_nhom_thuc_pham ={0} and trang_thai = 'true'", data.ma_nhom_thuc_pham) != null)
                            {
                                return Json(new { success = false, error = "Mạ nhóm thực phẩm đã tồn tại. Vui lòng nhập mã khác" });
                            }
                            data.nguoi_tao = currentUser.ma_nguoi_dung;
                            data.ngay_tao = DateTime.Now;
                            dbConn.Insert(data);
                        }
                        else
                        {
                            return Json(new { success = true, error = "Bạn không có quyền thêm. Vui lòng liên hệ với ban quản trị để cập nhật quyền." });
                        }
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });

            }
        }
        public ActionResult ExportTemplate()
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                FileInfo fileInfo = new FileInfo(Server.MapPath(@"~\ExportExcelFile\Template_GroupFood.xlsx"));
                var excelPkg = new ExcelPackage(fileInfo);
                string fileName = "Group_Import_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                MemoryStream output = new MemoryStream();
                excelPkg.SaveAs(output);
                output.Position = 0;
                return File(output.ToArray(), contentType, fileName);
            }
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
                FileInfo template = new FileInfo(Server.MapPath(@"~\ExportExcelFile\Template_GroupFood.xlsx"));
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
                        string ma_nhom_thuc_pham = oSheet.Cells[i, 1].Value != null ? oSheet.Cells[i, 1].Value.ToString().Trim() : "";
                        if (ma_nhom_thuc_pham == "")
                        {
                            continue;
                        }
                        string ten_nhom_thuc_pham = oSheet.Cells[i, 2].Value != null ? oSheet.Cells[i, 2].Value.ToString().Trim() : "";
                        string trang_thai = "true";
                        if (oSheet.Cells[i, 3].Value.ToString() == "Hoạt động")
                        {
                            trang_thai = "true";
                        }
                        else if (oSheet.Cells[i, 3].Value.ToString() == "Ngưng hoạt động")
                        {
                            trang_thai = "false";
                        }
                        string ghi_chu = oSheet.Cells[i, 4].Value != null ? oSheet.Cells[i, 7].Value.ToString().Trim() : "";

                        try
                        {
                            var itemeexit = dbConn.FirstOrDefault<GroupFood>(s => s.ma_nhom_thuc_pham == ma_nhom_thuc_pham);
                            if (itemeexit != null)
                            {
                                itemeexit.ten_nhom_thuc_pham = ten_nhom_thuc_pham;
                                itemeexit.trang_thai = trang_thai;
                                itemeexit.ghi_chu = ghi_chu;
                                itemeexit.ngay_cap_nhat = DateTime.Now;
                                itemeexit.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                                dbConn.Update<GroupFood>(itemeexit);
                            }
                            else
                            {
                                var item = new GroupFood();
                                item.ma_nhom_thuc_pham = ma_nhom_thuc_pham;
                                item.ten_nhom_thuc_pham = ten_nhom_thuc_pham;
                                item.trang_thai = trang_thai;
                                item.ghi_chu = ghi_chu;
                                item.ngay_tao = DateTime.Now;
                                item.nguoi_tao = currentUser.ma_nguoi_dung;
                                item.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                                item.nguoi_cap_nhat = "";
                                dbConn.Insert<GroupFood>(item);
                            }
                            total++;
                            rownumber++;
                        }
                        catch (Exception e)
                        {
                            eSheet.Cells[rownumber, 1].Value = ma_nhom_thuc_pham;
                            eSheet.Cells[rownumber, 2].Value = ten_nhom_thuc_pham;
                            eSheet.Cells[rownumber, 3].Value = trang_thai;
                            eSheet.Cells[rownumber, 4].Value = ghi_chu;
                            eSheet.Cells[rownumber, 5].Value = e.Message;
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
        public ActionResult ExportData()
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                FileInfo fileInfoTemplate = new FileInfo(Server.MapPath(@"~\ExportExcelFile\Template_GroupFood.xlsx"));
                var excelPkgTemplate = new ExcelPackage(fileInfoTemplate);
                string fileName = "GroupFood_Data_Export" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                var product = dbConn.Select<GroupFood>();
                ExcelWorksheet Sheet = excelPkgTemplate.Workbook.Worksheets["Data"];
                int rowData = 1;
                foreach (var item in product)
                {
                    rowData++;
                    Sheet.Cells[rowData, 1].Value = item.ma_nhom_thuc_pham;
                    Sheet.Cells[rowData, 2].Value = item.ten_nhom_thuc_pham;
                   
                    if (item.trang_thai == "true")
                    {
                        Sheet.Cells[rowData, 3].Value = "Hoạt động";
                    }
                    else
                    {
                        Sheet.Cells[rowData, 3].Value = "Không hoạt động";
                    }
                    Sheet.Cells[rowData, 4].Value = item.ghi_chu;
                }

                MemoryStream output = new MemoryStream();
                excelPkgTemplate.SaveAs(output);
                output.Position = 0;
                return File(output.ToArray(), contentType, fileName);
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
                        dbConn.Delete<Core.Entities.GroupFood>("id={0}", item);
                    }
                    return Json(new { success = true });
                }
            }
            else
            {
                return Json(new { success = false, error = "Bạn không có quyền xóa dữ liệu. Vui lòng liên hệ với ban quản trị để cập nhật quyền." });
            }
        }
    }
}