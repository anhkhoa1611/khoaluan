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
using HPSTD.Core.Entities;
using System.IO;
using OfficeOpenXml;

namespace HPSTD.Controllers
{
    [Authorize]
    public class DepartmentHeirarchyController : CustomController
    {
        // GET: Screen
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listAll = dbConn.Select<DepartmentHeirarchy>("Select isnull(ma_phan_cap,'') as ma_phan_cap , isnull(ten_phan_cap,'') as ten_phan_cap From DepartmentHeirarchy");

                }
                return View("DepartmentHeirarchy");
            }
            return RedirectToAction("NoAccessRights", "Error");
        }
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new DataSourceResult();
                data = KendoApplyFilter.KendoData<DepartmentHeirarchy>(request);
                return Json(data);
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.DepartmentHeirarchy> items)
        {
            if (accessDetail.them)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            if (string.IsNullOrEmpty(item.ma_phan_cap))
                            {
                                ModelState.AddModelError("", "Vui lòng nhập mã phân cấp.");
                                return Json(items.ToDataSourceResult(request, ModelState));
                            }
                            if (string.IsNullOrEmpty(item.ten_phan_cap))
                            {
                                ModelState.AddModelError("", "Vui lòng nhập tên phân cấp.");
                                return Json(items.ToDataSourceResult(request, ModelState));
                            }
                           
                            item.ngay_tao = DateTime.Now;
                            item.nguoi_tao = User.Identity.Name;
                            dbConn.Insert(item);
                        }
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Bạn không có quyền thêm. Vui lòng liên hệ ban quản trị để cập nhật quyền.");
            }
            return Json(items.ToDataSourceResult(request, ModelState));
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<DepartmentHeirarchy> items)
        {
            if (accessDetail.sua )
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var row in items)
                        {
                          
                            var checkID = dbConn.SingleOrDefault<DepartmentHeirarchy>("id={0}", row.id);
                            if (checkID != null)
                            {
                                if (string.IsNullOrEmpty(row.ma_phan_cap))
                                {
                                    ModelState.AddModelError("", "Vui lòng nhập mã phân cấp.");
                                    return Json(items.ToDataSourceResult(request, ModelState));
                                }
                                checkID.ma_phan_cap = row.ma_phan_cap;
                                checkID.cap = !string.IsNullOrEmpty(row.cap.ToString()) ? row.cap : 0;
                                checkID.ten_phan_cap = !string.IsNullOrEmpty(row.ten_phan_cap) ? row.ten_phan_cap : "";
                                checkID.loai_phan_cap = !string.IsNullOrEmpty(row.loai_phan_cap) ? row.loai_phan_cap : "";
                                checkID.ten_loai_phan_cap = !string.IsNullOrEmpty(row.ten_loai_phan_cap) ? row.ten_loai_phan_cap : "";
                                checkID.ma_phan_cap_cha = !string.IsNullOrEmpty(row.ma_phan_cap_cha) ? row.ma_phan_cap_cha : "";
                                if (checkID.cap == 0)
                                    checkID.ma_phan_cap_cha = "";
                                checkID.trang_thai = !string.IsNullOrEmpty(row.trang_thai) ? row.trang_thai : "true";
                                checkID.thu_tu = !string.IsNullOrEmpty(row.thu_tu.ToString()) ? row.thu_tu : 1;
                                checkID.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                                checkID.ngay_cap_nhat = DateTime.Now;
                                dbConn.Update(checkID, s => s.id == checkID.id);
                            }                            
                        }
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Bạn không có quyền cập nhật. Vui lòng liên hệ ban quản trị để cập nhật quyền.");
            }

            return Json(items.ToDataSourceResult(request, ModelState));
        }

        public ActionResult getParent(int cap, string loai_phan_cap, string ma_phan_cap)
        {
            IDbConnection db = OrmliteConnection.openConn();
            try
            {
                using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
                {
                    var data = dbConn.Select<DepartmentHeirarchy>(@"Select isnull(ma_phan_cap,'') as ma_phan_cap , isnull(ten_phan_cap,'') as ten_phan_cap from DepartmentHeirarchy where cap={0} and ma_phan_cap != {2}".Params((cap - 1), loai_phan_cap, ma_phan_cap));
                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }

        public ActionResult ExportTemplate()
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                FileInfo fileInfo = new FileInfo(Server.MapPath(@"~\ExportExcelFile\Department_Template.xlsx"));
                var excelPkg = new ExcelPackage(fileInfo);
                string fileName = "Department_Import_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var donviquanly = dbConn.Select<DepartmentHeirarchy>("Select id ,isnull(ma_phan_cap,'') as ma_phan_cap , isnull(ten_phan_cap,'') as ten_phan_cap From [DepartmentHeirarchy]");
                int rowdv = 0;
                ExcelWorksheet SheetDV = excelPkg.Workbook.Worksheets["DONVIQUANLY"];
                foreach (var item in donviquanly)
                {
                    rowdv++;
                    SheetDV.Cells[rowdv, 1].Value = item.ma_phan_cap;
                }
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
                FileInfo template = new FileInfo(Server.MapPath(@"~\ExportExcelFile\Department_Template.xlsx"));
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
                        if (oSheet.Cells[i, 1].Value == null)
                        {
                            i = totalRows;
                        }
                        else
                        {
                            string ma_don_vi = oSheet.Cells[i, 1].Value != null ? oSheet.Cells[i, 1].Value.ToString().Trim() : "";
                            string ten_don_vi = oSheet.Cells[i, 2].Value != null ? oSheet.Cells[i, 2].Value.ToString().Trim() : "";
                            string don_vi_quan_ly = oSheet.Cells[i, 3].Value != null ? oSheet.Cells[i, 3].Value.ToString().Trim() : "";
                            string trang_thai = "true";
                            if (oSheet.Cells[i, 4].Value.ToString() == "Hoạt động")
                            {
                                trang_thai = "DANG_HOAT_DONG";
                            }
                            else if (oSheet.Cells[i, 4].Value.ToString() == "Ngưng hoạt động")
                            {
                                trang_thai = "KHONG_HOAT_DONG";
                            }
                            string aliasname = oSheet.Cells[i, 5].Value != null ? oSheet.Cells[i, 5].Value.ToString().Trim() : "";
                            string cap_bac = oSheet.Cells[i, 6].Value != null ? oSheet.Cells[i, 6].Value.ToString().Trim() : "";
                            string thu_tu = oSheet.Cells[i, 7].Value != null ? oSheet.Cells[i, 7].Value.ToString().Trim() : "";
                            int capbacparse = 0;
                            int thutupart = 0;

                            try
                            {
                                var itemeexit = dbConn.FirstOrDefault<DepartmentHeirarchy>(s => s.ma_phan_cap == ma_don_vi);
                                if (itemeexit != null)
                                {
                                    itemeexit.ten_phan_cap = ten_don_vi;
                                    itemeexit.cap = int.TryParse(cap_bac, out capbacparse) ? capbacparse : 0;
                                 
                                    itemeexit.loai_phan_cap = "";
                                    itemeexit.ma_phan_cap_cha = don_vi_quan_ly;
                                    itemeexit.trang_thai = trang_thai;
                                    itemeexit.thu_tu = int.TryParse(thu_tu, out thutupart) ? thutupart : 0;
                                    itemeexit.ngay_cap_nhat = DateTime.Now;
                                    itemeexit.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                                    dbConn.Update<DepartmentHeirarchy>(itemeexit);
                                }
                                else
                                {
                                    var item = new DepartmentHeirarchy();
                                    item.ma_phan_cap = ma_don_vi;
                                    item.ten_phan_cap = ten_don_vi;
                                    item.cap = int.TryParse(cap_bac, out capbacparse) ? capbacparse : 0;
                                
                                    item.loai_phan_cap = "";
                                    item.ma_phan_cap_cha = don_vi_quan_ly;
                                    item.trang_thai = trang_thai;
                                    item.ngay_tao = DateTime.Now;
                                    item.nguoi_tao = currentUser.ma_nguoi_dung;
                                    item.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                                    item.nguoi_cap_nhat = "";
                                    dbConn.Insert<DepartmentHeirarchy>(item);
                                    total++;
                                    rownumber++;
                                }
                                total++;
                                rownumber++;
                            }
                            catch (Exception e)
                            {
                                eSheet.Cells[rownumber, 1].Value = ma_don_vi;
                                eSheet.Cells[rownumber, 2].Value = ten_don_vi;
                                eSheet.Cells[rownumber, 3].Value = don_vi_quan_ly;
                                eSheet.Cells[rownumber, 4].Value = trang_thai;
                                eSheet.Cells[rownumber, 5].Value = aliasname;
                                eSheet.Cells[rownumber, 6].Value = cap_bac;
                                eSheet.Cells[rownumber, 7].Value = thu_tu;
                                eSheet.Cells[rownumber, 8].Value = e.Message;
                                rownumber++;
                                error++;
                                continue;
                            }
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
                        var checkProduct = dbConn.Select<DepartmentHeirarchy>(s => s.ma_phan_cap == item);
                        if (checkProduct != null && checkProduct.Count > 0)
                        {
                            return Json(new { success = false, error = "Bạn không thể xóa" });
                        }
                        else
                        {
                            dbConn.Delete<Core.Entities.DepartmentHeirarchy>("id={0}", item);
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



    }
}