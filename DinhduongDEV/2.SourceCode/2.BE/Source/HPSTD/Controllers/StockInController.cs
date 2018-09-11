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
using Dapper;

namespace HPSTD.Controllers
{
    [Authorize]
    public class StockInController : CustomController
    {
        // GET: PO
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listTrangThai = dbConn.Select<Parameters>("loai_tham_so='TRANGTHAI'");
                    ViewBag.listProduct = dbConn.Select<Product>();
                    //ViewBag.listDonVi = dbConn.Select<DepartmentHeirarchy>("cap=3");
                    ViewBag.listVendor = dbConn.Select<Vendor>("select * from Vendor where nha_cung_cap_id in (select distinct ma_nha_cung_cap from POHeader)");
                    return View();
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
                    var data = new DataSourceResult();
                    data = KendoApplyFilter.KendoData<StockInHeader>(request);
                    return Json(data);
                }
                return RedirectToAction("NoAccessRights", "Error");
            }
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateUpdate(POHeader data, List<PODetail> details)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    int id = 0;
                    if (accessDetail.them)
                    {
                        string ma_phieu = "";
                        var loai = "PO";
                        //var ma_don_vi = currentUser.ma_don_vi;
                        var yyMMdd = DateTime.Now.ToString("yyMMdd");
                        var existLast = dbConn.SingleOrDefault<POHeader>("SELECT TOP 1 * FROM POHeader ORDER BY id DESC");
                        var nextNo = 0;
                        var yearNow = int.Parse(DateTime.Now.Year.ToString().Substring(2, 2));
                        if (existLast != null)
                        {
                            nextNo = int.Parse(existLast.ma_phieu.Substring(8, existLast.ma_phieu.Length - 8)) + 1;
                            var yearOld = int.Parse(existLast.ma_phieu.Substring(2, 2));
                            if (yearOld == yearNow)
                            {
                                ma_phieu = loai + yyMMdd + String.Format("{0:00000}", nextNo);
                            }
                            else
                            {
                                ma_phieu = loai + yyMMdd + "00001";
                            }
                        }
                        else
                        {
                            ma_phieu = loai + yyMMdd + "00001";
                        }

                        data.ma_phieu = ma_phieu;
                        //data.ngay_tao_yeu_cau = !string.IsNullOrEmpty(Request["ngay_tao_yeu_cau"]) ? DateTime.Parse(DateTime.ParseExact(Request["ngay_tao_yeu_cau"], "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")) : DateTime.Parse("1900-01-01");
                        //data.ngay_cap_thiet_bi = !string.IsNullOrEmpty(Request["ngay_cap_thiet_bi"]) ? DateTime.Parse(DateTime.ParseExact(Request["ngay_cap_thiet_bi"], "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")) : DateTime.Parse("1900-01-01");
                        //data.ten_phieu = data.ma_phieu;
                        data.ngay_tao = DateTime.Now;
                        data.nguoi_tao = currentUser.ma_nguoi_dung;
                        data.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                        data.nguoi_cap_nhat = "";
                        data.trang_thai = "MOI";
                        dbConn.Insert(data);
                        id = (int)dbConn.GetLastInsertId();


                        foreach (var item in details)
                        {
                            PODetail newdata = new PODetail();
                            newdata.ma_phieu_header = data.ma_phieu;
                            newdata.ma_san_pham = item.ma_san_pham;
                            newdata.so_luong = item.so_luong;
                            newdata.thong_so_ky_thuat = item.thong_so_ky_thuat;
                            newdata.muc_dich_su_dung = "";
                            newdata.nguoi_tao = currentUser.ma_nguoi_dung;
                            newdata.ma_to_trinh = item.ma_to_trinh;
                            newdata.id_StatementDetail = item.id_StatementDetail;
                            newdata.don_gia_vat = item.don_gia_vat;
                            newdata.don_gia = item.don_gia;
                            newdata.thue_vat = item.thue_vat;
                            newdata.don_vi_tinh = item.don_vi_tinh;
                            newdata.chi_phi = item.chi_phi;
                            newdata.ma_don_vi = item.ma_don_vi;
                            newdata.ma_chi_nhanh = item.ma_chi_nhanh;
                            newdata.thong_tin_noi_bo = item.thong_tin_noi_bo;
                            newdata.ngay_tao = DateTime.Now;
                            newdata.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            newdata.nguoi_cap_nhat = "";
                            newdata.trang_thai = "";
                            dbConn.Insert<PODetail>(newdata);
                            StatementDetail detail = dbConn.FirstOrDefault<StatementDetail>(s => s.id == item.id_StatementDetail);
                            detail.ma_don_dat_hang = ma_phieu;
                            dbConn.Update(detail);
                        }
                    }
                    else
                    {
                        return Json(new { success = false, error = "Bạn không có quyền thêm dữ liệu" });
                    }
                    return Json(new { success = true, id = id });

                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = ex.Message });
                }
            }

        }
        public ActionResult CreateIn(InvoiceHeader data, List<InvoiceDetail> details)
        {
            return Json(new { success = true});
        }
        public ActionResult Edit(int id)
        {
            if (accessDetail.sua)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listProduct = dbConn.Select<Product>();
                    ViewBag.listDonVi = dbConn.Select<Branch>("Select ma_chi_nhanh, ten_chi_nhanh from Branch");
                    ViewBag.StockInHeader = dbConn.FirstOrDefault<StockInHeader>(s => s.id == id);
                    ViewBag.StatementHeader = dbConn.Select<StatementHeader>();
                    ViewBag.Vendor = dbConn.Select<Vendor>();
                    ViewBag.listTrangThai = dbConn.Select<Parameters>("loai_tham_so='TRANGTHAI'");
                    var data = dbConn.FirstOrDefault<StockInHeader>(s => s.id == id);
                    return View(data);
                }
            }
            else
                return RedirectToAction("NoAccess", "Error");
        }

        public ActionResult ReadDetail([DataSourceRequest]DataSourceRequest request, string ma_phieu_header)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                if (accessDetail.xem)
                {
                    var data = new DataSourceResult();
                    var strsql = @"SELECT * FROM (SELECT d.*, b.dia_chi as dia_chi_don_vi
                                                    FROM StockInDetail d
                                                    INNER JOIN Branch b
													on d.ma_chi_nhanh=b.ma_chi_nhanh
													and d.ma_don_vi=b.ma_don_vi
                                    ) data  ";

                    data = KendoApplyFilter.KendoDataByQuery<StockInDetail>(request, strsql, "ma_phieu_header = {0}".Params(ma_phieu_header));
                    return Json(data);
                }
                return RedirectToAction("NoAccessRights", "Error");
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<StockInDetail> items,string so_hoa_don, DateTime ngay_hoa_don, string ma_phieu_header)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    if (accessDetail.sua)
                    {
                        var isNhapKho = false;
                        foreach (var item in items)
                        {
                            if (item.so_luong_nhap == 0)
                            {
                                continue;
                            }
                            var detail = dbConn.FirstOrDefault<StockInDetail>(s => s.id == item.id);
                            if (item.so_luong == item.so_luong_da_nhap + item.so_luong_nhap)
                            {
                                detail.trang_thai = "NHAP_HOAN_TAT";
                            }
                            else if (item.so_luong_da_nhap + item.so_luong_nhap == 0)
                            {
                                detail.trang_thai = "MOI";
                            }
                            else
                            {
                                detail.trang_thai = "NHAP_MOT_PHAN";
                            }

                            if (item.so_luong_nhap > 0)
                            {
                                isNhapKho = true;
                            }
                            detail.ngay_nhap = item.ngay_nhap;
                            detail.so_luong_da_nhap = item.so_luong_da_nhap + item.so_luong_nhap;
                            detail.ngay_cap_nhat = DateTime.Now;
                            detail.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            dbConn.Update(detail);

                        }
                        if (isNhapKho)
                        {
                            var listDetail = dbConn.Select<StockInDetail>(p => p.ma_phieu_header == ma_phieu_header && p.trang_thai != "NHAP_HOAN_TAT");
                            if (listDetail.Count == 0)
                            {
                                var header = dbConn.Select<StockInHeader>(p => p.ma_phieu == ma_phieu_header).FirstOrDefault();
                                header.trang_thai = "NHAP_HOAN_TAT";
                                header.ngay_cap_nhat = DateTime.Now;
                                dbConn.Update<StockInHeader>(header);
                            }
                            else
                            {
                                var listDetailnew = dbConn.Select<StockInDetail>(p => p.ma_phieu_header == ma_phieu_header && p.trang_thai == "NHAP_MOT_PHAN");
                                if (listDetail.Count > 0)
                                {
                                    var header = dbConn.Select<StockInHeader>(p => p.ma_phieu == ma_phieu_header).FirstOrDefault();
                                    header.trang_thai = "NHAP_MOT_PHAN";
                                    header.ngay_cap_nhat = DateTime.Now;
                                    dbConn.Update<StockInHeader>(header);
                                }
                            }
                            //Invoice 
                            var stockInheader = dbConn.FirstOrDefault<StockInHeader>(s => s.ma_phieu == ma_phieu_header);
                            var stocIndetail = dbConn.Select<StockInDetail>(s => s.ma_phieu_header == stockInheader.ma_phieu);

                            var lstStockIn = stocIndetail.GroupBy(
                                            p => p.ma_chi_nhanh,
                                            (key, g) => new { ma_chi_nhanh = key, Details = g.ToList() });
                            foreach (var stockin in lstStockIn)
                            {
                                InvoiceHeader invoiceHeader = new InvoiceHeader();
                                string ma_phieu = "";
                                var loai = "IV";
                                //var ma_don_vi = currentUser.ma_don_vi;
                                var yyMMdd = DateTime.Now.ToString("yyMMdd");
                                var existLast = dbConn.SingleOrDefault<InvoiceHeader>("SELECT TOP 1 * FROM InvoiceHeader ORDER BY id DESC");
                                var nextNo = 0;
                                var yearNow = int.Parse(DateTime.Now.Year.ToString().Substring(2, 2));
                                if (existLast != null)
                                {
                                    nextNo = int.Parse(existLast.ma_phieu.Substring(8, existLast.ma_phieu.Length - 8)) + 1;
                                    var yearOld = int.Parse(existLast.ma_phieu.Substring(2, 2));
                                    if (yearOld == yearNow)
                                    {
                                        ma_phieu = loai + yyMMdd + String.Format("{0:00000}", nextNo);
                                    }
                                    else
                                    {
                                        ma_phieu = loai + yyMMdd + "00001";
                                    }
                                }
                                else
                                {
                                    ma_phieu = loai + yyMMdd + "00001";
                                }

                                invoiceHeader.ma_phieu = ma_phieu;

                                invoiceHeader.ma_don_vi = stockin.ma_chi_nhanh;
                                invoiceHeader.ma_phieu_nhap_kho = stockInheader.ma_phieu;
                                invoiceHeader.ma_hoa_don = so_hoa_don;
                                invoiceHeader.ngay_hoa_don = ngay_hoa_don;
                                invoiceHeader.ngay_tao = DateTime.Now;
                                invoiceHeader.nguoi_tao = currentUser.ma_nguoi_dung;
                                invoiceHeader.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                                invoiceHeader.nguoi_cap_nhat = "";
                                invoiceHeader.trang_thai = "MOI";
                                dbConn.Insert(invoiceHeader);

                                foreach (var de in stockin.Details)
                                {
                                    InvoiceDetail newdata = new InvoiceDetail();
                                    newdata.ma_phieu_header = invoiceHeader.ma_phieu;
                                    newdata.ma_san_pham = de.ma_san_pham;
                                    newdata.so_luong = items.Where(p => p.id == de.id).FirstOrDefault().so_luong_nhap;
                                    newdata.don_gia_vat = de.don_gia_vat;
                                    newdata.don_gia = de.don_gia;
                                    newdata.thue_vat = de.thue_vat;
                                    newdata.don_vi_tinh = de.don_vi_tinh;
                                    newdata.chi_phi = items.Where(p => p.id == de.id).FirstOrDefault().so_luong_nhap * de.don_gia_vat;
                                    newdata.id_nhap_kho = de.id;
                                    newdata.ma_phieu_nhap_kho = de.ma_phieu_header;
                                    newdata.thong_so_ky_thuat = de.thong_so_ky_thuat;
                                    newdata.nguoi_tao = currentUser.ma_nguoi_dung;
                                    newdata.ngay_tao = DateTime.Now;
                                    newdata.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                                    newdata.nguoi_cap_nhat = "";
                                    newdata.trang_thai = "";
                                    dbConn.Insert<InvoiceDetail>(newdata);
                                }
                            }
                        }
                    }
                    else
                    {
                        return Json(new { success = false, error = "Bạn không có quyền sửa dữ liệu" });
                    }
                    return Json(new { success = true });

                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = ex.Message });
                }
            }

        }
        public ActionResult NhapKho(string data)
        {
            if (accessDetail.sua)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    string[] separators = { "@@" };
                    var listItem = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    //foreach (var item in listItem)
                    //{

                    //    var poheader = dbConn.FirstOrDefault<POHeader>(s => s.id == int.Parse(item));
                    //    var podetail = dbConn.Select<PODetail>(s => s.ma_phieu_header == poheader.ma_phieu);

                    //    var lstStockIn = podetail.GroupBy(
                    //                    p => p.ma_chi_nhanh,
                    //                    (key, g) => new { ma_chi_nhanh = key, Details = g.ToList() });
                    //    foreach (var stockin in lstStockIn)
                    //    {
                    //        StockInHeader stockinheader = new StockInHeader();
                    //        string ma_phieu = "";
                    //        var loai = "SI";
                    //        //var ma_don_vi = currentUser.ma_don_vi;
                    //        var yyMMdd = DateTime.Now.ToString("yyMMdd");
                    //        var existLast = dbConn.SingleOrDefault<StockInHeader>("SELECT TOP 1 * FROM StockInHeader ORDER BY id DESC");
                    //        var nextNo = 0;
                    //        var yearNow = int.Parse(DateTime.Now.Year.ToString().Substring(2, 2));
                    //        if (existLast != null)
                    //        {
                    //            nextNo = int.Parse(existLast.ma_phieu_nhap_kho.Substring(8, existLast.ma_phieu_nhap_kho.Length - 8)) + 1;
                    //            var yearOld = int.Parse(existLast.ma_phieu_nhap_kho.Substring(2, 2));
                    //            if (yearOld == yearNow)
                    //            {
                    //                ma_phieu = loai + yyMMdd + String.Format("{0:00000}", nextNo);
                    //            }
                    //            else
                    //            {
                    //                ma_phieu = loai + yyMMdd + "00001";
                    //            }
                    //        }
                    //        else
                    //        {
                    //            ma_phieu = loai + yyMMdd + "00001";
                    //        }

                    //        stockinheader.ma_phieu_nhap_kho = ma_phieu;

                    //        stockinheader.ten_phieu_nhap_kho = stockinheader.ma_phieu_nhap_kho;
                    //        stockinheader.ma_don_vi = stockin.ma_chi_nhanh;
                    //        stockinheader.ma_phieu_po = poheader.ma_phieu;
                    //        stockinheader.ngay_tao = DateTime.Now;
                    //        stockinheader.nguoi_tao = currentUser.ma_nguoi_dung;
                    //        stockinheader.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                    //        stockinheader.nguoi_cap_nhat = "";
                    //        stockinheader.trang_thai = "MOI";
                    //        dbConn.Insert(stockinheader);

                    //        foreach (var detail in stockin.Details)
                    //        {
                    //            StockInDetail newdata = new StockInDetail();
                    //            newdata.ma_phieu_nhap_kho_header = stockinheader.ma_phieu_nhap_kho;
                    //            newdata.ma_san_pham = detail.ma_san_pham;
                    //            newdata.so_luong = detail.so_luong;
                    //            newdata.don_gia_vat = detail.don_gia_vat;
                    //            newdata.don_gia = detail.don_gia;
                    //            newdata.thue_vat = detail.thue_vat;
                    //            newdata.don_vi_tinh = detail.don_vi_tinh;
                    //            newdata.chi_phi = detail.chi_phi;
                    //            newdata.id_po = detail.id;
                    //            newdata.ma_phieu_po = detail.ma_phieu_header;
                    //            newdata.thong_so_ky_thuat = detail.thong_so_ky_thuat;
                    //            newdata.nguoi_tao = currentUser.ma_nguoi_dung;
                    //            newdata.ngay_tao = DateTime.Now;
                    //            newdata.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                    //            newdata.nguoi_cap_nhat = "";
                    //            newdata.trang_thai = "";
                    //            dbConn.Insert<StockInDetail>(newdata);
                    //        }
                    //    }

                    //    dbConn.Update<POHeader>(set: "trang_thai = {0}, ngay_duyet = {1}".Params(AllConstant.TRANGTHAI_DA_DUYET, DateTime.Now), where: "id = {0}".Params(int.Parse(item)));
                    //}
                    return Json(new { success = true });
                }
            }
            else
            {
                return Json(new { success = false, error = "Bạn không có quyền duyệt đơn đặt hàng" });
            }
        }
    }
}