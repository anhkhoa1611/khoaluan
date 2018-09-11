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
using HPSTD.Models;
using Kendo.Mvc.Extensions;

namespace HPSTD.Controllers
{
    [Authorize]
    public class UserProductCategoryController : CustomController
    {
        // GET: UserProductCategory
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listDonVi = dbConn.Select<Branch>();
                    ViewBag.listProductCategory = dbConn.Select<ProductCategory>();
                }
                return View();
            }
            return RedirectToAction("NoAccessRights", "Error");
        }

        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                if (accessDetail.xem)
                {
                    //var data = new List<User>();
                    var WhereCondition = "";
                    if (request.Filters != null)
                    {
                        if (request.Filters.Count > 0)
                        {
                            WhereCondition = " AND " + KendoApplyFilter.ApplyFilter(request.Filters[0], "");
                        }
                    }
                    var data = dbConn.SqlList<UserProductCategory>("EXEC p_Get_UserProductCategory 1,200,@WhereCondition", new { WhereCondition = WhereCondition });
                    return Json(data.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
                }
                return RedirectToAction("NoAccessRights", "Error");
            }
        }
        public ActionResult GetProductCategoryByID(int id_nguoi_dung)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = dbConn.GetFirstColumn<string>("SELECT ma_phan_cap FROM UserProductCategory where id_nguoi_dung =" + id_nguoi_dung);
                return Json(new { success = true, data = data, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public ActionResult CreateUpdate(UserProductCategory data)
        {
            try
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    if (data.lstma_phan_cap != null && data.lstma_phan_cap.Count() > 0)
                    {
                        foreach (string item in data.lstma_phan_cap)
                        {
                            var exist = dbConn.SingleOrDefault<UserProductCategory>("id_nguoi_dung={0} AND ma_phan_cap={1}", data.id_nguoi_dung, item);
                            if (exist == null)
                            {
                                var newdata = new UserProductCategory();
                                newdata.id_nguoi_dung = data.id_nguoi_dung;
                                newdata.ma_nguoi_dung = data.ma_nguoi_dung;
                                newdata.ma_phan_cap = item;
                                newdata.ngay_tao = DateTime.Now;
                                newdata.nguoi_tao = currentUser.ma_nguoi_dung;
                                newdata.ngay_cap_nhat = DateTime.Now;
                                newdata.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                                dbConn.Insert(newdata);
                            }
                        }
                        dbConn.Delete<UserProductCategory>("id_nguoi_dung={0} AND ma_phan_cap NOT IN(" + String.Join(",", data.lstma_phan_cap.Select(s => ("'" + s + "'"))) + ")", data.id_nguoi_dung);
                    }
                    else
                    {
                        dbConn.Delete<UserProductCategory>("id_nguoi_dung={0}", data.id_nguoi_dung);
                    }

                }
                return Json(new { success = true, data = data });
            }
            catch (Exception e)
            {
                return Json(new { success = false, error = e.Message });
            }
        }
    }
}