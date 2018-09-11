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
    public class ReportPRController : CustomController
    {
        // GET: ReportPR
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listProduct = dbConn.Select<Product>("select id,ma_san_pham,ten_san_pham from Product where ma_nhom_san_pham in (select ma_phan_cap from UserProductCategory where ma_nguoi_dung ={0})", currentUser.ma_nguoi_dung);
                    ViewBag.listVender = dbConn.Select<VendorModel>(@"SELECT nha_cung_cap_id,ten_nha_cung_cap FROM Vendor WHERE trang_thai='true' ");
                    ViewBag.listBranch = dbConn.Select<Branch>("Select * From Branch Where ma_chi_nhanh in (Select ma_chi_nhanh From UsersBranch Where ma_nguoi_dung = '" + currentUser.ma_nguoi_dung + "')");
                    ViewBag.listTrangThai = dbConn.Select<Parameters>("loai_tham_so='TRANGTHAI'");
                    return View("ReportPR");
                }
            }
            return RedirectToAction("NoAccessRights", "Error");
        }

        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new DataSourceResult();
                string str = @"
                        SELECT 
	                        d.id
	                        , h.ma_phieu
	                        , h.ma_chi_nhanh
	                        , h.ten_phieu
	                        , h.ngay_tao_yeu_cau

                            , h.y_kien_cua_don_vi
                            , h.ngay_duyet_TDV
                            , h.nguoi_duyet_TDV

                            , h.y_kien_HCQT
                            , h.ngay_duyet_HCQT
                            , h.nguoi_duyet_HCQT

                            , h.y_kien_TTCNTT_NHDT
                            , h.ngay_duyet_TTCNTT_NHDT
                            , h.nguoi_duyet_TTCNTT_NHDT

                            , h.y_kien_QLDVKH_NQT
                            , h.ngay_duyet_QLDVKH_NQT
                            , h.nguoi_duyet_QLDVKH_NQT

                            , h.y_kien_khac_HO
                            , h.ngay_duyet_khac_HO
                            , h.nguoi_duyet_khac_HO

	                        , d.ma_san_pham
	                        , d.so_luong
	                        , d.ma_nha_cung_cap
	                        , d.don_gia_vat
	                        , d.thong_so_ky_thuat
	                        , d.chuc_danh_nguoi_su_dung
	                        , d.ke_hoach_nam
	                        , d.trang_thai
                        FROM PRequestHeader h
                        INNER JOIN PRequestDetail d
                        ON h.ma_phieu = d.ma_phieu
                    ";

                if (request.Sorts == null)
                    request.Sorts = new List<Kendo.Mvc.SortDescriptor>();
                request.Sorts.Add(new Kendo.Mvc.SortDescriptor("ma_phieu", System.ComponentModel.ListSortDirection.Ascending));

                if (request.Filters == null)
                    request.Filters = new List<Kendo.Mvc.IFilterDescriptor>();
                data = KendoApplyFilter.KendoDataByQuery<ReportPR>(request, str, "");
                return Json(data);
            }
        }
    }
}