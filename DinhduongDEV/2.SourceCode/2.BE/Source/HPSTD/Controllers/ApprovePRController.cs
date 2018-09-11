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

namespace HPSTD.Controllers
{
    [Authorize]
    public class ApprovePRController : CustomController
    {
        // GET: ApprovePR
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listProduct = dbConn.Select<Product>("select id,ma_san_pham,ten_san_pham from Product where ma_nhom_san_pham in (select ma_phan_cap from UserProductCategory where ma_nguoi_dung ={0})", currentUser.ma_nguoi_dung);
                    ViewBag.listVender = dbConn.Select<VendorModel>(@"SELECT nha_cung_cap_id,ten_nha_cung_Cap FROM Vendor WHERE trang_thai='true' ");
                    ViewBag.listBranch = dbConn.Select<Branch>("Select * From Branch Where ma_chi_nhanh in (Select ma_chi_nhanh From UsersBranch Where ma_nguoi_dung = '" + currentUser.ma_nguoi_dung + "')");
                    ViewBag.listTrangThai = dbConn.Select<Parameters>("loai_tham_so='TRANGTHAI'");


                    var existapprovec1 = dbConn.FirstOrDefault<ApproveLevel>(@"Select a.* From ApproveLevel a
                                                                                    INNER JOIN ApproveLevelUsers al
                                                                                    On a.id =  al.id_cap_duyet
                                                                                    WHERE a.cap_duyet = 'CAPDUYET1'
                                                                                    and al.ma_nguoi_dung = {0}".Params(currentUser.ma_nguoi_dung));

                    if (existapprovec1 != null)
                    {
                        ViewBag.isDuyetLevel1 = existapprovec1.ma_nhom_chuyen_mon;
                    }
                    else
                        ViewBag.isDuyetLevel1 = "";

                    var existapprovec2 = dbConn.FirstOrDefault<ApproveLevel>(@"Select a.* From ApproveLevel a
                                                                                    INNER JOIN ApproveLevelUsers al
                                                                                    On a.id =  al.id_cap_duyet
                                                                                    WHERE a.cap_duyet = 'CAPDUYET2'
                                                                                    and al.ma_nguoi_dung = {0}".Params(currentUser.ma_nguoi_dung));
                    if (existapprovec2 != null)
                    {
                        ViewBag.isDuyetLevel2 = existapprovec2.ma_nhom_chuyen_mon;
                    }
                    else
                        ViewBag.isDuyetLevel2 = "";

                    var existapprovec3CNTT = dbConn.FirstOrDefault<ApproveLevel>(@"Select a.* From ApproveLevel a
                                                                                    INNER JOIN ApproveLevelUsers al
                                                                                    on a.id =  al.id_cap_duyet
                                                                                    WHERE a.cap_duyet = 'CAPDUYET3' and a.ma_nhom_chuyen_mon = 'Y_KIEN_TTCN_NHDT'
                                                                                    and al.ma_nguoi_dung = {0}".Params(currentUser.ma_nguoi_dung));
                    if (existapprovec3CNTT != null)
                    {
                        ViewBag.isDuyetLevel3CNTT = existapprovec3CNTT.ma_nhom_chuyen_mon;
                    }
                    else
                        ViewBag.isDuyetLevel3CNTT = "";

                    var existapprovec3QLDVKH_NQT = dbConn.FirstOrDefault<ApproveLevel>(@"Select a.* From ApproveLevel a
                                                                                    INNER JOIN ApproveLevelUsers al
                                                                                    on a.id =  al.id_cap_duyet
                                                                                    WHERE a.cap_duyet = 'CAPDUYET3' and a.ma_nhom_chuyen_mon = 'Y_KIEN_QLDVKH_NQT'
                                                                                    and al.ma_nguoi_dung = {0}".Params(currentUser.ma_nguoi_dung));
                    if (existapprovec3QLDVKH_NQT != null)
                    {
                        ViewBag.isDuyetLevel3QLDVKH_NQ = existapprovec3QLDVKH_NQT.ma_nhom_chuyen_mon;
                    }
                    else
                        ViewBag.isDuyetLevel3QLDVKH_NQ = "";

                    var existapprovec3HO = dbConn.FirstOrDefault<ApproveLevel>(@"Select a.* From ApproveLevel a
                                                                                    INNER JOIN ApproveLevelUsers al
                                                                                    on a.id =  al.id_cap_duyet
                                                                                    WHERE a.cap_duyet = 'CAPDUYET3' and a.ma_nhom_chuyen_mon = 'Y_KIEN_KHAC_HO'
                                                                                    and al.ma_nguoi_dung = {0}".Params(currentUser.ma_nguoi_dung));
                    if (existapprovec3HO != null)
                    {
                        ViewBag.isDuyetLevel3HO = existapprovec3HO.ma_nhom_chuyen_mon;
                    }
                    else
                        ViewBag.isDuyetLevel3HO = "";

                    return View("ApprovePR");
                }
            }
            return RedirectToAction("NoAccessRights", "Error");
        }
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new List<PRequestHeader>();
                if (accessDetail.xem)
                {
                    if (request.Filters != null && request.Filters.Count > 0)
                    {
                        var where = KendoApplyFilter.ApplyFilter(request.Filters[0]);
                        data = dbConn.Select<PRequestHeader>(@"Select a.* From PRequestHeader a
                                                               Inner Join UsersBranch b on b.ma_chi_nhanh = a.ma_chi_nhanh
                                                               Where b.ma_nguoi_dung = '" + currentUser.ma_nguoi_dung + "' AND " + where);
                    }
                    else
                    {
                        data = dbConn.Select<PRequestHeader>(@"Select a.* From PRequestHeader a
                                                               Inner Join UsersBranch b on b.ma_chi_nhanh = a.ma_chi_nhanh
                                                               Where b.ma_nguoi_dung = '" + currentUser.ma_nguoi_dung + "'");

                    }
                }
                else
                {
                    return RedirectToAction("NoAccessRights", "Error");
                }
                return Json(data.ToDataSourceResult(request));
            }
        }
        public ActionResult ReadDetail([DataSourceRequest]DataSourceRequest request, string ma_phieu_header)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                if (accessDetail.xem)
                {
                    var data = new List<PRequestDetail>();
                    var branch = dbConn.Select<Product>("select ma_san_pham from Product where ma_nhom_san_pham in (select ma_phan_cap from UserProductCategory where ma_nguoi_dung ={0})", currentUser.ma_nguoi_dung);
                    if (branch.Count() > 0)
                    {
                        string inBranch = string.Empty;
                        inBranch = " AND ma_san_pham IN (" + String.Join(",", branch.Select(s => "'" + s.ma_san_pham + "'")) + ")";
                        data = dbConn.Select<PRequestDetail>("ma_phieu='" + ma_phieu_header + "'" + inBranch);
                    }
                    else
                    {
                        data = new List<PRequestDetail>();
                    }
                    return Json(data.ToDataSourceResult(request));
                }
                return RedirectToAction("NoAccessRights", "Error");
            }
        }


        public ActionResult GetVenderByProduct(string ma_san_pham)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = dbConn.SqlList<VendorModelView>(@"
                                SELECT DISTINCT data.nha_cung_cap_id,data.ten_nha_cung_Cap
                                FROM 
                                (SELECT a.nha_cung_cap_id,b.ten_nha_cung_Cap
                                FROM dbo.VendorProductCategory a LEFT JOIN dbo.Vendor b ON b.nha_cung_cap_id = a.nha_cung_cap_id
                                WHERE a.ma_phan_cap=
                                	(SELECT TOP 1 ISNULL(ma_nhom_san_pham,'') 
                                			FROM dbo.Product 
                                			WHERE ma_san_pham={0})
                                			) data 
                                		LEFT JOIN	dbo.ProductPriceHeader a ON data.nha_cung_cap_id = a.nha_cung_cap_id
                                		LEFT JOIN dbo.ProductPriceDetail b ON b.id = a.id
                                WHERE a.[trang_thai] ='DANG_HOAT_DONG'".Params(ma_san_pham));
                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetPriceByCGSVender(string ma_san_pham, int so_luong, string ma_nha_cung_cap)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = dbConn.SqlScalar<double>(@"
                                SELECT DISTINCT ISNULL(b.gia_bao_gom_vat,0) AS gia_bao_gom_vat
                                FROM 
                                (SELECT a.nha_cung_cap_id,b.ten_nha_cung_Cap
                                FROM dbo.VendorProductCategory a LEFT JOIN dbo.Vendor b ON b.nha_cung_cap_id = a.nha_cung_cap_id
                                WHERE a.ma_phan_cap=
	                                (SELECT TOP 1 ISNULL(ma_nhom_san_pham,'') 
			                                FROM dbo.Product 
			                                WHERE ma_san_pham={0})
			                                ) data 
		                                LEFT JOIN	dbo.ProductPriceHeader a ON data.nha_cung_cap_id = a.nha_cung_cap_id
		                                LEFT JOIN dbo.ProductPriceDetail b ON b.id = a.id
                                WHERE a.[trang_thai] ='DANG_HOAT_DONG' AND b.sl_min <= {1} AND b.sl_max >= {1}
                                AND a.nha_cung_cap_id={2} 
                                AND (CONVERT(DATE,b.ngay_ap_dung,101) <= convert(DATE,getdate(),101) AND 
                                CONVERT(DATE,b.ngay_ket_thuc,101)  >= convert(DATE,getdate(),101))".Params(ma_san_pham, so_luong, ma_nha_cung_cap));
                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult XacNhanTDV(string data, string note)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                if (accessDetail.sua)
                {
                    try
                    {
                        string[] separators = { "@@" };
                        var listdata = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var prid in listdata)
                        {
                            var prheader = dbConn.FirstOrDefault<PRequestHeader>(@"Select ma_phieu From PRequestHeader Where id = {0}", prid);
                            var ma_phieu = prheader.ma_phieu;
                            dbConn.Update<PRequestHeader>(set: "trang_thai='TDV_DA_XAC_NHAN', y_kien_cua_don_vi = {0}, ngay_duyet_TDV = GETDATE(), nguoi_duyet_TDV = {1}".Params(note, currentUser.ma_nguoi_dung), where: "id={0}".Params(prid));
                            dbConn.Update<PRequestDetail>(set: "trang_thai='TDV_DA_XAC_NHAN'", where: "ma_phieu={0}".Params(ma_phieu));
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
                    return Json(new { success = false, error = "Bạn không có quyền duyệt dữ liệu" });
                }
            }
        }
        public ActionResult XacNhanTonKho(string data, string note, string ykien)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                if (accessDetail.sua)
                {
                    try
                    {
                        string[] separators = { "@@" };
                        var listdata = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        var listykien = ykien.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        int i = 0;
                        foreach (var priddetail in listdata)
                        {
                            var prheader = dbConn.FirstOrDefault<PRequestDetail>(@"Select ma_phieu From PRequestDetail Where id = {0}", priddetail);
                            var ma_phieu = prheader.ma_phieu;

                            dbConn.Update<PRequestDetail>(set: "trang_thai='DA_XAC_NHAN_TON_KHO', noi_dung_xac_nhan_ton_kho = {0}".Params(listykien[i++]), where: "id={0}".Params(priddetail));
                            var det = dbConn.FirstOrDefault<PRequestDetail>(@"Select ma_phieu From PRequestDetail Where trang_thai != 'DA_XAC_NHAN_TON_KHO' and ma_phieu = {0}", ma_phieu);
                            if (det != null)
                            {
                                dbConn.Update<PRequestHeader>(set: "trang_thai='DA_XAC_NHAN_TON_KHO_MOT_PHAN'", where: "ma_phieu={0}".Params(ma_phieu));
                            }
                            else dbConn.Update<PRequestHeader>(set: "trang_thai='DA_XAC_NHAN_TON_KHO'", where: "ma_phieu={0}".Params(ma_phieu));

                            dbConn.Update<PRequestHeader>(set: "y_kien_HCQT={0}, ngay_duyet_HCQT = GETDATE(), nguoi_duyet_HCQT = {1}".Params(note, currentUser.ma_nguoi_dung), where: "ma_phieu={0}".Params(ma_phieu));
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
                    return Json(new { success = false, error = "Bạn không có quyền duyệt dữ liệu" });
                }
            }
        }
        public ActionResult DuyetCMCNTT(string data, string note, string ykien)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                if (accessDetail.sua)
                {
                    try
                    {
                        string[] separators = { "@@" };
                        var listykien = ykien.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        var listdata = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        int i = 0;
                        foreach (var priddetail in listdata)
                        {
                            var prheader = dbConn.FirstOrDefault<PRequestDetail>(@"Select ma_phieu From PRequestDetail Where id = {0}", priddetail);
                            var ma_phieu = prheader.ma_phieu;

                            dbConn.Update<PRequestDetail>(set: "trang_thai='CNTT_DA_DUYET_CHUYEN_MON', noi_dung_xac_nhan_cap_3 = {0}".Params(listykien[i++]), where: "id={0}".Params(priddetail));
                            var det = dbConn.FirstOrDefault<PRequestDetail>(@"Select ma_phieu From PRequestDetail Where trang_thai  not in ('CNTT_DA_DUYET_CHUYEN_MON','QLDVKH_DA_DUYET_CHUYEN_MON','HO_DA_DUYET_CHUYEN_MON') and ma_phieu = {0}", ma_phieu);
                            if (det != null)
                            {
                                dbConn.Update<PRequestHeader>(set: "trang_thai='CNTT_DA_DUYET_CHUYEN_MON'", where: "ma_phieu={0}".Params(ma_phieu));
                            }
                            else dbConn.Update<PRequestHeader>(set: "trang_thai='DA_DUYET'", where: "ma_phieu={0}".Params(ma_phieu));

                            dbConn.Update<PRequestHeader>(set: "y_kien_TTCNTT_NHDT={0}, ngay_duyet_TTCNTT_NHDT = GETDATE(), nguoi_duyet_TTCNTT_NHDT = {1}".Params(note, currentUser.ma_nguoi_dung), where: "ma_phieu={0}".Params(ma_phieu));
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
                    return Json(new { success = false, error = "Bạn không có quyền duyệt dữ liệu" });
                }
            }
        }
        public ActionResult DuyetCMQLDVKH(string data, string note, string ykien)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                if (accessDetail.sua)
                {
                    try
                    {
                        string[] separators = { "@@" };
                        var listdata = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        var listykien = ykien.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        int i = 0;
                        foreach (var priddetail in listdata)
                        {
                            var prheader = dbConn.FirstOrDefault<PRequestDetail>(@"Select ma_phieu From PRequestDetail Where id = {0}", priddetail);
                            var ma_phieu = prheader.ma_phieu;

                            dbConn.Update<PRequestDetail>(set: "trang_thai='QLDVKH_DA_DUYET_CHUYEN_MON', noi_dung_xac_nhan_cap_3 = {0}".Params(listykien[i++]), where: "id={0}".Params(priddetail));
                            var det = dbConn.FirstOrDefault<PRequestDetail>(@"Select ma_phieu From PRequestDetail Where trang_thai  not in ('CNTT_DA_DUYET_CHUYEN_MON','QLDVKH_DA_DUYET_CHUYEN_MON','HO_DA_DUYET_CHUYEN_MON') and ma_phieu = {0}", ma_phieu);
                            if (det != null)
                            {
                                dbConn.Update<PRequestHeader>(set: "trang_thai='QLDVKH_DA_DUYET_CHUYEN_MON'", where: "ma_phieu={0}".Params(ma_phieu));
                            }
                            else dbConn.Update<PRequestHeader>(set: "trang_thai='DA_DUYET'", where: "ma_phieu={0}".Params(ma_phieu));

                            dbConn.Update<PRequestHeader>(set: "y_kien_QLDVKH_NQT={0}, ngay_duyet_QLDVKH_NQT = GETDATE(), nguoi_duyet_QLDVKH_NQT = {1}".Params(note, currentUser.ma_nguoi_dung), where: "ma_phieu={0}".Params(ma_phieu));
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
                    return Json(new { success = false, error = "Bạn không có quyền duyệt dữ liệu" });
                }
            }
        }
        public ActionResult DuyetCMHO(string data, string note, string ykien)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                if (accessDetail.sua)
                {
                    try
                    {
                        string[] separators = { "@@" };
                        var listdata = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        var listykien = ykien.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        int i = 0;
                        foreach (var priddetail in listdata)
                        {
                            var prheader = dbConn.FirstOrDefault<PRequestDetail>(@"Select ma_phieu From PRequestDetail Where id = {0}", priddetail);
                            var ma_phieu = prheader.ma_phieu;

                            dbConn.Update<PRequestDetail>(set: "trang_thai='HO_DA_DUYET_CHUYEN_MON', noi_dung_xac_nhan_cap_3 = {0}".Params(listykien[i++]), where: "id={0}".Params(priddetail));
                            var det = dbConn.FirstOrDefault<PRequestDetail>(@"Select ma_phieu From PRequestDetail Where trang_thai  not in ('CNTT_DA_DUYET_CHUYEN_MON','QLDVKH_DA_DUYET_CHUYEN_MON','HO_DA_DUYET_CHUYEN_MON') and ma_phieu = {0}", ma_phieu);
                            if (det != null)
                            {
                                dbConn.Update<PRequestHeader>(set: "trang_thai='HO_DA_DUYET_CHUYEN_MON'", where: "ma_phieu={0}".Params(ma_phieu));
                            }
                            else dbConn.Update<PRequestHeader>(set: "trang_thai='DA_DUYET'", where: "ma_phieu={0}".Params(ma_phieu));

                            dbConn.Update<PRequestHeader>(set: "y_kien_khac_HO={0}, ngay_duyet_khac_HO = GETDATE(), nguoi_duyet_khac_HO = {1}".Params(note, currentUser.ma_nguoi_dung), where: "ma_phieu={0}".Params(ma_phieu));
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
                    return Json(new { success = false, error = "Bạn không có quyền duyệt dữ liệu" });
                }
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult UpdateDetail([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<PRequestDetail> items, string ma_phieu)
        {
            if (accessDetail.sua || accessDetail.them)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var row in items)
                            if (!string.IsNullOrEmpty(row.ma_san_pham))
                            {
                                var checkID = dbConn.FirstOrDefault<PRequestDetail>("ma_phieu={0} AND id={1}", ma_phieu, row.id);
                                if (checkID != null)
                                {
                                    row.id = checkID.id;
                                    row.so_luong_duyet = row.so_luong_duyet;
                                    row.nguoi_tao = checkID.nguoi_tao;
                                    row.ngay_tao = checkID.ngay_tao;
                                    row.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                                    row.ngay_cap_nhat = DateTime.Now;
                                    //row.trang_thai = "";
                                    dbConn.Update(row);
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

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UploadFile(string data_id)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var file = Request.Files["FileChuKy"];
                if (file != null)
                {
                    string[] separators = { "@@" };
                    var listdata = data_id.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    string destinationPath = Helpers.Upload.UploadFile("PR", file);
                    foreach (var prid in listdata)
                    {
                        dbConn.Update<PRequestHeader>(set: "file_chu_ki_TDV = {0}".Params(destinationPath), where: "id = {0}".Params(prid));
                    }
                }
                return Json(new { success = true, error = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ApproveLevel3(PRequestHeader data, List<PRequestDetail> details, string nhom_chuyen_mon, string note)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    foreach (var priddetail in details)
                    {
                        var prheader = dbConn.FirstOrDefault<PRequestDetail>(@"Select ma_phieu From PRequestDetail Where id = {0}", priddetail.id);
                        var ma_phieu = prheader.ma_phieu;

                        if (nhom_chuyen_mon == AllConstant.Y_KIEN_KHAC_HO)
                        {
                            dbConn.Update<PRequestDetail>(set: "trang_thai='HO_DA_DUYET_CHUYEN_MON', noi_dung_xac_nhan_cap_3 = {0},so_luong_duyet={1}".Params(priddetail.noi_dung_xac_nhan_cap_3,priddetail.so_luong_duyet), where: "id={0}".Params(priddetail.id));
                            dbConn.Update<PRequestHeader>(set: "y_kien_khac_HO={0}, ngay_duyet_khac_HO = GETDATE(), nguoi_duyet_khac_HO = {1}".Params(note, currentUser.ma_nguoi_dung), where: "ma_phieu={0}".Params(ma_phieu));
                        }

                        if (nhom_chuyen_mon == AllConstant.Y_KIEN_TTCN_NHDT)
                        {
                            dbConn.Update<PRequestDetail>(set: "trang_thai='CNTT_DA_DUYET_CHUYEN_MON', noi_dung_xac_nhan_cap_3 = {0},so_luong_duyet={1}".Params(priddetail.noi_dung_xac_nhan_cap_3, priddetail.so_luong_duyet), where: "id={0}".Params(priddetail.id));
                            dbConn.Update<PRequestHeader>(set: "y_kien_TTCNTT_NHDT={0}, ngay_duyet_TTCNTT_NHDT = GETDATE(), nguoi_duyet_TTCNTT_NHDT = {1}".Params(note, currentUser.ma_nguoi_dung), where: "ma_phieu={0}".Params(ma_phieu));
                        }

                        if (nhom_chuyen_mon == AllConstant.Y_KIEN_QLDVKH_NQT)
                        {

                            dbConn.Update<PRequestDetail>(set: "trang_thai='QLDVKH_DA_DUYET_CHUYEN_MON', noi_dung_xac_nhan_cap_3 = {0},so_luong_duyet={1}".Params(priddetail.noi_dung_xac_nhan_cap_3, priddetail.so_luong_duyet), where: "id={0}".Params(priddetail.id));
                            dbConn.Update<PRequestHeader>(set: "y_kien_QLDVKH_NQT={0}, ngay_duyet_QLDVKH_NQT = GETDATE(), nguoi_duyet_QLDVKH_NQT = {1}".Params(note, currentUser.ma_nguoi_dung), where: "ma_phieu={0}".Params(ma_phieu));
                        }

                        var det = dbConn.FirstOrDefault<PRequestDetail>(@"Select ma_phieu From PRequestDetail Where trang_thai  not in ('CNTT_DA_DUYET_CHUYEN_MON','QLDVKH_DA_DUYET_CHUYEN_MON','HO_DA_DUYET_CHUYEN_MON') and ma_phieu = {0}", ma_phieu);
                        if (det != null)
                        {
                            dbConn.Update<PRequestHeader>(set: "trang_thai='QLDVKH_DA_DUYET_CHUYEN_MON'", where: "ma_phieu={0}".Params(ma_phieu));
                        }
                        else dbConn.Update<PRequestHeader>(set: "trang_thai='DA_DUYET'", where: "ma_phieu={0}".Params(ma_phieu));

                        if (!string.IsNullOrEmpty(priddetail.ma_san_pham_thay_the) && priddetail.ma_san_pham_thay_the != priddetail.ma_san_pham)
                        {
                            var detail = dbConn.FirstOrDefault<PRequestDetail>(s => s.id == priddetail.id);
                            detail.ma_san_pham_thay_the = priddetail.ma_san_pham_thay_the;
                            detail.ma_nha_cung_cap = priddetail.ma_nha_cung_cap;
                            detail.don_gia_vat = priddetail.don_gia_vat;
                            detail.don_gia = priddetail.don_gia;
                            detail.thue_vat = priddetail.thue_vat;
                            detail.don_vi_tinh = priddetail.don_vi_tinh;
                            detail.thanh_tien = priddetail.thanh_tien;
                            detail.ma_chinh_sach_gia = priddetail.ma_chinh_sach_gia;
                            dbConn.Update(detail);
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
    }
}