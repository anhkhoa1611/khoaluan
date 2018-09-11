using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HPSTD.Core.Entities;
using ServiceStack.OrmLite;
using Kendo.Mvc.Extensions;

namespace HPSTD.Controllers
{
    public class ActivityLogsController : CustomController
    {
        // GET: ActivityLogs
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                   // ViewBag.listCNPBPGD = dbConn.Select<Branch>();
                    //ViewBag.listMenu = dbConn.Select<Core.Entities.Menu>("Select * From Menu Where cap = 0 Order by thu_tu");
                    return View("ActivityLogs");
                }
            }
            return RedirectToAction("NoAccessRights", "Error");

        }
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new List<ActivityLogs>();
                if (accessDetail.xem)
                {
                    if (currentUser.ma_nguoi_dung == "administrator")
                    {
                        String query = "select ActivityLogs.*, [User].ten_nguoi_dung, Screen.ten_man_hinh from ActivityLogs, [User], Screen where [User].ma_nguoi_dung = ActivityLogs.ma_nguoi_dung and Screen.ma_man_hinh = ActivityLogs.ma_man_hinh order by id DESC";
                        data = dbConn.Select<ActivityLogs>(query);
                    }
                    else
                    {
                        String query = "select ActivityLogs.*, [User].ten_nguoi_dung, Screen.ten_man_hinh from ActivityLogs, [User], Screen where [User].ma_nguoi_dung = ActivityLogs.ma_nguoi_dung and Screen.ma_man_hinh = ActivityLogs.ma_man_hinh and ActivityLogs.ma_nguoi_dung = '" + currentUser.ma_nguoi_dung + "' order by id DESC";
                        data = dbConn.Select<ActivityLogs>(query);
                    }
                }
                return Json(data.ToDataSourceResult(request));
            }
        }
        public static void CreateLogs(string ma_nguoi_dung, string ma_man_hinh, string thao_tac, string chi_tiet)
        {
            try
            {
                var data = new ActivityLogs();
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    data.ma_nguoi_dung = ma_nguoi_dung;
                    data.nguoi_tao = ma_nguoi_dung;
                    data.ma_man_hinh = ma_man_hinh;
                    data.thao_tac = thao_tac;
                    data.chi_tiet = chi_tiet;
                    data.ngay_tao = DateTime.Now;
                    dbConn.Insert(data);

                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}