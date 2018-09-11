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
using OfficeOpenXml.Style;

namespace HPSTD.Controllers
{
    [Authorize]
    public class PlanController : CustomController
    {
        // GET: ProductCategory
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listDonVi = dbConn.Select<DepartmentHeirarchy>();
                    ViewBag.listProduct = dbConn.Select<Product>();
                    ViewBag.listDonviTinh = dbConn.Select<Parameters>(p => p.loai_tham_so == "DONVITINH");
                    ViewBag.listDonviPhuTrach = dbConn.Select<Branch>();
                    //var userInBranch = dbConn.Select<Branch>(p => p.ma_chi_nhanh == currentUser.ma_chi_nhanh);
                    //if (userInBranch.Count != 0)
                    //{
                    //    ViewBag.listDonviPhuTrach = userInBranch;
                    //}


                }
                return View("Plan");
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
                data = KendoApplyFilter.KendoData<PlanHeader>(request);
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


        public ActionResult ReadDetail([DataSourceRequest]DataSourceRequest request, int id)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = dbConn.Select<PlanDetail>(s => s.nam_ke_hoach == id);
                return Json(data.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
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
                        var checkProduct = dbConn.Select<PlanHeader>(s => s.id == int.Parse(item) && s.trang_thai == "DA_DUYET");
                        if (checkProduct != null && checkProduct.Count > 0)
                        {
                            return Json(new { success = false, error = "Bạn không thể xóa kế hoạch đã duyệt." });
                        }
                        else
                        {
                            dbConn.Delete<Core.Entities.PlanHeader>("id={0}", item);
                            dbConn.Delete<Core.Entities.PlanDetail>("nam_ke_hoach={0}", item);
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
        public ActionResult Approved(string data, string ghi_chu)
        {
            if (accessDetail.xoa)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    string[] separators = { "@@" };
                    var listItem = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in listItem)
                    {
                        var checkProduct = dbConn.Select<PlanHeader>(s => s.id == int.Parse(item) && s.trang_thai == "DA_DUYET");
                        if (checkProduct != null && checkProduct.Count > 0)
                        {
                            return Json(new { success = false, error = "Bạn không thể duyệt kế hoạch đã duyệt." });
                        }
                        else
                        {
                            var exits = dbConn.FirstOrDefault<PlanHeader>(s => s.id == int.Parse(item));
                            exits.ghi_chu = ghi_chu;
                            exits.trang_thai = "DA_DUYET";
                            exits.nguoi_duyet = currentUser.ma_nguoi_dung;
                            exits.ngay_duyet = DateTime.Now;
                            dbConn.Update(exits, s => s.id == exits.id);

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

        public ActionResult Import(PlanHeader items)
        {
            var file = Request.Files["FileUpload"];
            try
            {
                if (file == null || file.ContentLength == 0)
                {
                    using (var dbConn = OrmliteConnection.openConn())
                    {
                        var id_head = 0;
                        var nam_ke_hoach = 0;
                        var check_head = dbConn.FirstOrDefault<PlanHeader>(s => s.don_vi_phu_trach == items.don_vi_phu_trach && s.nam_ke_hoach == items.nam_ke_hoach);
                        if (check_head == null)
                        {
                            items.ngay_tao = DateTime.Now;
                            items.nguoi_tao = currentUser.ma_nguoi_dung;
                            items.nguoi_cap_nhat = "";
                            items.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            items.nguoi_duyet = "";
                            items.ngay_duyet = DateTime.Parse("1900-01-01");
                            items.trang_thai = "MOI";
                            dbConn.Insert(items);
                            nam_ke_hoach = items.nam_ke_hoach;
                            id_head = (int)dbConn.GetLastInsertId();
                        }
                        else
                        {
                            return Json(new { success = false, message = "Kế hoạch năm đã được tạo. Vui  lòng cập nhật kế hoạch năm" });
                        }
                        return Json(new { success = true });
                    }
                }
                else
                {
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
                    FileInfo template = new FileInfo(Server.MapPath(@"~\ExportExcelFile\Plan.xlsx"));
                    template.CopyTo(errorFileLocation);
                    FileInfo _fileInfo = new FileInfo(errorFileLocation);
                    var _excelPkg = new ExcelPackage(_fileInfo);
                    ExcelWorksheet oSheet = excelPkg.Workbook.Worksheets["Data"];
                    ExcelWorksheet eSheet = _excelPkg.Workbook.Worksheets["Data"];
                    int totalRows = oSheet.Dimension.End.Row;
                    using (var dbConn = OrmliteConnection.openConn())
                    {
                        //tao hearder
                        var id_head = 0;
                        var nam_ke_hoach = 0;
                        var check_head = dbConn.FirstOrDefault<PlanHeader>(s => s.don_vi_phu_trach == items.don_vi_phu_trach && s.nam_ke_hoach == items.nam_ke_hoach);
                        if (check_head == null)
                        {
                            items.ngay_tao = DateTime.Now;
                            items.nguoi_tao = currentUser.ma_nguoi_dung;
                            items.nguoi_cap_nhat = "";
                            items.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            items.nguoi_duyet = "";
                            items.ngay_duyet = DateTime.Parse("1900-01-01");
                            items.trang_thai = "MOI";
                            dbConn.Insert(items);
                            nam_ke_hoach = items.nam_ke_hoach;
                            id_head = (int)dbConn.GetLastInsertId();
                        }
                        else
                        {
                            return Json(new { success = false, message = "Kế hoạch năm đã được tạo. Vui  lòng cập nhật kế hoạch năm" });
                        }
                        for (int i = 7; i <= totalRows; i++)
                        {
                            string idNhomSP = oSheet.Cells[i, 1].Value != null ? oSheet.Cells[i, 1].Value.ToString() : "";
                            if (idNhomSP != "")
                            {
                                continue;
                            }

                            string ma_san_pham = oSheet.Cells[i, 2].Value != null ? oSheet.Cells[i, 2].Value.ToString() : "";
                            string don_vi_phu_trach = oSheet.Cells[i, 3].Value != null ? oSheet.Cells[i, 3].Value.ToString().Split('/')[0] : "";
                            string don_vi_tinh = oSheet.Cells[i, 4].Value != null ? oSheet.Cells[i, 4].Value.ToString().Split('/')[0] : "";
                            string so_luong_du_kien = oSheet.Cells[i, 5].Value != null ? oSheet.Cells[i, 5].Value.ToString() : "";
                            string don_gia_du_kien = oSheet.Cells[i, 6].Value != null ? oSheet.Cells[i, 6].Value.ToString() : "";
                            string tong_tien_ke_hoach_nam = oSheet.Cells[i, 7].Value != null ? oSheet.Cells[i, 7].Value.ToString() : "";
                            string ke_hoach_nam_truoc = oSheet.Cells[i, 8].Value != null ? oSheet.Cells[i, 8].Value.ToString() : "";
                            string thuc_hien_nam_truoc = oSheet.Cells[i, 9].Value != null ? oSheet.Cells[i, 9].Value.ToString() : "";
                            string chech_lech = oSheet.Cells[i, 10].Value != null ? oSheet.Cells[i, 10].Value.ToString() : "";
                            string ghi_chu = oSheet.Cells[i, 11].Value != null ? oSheet.Cells[i, 11].Value.ToString() : "";
                            try
                            {
                                if (!string.IsNullOrEmpty(ma_san_pham))
                                {
                                    ma_san_pham = ma_san_pham.Split('-')[0];
                                    if (so_luong_du_kien == "")
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                                var itemeexit = dbConn.FirstOrDefault<PlanDetail>(s => s.ma_san_pham == int.Parse(ma_san_pham) && s.nam_ke_hoach == id_head && s.don_vi_phu_trach == items.don_vi_phu_trach);
                                if (itemeexit != null)
                                {
                                    itemeexit.ma_san_pham = int.Parse(ma_san_pham);
                                    //itemeexit.ma_don_vi = int.Parse(don_vi_phu_trach);
                                    itemeexit.don_vi_phu_trach = don_vi_phu_trach;
                                    itemeexit.ma_don_vi_tinh = don_vi_tinh;
                                    itemeexit.so_luong_du_kien = int.Parse(so_luong_du_kien);
                                    itemeexit.don_gia_du_kien_vat = double.Parse(don_gia_du_kien);
                                    itemeexit.total_tien_du_kien = double.Parse(tong_tien_ke_hoach_nam);
                                    itemeexit.ke_hoach_nam_truoc = double.Parse(ke_hoach_nam_truoc);
                                    itemeexit.thuc_hien_nam_truoc = double.Parse(thuc_hien_nam_truoc);
                                    itemeexit.chech_lech = double.Parse(chech_lech);
                                    itemeexit.ghi_chu = ghi_chu;
                                    itemeexit.ngay_tao = DateTime.Now;
                                    itemeexit.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                                    itemeexit.nguoi_tao = currentUser.ma_nguoi_dung;
                                    itemeexit.nguoi_cap_nhat = "";
                                    dbConn.Update<PlanDetail>(itemeexit);
                                }
                                else
                                {
                                    var item = new PlanDetail();
                                    item.ma_san_pham = int.Parse(ma_san_pham);
                                    //item.ma_don_vi = items.ma_don_vi;
                                    item.don_vi_phu_trach = don_vi_phu_trach;
                                    item.ma_don_vi_tinh = don_vi_tinh;
                                    item.nam_ke_hoach = id_head;
                                    item.so_luong_du_kien = int.Parse(so_luong_du_kien);
                                    item.don_gia_du_kien_vat = double.Parse(don_gia_du_kien);
                                    item.total_tien_du_kien = double.Parse(tong_tien_ke_hoach_nam);
                                    item.ke_hoach_nam_truoc = double.Parse(ke_hoach_nam_truoc);
                                    item.thuc_hien_nam_truoc = double.Parse(thuc_hien_nam_truoc);
                                    item.chech_lech = double.Parse(chech_lech);
                                    item.trang_thai = "MOi";
                                    item.ghi_chu = ghi_chu;
                                    item.nguoi_tao = currentUser.ma_nguoi_dung;
                                    item.nguoi_cap_nhat = "";
                                    dbConn.Insert<PlanDetail>(item);
                                    total++;
                                    rownumber++;
                                }
                                total++;
                                rownumber++;
                            }
                            catch (Exception e)
                            {
                                eSheet.Cells[rownumber, 2].Value = ma_san_pham;
                                eSheet.Cells[rownumber, 3].Value = don_vi_phu_trach;
                                eSheet.Cells[rownumber, 4].Value = don_vi_tinh;
                                eSheet.Cells[rownumber, 5].Value = so_luong_du_kien;
                                eSheet.Cells[rownumber, 6].Value = don_gia_du_kien;
                                eSheet.Cells[rownumber, 7].Value = tong_tien_ke_hoach_nam;
                                eSheet.Cells[rownumber, 8].Value = ke_hoach_nam_truoc;
                                eSheet.Cells[rownumber, 9].Value = thuc_hien_nam_truoc;
                                eSheet.Cells[rownumber, 10].Value = chech_lech;
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
                FileInfo fileInfo = new FileInfo(Server.MapPath(@"~\ExportExcelFile\KHMS_Template.xlsx"));
                var excelPkg = new ExcelPackage(fileInfo);
                string fileName = "KHMS_NAM" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                var data = dbConn.Select<ProductCategory>();
                var product = dbConn.Select<Product>();
                var don_vi_tinh = dbConn.Select<Parameters>("loai_tham_so='DONVITINH'");
                var don_vi_phu_trachs = dbConn.Select<Branch>("trang_thai='true'");
                var userInBranchs = dbConn.Select<UsersBranch>(p => p.ma_nguoi_dung == currentUser.ma_nguoi_dung);
                int rowData = 6;
                ExcelWorksheet Sheet = excelPkg.Workbook.Worksheets["Data"];
                foreach (var item in data)
                {
                    int i = 2;
                    rowData = rowData + 1;
                    Sheet.Cells[rowData, 1].Value = item.ma_phan_cap;
                    Sheet.Cells[rowData, 2].Value = item.ten_phan_cap;
                    Sheet.Cells[rowData, 1, rowData, 11].Style.Font.Bold = true;
                    Sheet.Cells[rowData, 1, rowData, 11].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                    //Sheet.Cells[rowData, 1, rowData + 1, 11].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    var productitems = product.Where(p => p.ma_nhom_san_pham == item.ma_phan_cap);
                    foreach (var item2 in productitems)
                    {
                        rowData = rowData + 1;
                        Sheet.Cells[rowData, 2].Value = item2.id + " - " + item2.ma_san_pham + " - " + item2.ten_san_pham;
                        var dvt = don_vi_tinh.Where(s => s.ma_tham_so == item2.ma_don_vi_tinh).FirstOrDefault();
                        if (dvt != null)
                        {
                            Sheet.Cells[rowData, 4].Value = dvt.ma_tham_so + "/" + dvt.gia_tri;
                        }
                        var chi_nhanh_dau_tien = userInBranchs.FirstOrDefault();
                        if (chi_nhanh_dau_tien != null)
                        {
                            Sheet.Cells[rowData, 3].Value = chi_nhanh_dau_tien.ma_chi_nhanh + "/" + chi_nhanh_dau_tien.ten_chi_nhanh;
                        }
                    }

                }
                Sheet = excelPkg.Workbook.Worksheets["DonViTinh"];

                rowData = 2;
                foreach (var item3 in don_vi_tinh)
                {
                    int j = 1;
                    Sheet.Cells[rowData++, j].Value = item3.id + "/" + item3.gia_tri;
                }
                Sheet = excelPkg.Workbook.Worksheets["DonViPhuTrach"];
                rowData = 2;
                if (userInBranchs.Count == 0)
                {
                    foreach (var item in don_vi_phu_trachs)
                    {
                        int j = 1;
                        Sheet.Cells[rowData++, j].Value = item.ma_chi_nhanh + "/" + item.ten_chi_nhanh;
                    }
                }
                else
                {
                    foreach (var item in userInBranchs)
                    {
                        int j = 1;
                        var chi_nhanh = don_vi_phu_trachs.Where(p => p.ma_chi_nhanh == item.ma_chi_nhanh).FirstOrDefault();
                        if (chi_nhanh != null)
                            Sheet.Cells[rowData++, j].Value = chi_nhanh.ma_chi_nhanh + "/" + chi_nhanh.ten_chi_nhanh;
                    }
                }
                MemoryStream output = new MemoryStream();
                excelPkg.SaveAs(output);
                output.Position = 0;
                return File(output.ToArray(), contentType, fileName);
            }
        }
        public ActionResult ExportData(string listids)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                FileInfo fileInfoTemplate = new FileInfo(Server.MapPath(@"~\ExportExcelFile\KHMS_Template.xlsx"));
                var excelPkgTemplate = new ExcelPackage(fileInfoTemplate);
                var excelPkg = new ExcelPackage();
                string fileName = "KHMS_NAM_Data_Export" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string[] separators = { "@@" };
                var listItem = listids.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                foreach (var iditem in listItem)
                {

                    var header = dbConn.Select<PlanHeader>(s => s.id == int.Parse(iditem)).FirstOrDefault();

                    var data = dbConn.Select<ProductCategory>(@"
                                          SELECT c.id,c.ma_phan_cap,c.ten_phan_cap 
                                          FROM [dbo].[PlanDetail] a  INNER JOIN [dbo].[Product] b
                                          ON a.ma_san_pham=b.id
                                          LEFT JOIN [dbo].[ProductCategory] c
                                          ON b.ma_nhom_san_pham=c.id  where a.nam_ke_hoach={0} 
                                          GROUP BY c.id,c.ma_phan_cap,c.ten_phan_cap ".Params(iditem));
                    var product = dbConn.Select<PlanDetail>(@"  SELECT a.*, b.ten_san_pham, b.ma_nhom_san_pham , b.ma_don_vi_tinh
                                                      FROM [dbo].[PlanDetail] a  INNER JOIN [dbo].[Product] b
                                                      ON a.ma_san_pham=b.id where a.nam_ke_hoach={0}
                                                      ".Params(iditem));
                    var don_vi_tinh = dbConn.Select<Parameters>("loai_tham_so='DONVITINH'");
                    var don_vi_phu_trachs = dbConn.Select<Branch>("trang_thai='true'");

                    //raw header
                    ExcelWorksheet Sheet = excelPkg.Workbook.Worksheets["Data"];
                    var sheetName = header.id + "_" + header.ten_ke_hoach + "_" + header.nam_ke_hoach;
                    if (Sheet == null)
                    {
                        var templateWorksheet = excelPkgTemplate.Workbook.Worksheets["Data"];
                        excelPkg.Workbook.Worksheets.Add(sheetName, templateWorksheet);
                        Sheet = excelPkg.Workbook.Worksheets[sheetName];
                    }
                    else
                    {
                        Sheet.Name = sheetName;
                    }

                    int rowData = 6;
                    foreach (var item in data)
                    {
                        rowData++;
                        Sheet.Cells[rowData, 1].Value = item.ma_phan_cap;
                        Sheet.Cells[rowData, 2].Value = item.ten_phan_cap;
                        Sheet.Cells[rowData, 1, rowData, 11].Style.Font.Bold = true;
                        Sheet.Cells[rowData, 1, rowData, 11].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                        Sheet.Cells[rowData, 1, rowData, 11].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        Sheet.Cells[rowData, 1, rowData, 11].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        var productitems = product.Where(p => p.ma_nhom_san_pham == item.id);
                        foreach (var item2 in productitems)
                        {
                            if (item2.ma_nhom_san_pham == item.id)
                            {
                                int stt = 0;
                                stt++;
                                rowData++;
                                Sheet.Cells[rowData, stt].Value = stt;
                                Sheet.Cells[rowData, 2].Value = item2.ten_san_pham;
                                var dvpt = don_vi_phu_trachs.Where(s => s.ma_chi_nhanh == item2.ma_don_vi_tinh).FirstOrDefault();
                                if (dvpt != null)
                                {
                                    Sheet.Cells[rowData, 4].Value = dvpt.ma_chi_nhanh + "/" + dvpt.ten_chi_nhanh;
                                }
                                Sheet.Cells[rowData, 3].Value = dbConn.FirstOrDefault<Branch>(s => s.ma_chi_nhanh == header.don_vi_phu_trach).ten_chi_nhanh;
                                var dvt = don_vi_tinh.Where(s => s.ma_tham_so == item2.ma_don_vi_tinh).FirstOrDefault();
                                if (dvt != null)
                                {
                                    Sheet.Cells[rowData, 4].Value = dvt.ma_tham_so + "/" + dvt.gia_tri;
                                }
                                Sheet.Cells[rowData, 5].Value = item2.so_luong_du_kien.ToString();
                                Sheet.Cells[rowData, 6].Value = item2.don_gia_du_kien_vat.ToString();
                                Sheet.Cells[rowData, 7].Value = item2.total_tien_du_kien.ToString();
                                Sheet.Cells[rowData, 8].Value = item2.ke_hoach_nam_truoc.ToString();
                                Sheet.Cells[rowData, 9].Value = item2.thuc_hien_nam_truoc.ToString();
                                Sheet.Cells[rowData, 10].Value = item2.chech_lech.ToString();
                                Sheet.Cells[rowData, 11].Value = item2.ghi_chu;
                            }
                        }
                    }
                }
                MemoryStream output = new MemoryStream();
                excelPkg.SaveAs(output);
                output.Position = 0;
                return File(output.ToArray(), contentType, fileName);
            }
        }

        public ActionResult CreateDetail([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.PlanDetail> items, int id)
        {
            if (accessDetail.them)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            item.nam_ke_hoach = id;
                            item.ngay_tao = DateTime.Now;
                            item.nguoi_tao = User.Identity.Name;
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
        public ActionResult DeleteDetail(string data)
        {
            if (accessDetail.xoa)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    string[] separators = { "@@" };
                    var listItem = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in listItem)
                    {
                        dbConn.Delete<Core.Entities.PlanDetail>("id={0}", item);
                    }
                    return Json(new { success = true });
                }
            }
            else
            {
                return Json(new { success = false, error = "Bạn không có quyền xóa dữ liệu. Vui lòng liên hệ với ban quản trị để cập nhật quyền." });
            }
        }
        public ActionResult UpdateDetail([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.PlanDetail> items)
        {
            if (accessDetail.sua)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            var check = dbConn.FirstOrDefault<Core.Entities.PlanDetail>(s => s.id == item.id);
                            if (check != null)
                            {
                                item.id = check.id;
                                item.ngay_cap_nhat = DateTime.Now;
                                item.nguoi_cap_nhat = User.Identity.Name;
                                dbConn.Update(item, s => s.id == check.id);
                            }
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

        public ActionResult GetProduct(int ma_san_pham)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = dbConn.FirstOrDefault<Product>(s => s.id == ma_san_pham);

                var dataPrice = dbConn.Select<ProductPriceDetail>(@"
                                SELECT b.*
                                FROM 
                                (SELECT a.nha_cung_cap_id,b.ten_nha_cung_Cap
                                FROM dbo.VendorProductCategory a INNER JOIN dbo.Vendor b ON b.nha_cung_cap_id = a.nha_cung_cap_id
                                WHERE a.ma_phan_cap=
	                                (SELECT TOP 1 ISNULL(ma_nhom_san_pham,'') 
			                                FROM dbo.Product 
			                                WHERE ma_san_pham={0})
			                                ) data 
		                                INNER JOIN dbo.ProductPriceHeader a ON data.nha_cung_cap_id = a.nha_cung_cap_id
		                                INNER JOIN dbo.ProductPriceDetail b ON b.ma_chinh_sach_gia = a.ma_chinh_sach_gia
                                WHERE a.[trang_thai] ='DANG_HOAT_DONG'
                                AND  b.ma_vat_tu={0}
                                AND (CONVERT(DATE,b.ngay_ap_dung,101) <= convert(DATE,getdate(),101) AND 
                                CONVERT(DATE,b.ngay_ket_thuc,101)  >= convert(DATE,getdate(),101))".Params(data.ma_san_pham)).OrderByDescending(s=>s.gia_bao).FirstOrDefault();

                return Json(new { success = true, data = dataPrice ?? new ProductPriceDetail() }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}