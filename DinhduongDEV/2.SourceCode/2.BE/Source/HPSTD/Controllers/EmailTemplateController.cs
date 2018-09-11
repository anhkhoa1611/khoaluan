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
    public class EmailTemplateController : CustomController
    {
        // GET: EmailTemplate
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listEmailTemplate = dbConn.Select<Parameters>("loai_tham_so= 'EMAILTEMPLATE'");
                    return View();
                }
            }
            return RedirectToAction("NoAccessRights", "Error");
        }
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new DataSourceResult();
                data = KendoApplyFilter.KendoData<EmailContent>(request);
                return Json(data);
            }
        }
        public ActionResult CreateUpdate(EmailContent data)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    if (data.id > 0)
                    {
                        if (accessDetail.sua)
                        {
                            var exist = dbConn.SingleOrDefault<EmailContent>("id={0}", data.id);
                            exist.ngay_cap_nhat = DateTime.Now;
                            exist.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            exist.tieu_de = data.tieu_de;
                            exist.mailTo = data.mailTo;
                            exist.mailCc = !string.IsNullOrEmpty(data.mailCc) ? data.mailCc : "";
                            exist.mailBcc = !string.IsNullOrEmpty(data.mailBcc) ? data.mailBcc : "";
                            exist.mau_email = !string.IsNullOrEmpty(data.mau_email) ? data.mau_email : "";
                            exist.noi_dung = !string.IsNullOrEmpty(data.noi_dung) ? data.noi_dung : "";
                            dbConn.Update(exist);
                        }
                        else
                        {
                            return Json(new { success = false, error = "Bạn không có quyền cập nhật dữ liệu" });
                        }
                    }
                    else
                    {
                        if (accessDetail.them)
                        {
                            data.ngay_tao = DateTime.Now;
                            data.nguoi_tao = currentUser.ma_nguoi_dung;
                            data.nguoi_cap_nhat = "";
                            data.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            dbConn.Insert(data);
                        }
                        else
                        {
                            return Json(new { success = false, error = "Bạn không có quyền thêm dữ liệu" });
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
        public ActionResult Delete(int id)
        {
            if (accessDetail.xoa)
            {
                try
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        dbConn.Delete<EmailContent>("id={0}", id);
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