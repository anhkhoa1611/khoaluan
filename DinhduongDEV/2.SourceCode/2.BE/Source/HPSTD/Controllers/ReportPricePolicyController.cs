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
    public class ReportPricePolicyController : CustomController
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
                return View("ReportPricePolicy");
            }
            return RedirectToAction("NoAccessRights", "Error");
        }
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new DataSourceResult();
                var query = @"SELECT  
	                           Detail.*,
                               P.nha_cung_cap_id,
	                           V.ten_nha_cung_Cap, 
	                           V.dien_thoai, 
	                           P.so_hop_dong,
                               V.email, 
	                           P.ghi_chu

                            FROM ProductPriceHeader P LEFT JOIN Vendor V ON P.nha_cung_cap_id = V.nha_cung_cap_id
	                        LEFT JOIN dbo.ProductPriceDetail Detail ON Detail.ma_chinh_sach_gia = P.ma_chinh_sach_gia
                            ";
                if (accessDetail.xem)
                {
                    if (request.Filters.Any())
                    {
                        var where = KendoApplyFilter.ApplyFilter(request.Filters[0]);
                        data = KendoApplyFilter.KendoDataByQuery<ProductPriceReport>(request, query, where);
                    }
                    else
                    {
                        data = KendoApplyFilter.KendoDataByQuery<ProductPriceReport>(request, query, "");
                    }
                }
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }
      
    }
}