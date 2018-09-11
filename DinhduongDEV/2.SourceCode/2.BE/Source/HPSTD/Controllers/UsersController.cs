using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HPSTD.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using HPSTD.Helpers;
using System.Data;
using System.IO;
using HPSTD.Core.Entities;
using OfficeOpenXml;
using System.Globalization;
using OfficeOpenXml.Style;

namespace HPSTD.Controllers
{
    [Authorize]
    public class UsersController : CustomController
    {
        //
        // GET: /Users/
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listCNPBPGD = dbConn.Select<Branch>();
                    ViewBag.listDonVi = dbConn.Select<Branch>("Select distinct ma_don_vi From Branch");
                    ViewBag.listProfitCenter = dbConn.Select<Parameters>("loai_tham_so = {0}".Params(AllConstant.PROFIT_CENTER));
                    ViewBag.listChucDanh = dbConn.Select<Parameters>("loai_tham_so ='CHUCDANH' And trang_thai ='true'");
                    ViewBag.listGender = dbConn.Select<Parameters>("loai_tham_so ='GIOITINH' And trang_thai ='true'");
                    ViewBag.listChucVu = dbConn.Select<Parameters>("loai_tham_so ='CHUCVU' And trang_thai ='true'");
                   // ViewBag.listBCDinhBien = dbConn.Select<Parameters>("loai_tham_so ='BCDINHBIEN' And trang_thai ='true'");
                    ViewBag.listGroup = dbConn.Select<Group>();

                }
                return View("Users");
            }
            else
            {
                return RedirectToAction("NoAccessRights", "Error");
            }
        }

        public ActionResult Active(string data)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    string[] separators = { "@@" };
                    var listid = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var id in listid)
                    {
                        var item = dbConn.FirstOrDefault<User>("id={0}", id);
                        if (accessDetail.sua)
                        {
                            item.trang_thai = "true";
                            item.ngay_cap_nhat = DateTime.Now;
                            item.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            dbConn.Update(item);
                        }
                    }
                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    return Json(new { success = false, error = e.Message });
                }
            }
        }
        public ActionResult Inactive(string data)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    string[] separators = { "@@" };
                    var listid = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var id in listid)
                    {
                        var item = dbConn.FirstOrDefault<User>("id={0}", id);
                        if (accessDetail.sua)
                        {
                            item.trang_thai = "false";
                            item.ngay_cap_nhat = DateTime.Now;
                            item.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            dbConn.Update(item);
                        }
                    }
                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    return Json(new { success = false, error = e.Message });
                }
            }
        }
        public ActionResult ResetPass(string data)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    string[] separators = { "@@" };
                    var listid = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var id in listid)
                    {
                        if (accessDetail.sua)
                        {
                            var exist = dbConn.SingleOrDefault<User>("id={0}", id);
                            if (exist.email != null)
                            {
                                Random r = new Random();
                                var random = r.Next(0, 1000000).ToString("D6");
                                exist.mat_khau = Helpers.GetMd5Hash.Generate(exist.ma_nguoi_dung + random);
                                exist.ngay_cap_nhat = DateTime.Now;
                                exist.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                                dbConn.Update(exist);
                                string rootUrl = default(string);
                                rootUrl = Request.Url.Scheme + "://" + Request.Url.Authority + VirtualPathUtility.ToAbsolute("~/");
                                SendMail.SendEmail(exist.email, "ResetPassword", exist.ma_nguoi_dung, exist.ma_nguoi_dung + random, rootUrl, exist.ten_nguoi_dung);
                            }
                            else
                            {
                                return Json(new { success = false, error = "Email không tồn tại. Vui lòng kiểm tra lại" });
                            }
                        }
                    }
                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    return Json(new { success = false, error = e.Message });
                }
            }
        }
        //CanhLV
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new List<User>();
                if (accessDetail.xem)
                {
                    if (request.Filters.Any())
                    {
                        var where = KendoApplyFilter.ApplyFilter(request.Filters[0]);
                        data = dbConn.Select<User>(@" Select * From 
                                                      (
                                                        Select  *
                                                                    ,ISNULL(STUFF ((
                                                                    Select DISTINCT ',' + a.ma_chi_nhanh
                                                                    From UsersBranch a
                                                                        Inner Join Branch b ON a.ma_chi_nhanh = b.ma_chi_nhanh
                                                                    Where a.ma_nguoi_dung = usr.ma_nguoi_dung
                                                                    For XML Path('') 
                                                                    ), 1, 1, ''),'') AS ma_chi_nhanh
                                                            ,ISNULL(STUFF ((
                                                                    Select DISTINCT ',' + b.ten_chi_nhanh
                                                                    From UsersBranch a
                                                                        Inner Join Branch b ON a.ma_chi_nhanh = b.ma_chi_nhanh
                                                                    Where a.ma_nguoi_dung = usr.ma_nguoi_dung
                                                                    For XML Path('') 
                                                                    ), 1, 1, ''),'') AS ten_chi_nhanh
                                                        From [User] usr
                                                       ) data Where " + where
                                                   );
                    }
                    else
                    {
                        data = dbConn.Select<User>(@"Select  *
                                                        ,ISNULL(STUFF ((
                                                                Select DISTINCT ',' + a.ma_chi_nhanh
                                                                From UsersBranch a
                                                                    Inner Join Branch b ON a.ma_chi_nhanh = b.ma_chi_nhanh
                                                                Where a.ma_nguoi_dung = usr.ma_nguoi_dung
                                                                For XML Path('') 
                                                                ), 1, 1, ''),'') AS ma_chi_nhanh
                                                         ,ISNULL(STUFF ((
                                                                Select DISTINCT ',' + b.ten_chi_nhanh
                                                                From UsersBranch a
                                                                   Inner Join Branch b ON a.ma_chi_nhanh = b.ma_chi_nhanh
                                                                Where a.ma_nguoi_dung = usr.ma_nguoi_dung
                                                                For XML Path('') 
                                                                ), 1, 1, ''),'') AS ten_chi_nhanh
                                                From [User] usr"
                                                    );
                    }
                }
                return Json(data.ToDataSourceResult(request));
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateUpdate(User data)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    if (data.id > 0)
                    {
                        if (accessDetail.sua)
                        {
                            var exist = dbConn.SingleOrDefault<User>("id ={0}", data.id);
                            exist.dia_chi = !string.IsNullOrEmpty(data.dia_chi) ? data.dia_chi : "";
                            exist.dien_thoai = !string.IsNullOrEmpty(data.dien_thoai) ? data.dien_thoai : "";
                            exist.ghi_chu = !string.IsNullOrEmpty(data.ghi_chu) ? data.ghi_chu : "";
                            exist.ten_nguoi_dung = data.ten_nguoi_dung;
                            exist.domain = data.domain;
                            exist.trang_thai = data.trang_thai;
                            exist.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            exist.ngay_cap_nhat = DateTime.Now;
                            exist.gioi_tinh = !string.IsNullOrEmpty(data.gioi_tinh) ? data.gioi_tinh : "";
                            dbConn.Update(exist);
                        }
                        else
                        {
                            return Json(new { success = false, error = "Bạn không có quyền cập nhập tài khoản" });
                        }
                    }
                    else
                    {
                        if (accessDetail.them)
                        {
                            var exist = dbConn.SingleOrDefault<User>("ma_nguoi_dung={0} or email={1}", data.ma_nguoi_dung, data.email);
                            var userproductcate = dbConn.Select<ProductCategory>("Select id ,isnull(ma_phan_cap,'') as ma_phan_cap , isnull(ten_phan_cap,'') as ten_phan_cap From [ProductCategory]");
                            var product_user = new UserProductCategory();
                            if (exist != null)
                            {
                                return Json(new { success = false, error = "Mã người dùng hoặc email đã tồn tại." });
                            }
                           
                            data.mat_khau = Helpers.GetMd5Hash.Generate(data.mat_khau);
                            data.ngay_tao = DateTime.Now;
                            data.nguoi_tao = currentUser.ma_nguoi_dung;
                            data.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            data.dia_chi = !string.IsNullOrEmpty(data.dia_chi) ? data.dia_chi : "";
                            data.dien_thoai = !string.IsNullOrEmpty(data.dien_thoai) ? data.dien_thoai : "";
                            data.ghi_chu = !string.IsNullOrEmpty(data.ghi_chu) ? data.ghi_chu : "";
                            dbConn.Insert(data);
                            data.id = (int)dbConn.GetLastInsertId();
                            int id = data.id;
                            foreach (var itemuser in userproductcate)
                            {
                                product_user.id_nguoi_dung = id;
                                product_user.ma_nguoi_dung = data.ma_nguoi_dung;
                                product_user.ma_phan_cap = itemuser.ma_phan_cap;
                                product_user.ngay_tao = DateTime.Now;
                                product_user.nguoi_tao = currentUser.ma_nguoi_dung;
                                product_user.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                                product_user.nguoi_cap_nhat = "";
                                dbConn.Insert(product_user);
                            }
                        }
                        else
                        {
                            return Json(new { success = false, error = "Bạn không có quyền tạo tài khoản" });
                        }

                    }

                    dbConn.Delete<UserInGroup>("ma_nguoi_dung={0}", data.ma_nguoi_dung);
                    if (data.groupId != null && data.groupId.Count() > 0)
                    {
                        foreach (var item in data.groupId)
                        {
                            var exist = dbConn.SingleOrDefault<UserInGroup>("ma_nguoi_dung={0} AND id_nhom_nguoi_dung={1}", data.ma_nguoi_dung, item);
                            if (exist == null)
                            {
                                var userInGroup = new UserInGroup();
                                userInGroup.ma_nguoi_dung = data.ma_nguoi_dung;
                                userInGroup.id_nhom_nguoi_dung = item;
                                userInGroup.ngay_tao = DateTime.Now;
                                userInGroup.nguoi_tao = currentUser.ma_nguoi_dung;
                                userInGroup.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                                dbConn.Insert(userInGroup);
                            }
                        }
                        //dbConn.Delete<UserInGroup>("ma_nguoi_dung = {0} AND id_nhom_nguoi_dung NOT IN (" + String.Join(",", data.groupId.Select(s => s)) + ")", data.ma_nguoi_dung);
                    }

                    dbConn.Delete<UsersBranch>("id_nguoi_dung={0}", data.id);
                    if (data.lstbranch != null)
                    {
                        foreach (string item in data.lstbranch)
                        {
                            var newdata = new UsersBranch();
                            newdata.id_nguoi_dung = data.id;
                            newdata.ma_nguoi_dung = data.ma_nguoi_dung;
                            newdata.ma_chi_nhanh = item;
                            newdata.ngay_tao = DateTime.Now;
                            newdata.nguoi_tao = currentUser.ma_nguoi_dung;
                            newdata.ngay_cap_nhat = DateTime.Now;
                            newdata.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            dbConn.Insert(newdata);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = ex.Message });
                }

                return Json(new { success = true });

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
                        dbConn.Delete<User>("id={0}", id);
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
        public ActionResult SaveImage(IEnumerable<HttpPostedFileBase> files)
        {
            // The Name of the Upload component is "files"
            if (files != null)
            {
                foreach (var file in files)
                {
                    // Some browsers send file names with full path.
                    // We are only interested in the file name.
                    var fileName = Path.GetFileName(file.FileName);
                    var physicalPath = Path.Combine(Server.MapPath("~/Content/image/upload/"), fileName);

                    // The files are not actually saved in this demo
                    TempData["fileName"] = fileName;
                    file.SaveAs(physicalPath);
                }
            }
            // Return an empty string to signify success
            return Content("");
        }

        public ActionResult Import()
        {
            var file = Request.Files["FileUpload"];
            try
            {
                if (file == null || file.ContentLength == 0) return Json(new { success = false, message = "File rỗng." });
                var fileExtension = System.IO.Path.GetExtension(file.FileName);
                if (fileExtension != ".xlsx" && fileExtension != ".xls") return Json(new { success = false, message = "File không không đúng định dạng excel *.xlsx,*.xls" });
                string datetime = DateTime.Now.ToString("yyyyMMddHHmmss");
                string fileLocation = string.Format("{0}/{1}", Server.MapPath("~/ExcelImport"), "[" + currentUser.ma_nguoi_dung + "-" + datetime + file.FileName);
                string errorFileLocation = string.Format("{0}/{1}", Server.MapPath("~/ExcelImport"), "[" + currentUser.ma_nguoi_dung + "-" + datetime + "-Error]" + file.FileName);
                string linkerror = "[" + currentUser.ma_nguoi_dung + "-" + datetime + "-Error]" + file.FileName;
                if (!System.IO.Directory.Exists(Server.MapPath("~/ExcelImport")))
                {
                    System.IO.Directory.CreateDirectory(fileLocation);
                }
                if (System.IO.File.Exists(fileLocation)) System.IO.File.Delete(fileLocation);

                file.SaveAs(fileLocation);

                var rownumber = 2;
                var total = 0;
                var error = 0;
                FileInfo fileInfo = new FileInfo(fileLocation);
                var excelPkg = new ExcelPackage(fileInfo);
                FileInfo template = new FileInfo(Server.MapPath(@"~\ExportExcelFile\User_Template.xlsx"));
                template.CopyTo(errorFileLocation);
                FileInfo _fileInfo = new FileInfo(errorFileLocation);
                var _excelPkg = new ExcelPackage(_fileInfo);
                ExcelWorksheet oSheet = excelPkg.Workbook.Worksheets["Data"];
                ExcelWorksheet eSheet = _excelPkg.Workbook.Worksheets["Data"];
                int totalRows = oSheet.Dimension.End.Row;
                using (var dbConn = OrmliteConnection.openConn())
                {
                    for (int i = 2; i <= totalRows; i++)
                    {
                        if (oSheet.Cells[i, 1].Value == null)
                        {
                            i = totalRows;
                        }
                        else
                        {
                            string ma_nguoi_dung = oSheet.Cells[i, 1].Value != null ? oSheet.Cells[i, 1].Value.ToString().Trim() : "";
                            string domain = oSheet.Cells[i, 2].Value != null ? oSheet.Cells[i, 2].Value.ToString().Trim() : "";
                            string ma_nguoi_dung_domain = oSheet.Cells[i, 3].Value != null ? oSheet.Cells[i, 3].Value.ToString().Trim() : "";
                            string ma_nhan_vien = oSheet.Cells[i, 4].Value != null ? oSheet.Cells[i, 4].Value.ToString().Trim() : "";
                            string ten_nguoi_dung = oSheet.Cells[i, 5].Value != null ? oSheet.Cells[i, 5].Value.ToString().Trim() : "";
                            string gioi_tinh = oSheet.Cells[i, 6].Value != null ? oSheet.Cells[i, 6].Value.ToString().Split('-')[0].ToString().Trim() : "";
                            string email = oSheet.Cells[i, 7].Value != null ? oSheet.Cells[i, 7].Value.ToString().Trim() : "";
                            string dien_thoai = oSheet.Cells[i, 8].Value != null ? oSheet.Cells[i, 8].Value.ToString().Trim() : "";
                            string trang_thai = "true";
                            if (oSheet.Cells[i, 9].Value.ToString() == "Hoạt động")
                            {
                                trang_thai = "true";
                            }
                            else if (oSheet.Cells[i, 9].Value.ToString() == "Ngưng hoạt động")
                            {
                                trang_thai = "false";
                            }
                            string don_vi = oSheet.Cells[i, 10].Value != null ? oSheet.Cells[i, 10].Value.ToString().Trim() : "";
                            string don_vi_quan_ly = oSheet.Cells[i, 11].Value != null ? oSheet.Cells[i, 11].Value.ToString().Split('-')[0].ToString().Trim() : "";
                            string profit = oSheet.Cells[i, 12].Value != null ? oSheet.Cells[i, 12].Value.ToString().Split('-')[0].ToString().Trim() : "";
                            string chuc_danh = oSheet.Cells[i, 13].Value != null ? oSheet.Cells[i, 13].Value.ToString().Split('-')[0].ToString().Trim() : "";
                            string cap_bac = oSheet.Cells[i, 14].Value != null ? oSheet.Cells[i, 14].Value.ToString().Split('-')[0].ToString().Trim() : "";
                            string bc_dinh_bien = oSheet.Cells[i, 15].Value != null ? oSheet.Cells[i, 15].Value.ToString().Split('-')[0].ToString().Trim() : "";
                            string dia_chi = oSheet.Cells[i, 16].Value != null ? oSheet.Cells[i, 16].Value.ToString().Trim() : "";
                            string ghi_chu = oSheet.Cells[i, 17].Value != null ? oSheet.Cells[i, 17].Value.ToString().Trim() : "";
                            string ngay_vao_lam = oSheet.Cells[i, 18].Value != null ? oSheet.Cells[i, 18].Value.ToString().Trim() : "";
                            don_vi_quan_ly = don_vi_quan_ly.Replace(" ", "");

                            try
                            {
                                var itemeexit = dbConn.FirstOrDefault<User>(s => s.ma_nguoi_dung == ma_nguoi_dung);
                                var exituserBrand = dbConn.FirstOrDefault<UsersBranch>(s => s.ma_nguoi_dung == ma_nguoi_dung);
                                var itemubr = new UsersBranch();
                                var listBranch = dbConn.Select<Branch>();
                                if (itemeexit != null)
                                {
                                    itemeexit.domain = domain;
                                   
                                    itemeexit.ten_nguoi_dung = ten_nguoi_dung;
                                    itemeexit.gioi_tinh = gioi_tinh;
                                    itemeexit.email = email;
                                    itemeexit.dien_thoai = dien_thoai;
                                    itemeexit.trang_thai = trang_thai;
                                   
                                    itemeexit.dia_chi = dia_chi;
                                    itemeexit.ghi_chu = ghi_chu;
                                 
                                    itemeexit.ngay_cap_nhat = DateTime.Now;
                                    itemeexit.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                                    if (exituserBrand != null)
                                    {
                                        
                                      
                                        
                                        dbConn.Update<User>(itemeexit);
                                        dbConn.Update<UsersBranch>(exituserBrand);
                                    }
                                    else
                                    {
                                        itemubr.id_nguoi_dung = itemeexit.id;
                                        itemubr.ma_nguoi_dung = ma_nguoi_dung;
                                        itemubr.ma_chi_nhanh = don_vi_quan_ly;
                                        itemubr.ngay_tao = DateTime.Now;
                                        itemubr.nguoi_tao = currentUser.ma_nguoi_dung;
                                        itemubr.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                                        itemubr.nguoi_cap_nhat = "";
                                        dbConn.Insert<UsersBranch>(itemubr);
                                    }

                                }
                                else
                                {
                                    var item = new User();
                                    var product_user = new UserProductCategory();
                                    var userproductcate = dbConn.Select<ProductCategory>("Select id ,isnull(ma_phan_cap,'') as ma_phan_cap , isnull(ten_phan_cap,'') as ten_phan_cap From [ProductCategory]");
                                    item.ma_nguoi_dung = ma_nguoi_dung;
                                    item.domain = domain;
                                 
                                    item.ten_nguoi_dung = ten_nguoi_dung;
                                    item.gioi_tinh = gioi_tinh;
                                    item.email = email;
                                    item.dien_thoai = dien_thoai;
                                    item.mat_khau = Helpers.GetMd5Hash.Generate(ma_nguoi_dung + "@123");
                                    item.trang_thai = trang_thai;
                                  
                                    item.dia_chi = dia_chi;
                                    item.ghi_chu = ghi_chu;
                                 
                                    item.ngay_tao = DateTime.Now;
                                    item.nguoi_tao = currentUser.ma_nguoi_dung;
                                    item.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                                    item.nguoi_cap_nhat = "";
                                    
                                    dbConn.Insert<User>(item);
                                    int id = (int)dbConn.GetLastInsertId();
                                    itemubr.id_nguoi_dung = id;
                                    itemubr.ma_nguoi_dung = ma_nguoi_dung;
                                  
                                        itemubr.ma_chi_nhanh = don_vi_quan_ly;
                                        itemubr.ngay_tao = DateTime.Now;
                                        itemubr.nguoi_tao = currentUser.ma_nguoi_dung;
                                        itemubr.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                                        itemubr.nguoi_cap_nhat = "";
                                        dbConn.Insert<UsersBranch>(itemubr);
                                  

                                    foreach (var itemuser in userproductcate)
                                    {
                                        product_user.id_nguoi_dung = id;
                                        product_user.ma_nguoi_dung = item.ma_nguoi_dung;
                                        product_user.ma_phan_cap = itemuser.ma_phan_cap;
                                        product_user.ngay_tao = DateTime.Now;
                                        product_user.nguoi_tao = currentUser.ma_nguoi_dung;
                                        product_user.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                                        product_user.nguoi_cap_nhat = "";
                                        dbConn.Insert(product_user);
                                    }
                                    total++;
                                    rownumber++;
                                }
                                total++;
                                rownumber++;
                            }
                            catch (Exception e)
                            {
                                eSheet.Cells[rownumber, 1].Value = ma_nguoi_dung;
                                eSheet.Cells[rownumber, 2].Value = domain;
                                eSheet.Cells[rownumber, 3].Value = ma_nguoi_dung_domain;
                                eSheet.Cells[rownumber, 4].Value = ma_nhan_vien;
                                eSheet.Cells[rownumber, 5].Value = ten_nguoi_dung;
                                eSheet.Cells[rownumber, 6].Value = gioi_tinh;
                                eSheet.Cells[rownumber, 7].Value = email;
                                eSheet.Cells[rownumber, 8].Value = dien_thoai;
                                eSheet.Cells[rownumber, 9].Value = trang_thai;
                                eSheet.Cells[rownumber, 10].Value = don_vi;
                                eSheet.Cells[rownumber, 11].Value = don_vi_quan_ly;
                                eSheet.Cells[rownumber, 12].Value = profit;
                                eSheet.Cells[rownumber, 13].Value = chuc_danh;
                                eSheet.Cells[rownumber, 14].Value = cap_bac;
                                eSheet.Cells[rownumber, 15].Value = bc_dinh_bien;
                                eSheet.Cells[rownumber, 16].Value = dia_chi;
                                eSheet.Cells[rownumber, 17].Value = ghi_chu;
                                eSheet.Cells[rownumber, 18].Value = ngay_vao_lam;
                                eSheet.Cells[rownumber, 19].Value = e.Message;
                                rownumber++;
                                error++;
                                continue;
                            }
                        }
                    }
                }
                _excelPkg.Save();
                return Json(new { success = true, total = total, totalError = error, link = linkerror });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        public ActionResult ExportTemplate()
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                FileInfo fileInfo = new FileInfo(Server.MapPath(@"~\ExportExcelFile\User_Template.xlsx"));
                var excelPkg = new ExcelPackage(fileInfo);
                string fileName = "User_Import_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                var gioitinh = dbConn.Select<Parameters>("Select id ,isnull(ma_tham_so,'') as ma_tham_so , isnull(gia_tri,'') as gia_tri From [Parameters] Where loai_tham_so = 'GIOITINH'and trang_thai = 'true' order by ma_tham_so");
                int rowgt = 0;
                ExcelWorksheet SheetGT = excelPkg.Workbook.Worksheets["GIOITINH"];
                foreach (var item in gioitinh)
                {
                    rowgt++;
                    SheetGT.Cells[rowgt, 1].Value = item.ma_tham_so + "-" + item.gia_tri;
                }
                var donvi = dbConn.Select<Branch>("Select DISTINCT  isnull(ma_don_vi,'') as ma_don_vi  From [Branch] Where trang_thai = 'true' order by ma_don_vi");
                int rowdv = 0;
                ExcelWorksheet SheetDV = excelPkg.Workbook.Worksheets["DONVI"];
                foreach (var item in donvi)
                {
                    rowdv++;
                    SheetDV.Cells[rowdv, 1].Value = item.ma_don_vi;
                }

                var chinhanh = dbConn.Select<Branch>("Select id ,isnull(ma_chi_nhanh,'') as ma_chi_nhanh , isnull(ten_chi_nhanh,'') as ten_chi_nhanh From [Branch] Where trang_thai = 'true' order by ma_chi_nhanh");
                int rowcn = 1;
                ExcelWorksheet SheetCN = excelPkg.Workbook.Worksheets["CHINHANH"];
                SheetCN.Cells[rowcn, 1].Value = "ALL" + "-" + "Tất cả chi nhánh";
                foreach (var item in chinhanh)
                {
                    rowcn++;
                    SheetCN.Cells[rowcn, 1].Value = item.ma_chi_nhanh + "-" + item.ten_chi_nhanh;
                }
                //var profit = dbConn.Select<Parameters>("Select id ,isnull(ma_tham_so,'') as ma_tham_so , isnull(gia_tri,'') as gia_tri From [Parameters] Where loai_tham_so = 'PROFIT_CENTER' and trang_thai = 'true' order by ma_tham_so");
                //int rowfit = 0;
                //ExcelWorksheet SheetPROFIT = excelPkg.Workbook.Worksheets["PROFIT"];
                //foreach (var item in profit)
                //{
                //    rowfit++;
                //    SheetPROFIT.Cells[rowfit, 1].Value = item.ma_tham_so + "-" + item.gia_tri;
                //}


                var chucdanh = dbConn.Select<Parameters>("Select id ,isnull(ma_tham_so,'') as ma_tham_so , isnull(gia_tri,'') as gia_tri From [Parameters] Where loai_tham_so = 'CHUCDANH' and trang_thai = 'true' order by ma_tham_so");
                int rowcd = 0;
                ExcelWorksheet SheetCHUCDANH = excelPkg.Workbook.Worksheets["CHUCDANH"];
                foreach (var item in chucdanh)
                {
                    rowcd++;
                    SheetCHUCDANH.Cells[rowcd, 1].Value = item.ma_tham_so + "-" + item.gia_tri;
                }

                var chucvu = dbConn.Select<Parameters>("Select id ,isnull(ma_tham_so,'') as ma_tham_so , isnull(gia_tri,'') as gia_tri From [Parameters] Where loai_tham_so = 'CHUCVU' and trang_thai = 'true' order by ma_tham_so");
                int rowcv = 0;
                ExcelWorksheet SheetCHUCVU = excelPkg.Workbook.Worksheets["CHUCVU"];
                foreach (var item in chucvu)
                {
                    rowcv++;
                    SheetCHUCVU.Cells[rowcv, 1].Value = item.ma_tham_so + "-" + item.gia_tri;
                }

                var bcdinhbien = dbConn.Select<Parameters>("Select id ,isnull(ma_tham_so,'') as ma_tham_so , isnull(gia_tri,'') as gia_tri From [Parameters] Where loai_tham_so = 'BCDINHBIEN' and trang_thai = 'true' order by ma_tham_so");
                int rowbcdb = 0;
                ExcelWorksheet SheetBCDB = excelPkg.Workbook.Worksheets["BCDINHBIEN"];
                foreach (var item in bcdinhbien)
                {
                    rowbcdb++;
                    SheetBCDB.Cells[rowbcdb, 1].Value = item.ma_tham_so + "-" + item.gia_tri;
                }
                var domain = dbConn.Select<Parameters>("Select id ,isnull(ma_tham_so,'') as ma_tham_so , isnull(gia_tri,'') as gia_tri From [Parameters] Where loai_tham_so = 'DOMAIN' and trang_thai = 'true' order by ma_tham_so");
                int rowdm = 0;
                ExcelWorksheet SheetDOMAIN = excelPkg.Workbook.Worksheets["DOMAIN"];
                foreach (var item in domain)
                {
                    rowdm++;
                    SheetDOMAIN.Cells[rowdm, 1].Value = item.gia_tri;
                }
                MemoryStream output = new MemoryStream();
                excelPkg.SaveAs(output);
                output.Position = 0;
                return File(output.ToArray(), contentType, fileName);
            }
        }

        public ActionResult ExportData([DataSourceRequest]DataSourceRequest request)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                FileInfo fileInfo = new FileInfo(Server.MapPath(@"~\ExportExcelFile\UsersList.xlsx"));
                var excelPkg = new ExcelPackage(fileInfo);

                string fileName = "DS_Người_Dùng_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var data = new List<User>();
                if (accessDetail.xuat)
                {
                    if (request.Filters.Any())
                    {
                        var where = KendoApplyFilter.ApplyFilter(request.Filters[0]);
                        data = dbConn.Select<User>(@" Select * 
                                                      From 
                                                      (
                                                        Select u.*
                                                                ,ISNULL(STUFF ((
                                                                        Select DISTINCT ',' + b.ten_chi_nhanh
                                                                            From UsersBranch a
                                                                            Inner Join Branch b ON a.ma_chi_nhanh = b.ma_chi_nhanh
                                                                            Where a.ma_nguoi_dung = u.ma_nguoi_dung
                                                                        For XML Path('') 
                                                                            ), 1, 1, ''),'') AS ten_chi_nhanh
                                                                ,isnull(pgt.gia_tri,'') As gioi_tinh_value
                                                                ,isnull(pcd.gia_tri,'') As chuc_danh_value
                                                                ,isnull(pcv.gia_tri,'') As cap_bac_value
                                                                ,isnull(pbcdb.gia_tri,'') As bien_che_dinh_bien_value
                                                        From [User] u
                                                        Left Join Parameters pgt On pgt.ma_tham_so = u.gioi_tinh
                                                        Left Join Parameters pcd On pcd.ma_tham_so = u.chuc_danh
                                                        Left Join Parameters pcv On pcv.ma_tham_so = u.cap_bac
                                                        Left Join Parameters pbcdb On pbcdb.ma_tham_so = u.bien_che_dinh_bien
                                                        ) data where " + where);
                    }
                    else
                    {
                        data = dbConn.Select<User>(@" Select u.*
                                                            ,ISNULL(STUFF ((
                                                                    Select DISTINCT ',' + b.ten_chi_nhanh
                                                                        From UsersBranch a
                                                                        Inner Join Branch b ON a.ma_chi_nhanh = b.ma_chi_nhanh
                                                                        Where a.ma_nguoi_dung = u.ma_nguoi_dung
                                                                    For XML Path('') 
                                                                        ), 1, 1, ''),'') AS ten_chi_nhanh
                                                            ,isnull(pgt.gia_tri,'') As gioi_tinh_value
                                                            ,isnull(pcd.gia_tri,'') As chuc_danh_value
                                                            ,isnull(pcv.gia_tri,'') As cap_bac_value
                                                            ,isnull(pbcdb.gia_tri,'') As bien_che_dinh_bien_value
                                                        From [User] u
                                                        Left Join Parameters pgt On pgt.ma_tham_so = u.gioi_tinh
                                                        Left Join Parameters pcd On pcd.ma_tham_so = u.chuc_danh
                                                        Left Join Parameters pcv On pcv.ma_tham_so = u.cap_bac
                                                        Left Join Parameters pbcdb On pbcdb.ma_tham_so = u.bien_che_dinh_bien");
                    }

                    ExcelWorksheet Sheet = excelPkg.Workbook.Worksheets["DSNGUOIDUNG"];
                    int rowData = 1;
                    foreach (var item in data)
                    {
                        int i = 1;
                        rowData++;
                        Sheet.Cells[rowData, i++].Value = item.ma_nguoi_dung;
                        Sheet.Cells[rowData, i++].Value = item.domain;
                       
                        Sheet.Cells[rowData, i++].Value = item.ten_nguoi_dung;
                        Sheet.Cells[rowData, i++].Value = item.gioi_tinh;
                        Sheet.Cells[rowData, i++].Value = item.gioi_tinh_value;
                        Sheet.Cells[rowData, i++].Value = item.email;
                        Sheet.Cells[rowData, i++].Value = item.dien_thoai;
                        Sheet.Cells[rowData, i++].Value = item.trang_thai == "true" ? "Hoạt động" : "Ngưng hoạt động";                     
                        Sheet.Cells[rowData, i++].Value = item.dia_chi;
                        Sheet.Cells[rowData, i++].Value = item.ghi_chu;                       
                        Sheet.Cells[rowData, i++].Value = item.ngay_tao.ToShortDateString();
                        Sheet.Cells[rowData, i++].Value = item.nguoi_tao;
                        Sheet.Cells[rowData, i++].Value = item.ngay_cap_nhat.ToShortDateString();
                        Sheet.Cells[rowData, i++].Value = item.nguoi_cap_nhat;
                    }
                }
                MemoryStream output = new MemoryStream();
                excelPkg.SaveAs(output);
                output.Position = 0;
                return File(output.ToArray(), contentType, fileName);
            }
        }
    }
}