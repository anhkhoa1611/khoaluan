﻿using HPSTD.Helpers;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ServiceStack.OrmLite;
using HPSTD.Core.Entities;
using System.IO;
using OfficeOpenXml;
using System.Collections;
using Kendo.Mvc.Extensions;

namespace HPSTD.Controllers
{
    [Authorize]
    public class EmailConfigController : CustomController
    {
        // GET: Article
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                return View("EmailConfig");
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
                data = KendoApplyFilter.KendoData<Emails>(request);
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
                        dbConn.Delete<Core.Entities.Emails>("id={0}", item);
                    }
                    return Json(new { success = true });
                }
            }
            else
            {
                return Json(new { success = false, error = "Bạn không có quyền xóa dữ liệu" });
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.Emails> items)
        {
            if (accessDetail.them)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            try
                            {

                                var exist = dbConn.SingleOrDefault<Emails>("email={0}", item.email);
                                if (exist != null)
                                {
                                    ModelState.AddModelError("", "Email này đã tồn tại trong hệ thống.");
                                    return Json(items.ToDataSourceResult(request, ModelState));
                                }

                                if (string.IsNullOrEmpty(item.email))
                                {
                                    ModelState.AddModelError("", "Email không được để trống. Vui lòng nhập email.");
                                    return Json(items.ToDataSourceResult(request, ModelState));
                                }
                                item.ngay_tao = DateTime.Now;
                                item.nguoi_tao = currentUser.ma_nguoi_dung;
                                item.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                                item.nguoi_cap_nhat = "";
                                dbConn.Insert(item);
                            }
                            catch (Exception ex)
                            {
                                ModelState.AddModelError("", ex.Message);
                                return Json(items.ToDataSourceResult(request, ModelState));
                            }
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
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.Emails> items)
        {
            if (accessDetail.sua)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            var exist = dbConn.SingleOrDefault<Emails>("id={0}", item.id);
                            {
                                var checkDup = dbConn.SingleOrDefault<Emails>("email={0} and id != {1}", item.email, item.id);
                                if (checkDup != null)
                                {
                                    ModelState.AddModelError("", "Email này đã tồn tại trong hệ thống.");
                                    return Json(items.ToDataSourceResult(request, ModelState));
                                }
                                else
                                {
                                    exist.email = item.email;
                                }
                            }
                            exist.password = item.password;
                            exist.mail_server = item.mail_server;
                            exist.port = item.port;
                            exist.enable_ssl = item.enable_ssl;
                            exist.is_default = item.is_default;
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
    }
}