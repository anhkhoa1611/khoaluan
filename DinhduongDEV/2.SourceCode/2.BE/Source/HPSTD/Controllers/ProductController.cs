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
    public class ProductController : CustomController
    {
        // GET: Product
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listProductCategory = dbConn.Select<ProductCategory>();
                    ViewBag.listDonViTinh = dbConn.Select<Parameters>("loai_tham_so ='DONVITINH'");
                    ViewBag.listLoaiHangHoa = dbConn.Select<Parameters>("loai_tham_so ='LOAI_HANG_HOA'");
                    return View("Product");
                }
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
                data = KendoApplyFilter.KendoData<Product>(request);
                return Json(data);
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
                        var checkProduct = dbConn.Select<PRequestDetail>(s => s.ma_san_pham == item);
                        if (checkProduct != null && checkProduct.Count > 0)
                        {
                            return Json(new { success = false, error = "Bạn không có thể xóa sản phẩm đang được sử dụng" });
                        }
                        else
                        {
                            dbConn.Delete<Core.Entities.Product>("id={0}", item);
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
        public ActionResult getGetProductCategory(string text)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = dbConn.Select<ProductCategory>("select id, id as ma_nhom_san_pham, ten_phan_cap from ProductCategory where ten_phan_cap COLLATE Latin1_General_CI_AI LIKE N'%" + text + "%'");
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetProductById(int ma_sp)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = dbConn.FirstOrDefault<Product>(s => s.id == ma_sp);
                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetProductByCode(string ma_san_pham)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = dbConn.FirstOrDefault<Product>(s => s.ma_san_pham == ma_san_pham);
                var dvt = dbConn.First<Parameters>("loai_tham_so ='DONVITINH' AND ma_tham_so={0}".Params(data.ma_don_vi_tinh));
                return Json(new { success = true, data = data, dvt = dvt }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetProductByVenderId(string ma_ncc, int ma_sp)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var masp = dbConn.SqlScalar<string>("SELECT ma_san_pham FROM dbo.Product WHERE id={0}".Params(ma_sp));
                var data = dbConn.FirstOrDefault<Product>(@"SELECT c.id,c.ma_san_pham,c.ten_san_pham,c.thong_so_ky_thuat,
                                                                      a.gia_bao_gom_vat , a.don_vi_tinh, a.thue_vat, a.gia_bao as don_gia
                                                                 FROM [dbo].[ProductPriceDetail]
                                                                 a LEFT JOIN [dbo].[ProductPriceHeader]
                                                                 b ON a.ma_chinh_sach_gia=b.ma_chinh_sach_gia
                                                                 LEFT JOIN dbo.Product c
                                                                 ON c.ma_san_pham=a.ma_vat_tu
                                                                 WHERE b.nha_cung_cap_id={0}  AND c.ma_san_pham={1}".Params(ma_ncc, masp));
                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
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
                FileInfo template = new FileInfo(Server.MapPath(@"~\ExportExcelFile\Product.xlsx"));
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
                        string ma_san_pham = oSheet.Cells[i, 1].Value != null ? oSheet.Cells[i, 1].Value.ToString().Trim() : "";
                        string ten_san_pham = oSheet.Cells[i, 2].Value != null ? oSheet.Cells[i, 2].Value.ToString().Trim() : "";
                        string thong_so_ky_thuat = oSheet.Cells[i, 3].Value != null ? oSheet.Cells[i, 3].Value.ToString().Trim() : "";
                        string don_vi_tinh = oSheet.Cells[i, 4].Value != null ? oSheet.Cells[i, 4].Value.ToString().Split('-')[0].ToString().Trim() : "";
                        string ma_nhom_san_pham = oSheet.Cells[i, 5].Value != null ? oSheet.Cells[i, 5].Value.ToString().Split('-')[0].ToString().Trim() : "";
                        string loai_hang_hoa = oSheet.Cells[i, 6].Value != null ? oSheet.Cells[i, 6].Value.ToString() : "";
                        var Status = "";
                        if (oSheet.Cells[i, 7].Value.ToString() == "Hoạt động")
                        {
                            Status = "true";
                        }
                        else if (oSheet.Cells[i, 7].Value.ToString() == "Ngưng hoạt động")
                        {
                            Status = "false";
                        }
                        try
                        {
                            var itemeexit = dbConn.FirstOrDefault<Product>(s => s.ma_san_pham == ma_san_pham);
                            if (itemeexit != null)
                            {
                                itemeexit.ten_san_pham = ten_san_pham;
                                itemeexit.thong_so_ky_thuat = thong_so_ky_thuat;
                                itemeexit.ma_don_vi_tinh = don_vi_tinh;
                                itemeexit.ma_nhom_san_pham = ma_nhom_san_pham;
                                itemeexit.loai_hang_hoa = loai_hang_hoa;
                                itemeexit.trang_thai = Status;
                                itemeexit.ngay_cap_nhat = DateTime.Now;
                                itemeexit.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                                dbConn.Update<Product>(itemeexit);
                            }
                            else
                            {
                                var item = new Product();
                                item.ma_san_pham = ma_san_pham;
                                item.ten_san_pham = ten_san_pham;
                                item.thong_so_ky_thuat = thong_so_ky_thuat;
                                item.ma_don_vi_tinh = don_vi_tinh;
                                item.ma_nhom_san_pham = ma_nhom_san_pham;
                                item.loai_hang_hoa = loai_hang_hoa;
                                item.trang_thai = Status;
                                item.ngay_tao = DateTime.Now;
                                item.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                                item.nguoi_tao = currentUser.ma_nguoi_dung;
                                item.nguoi_cap_nhat = "";
                                dbConn.Insert<Product>(item);
                            }
                            total++;
                            rownumber++;
                        }
                        catch (Exception e)
                        {
                            eSheet.Cells[rownumber, 1].Value = ma_san_pham;
                            eSheet.Cells[rownumber, 2].Value = ten_san_pham;
                            eSheet.Cells[rownumber, 3].Value = thong_so_ky_thuat;
                            eSheet.Cells[rownumber, 4].Value = don_vi_tinh;
                            eSheet.Cells[rownumber, 5].Value = ma_nhom_san_pham;
                            eSheet.Cells[rownumber, 6].Value = loai_hang_hoa;
                            eSheet.Cells[rownumber, 7].Value = Status;
                            eSheet.Cells[rownumber, 8].Value = e.Message;
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
        public ActionResult Create([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.Product> items)
        {
            if (accessDetail.them)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            var exist = dbConn.SingleOrDefault<Product>("ma_san_pham={0}", item.ma_san_pham);
                            if (exist != null)
                            {
                                ModelState.AddModelError("", "Mã hàng hóa dịch vụ này đã tồn tại.");
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
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.Product> items)
        {
            if (accessDetail.sua)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            var exist = dbConn.SingleOrDefault<Product>("id={0}", item.id);
                            if (item.ma_san_pham != exist.ma_san_pham)
                            {
                                var checkDup = dbConn.SingleOrDefault<Product>("ma_san_pham={0}", item.ma_san_pham);
                                if (checkDup != null)
                                {
                                    ModelState.AddModelError("", "Mã nhóm hàng hóa dịch vụ này đã tồn tại.");
                                    return Json(items.ToDataSourceResult(request, ModelState));
                                }
                                else
                                {
                                    exist.ma_san_pham = item.ma_san_pham;
                                }
                            }

                            exist.ten_san_pham = item.ten_san_pham;
                            exist.thong_so_ky_thuat = item.thong_so_ky_thuat;
                            exist.loai_hang_hoa = item.loai_hang_hoa;
                            exist.ma_don_vi_tinh = item.ma_don_vi_tinh;
                            exist.ma_nhom_san_pham = item.ma_nhom_san_pham;
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

        public ActionResult ExportTeamplate()
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                FileInfo fileInfo = new FileInfo(Server.MapPath(@"~\ExportExcelFile\Product_Template.xlsx"));
                var excelPkg = new ExcelPackage(fileInfo);
                string fileName = "Product_Import_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                var procat = dbConn.Select<ProductCategory>("Select id ,isnull(ma_phan_cap,'') as ma_phan_cap , isnull(ten_phan_cap,'') as ten_phan_cap from ProductCategory order by ma_phan_cap");

                int rowprocat = 0;
                ExcelWorksheet SheetNhom = excelPkg.Workbook.Worksheets["Nhom"];

                foreach (var item in procat)
                {
                    rowprocat++;
                    SheetNhom.Cells[rowprocat, 1].Value = item.ma_phan_cap + " - " + item.ten_phan_cap;
                }

                var dvt = dbConn.Select<Parameters>("Select id ,isnull(ma_tham_so,'') as ma_tham_so , isnull(gia_tri,'') as gia_tri From [Parameters] Where loai_tham_so = 'DONVITINH' order by ma_tham_so");
                ExcelWorksheet SheetDVT = excelPkg.Workbook.Worksheets["DVT"];
                int rowDVT = 0;
                foreach (var item in dvt)
                {
                    rowDVT++;
                    SheetDVT.Cells[rowDVT, 1].Value = item.ma_tham_so + " - " + item.gia_tri;
                }
                MemoryStream output = new MemoryStream();
                excelPkg.SaveAs(output);
                output.Position = 0;
                return File(output.ToArray(), contentType, fileName);
            }
        }
    }
}