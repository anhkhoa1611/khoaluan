using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ServiceStack.OrmLite;
using HPSTD.Core.Entities;
using HPSTD.Helpers;
using Kendo.Mvc.UI;

namespace HPSTD.Controllers
{
    [Authorize]
    public class ReportPlanController : CustomController
    {
        // GET: ReportPlan
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                return View();
            }
            return RedirectToAction("NoAccessRights", "Error");
        }
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new DataSourceResult();
                string str = @"
                        select 
	                        b.ten_chi_nhanh
	                        ,h.nam_ke_hoach
	                        ,isnull(d.so_luong_du_kien, 0) so_luong_du_kien
	                        ,isnull(d.ten_san_pham, '') ten_san_pham
	                        ,isnull(d.total_tien_du_kien, 0) total_tien_du_kien
	                        ,isnull(d.ke_hoach_nam_truoc, 0) ke_hoach_nam_truoc
	                        ,isnull(d.thuc_hien_nam_truoc, 0) thuc_hien_nam_truoc
	                        ,isnull(d.chech_lech, 0) chech_lech
                        from PlanHeader h 
                        left join Branch b on b.ma_chi_nhanh = h.don_vi_phu_trach
                        left join (
	                        select
		                        d.nam_ke_hoach
		                        ,p.ten_san_pham
		                        ,sum(so_luong_du_kien) as so_luong_du_kien
		                        ,sum(total_tien_du_kien) as total_tien_du_kien
		                        ,sum(ke_hoach_nam_truoc) as ke_hoach_nam_truoc
		                        ,sum(thuc_hien_nam_truoc) as thuc_hien_nam_truoc
		                        ,sum(chech_lech) as chech_lech
	                        from PlanDetail d 
	                        left join Product p on p.id = d.ma_san_pham
	                        group by ten_san_pham, nam_ke_hoach
                        ) d on d.nam_ke_hoach = h.id
                    ";
                data = KendoApplyFilter.KendoDataByQuery<ReportPlan>(request, str, "");
                return Json(data);
            }
        }
    }
}