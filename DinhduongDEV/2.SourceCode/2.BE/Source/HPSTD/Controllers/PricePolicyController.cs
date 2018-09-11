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


namespace HPSTD.Controllers
{
    [Authorize]
    public class PricePolicyController : CustomController
    {
        // GET: Prices
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listDonVi = dbConn.Select<DepartmentHeirarchy>("cap=3");
                    ViewBag.listDonViTinh = dbConn.Select<Parameters>("loai_tham_so ='DONVITINH'");
                    ViewBag.listVendor = dbConn.Select<Vendor>(p => p.trang_thai == "true");
                    ViewBag.listProduct = dbConn.Select<Product>();
                }
                return View("PricePolicy");
            }
            return RedirectToAction("NoAccessRights", "Error");
        }
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new DataSourceResult();
                var query = @"SELECT P.id, P.ma_chinh_sach_gia, P.nha_cung_cap_id, V.ten_nha_cung_Cap, V.dien_thoai, P.so_hop_dong,
                                    V.email, V.dia_chi, V.website, P.ngay_ap_dung, P.ngay_ket_thuc, P.trang_thai, P.ghi_chu, P.ngay_bao_gia,
                                    P.file_hop_dong, P.file_bao_gia, P.ngay_ky_hop_dong, p.nguoi_tao, p.ngay_tao, p.nguoi_cap_nhat, p.ngay_cap_nhat
                            FROM ProductPriceHeader P LEFT JOIN Vendor V ON P.nha_cung_cap_id = V.nha_cung_cap_id";
                if (accessDetail.xem)
                {
                    if (request.Filters.Any())
                    {
                        var where = KendoApplyFilter.ApplyFilter(request.Filters[0]);
                        data = KendoApplyFilter.KendoDataByQuery<ProductPriceHeader>(request, query, where);
                    }
                    else
                    {
                        data = KendoApplyFilter.KendoDataByQuery<ProductPriceHeader>(request, query, "");
                    }
                }
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ReadDetail([DataSourceRequest]DataSourceRequest request, string ma_chinh_sach_gia = "")
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                if (accessDetail.xem)
                {
                    var data = new DataSourceResult();
                    data = KendoApplyFilter.KendoData<ProductPriceDetail>(request, "ma_chinh_sach_gia={0}".Params(ma_chinh_sach_gia));
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                return RedirectToAction("NoAccessRights", "Error");
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateUpdate(ProductPriceHeader data)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    if (data.id > 0)
                    {
                        //if (accessDetail.them && ModelState.IsValid)
                        //{
                        var exist = dbConn.SingleOrDefault<ProductPriceHeader>("id ={0}", data.id);
                        if (data.ngay_ap_dung > data.ngay_ket_thuc)
                        {
                            return Json(new { success = false, error = "Ngày kết thúc phải >= ngày áp dụng chính sách giá" });
                        }
                        var csGiaNCC = dbConn.Select<ProductPriceHeader>(p => p.nha_cung_cap_id == data.nha_cung_cap_id && p.id != data.id && p.trang_thai == "DANG_HOAT_DONG");
                        if (!checkDuplicateRangeDateNCC(csGiaNCC, data))
                        {
                            return Json(new { success = false, error = "Thời gian áp dụng Chính sách giá nhà cung cấp đã có" });
                        }
                        exist.ngay_ket_thuc = data.ngay_ket_thuc;
                        exist.ngay_ap_dung = data.ngay_ap_dung;
                        exist.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                        exist.ngay_cap_nhat = DateTime.Now;
                        exist.ghi_chu = !string.IsNullOrEmpty(data.ghi_chu) ? data.ghi_chu : "";
                        exist.trang_thai = data.trang_thai;
                        dbConn.Update(exist);
                        //}
                        //else
                        //{
                        //    return Json(new { success = false, error = "Bạn không có quyền cập nhập chi nhánh" });
                        //}
                    }
                    else
                    {
                        //if (accessDetail.them && ModelState.IsValid)
                        //{
                        if (data.ngay_ap_dung > data.ngay_ket_thuc)
                        {
                            return Json(new { success = false, error = "Ngày kết thúc phải >= ngày áp dụng chính sách giá" });
                        }
                        var csGiaNCC = dbConn.Select<ProductPriceHeader>(p => p.nha_cung_cap_id == data.nha_cung_cap_id && p.trang_thai == "DANG_HOAT_DONG");
                        if (!checkDuplicateRangeDateNCC(csGiaNCC, data))
                        {
                            return Json(new { success = false, error = "Thời gian áp dụng Chính sách giá nhà cung cấp đã có" });
                        }
                        var isExist = dbConn.SingleOrDefault<ProductPriceHeader>("select * from ProductPriceHeader order by id desc");
                        var prefix = "CSG";
                        if (isExist != null)
                        {
                            var nextNo = int.Parse(isExist.ma_chinh_sach_gia.Substring(9, isExist.ma_chinh_sach_gia.Length - 9)) + 1;
                            data.ma_chinh_sach_gia = prefix + DateTime.Now.ToString("yyMMdd") + String.Format("{0:00}", nextNo);
                        }
                        else
                        {
                            data.ma_chinh_sach_gia = prefix + DateTime.Now.ToString("ddMMyy") + "01";
                        }


                        data.ghi_chu = !string.IsNullOrEmpty(data.ghi_chu) ? data.ghi_chu : "";
                        data.trang_thai = data.trang_thai;
                        data.ngay_tao = DateTime.Now;
                        data.nguoi_tao = User.Identity.Name;
                        data.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                        dbConn.Insert(data);
                        //}
                        //else
                        //{
                        //    return Json(new { success = false, error = "Bạn không có quyền tạo chi nhánh" });
                        //}

                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = ex.Message });
                }

                return Json(new { success = true });

            }
        }
        private bool checkDuplicateRangeDateNCC(List<ProductPriceHeader> list, ProductPriceHeader data)
        {
            foreach (var item in list)
            {
                if (data.ngay_ap_dung <= item.ngay_ket_thuc ||
                    data.ngay_ket_thuc <= item.ngay_ket_thuc)
                {
                    return false;
                }
            }
            return true;
        }
        private bool checkRangeDateNhomVatTuNCC(ProductPriceHeader header, ProductPriceDetail data)
        {
            if (data.ngay_ap_dung > header.ngay_ket_thuc || data.ngay_ap_dung < header.ngay_ap_dung ||
                data.ngay_ket_thuc > header.ngay_ket_thuc)
            {
                return false;
            }
            return true;
        }
        [HttpPost]
        public ActionResult CreateUpdateDetail(ProductPriceDetail data)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    if (data.id > 0)
                    {
                        //if (accessDetail.sua && ModelState.IsValid)
                        //{
                        var exist = dbConn.SingleOrDefault<ProductPriceDetail>("id ={0}", data.id);
                        if (data.ngay_ap_dung > data.ngay_ket_thuc)
                        {
                            return Json(new { success = false, error = "Ngày kết thúc phải >= ngày áp dụng chính sách giá" });
                        }
                        var csGiaNCC = dbConn.Select<ProductPriceHeader>(p => p.ma_chinh_sach_gia == data.ma_chinh_sach_gia).FirstOrDefault();
                        if (!checkRangeDateNhomVatTuNCC(csGiaNCC, data))
                        {
                            return Json(new { success = false, error = "Thời gian áp dụng Chính sách giá được phép trong khoảng từ {0} đến {1}".Params(csGiaNCC.ngay_ap_dung, csGiaNCC.ngay_ket_thuc) });
                        }
                        data.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                        data.ngay_cap_nhat = DateTime.Now;
                        data.trang_thai = data.trang_thai;
                        dbConn.Update(data);
                        //}
                        //else
                        //{
                        //    return Json(new { success = false, error = "Bạn không có quyền cập nhập." });
                        //}
                    }
                    else
                    {
                        //    if (accessDetail.them && ModelState.IsValid)
                        //    {
                        var isExist = dbConn.Select<ProductPriceDetail>(p => p.ma_chinh_sach_gia == data.ma_chinh_sach_gia && p.ma_vat_tu == data.ma_vat_tu).FirstOrDefault();
                        if (isExist != null)
                        {
                            return Json(new { success = false, error = "Nhóm vật tư đã có trong chính sách giá NCC" });
                        }
                        if (data.ngay_ap_dung > data.ngay_ket_thuc)
                        {
                            return Json(new { success = false, error = "Ngày kết thúc phải >= ngày áp dụng chính sách giá" });
                        }
                        var csGiaNCC = dbConn.Select<ProductPriceHeader>(p => p.ma_chinh_sach_gia == data.ma_chinh_sach_gia).FirstOrDefault();
                        if (!checkRangeDateNhomVatTuNCC(csGiaNCC, data))
                        {
                            return Json(new { success = false, error = "Thời gian áp dụng Chính sách giá được phép trong khoảng từ {0} đến {1}".Params(csGiaNCC.ngay_ap_dung, csGiaNCC.ngay_ket_thuc) });
                        }
                        data.trang_thai = data.trang_thai;
                        data.ngay_tao = DateTime.Now;
                        data.nguoi_tao = User.Identity.Name;
                        data.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                        dbConn.Insert(data);
                        //}
                        //else
                        //{
                        //    return Json(new { success = false, error = "Bạn không có quyền thêm dữ liệu" });
                        //}
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = ex.Message });
                }
                return Json(new { success = true });
            }
        }
        public ActionResult DeleteList(string data)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            using (var dbTrans = dbConn.OpenTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    string[] separators = { "@@" };
                    var listid = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var id in listid)
                    {
                        dbConn.Delete<ProductPriceHeader>(p => p.ma_chinh_sach_gia == id);
                        dbConn.Delete<ProductPriceDetail>(p => p.ma_chinh_sach_gia == id);
                    }
                    dbTrans.Commit();
                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    dbTrans.Rollback();
                    return Json(new { success = false, error = e.Message });
                }
            }
        }
        public ActionResult Active(string data)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            using (var dbTrans = dbConn.OpenTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    int i = 0;
                    string[] separators = { "@@" };
                    var listid = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var id in listid)
                    {
                        var item = dbConn.FirstOrDefault<Branch>("Id={0}", id);
                        item.trang_thai = "DANG_HOAT_DONG";
                        item.ngay_cap_nhat = DateTime.Now;
                        item.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                        dbConn.Update(item);
                        i++;
                    }
                    dbTrans.Commit();
                    return Json(new { success = true, message = i });
                }
                catch (Exception e)
                {
                    dbTrans.Rollback();
                    return Json(new { success = false, error = e.Message });
                }
            }
        }
        public ActionResult Inactive(string data)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            using (var dbTrans = dbConn.OpenTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    int i = 0;
                    string[] separators = { "@@" };
                    var listid = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var id in listid)
                    {
                        var item = dbConn.FirstOrDefault<Branch>("Id={0}", id);
                        item.trang_thai = "KHONG_HOAT_DONG";
                        item.ngay_cap_nhat = DateTime.Now;
                        item.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                        dbConn.Update(item);
                        i++;
                    }
                    dbTrans.Commit();
                    return Json(new { success = true, message = i });
                }
                catch (Exception e)
                {
                    dbTrans.Rollback();
                    return Json(new { success = false, error = e.Message });
                }
            }
        }
        [HttpPost]
        public ActionResult Excel_Export(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);

            return File(fileContents, contentType, fileName);
        }
        //BaoHV
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<ProductPriceDetail> items, ProductPriceHeader inputHead)
        {
            if (accessDetail.sua || accessDetail.them)
            {
                var Ma_CSG = "";
                var IdHead = 0;
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        //insert header
                        var checkHead = dbConn.FirstOrDefault<ProductPriceHeader>(s => s.id == inputHead.id);
                        if (checkHead != null)
                        {
                            checkHead.ghi_chu = !string.IsNullOrEmpty(inputHead.ghi_chu) ? inputHead.ghi_chu : "";
                            checkHead.trang_thai = inputHead.trang_thai;
                            checkHead.ngay_bao_gia = inputHead.ngay_bao_gia;
                            checkHead.ngay_ky_hop_dong = inputHead.ngay_ky_hop_dong;
                            checkHead.ngay_tao = checkHead.ngay_tao;
                            checkHead.ngay_ap_dung = inputHead.ngay_ap_dung;
                            checkHead.ngay_ket_thuc = inputHead.ngay_ket_thuc;
                            checkHead.nguoi_tao = checkHead.nguoi_tao;
                            checkHead.ngay_cap_nhat = DateTime.Now;
                            checkHead.so_hop_dong = !string.IsNullOrEmpty(inputHead.so_hop_dong) ? inputHead.so_hop_dong : "";
                            checkHead.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            dbConn.Update(checkHead);

                            IdHead = checkHead.id;
                            Ma_CSG = checkHead.ma_chinh_sach_gia;
                        }
                        else
                        {
                            var prefix = "CSG";
                            var isExist = dbConn.SingleOrDefault<ProductPriceHeader>("select id,ma_chinh_sach_gia from ProductPriceHeader order by id desc");
                            if (isExist != null)
                            {
                                var nextNo = int.Parse(isExist.ma_chinh_sach_gia.Substring(9, isExist.ma_chinh_sach_gia.Length - 9)) + 1;
                                inputHead.ma_chinh_sach_gia = prefix + DateTime.Now.ToString("yyMMdd") + String.Format("{0:00}", nextNo);
                            }
                            else
                            {
                                inputHead.ma_chinh_sach_gia = prefix + DateTime.Now.ToString("ddMMyy") + "01";
                            }
                            inputHead.so_hop_dong = !string.IsNullOrEmpty(inputHead.so_hop_dong) ? inputHead.so_hop_dong : "";
                            inputHead.ghi_chu = !string.IsNullOrEmpty(inputHead.ghi_chu) ? inputHead.ghi_chu : "";
                            inputHead.trang_thai = inputHead.trang_thai;
                            inputHead.ngay_tao = DateTime.Now;
                            inputHead.nguoi_tao = User.Identity.Name;
                            inputHead.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            dbConn.Insert(inputHead);
                            IdHead = (int)dbConn.GetLastInsertId();
                            Ma_CSG = inputHead.ma_chinh_sach_gia;
                        }
                        //insert detail
                        foreach (var row in items)
                        {
                            if (string.IsNullOrEmpty(row.ma_vat_tu))
                            {
                                continue;
                            }
                            var checkDetail = dbConn.FirstOrDefault<ProductPriceDetail>(p => p.ma_chinh_sach_gia == Ma_CSG && p.ma_vat_tu == row.ma_vat_tu);
                            if (checkDetail != null)
                            {
                                checkDetail.gia_bao = row.gia_bao;
                                checkDetail.thue_vat = row.thue_vat;
                                checkDetail.gia_bao_gom_vat = row.gia_bao_gom_vat;
                                checkDetail.ngay_ap_dung = inputHead.ngay_ap_dung;
                                checkDetail.ngay_ket_thuc = inputHead.ngay_ket_thuc;
                                checkDetail.ngay_cap_nhat = DateTime.Now;
                                checkDetail.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                                checkDetail.don_vi_tinh = !string.IsNullOrEmpty(row.don_vi_tinh) ? row.don_vi_tinh : "";
                                checkDetail.sl_min = row.sl_min != 0 ? row.sl_min : 0;
                                checkDetail.sl_max = row.sl_max != 0 ? row.sl_max : 0;
                                dbConn.Update(checkDetail);
                            }
                            else
                            {
                                row.trang_thai = row.trang_thai;
                                row.ngay_tao = DateTime.Now;
                                row.nguoi_tao = User.Identity.Name;
                                row.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                                row.nguoi_cap_nhat = "";
                                row.ma_chinh_sach_gia = Ma_CSG;
                                row.don_vi_tinh = !string.IsNullOrEmpty(row.don_vi_tinh) ? row.don_vi_tinh : "";
                                row.sl_min = row.sl_min != 0 ? row.sl_min : 0;
                                row.sl_max = row.sl_max != 0 ? row.sl_max : 0;
                                dbConn.Insert(row);
                            }
                        }
                    }
                }
                return Json(new { success = true, ma_csg = Ma_CSG, error = "", IdHead = IdHead });
            }
            else
            {
                return Json(new { success = false, error = "Bạn không có quyền cập nhật" });
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UploadFile(string ma_csg)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var file = Request.Files["FileUploadBG"];
                if (file != null)
                {
                    string destinationPath = Helpers.Upload.UploadFile("BaoGia", file);
                    dbConn.Update<ProductPriceHeader>(set: "file_bao_gia = {0}".Params(destinationPath), where: "ma_chinh_sach_gia = {0}".Params(ma_csg));
                }
                var fileHD = Request.Files["FileUploadHD"];
                if (fileHD != null)
                {
                    string destinationPath = Helpers.Upload.UploadFile("HopDong", fileHD);
                    dbConn.Update<ProductPriceHeader>(set: "file_hop_dong = {0}".Params(destinationPath), where: "ma_chinh_sach_gia = {0}".Params(ma_csg));
                }
                var data = dbConn.FirstOrDefault<ProductPriceHeader>("ma_chinh_sach_gia = {0}".Params(ma_csg));
                return Json(new { success = true, error = "", data = data }, JsonRequestBehavior.AllowGet);
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
                        dbConn.Delete<ProductPriceDetail>("id={0}", id);
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
    }
}