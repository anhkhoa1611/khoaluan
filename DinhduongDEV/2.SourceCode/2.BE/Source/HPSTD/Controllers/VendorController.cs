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
using System.IO;
using OfficeOpenXml;

namespace HPSTD.Controllers
{
    public class VendorController : CustomController
    {
        // GET: Vendor
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listProductCategory = dbConn.Select<ProductCategory>();
                }
                return View("Vendor");
            }
            return RedirectToAction("NoAccessRights", "Error");
        }



        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new DataSourceResult();
                data = KendoApplyFilter.KendoData<Vendor>(request);
                foreach (Vendor d in data.Data)
                {
                    d.listMa_phan_cap = dbConn.GetFirstColumn<string>("SELECT ma_phan_cap FROM VendorProductCategory where nha_cung_cap_id ={0}", d.nha_cung_cap_id);
                }
                return Json(data);
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateUpdate(Vendor data, HttpPostedFileBase file_dinh_kem)
        {
            if (accessDetail.sua || accessDetail.them)
            {
                try
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        string ma_id = "";
                        var exist = dbConn.SingleOrDefault<Vendor>("id ={0}", data.id);
                        if (exist != null)
                        {
                            exist.ten_nha_cung_cap = data.ten_nha_cung_cap;
                            exist.ten_thuong_goi = data.ten_thuong_goi;
                            exist.ma_so_thue = data.ma_so_thue;
                            exist.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            exist.ngay_cap_nhat = DateTime.Now;
                            exist.dien_thoai = !string.IsNullOrEmpty(data.dien_thoai) ? data.dien_thoai : "";
                            exist.von_dieu_le = data.von_dieu_le;
                            exist.email = !string.IsNullOrEmpty(data.email) ? data.email : "";
                            exist.quy_mo = !string.IsNullOrEmpty(data.quy_mo) ? data.quy_mo : "";
                            exist.pham_vi_cung_ung = !string.IsNullOrEmpty(data.pham_vi_cung_ung) ? data.pham_vi_cung_ung : "";
                            exist.bao_hanh = !string.IsNullOrEmpty(data.bao_hanh) ? data.bao_hanh : "";
                            exist.thoi_gian_cung_ung = !string.IsNullOrEmpty(data.thoi_gian_cung_ung) ? data.thoi_gian_cung_ung : "";
                            exist.khach_hang_cung_cap = !string.IsNullOrEmpty(data.khach_hang_cung_cap) ? data.khach_hang_cung_cap : "";
                            exist.chung_loai_hang_hoa_ncc = !string.IsNullOrEmpty(data.chung_loai_hang_hoa_ncc) ? data.chung_loai_hang_hoa_ncc : "";
                            exist.so_luong_tieu_chuan = data.so_luong_tieu_chuan;
                            exist.thoi_gian_giao_hang = !string.IsNullOrEmpty(data.thoi_gian_giao_hang) ? data.thoi_gian_giao_hang : "";
                            exist.phuong_thuc_thanh_toan = !string.IsNullOrEmpty(data.phuong_thuc_thanh_toan) ? data.phuong_thuc_thanh_toan : "";
                            exist.dieu_kien_thanh_toan = !string.IsNullOrEmpty(data.dieu_kien_thanh_toan) ? data.dieu_kien_thanh_toan : "";
                            exist.dia_chi = !string.IsNullOrEmpty(data.dia_chi) ? data.dia_chi : "";
                            exist.website = !string.IsNullOrEmpty(data.website) ? data.website : "";
                            exist.ghi_chu = !string.IsNullOrEmpty(data.ghi_chu) ? data.ghi_chu : "";
                            exist.thong_bao_giao_hang = !string.IsNullOrEmpty(data.thong_bao_giao_hang) ? data.thong_bao_giao_hang : "";
                            exist.thoi_han_thanh_toan = !string.IsNullOrEmpty(data.thoi_han_thanh_toan) ? data.thoi_han_thanh_toan : "";
                            exist.trang_thai = data.trang_thai;
                            exist.nha_cung_cap_cua_hdbank = data.nha_cung_cap_cua_hdbank;


                            if (file_dinh_kem != null && file_dinh_kem.ContentLength > 0)
                            {
                                string pathForSaving = Server.MapPath("~/FileUpload/");
                                if (!Directory.Exists(pathForSaving))
                                {
                                    Directory.CreateDirectory(pathForSaving);
                                }
                                if (file_dinh_kem.ContentLength > 5242880)
                                {
                                    return Json(new { success = false, error = "Vui lòng chọn file không được lớn hơn 5M" });
                                }
                                var filename = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + Locdau.ConvertFileName(file_dinh_kem.FileName);
                                file_dinh_kem.SaveAs(Path.Combine(pathForSaving, filename));
                                exist.file_dinh_kem = "/FileUpload/" + filename;
                            }
                            ma_id = exist.nha_cung_cap_id;
                            dbConn.Update(exist);
                        }
                        else
                        {
                            var isExist = dbConn.SingleOrDefault<Vendor>("select * from Vendor order by id desc");
                            var prefix = "NCC";
                            if (isExist != null)
                            {
                                var nextNo = int.Parse(isExist.nha_cung_cap_id.Substring(9, isExist.nha_cung_cap_id.Length - 9)) + 1;
                                data.nha_cung_cap_id = prefix + DateTime.Now.ToString("yyMMdd") + String.Format("{0:0000}", nextNo);
                            }
                            else
                            {
                                data.nha_cung_cap_id = prefix + DateTime.Now.ToString("ddMMyy") + "0001";
                            }
                            var existName = dbConn.SingleOrDefault<Vendor>("ma_so_thue={0}", data.ma_so_thue);
                            if (existName != null)
                            {
                                return Json(new { success = false, error = "Nhà cung cấp này đã tồn tại." });
                            }
                            if (file_dinh_kem != null && file_dinh_kem.ContentLength > 0)
                            {
                                string pathForSaving = Server.MapPath("~/FileUpload/");
                                if (!Directory.Exists(pathForSaving))
                                {
                                    Directory.CreateDirectory(pathForSaving);
                                }
                                if (file_dinh_kem.ContentLength > 5242880)
                                {
                                    return Json(new { success = false, error = "Vui lòng chọn file không được lớn hơn 5M" });
                                }
                                var filename = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + Locdau.ConvertFileName(file_dinh_kem.FileName);
                                file_dinh_kem.SaveAs(Path.Combine(pathForSaving, filename));
                                data.file_dinh_kem = "/FileUpload/" + filename;
                            }
                            data.ngay_tao = DateTime.Now;
                            data.nguoi_tao = User.Identity.Name;
                            data.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            data.trang_thai = "true";
                            dbConn.Insert(data);
                            ma_id = data.nha_cung_cap_id;
                        }
                        dbConn.Delete<VendorProductCategory>("nha_cung_cap_id={0}", ma_id);
                        if (data.ma_phan_cap != null && data.ma_phan_cap.Count() > 0)
                        {
                            foreach (var item in data.ma_phan_cap)
                            {
                                var newVendor = new VendorProductCategory();
                                newVendor.ma_phan_cap = item;
                                newVendor.nha_cung_cap_id = ma_id;
                                newVendor.nguoi_tao = currentUser.ma_nguoi_dung;
                                newVendor.ngay_tao = DateTime.Now;
                                dbConn.Insert(newVendor);
                            }
                        }
                        data = dbConn.SingleOrDefault<Vendor>("nha_cung_cap_id = {0}", data.nha_cung_cap_id);
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = ex.Message });
                }
            }
            else
            {
                ModelState.AddModelError("", "Bạn không có quyền. Vui lòng liên hệ ban quản trị để cập nhật quyền.");
            }
            return Json(new { success = true, data = data });
        }
        public ActionResult Delete(int id)
        {
            if (accessDetail.xoa)
            {
                try
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        dbConn.Delete<Vendor>("id={0}", id);
                        var exist = dbConn.SingleOrDefault<Vendor>("id={0}", id);
                        dbConn.Delete<ContactInfor>("nha_cung_cap_id={0}", exist.nha_cung_cap_id);
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
                        dbConn.Delete<ContactInfor>("id={0}", id);
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

        public ActionResult ExportData([DataSourceRequest]DataSourceRequest request)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                //using (ExcelPackage excelPkg = new ExcelPackage())
                FileInfo fileInfo = new FileInfo(Server.MapPath(@"~\ExportExcelFile\NCC.xlsx"));
                var excelPkg = new ExcelPackage(fileInfo);
                string fileName = "NCC_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                var data = new List<Vendor>();
                if (accessDetail.xuat)
                {
                    if (request.Filters.Any())
                    {
                        var where = KendoApplyFilter.ApplyFilter(request.Filters[0], "");
                        data = dbConn.Select<Vendor>(where).ToList();
                    }
                    else
                    {
                        data = dbConn.Select<Vendor>().ToList();
                    }
                }
                ExcelWorksheet Sheet = excelPkg.Workbook.Worksheets["data"];

                int rowData = 7;
                int stt = 0;
                foreach (var item in data)
                {
                    var contact = dbConn.SingleOrDefault<ContactInfor>("nha_cung_cap_id={0} and nguoi_lien_he_chinh = 1", item.nha_cung_cap_id);
                    int i = 1;
                    stt++;
                    rowData++;
                    Sheet.Cells[rowData, i++].Value = stt;
                    Sheet.Cells[rowData, i++].Value = item.ten_nha_cung_cap;
                    Sheet.Cells[rowData, i++].Value = item.ten_thuong_goi;
                    Sheet.Cells[rowData, i++].Value = item.ma_so_thue;
                    Sheet.Cells[rowData, i++].Value = item.dia_chi;
                    Sheet.Cells[rowData, i++].Value = item.von_dieu_le;
                    Sheet.Cells[rowData, i++].Value = item.quy_mo;
                    Sheet.Cells[rowData, i++].Value = item.pham_vi_cung_ung;
                    //Mat hang kinh doanh
                    Sheet.Cells[rowData, i++].Value = "";
                    Sheet.Cells[rowData, i++].Value = item.thoi_gian_cung_ung;
                    Sheet.Cells[rowData, i++].Value = item.khach_hang_cung_cap;
                    Sheet.Cells[rowData, i++].Value = item.nha_cung_cap_cua_hdbank;
                    Sheet.Cells[rowData, i++].Value = contact != null ? contact.ten_nguoi_lien_he : "";
                    Sheet.Cells[rowData, i++].Value = contact != null ? contact.so_dien_thoai : "";
                    Sheet.Cells[rowData, i++].Value = contact != null ? contact.email : "";

                    Sheet.Cells[rowData, i++].Value = item.file_dinh_kem;
                    Sheet.Cells[rowData, i++].Value = item.ghi_chu;

                }
                MemoryStream output = new MemoryStream();
                excelPkg.SaveAs(output);
                output.Position = 0;
                return File(output.ToArray(), contentType, fileName);
            }
        }

        public ActionResult ReadContactInfor([DataSourceRequest] DataSourceRequest request, string nha_cung_cap_id)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new DataSourceResult();
                data = KendoApplyFilter.KendoData<ContactInfor>(request, "nha_cung_cap_id={0}".Params(nha_cung_cap_id));
                return Json(data);
            }
        }
        public ActionResult ReadProductPrice([DataSourceRequest] DataSourceRequest request, string nha_cung_cap_id)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new DataSourceResult();
                data = KendoApplyFilter.KendoData<ProductPriceHeader>(request, "nha_cung_cap_id={0}".Params(nha_cung_cap_id));
                return Json(data);
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<ContactInfor> items, string ma_ncc)
        {
            if (accessDetail.sua || accessDetail.them)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var row in items)
                        {

                            var checkID = dbConn.SingleOrDefault<ContactInfor>("nha_cung_cap_id={0}", ma_ncc);
                            if (checkID != null)
                            {
                                checkID.ten_nguoi_lien_he = !string.IsNullOrEmpty(row.ten_nguoi_lien_he) ? row.ten_nguoi_lien_he : "";
                                checkID.so_dien_thoai = !string.IsNullOrEmpty(row.so_dien_thoai) ? row.so_dien_thoai : "";
                                checkID.nguoi_lien_he_chinh = row.nguoi_lien_he_chinh;
                                checkID.nha_cung_cap_id = ma_ncc;
                                checkID.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                                checkID.ngay_cap_nhat = DateTime.Now;
                                dbConn.Update(checkID);
                            }
                            else
                            {
                                row.nha_cung_cap_id = ma_ncc;
                                row.nguoi_tao = currentUser.ma_nguoi_dung;
                                row.ngay_tao = DateTime.Now;
                                row.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                                row.ngay_cap_nhat = DateTime.Now;
                                dbConn.Insert(row);
                            }
                        }
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Bạn không có quyền cập nhật");
            }
            return Json(items.ToDataSourceResult(request, ModelState));
        }

        public ActionResult ExportTemplate()
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                FileInfo fileInfo = new FileInfo(Server.MapPath(@"~\ExportExcelFile\Vendor_Template.xlsx"));
                var excelPkg = new ExcelPackage(fileInfo);
                string fileName = "Vendor_Import_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
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
                FileInfo template = new FileInfo(Server.MapPath(@"~\ExportExcelFile\NCC.xlsx"));
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
                            //string ma_nha_cung_cap = oSheet.Cells[i, 1].Value != null ? oSheet.Cells[i, 1].Value.ToString().Trim() : "";
                            string ten_nha_cung_cap = oSheet.Cells[i, 1].Value != null ? oSheet.Cells[i, 1].Value.ToString().Trim() : "";
                            string ten_thuong_goi = oSheet.Cells[i, 2].Value != null ? oSheet.Cells[i, 2].Value.ToString().Trim() : "";
                            string ma_so_thue = oSheet.Cells[i, 3].Value != null ? oSheet.Cells[i, 3].Value.ToString().Trim() : "";
                            string dia_chi_ncc = oSheet.Cells[i, 4].Value != null ? oSheet.Cells[i, 4].Value.ToString().Trim() : "";
                            string dien_thoai_ncc = oSheet.Cells[i, 5].Value != null ? oSheet.Cells[i, 5].Value.ToString().Trim() : "";
                            string bao_hanh_ncc = oSheet.Cells[i, 6].Value != null ? oSheet.Cells[i, 6].Value.ToString().Trim() : "";
                            string dieu_kien_thanh_toan = oSheet.Cells[i, 7].Value != null ? oSheet.Cells[i, 7].Value.ToString().Trim() : "";
                            string thoi_gian_cung_ung = oSheet.Cells[i, 8].Value != null ? oSheet.Cells[i, 8].Value.ToString().Trim() : "";
                            string email_ncc = oSheet.Cells[i, 9].Value != null ? oSheet.Cells[i, 9].Value.ToString().Trim() : "";
                            string khach_hang_cung_cap = oSheet.Cells[i, 10].Value != null ? oSheet.Cells[i, 10].Value.ToString().Trim() : "";
                            string von_dieu_le = oSheet.Cells[i, 11].Value != null ? oSheet.Cells[i, 11].Value.ToString().Trim() : "";
                            string chung_loai_hh = oSheet.Cells[i, 12].Value != null ? oSheet.Cells[i, 12].Value.ToString().Trim() : "";
                            string website_ncc = oSheet.Cells[i, 13].Value != null ? oSheet.Cells[i, 13].Value.ToString().Trim() : "";
                            string quy_mo_ncc = oSheet.Cells[i, 14].Value != null ? oSheet.Cells[i, 14].Value.ToString().Trim() : "";
                            string ncc_cua_hdbank = oSheet.Cells[i, 15].Value != null ? oSheet.Cells[i, 15].Value.ToString().Split('/')[1].ToString().Trim() : ""; ;
                            string ghi_chu = oSheet.Cells[i, 16].Value != null ? oSheet.Cells[i, 16].Value.ToString().Trim() : "";
                            string thoi_gian_giao_hang = oSheet.Cells[i, 17].Value != null ? oSheet.Cells[i, 17].Value.ToString().Trim() : "";
                            string so_luong_tieu_chuan = oSheet.Cells[i, 18].Value != null ? oSheet.Cells[i, 18].Value.ToString().Trim() : "";
                            string pham_vi_ung_dung = oSheet.Cells[i, 19].Value != null ? oSheet.Cells[i, 19].Value.ToString().Trim() : "";
                            string phuong_thuc_thanh_toan = oSheet.Cells[i, 20].Value != null ? oSheet.Cells[i, 20].Value.ToString().Trim() : "";
                            string trang_thai = "true";
                            if (oSheet.Cells[i, 21].Value.ToString() == "Hoạt động")
                            {
                                trang_thai = "true";
                            }
                            else if (oSheet.Cells[i, 21].Value.ToString() == "Ngưng hoạt động")
                            {
                                trang_thai = "false";
                            }

                            double vondl = 0.0;
                            int sltc = 0;

                            try
                            {
                                var itemeexit = dbConn.FirstOrDefault<Vendor>(s => s.ma_so_thue == ma_so_thue);
                                if (itemeexit != null)
                                {
                                    itemeexit.ten_nha_cung_cap = ten_nha_cung_cap;
                                    itemeexit.ten_thuong_goi = ten_thuong_goi;
                                    itemeexit.ma_so_thue = ma_so_thue;
                                    itemeexit.dia_chi = dia_chi_ncc;
                                    itemeexit.dien_thoai = dien_thoai_ncc;
                                    itemeexit.bao_hanh = bao_hanh_ncc;
                                    itemeexit.dieu_kien_thanh_toan = dieu_kien_thanh_toan;
                                    itemeexit.thoi_gian_cung_ung = thoi_gian_cung_ung;
                                    itemeexit.email = email_ncc;
                                    itemeexit.khach_hang_cung_cap = khach_hang_cung_cap;
                                    itemeexit.von_dieu_le = double.TryParse(von_dieu_le, out vondl) ? vondl : 0.0; ;
                                    itemeexit.chung_loai_hang_hoa_ncc = chung_loai_hh;
                                    itemeexit.website = website_ncc;
                                    itemeexit.quy_mo = quy_mo_ncc;
                                    itemeexit.nha_cung_cap_cua_hdbank = ncc_cua_hdbank;
                                    itemeexit.ghi_chu = ghi_chu;
                                    itemeexit.thoi_gian_giao_hang = thoi_gian_giao_hang;
                                    itemeexit.so_luong_tieu_chuan = int.TryParse(so_luong_tieu_chuan, out sltc) ? sltc : 0;
                                    itemeexit.pham_vi_cung_ung = pham_vi_ung_dung;
                                    itemeexit.phuong_thuc_thanh_toan = phuong_thuc_thanh_toan;
                                    itemeexit.trang_thai = trang_thai;
                                    itemeexit.ngay_cap_nhat = DateTime.Now;
                                    itemeexit.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                                    dbConn.Update<Vendor>(itemeexit);
                                }
                                else
                                {
                                    var item = new Vendor();
                                    var isExist = dbConn.SingleOrDefault<Vendor>("select * from Vendor order by id desc");
                                    var prefix = "NCC";
                                    if (isExist != null)
                                    {
                                        var nextNo = int.Parse(isExist.nha_cung_cap_id.Substring(9, isExist.nha_cung_cap_id.Length - 9)) + 1;
                                        item.nha_cung_cap_id = prefix + DateTime.Now.ToString("yyMMdd") + String.Format("{0:0000}", nextNo);
                                    }
                                    else
                                    {
                                        item.nha_cung_cap_id = prefix + DateTime.Now.ToString("ddMMyy") + "0001";
                                    }
                                    item.ten_nha_cung_cap = ten_nha_cung_cap;
                                    item.ten_thuong_goi = ten_thuong_goi;
                                    item.ma_so_thue = ma_so_thue;
                                    item.dia_chi = dia_chi_ncc;
                                    item.dien_thoai = dien_thoai_ncc;
                                    item.bao_hanh = bao_hanh_ncc;
                                    item.dieu_kien_thanh_toan = dieu_kien_thanh_toan;
                                    item.thoi_gian_cung_ung = thoi_gian_cung_ung;
                                    item.email = email_ncc;
                                    item.khach_hang_cung_cap = khach_hang_cung_cap;
                                    item.von_dieu_le = double.TryParse(von_dieu_le, out vondl) ? vondl : 0.0; ;
                                    item.chung_loai_hang_hoa_ncc = chung_loai_hh;
                                    item.website = website_ncc;
                                    item.quy_mo = quy_mo_ncc;
                                    item.nha_cung_cap_cua_hdbank = ncc_cua_hdbank;
                                    item.ghi_chu = ghi_chu;
                                    item.thoi_gian_giao_hang = thoi_gian_giao_hang;
                                    item.so_luong_tieu_chuan = int.TryParse(so_luong_tieu_chuan, out sltc) ? sltc : 0;
                                    item.pham_vi_cung_ung = pham_vi_ung_dung;
                                    item.phuong_thuc_thanh_toan = phuong_thuc_thanh_toan;
                                    item.trang_thai = trang_thai;
                                    item.ngay_tao = DateTime.Now;
                                    item.nguoi_tao = currentUser.ma_nguoi_dung;
                                    item.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                                    item.nguoi_cap_nhat = "";
                                    dbConn.Insert<Vendor>(item);
                                    total++;
                                    rownumber++;
                                }
                                total++;
                                rownumber++;
                            }
                            catch (Exception e)
                            {
                                eSheet.Cells[rownumber, 1].Value = ten_nha_cung_cap;
                                eSheet.Cells[rownumber, 2].Value = ten_thuong_goi;
                                eSheet.Cells[rownumber, 3].Value = ma_so_thue;
                                eSheet.Cells[rownumber, 4].Value = dia_chi_ncc;
                                eSheet.Cells[rownumber, 5].Value = dien_thoai_ncc;
                                eSheet.Cells[rownumber, 6].Value = bao_hanh_ncc;
                                eSheet.Cells[rownumber, 7].Value = dieu_kien_thanh_toan;
                                eSheet.Cells[rownumber, 8].Value = thoi_gian_cung_ung;
                                eSheet.Cells[rownumber, 9].Value = email_ncc;
                                eSheet.Cells[rownumber, 10].Value = khach_hang_cung_cap;
                                eSheet.Cells[rownumber, 11].Value = von_dieu_le;
                                eSheet.Cells[rownumber, 12].Value = chung_loai_hh;
                                eSheet.Cells[rownumber, 13].Value = website_ncc;
                                eSheet.Cells[rownumber, 14].Value = quy_mo_ncc;
                                eSheet.Cells[rownumber, 15].Value = ncc_cua_hdbank;
                                eSheet.Cells[rownumber, 16].Value = ghi_chu;
                                eSheet.Cells[rownumber, 17].Value = thoi_gian_giao_hang;
                                eSheet.Cells[rownumber, 18].Value = so_luong_tieu_chuan;
                                eSheet.Cells[rownumber, 19].Value = pham_vi_ung_dung;
                                eSheet.Cells[rownumber, 20].Value = phuong_thuc_thanh_toan;
                                eSheet.Cells[rownumber, 21].Value = trang_thai;
                                eSheet.Cells[rownumber, 22].Value = e.Message;
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
    }
}