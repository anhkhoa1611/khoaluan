using HPSTD.Helpers;
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

namespace HPSTD.Controllers
{
    [Authorize]
    public class ArticleController : CustomController
    {
        // GET: Article
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listLoaiTin = dbConn.Select<Parameters>("loai_tham_so='LOAITINTUC'");
                    ViewBag.listsChiNhanh = dbConn.Select<Branch>();
                    return View();
                }
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
                data = KendoApplyFilter.KendoData<Article>(request);
                foreach(Article item in data.Data)
                {
                    item.danh_sach_chi_nhanh = dbConn.GetFirstColumn<string>("select ma_chi_nhanh from ArticleBranch where ma_tin_id =" + item.id);
                }
                return Json(data);
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateUpdate(Article data, HttpPostedFileBase file_dinh_kem)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    if (data.id > 0)
                    {
                        if (accessDetail.sua)
                        {
                            var exist = dbConn.SingleOrDefault<Article>("id={0}", data.id);
                            exist.ngay_cap_nhat = DateTime.Now;
                            exist.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            exist.trang_thai = data.trang_thai;
                            exist.tieu_de = !string.IsNullOrEmpty(data.tieu_de) ? data.tieu_de : "";
                            exist.noi_dung = !string.IsNullOrEmpty(data.noi_dung) ? data.noi_dung : "";
                            exist.loai_tin = !string.IsNullOrEmpty(data.loai_tin) ? data.loai_tin : "";
                            exist.mo_ta = !string.IsNullOrEmpty(data.mo_ta) ? data.mo_ta : "";
                            exist.xem_truoc = data.xem_truoc;
                            if (file_dinh_kem != null && file_dinh_kem.ContentLength > 0)
                            {
                                string pathForSaving = Server.MapPath("~/FileUpload/Article/");
                                if (!Directory.Exists(pathForSaving))
                                {
                                    Directory.CreateDirectory(pathForSaving);
                                }
                                if (file_dinh_kem.ContentLength > 5242880)
                                {
                                    return Json(new { success = false, error = "Vui lòng chọn file không được lớn hơn 5M" });
                                }
                                var filename = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + Locdau.ConvertFileName(file_dinh_kem.FileName);
                                file_dinh_kem.SaveAs(Path.Combine(pathForSaving, filename));
                                exist.file_dinh_kem = "/FileUpload/Article/" + filename;
                            }
                            dbConn.Update(exist,s=>s.id==exist.id);
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
                            var exist = dbConn.SingleOrDefault<Article>("id={0}", data.id);
                            if (exist != null)
                            {
                                return Json(new { success = false, error = "Mã sản phẩm đã tồn tại." });
                            }
                            data.ngay_tao = DateTime.Now;
                            data.nguoi_tao = currentUser.ma_nguoi_dung;
                            data.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            data.tieu_de = !string.IsNullOrEmpty(data.tieu_de) ? data.tieu_de : "";
                            data.noi_dung = !string.IsNullOrEmpty(data.noi_dung) ? data.noi_dung : "";
                            data.nguoi_cap_nhat = "";
                            if (file_dinh_kem != null && file_dinh_kem.ContentLength > 0)
                            {
                                string pathForSaving = Server.MapPath("~/FileUpload/Article/");
                                if (!Directory.Exists(pathForSaving))
                                {
                                    Directory.CreateDirectory(pathForSaving);
                                }
                                if (file_dinh_kem.ContentLength > 5242880)
                                {
                                    return Json(new { success = false, error = "Vui lòng chọn file không được lớn hơn 5M" });
                                }
                                var filename = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + Locdau.ConvertFileName(file_dinh_kem.FileName);
                                file_dinh_kem.SaveAs(Path.Combine(pathForSaving, filename));
                                data.file_dinh_kem = "/FileUpload/Article/" + filename;
                            }
                            dbConn.Insert(data);
                            int Id = (int)dbConn.GetLastInsertId();
                            data.id = Id;
                        }
                        else
                        {
                            return Json(new { success = false, error = "Bạn không có quyền thêm dữ liệu" });
                        }
                    }
                    dbConn.Delete<ArticleBranch>("ma_tin_id={0}", data.id);
                    if (data.ma_chi_nhanh != null && data.ma_chi_nhanh.Count() > 0)
                    {
                        foreach (var item in data.ma_chi_nhanh)
                        {
                            var articleBranch = new ArticleBranch();
                            articleBranch.ma_tin_id = data.id;
                            articleBranch.ma_chi_nhanh = item;
                            articleBranch.ngay_tao = DateTime.Now;
                            articleBranch.nguoi_tao = currentUser.ma_nguoi_dung;
                            dbConn.Insert(articleBranch);
                        }
                    }

                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
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
                        dbConn.Delete<Article>("id={0}", id);
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
                        dbConn.Delete<Core.Entities.Article>("id={0}", item);
                    }
                    return Json(new { success = true });
                }
            }
            else
            {
                return Json(new { success = false, error = "Bạn không có quyền xóa dữ liệu" });
            }
        }
    }
}