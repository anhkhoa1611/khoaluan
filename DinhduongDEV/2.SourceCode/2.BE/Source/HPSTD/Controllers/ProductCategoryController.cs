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
    public class ProductCategoryController : CustomController
    {
        // GET: ProductCategory
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listAll = dbConn.Select<ProductCategory>("Select id ,isnull(ma_phan_cap,'') as ma_phan_cap , isnull(ten_phan_cap,'') as ten_phan_cap from ProductCategory");
                }
                return View("ProductCategory");
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
                var data = new DataSourceResult();
                data = KendoApplyFilter.KendoData<ProductCategory>(request);
                return Json(data);
            }
        }
        [HttpPost]
        public ActionResult CreateUpdate(ProductCategory data)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    if (data.id > 0)
                    {
                        if (accessDetail.sua)
                        {
                            var exist = dbConn.SingleOrDefault<ProductCategory>("id={0}", data.id);
                            if (data.ma_phan_cap != exist.ma_phan_cap)
                            {
                                var checkDup = dbConn.SingleOrDefault<ProductCategory>("ma_phan_cap={0}", data.ma_phan_cap);
                                if (checkDup != null)
                                {
                                    return Json(new { success = false, error = "Mã phân cấp đã tồn tại." });
                                }
                                else
                                {
                                    exist.ma_phan_cap = data.ma_phan_cap;
                                }
                            }
                            exist.ngay_cap_nhat = DateTime.Now;
                            exist.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            exist.trang_thai = data.trang_thai;
                            exist.ten_phan_cap = data.ten_phan_cap;
                            exist.cap = int.Parse(Request["cap"].Replace(",", ""));
                            exist.ma_phan_cap_cha = !string.IsNullOrEmpty(data.ma_phan_cap_cha) ? data.ma_phan_cap_cha : "";
                            dbConn.Update(exist, s => s.id == exist.id);
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
                            var exist = dbConn.SingleOrDefault<ProductCategory>("ma_phan_cap={0}", data.ma_phan_cap);
                            if (exist != null)
                            {
                                return Json(new { success = false, error = "Mã phân cấp đã tồn tại." });
                            }
                            data.ngay_tao = DateTime.Now;
                            data.nguoi_tao = currentUser.ma_nguoi_dung;
                            data.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            data.cap = int.Parse(Request["cap"].Replace(",", ""));
                            data.ma_phan_cap_cha = !string.IsNullOrEmpty(data.ma_phan_cap_cha) ? data.ma_phan_cap_cha : "";
                            data.nguoi_cap_nhat = "";
                            dbConn.Insert(data);
                        }
                        else
                        {
                            return Json(new { success = false, error = "Bạn không có quyền thêm dữ liệu" });
                        }
                    }
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = ex.Message });
                }
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
                        var productcat = dbConn.SingleOrDefault<Core.Entities.ProductCategory>("id={0}", item);
                        var checkProduct = dbConn.Select<Product>(s => s.ma_nhom_san_pham == productcat.ma_phan_cap);
                        if (checkProduct != null && checkProduct.Count > 0)
                        {
                            return Json(new { success = false, error = "Bạn không thể xóa nhóm hàng hóa dịch vụ này." });
                        }
                        else
                        {                          
                            dbConn.Update<Core.Entities.ProductCategory>(set: "ma_phan_cap_cha  =''", where: "ma_phan_cap_cha = {0}".Params(productcat.ma_phan_cap));
                            dbConn.Delete<Core.Entities.ProductCategory>("id={0}", item);
                        }
                    }
                    return Json(new { success = true });
                }
            }
            else
            {
                return Json(new { success = false, error = "Bạn không có quyền xóa dữ liệu. Vui lòng liên hệ với ban quản trị để cập nhật quyền." });
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
                FileInfo template = new FileInfo(Server.MapPath(@"~\ExportExcelFile\ProductCategory.xlsx"));
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
                        string ma_phan_cap = oSheet.Cells[i, 1].Value != null ? oSheet.Cells[i, 1].Value.ToString().Trim() : "";
                        string ten_phan_cap = oSheet.Cells[i, 2].Value != null ? oSheet.Cells[i, 2].Value.ToString().Trim() : "";
                        string ma_phan_cap_cha = oSheet.Cells[i, 3].Value != null ? oSheet.Cells[i, 3].Value.ToString().Split('-')[0].ToString().Trim() : "";
                        string cap = oSheet.Cells[i, 4].Value != null ? oSheet.Cells[i, 4].Value.ToString().Trim() : "";

                        var Status = "";
                        if (oSheet.Cells[i, 5].Value.ToString() == "Hoạt động")
                        {
                            Status = "true";
                        }
                        else if (oSheet.Cells[i, 5].Value.ToString() == "Ngưng hoạt động")
                        {
                            Status = "false";
                        }
                        try
                        {
                            var itemeexit = dbConn.FirstOrDefault<ProductCategory>(s => s.ma_phan_cap == ma_phan_cap);
                            if (itemeexit != null)
                            {
                                itemeexit.ten_phan_cap = ten_phan_cap;
                                itemeexit.ma_phan_cap = ma_phan_cap;
                                itemeexit.ma_phan_cap_cha = !string.IsNullOrEmpty(ma_phan_cap_cha) ? ma_phan_cap_cha : "";
                                itemeexit.cap = !string.IsNullOrEmpty(cap) ? int.Parse(cap): 0 ;
                                itemeexit.trang_thai = Status;                              
                                itemeexit.ngay_cap_nhat = DateTime.Now;                        
                                itemeexit.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                                dbConn.Update<ProductCategory>(itemeexit);
                            }
                            else
                            {
                                var item = new ProductCategory();
                                item.ten_phan_cap = ten_phan_cap;
                                item.ma_phan_cap = ma_phan_cap;
                                item.ma_phan_cap_cha = !string.IsNullOrEmpty(ma_phan_cap_cha) ? ma_phan_cap_cha : "";
                                item.cap = !string.IsNullOrEmpty(cap) ? int.Parse(cap) : 0;
                                item.trang_thai = Status;
                                item.ngay_tao = DateTime.Now;
                                item.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                                item.nguoi_tao = currentUser.ma_nguoi_dung;
                                item.nguoi_cap_nhat = "";
                                dbConn.Insert<ProductCategory>(item);                             
                            }
                            total++;
                            rownumber++;
                        }
                        catch (Exception e)
                        {
                            eSheet.Cells[rownumber, 1].Value = ma_phan_cap;
                            eSheet.Cells[rownumber, 2].Value = ten_phan_cap;
                            eSheet.Cells[rownumber, 3].Value = ma_phan_cap_cha;
                            eSheet.Cells[rownumber, 4].Value = cap;
                            eSheet.Cells[rownumber, 5].Value = Status;                         
                            eSheet.Cells[rownumber, 6].Value = e.Message;
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
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.ProductCategory> items)
        {
            if (accessDetail.them)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            var exist = dbConn.SingleOrDefault<ProductCategory>("ma_phan_cap={0}", item.ma_phan_cap);
                            if (exist != null)
                            {
                                ModelState.AddModelError("", "Mã nhóm hàng hóa dịch vụ này đã tồn tại.");
                                return Json(items.ToDataSourceResult(request, ModelState));
                            }
                            item.ngay_tao = DateTime.Now;
                            item.nguoi_tao = currentUser.ma_nguoi_dung;
                            item.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            item.cap = item.cap;
                            item.ma_phan_cap_cha = !string.IsNullOrEmpty(item.ma_phan_cap_cha) ? item.ma_phan_cap_cha : "";
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
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.ProductCategory> items)
        {
            if (accessDetail.sua)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            var exist = dbConn.SingleOrDefault<ProductCategory>("id={0}", item.id);
                            if (item.ma_phan_cap != exist.ma_phan_cap)
                            {
                                var checkDup = dbConn.SingleOrDefault<ProductCategory>("ma_phan_cap={0}", item.ma_phan_cap);
                                if (checkDup != null)
                                {
                                    ModelState.AddModelError("", "Mã nhóm hàng hóa dịch vụ này đã tồn tại.");
                                    return Json(items.ToDataSourceResult(request, ModelState));
                                }
                                else
                                {
                                    exist.ma_phan_cap = item.ma_phan_cap;
                                }
                            }
                            exist.ngay_cap_nhat = DateTime.Now;
                            exist.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            exist.trang_thai = item.trang_thai;
                            exist.ten_phan_cap = item.ten_phan_cap;
                            exist.cap = item.cap;
                            exist.ma_phan_cap_cha = !string.IsNullOrEmpty(item.ma_phan_cap_cha) ? item.ma_phan_cap_cha : "";
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
        
        public ActionResult ExportTeamplate()
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                FileInfo fileInfo = new FileInfo(Server.MapPath(@"~\ExportExcelFile\ProductCategory_Template.xlsx"));
                var excelPkg = new ExcelPackage(fileInfo);
                string fileName = "ProductCategory_Import_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                var data = dbConn.Select<ProductCategory>("Select id ,isnull(ma_phan_cap,'') as ma_phan_cap , isnull(ten_phan_cap,'') as ten_phan_cap from ProductCategory order by ma_phan_cap");

                int rowData = 0;
                ExcelWorksheet Sheet = excelPkg.Workbook.Worksheets["Nhom"];
                foreach (var item in data)
                {                  
                    rowData++;
                    Sheet.Cells[rowData,1].Value = item.ma_phan_cap + " - " + item.ten_phan_cap;
                }
                MemoryStream output = new MemoryStream();
                excelPkg.SaveAs(output);
                output.Position = 0;
                return File(output.ToArray(), contentType, fileName);
            }
        }

    }
}