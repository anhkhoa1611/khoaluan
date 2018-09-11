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
using System.Text.RegularExpressions;

namespace HPSTD.Controllers
{
    public class FoodController : CustomController
    {
        // GET: Food
        public ActionResult Index()
        {
            return View("Food");
        }
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new List<Food>();
                if (accessDetail.xem)
                {
                    if (request.Filters.Any())
                    {
                        var where = KendoApplyFilter.ApplyFilter(request.Filters[0]);
                        data = dbConn.Select<Food>(where);
                    }
                    else
                    {
                        data = dbConn.Select<Food>();
                    }
                }
                return Json(data.ToDataSourceResult(request));
            }
        }
        public ActionResult ExportTemplate()
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                FileInfo fileInfo = new FileInfo(Server.MapPath(@"~\ExportExcelFile\Template_GroupFood.xlsx"));
                var excelPkg = new ExcelPackage(fileInfo);
                string fileName = "Group_Import_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                MemoryStream output = new MemoryStream();
                excelPkg.SaveAs(output);
                output.Position = 0;
                return File(output.ToArray(), contentType, fileName);
            }
        }
        public ActionResult Add()
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                ViewBag.Groupfood = dbConn.Select<GroupFood>();
            }
            return View("AddFood");
        }
        public ActionResult IndexInfo(string ma_thuc_pham = "")
        {
            var Data = new Food();
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                ViewBag.Groupfood = dbConn.Select<GroupFood>();
                Data = dbConn.FirstOrDefault<Food>(@"
                                Select ten_thuc_pham,
                                       nuoc,nang_luong,protein,lipid,glucid,celluloza,tro,duong_tong_so,galactoza,maltoza,
                                   lactoza,fructoza,glucoza,sacaroza,calci,sat,magie,mangan,phospho,kali,natri,kem,dong,
                                    selen,vitaminc,vitaminb1,vitaminb2,vitaminpp,vitaminb5,vitaminb6,folat,vitaminb9,vitaminh,
                                   vitaminb12,vitamina,vitamind,vitamine,vitamink,beta_caroten,alpha_caroten,beta_cryptoxanthin,
                                    lycopen,lutein_zeaxanthin,purin,tong_so_isoflavon,daidzein,genistein,glycetin,tong_so_acid_beo_no,
                                    palmitic_c16,margaric_c17,stearic_c18,arachidic_c20,behenic_c22,lignoceric_c24,tong_so_acid_beo_khong_no_mot_noi_doi,
                                    myrictoleic_c14,palmitoleic_c16, oleic_c18,tong_so_acid_beo_khong_no_nhieu_noi_doi,linoleic_c18,linoleic_c18_n3,
                                    arachidonic_c20,eicosapentaenoic_c20,docosahexaenoic_c22,tong_so_acid_beo_trans,cholesterol,
                                    phytosterol,lysin,methionin,tryptophan,phenylalanin,threonin,valin,leucin,isoleucin,arginin,
                                    histidin, cystin,tyrosin,alanin,acid_aspartic,acid_glutamic,glycin,prolin,serin,url_anh,f.ma_nhom_thuc_pham,
                                    ten_nhom_thuc_pham
                            From Food f
                            Left join GroupFood gf on gf.ma_nhom_thuc_pham = f.ma_nhom_thuc_pham
                            where f.ma_thuc_pham = {0}
                            
                ".Params(ma_thuc_pham));
            }
            return View("AddFood", Data);
        }
        public ActionResult CreateUpdate(Food data)
        {
            long temp = 0;
            try
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    if (data.id > 0)
                    {
                        if (accessDetail.sua)
                        {
                            var exist = dbConn.SingleOrDefault<Food>("id={0}", data.id);
                            exist.ten_thuc_pham = data.ten_thuc_pham;
                            exist.nuoc = data.nuoc;
                            exist.nang_luong = data.nang_luong;
                            exist.protein = data.protein;
                            exist.lipid = data.lipid;
                            exist.glucid = data.glucid;
                            exist.celluloza = data.celluloza;
                            exist.tro = data.tro;
                            exist.duong_tong_so = data.duong_tong_so;
                            exist.galactoza = data.galactoza;
                            exist.maltoza = data.maltoza;
                            exist.lactoza = data.lactoza;
                            exist.fructoza = data.fructoza;
                            exist.glucoza = data.glucoza;
                            exist.sacaroza = data.sacaroza;
                            exist.calci = data.calci;
                            exist.sat = data.sat;
                            exist.magie = data.magie;
                            exist.mangan = data.mangan;
                            exist.phospho = data.phospho;
                            exist.kali = data.kali;
                            exist.natri = data.natri;
                            exist.kem = data.kem;
                            exist.dong = data.dong;
                            exist.selen = data.selen;
                            exist.vitaminc = data.vitaminc;
                            exist.vitaminb1 = data.vitaminb1;
                            exist.vitaminb2 = data.vitaminb2;
                            exist.vitaminpp = data.vitaminpp;
                            exist.vitaminb5 = data.vitaminb5;
                            exist.vitaminb6 = data.vitaminb6;
                            exist.folat = data.folat;
                            exist.vitaminb9 = data.vitaminb9;
                            exist.vitaminh = data.vitaminh;
                            exist.vitaminb12 = data.vitaminb12;
                            exist.vitamina = data.vitamina;
                            exist.vitamind = data.vitamind;
                            exist.vitamine = data.vitamine;
                            exist.vitamink = data.vitamink;
                            exist.beta_caroten = data.beta_caroten;
                            exist.alpha_caroten = data.alpha_caroten;
                            exist.beta_cryptoxanthin = data.beta_cryptoxanthin;
                            exist.lycopen = data.lycopen;
                            exist.lutein_zeaxanthin = data.lutein_zeaxanthin;
                            exist.purin = data.purin;
                            exist.tong_so_isoflavon = data.tong_so_isoflavon;
                            exist.daidzein = data.daidzein;
                            exist.genistein = data.genistein;
                            exist.glycetin = data.glycetin;
                            exist.tong_so_acid_beo_no = data.tong_so_acid_beo_no;
                            exist.palmitic_c16 = data.palmitic_c16;
                            exist.margaric_c17 = data.margaric_c17;
                            exist.stearic_c18 = data.stearic_c18;
                            exist.arachidic_c20 = data.arachidic_c20;
                            exist.behenic_c22 = data.behenic_c22;
                            exist.lignoceric_c24 = data.lignoceric_c24;
                            exist.tong_so_acid_beo_khong_no_mot_noi_doi = data.tong_so_acid_beo_khong_no_mot_noi_doi;
                            exist.myrictoleic_c14 = data.myrictoleic_c14;
                            exist.palmitoleic_c16 = data.palmitoleic_c16;
                            exist.oleic_c18 = data.oleic_c18;
                            exist.tong_so_acid_beo_khong_no_nhieu_noi_doi = data.tong_so_acid_beo_khong_no_nhieu_noi_doi;
                            exist.linoleic_c18 = data.linoleic_c18;
                            exist.linoleic_c18_n3 = data.linoleic_c18_n3;
                            exist.arachidonic_c20 = data.arachidonic_c20;
                            exist.eicosapentaenoic_c20 = data.eicosapentaenoic_c20;
                            exist.docosahexaenoic_c22 = data.docosahexaenoic_c22;
                            exist.tong_so_acid_beo_trans = data.tong_so_acid_beo_trans;
                            exist.cholesterol = data.cholesterol;
                            exist.phytosterol = data.phytosterol;
                            exist.lysin = data.lysin;
                            exist.methionin = data.methionin;
                            exist.tryptophan = data.tryptophan;
                            exist.phenylalanin = data.phenylalanin;
                            exist.threonin = data.threonin;
                            exist.valin = data.valin;
                            exist.leucin = data.leucin;
                            exist.isoleucin = data.isoleucin;
                            exist.arginin = data.arginin;
                            exist.histidin = data.histidin;
                            exist.cystin = data.cystin;
                            exist.tyrosin = data.tyrosin;
                            exist.alanin = data.alanin;
                            exist.acid_aspartic = data.acid_aspartic;
                            exist.acid_glutamic = data.acid_glutamic;
                            exist.glycin = data.glycin;
                            exist.prolin = data.prolin;
                            exist.serin = data.serin;
                            var file = Request.Files["hinh_anh"];
                            if (file != null)
                            {
                                string destinationPath = Helpers.Upload.UploadFile("avatar", file);
                                exist.url_anh = destinationPath;
                            }
                            else
                            {
                                exist.url_anh = null;
                            }
                            data.ma_nhom_thuc_pham = "";
                            exist.ngay_cap_nhat = DateTime.Now;
                            exist.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            dbConn.Update(exist, s => s.id == exist.id);
                            ActivityLogsController.CreateLogs(currentUser.ma_nguoi_dung, "Food", "Update", "Update Food " + data.id); //CreateActivityLog
                        }
                        else
                        {
                            return Json(new { success = true, error = "Bạn không có quyền cập nhật. Vui lòng liên hệ với ban quản trị để cập nhật quyền." });
                        }
                    }
                    else
                    {
                        if (accessDetail.them)
                        {
                            
                            string ma_thuc_pham = "";
                            var ma = "TP";
                            var existLast = dbConn.SingleOrDefault<Food>("SELECT TOP 1 * FROM Food ORDER BY id DESC");
                            if (existLast != null)
                            {
                                // Convert to int
                                int now = Int32.Parse(Regex.Match(existLast.ma_thuc_pham, @"\d+").Value);
                                // Increase
                                now++;
                                // Convert back to string
                                ma_thuc_pham = ma + now.ToString("D7");
                            }
                            else
                            {
                                ma_thuc_pham = ma + "0000001";
                            }
                            var file = Request.Files["hinh_anh"];
                            if (file != null)
                            {
                                string destinationPath = Helpers.Upload.UploadFile("avatar", file);
                                data.url_anh = destinationPath;
                            }
                            data.ma_thuc_pham = ma_thuc_pham;
                            data.nguoi_tao = currentUser.ma_nguoi_dung;
                            data.ngay_cap_nhat = DateTime.Now;
                            data.ngay_tao = DateTime.Now;
                            dbConn.Insert(data);
                        }
                        else
                        {
                            return Json(new { success = true, error = "Bạn không có quyền thêm. Vui lòng liên hệ với ban quản trị để cập nhật quyền." });
                        }
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });

            }
        }
        [HttpPost]
        public ActionResult UploadAvatarDetail(Food data)
        {
            try
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    var exist = dbConn.SingleOrDefault<Food>("id={0}", data.id);
                    if (exist != null)
                    {
                        if (accessDetail.sua)
                        {
                            var file = Request.Files["image"];
                            if (file != null)
                            {
                                string FileToDelete = Server.MapPath(exist.url_anh);
                                System.IO.File.Delete(FileToDelete);
                                string destinationPath = Helpers.Upload.UploadFile("Anhthucpham", file);
                                exist.url_anh = destinationPath;

                            }
                            else
                            {
                                exist.url_anh = exist.url_anh;
                            }
                            dbConn.Update(exist, s => s.id == exist.id);
                            ActivityLogsController.CreateLogs(currentUser.ma_nguoi_dung, "Food", "Update", "Update Food " + data.id); //CreateActivityLog
                        }
                        else
                        {
                            return Json(new { success = true, error = "Bạn không có quyền cập nhật. Vui lòng liên hệ với ban quản trị để cập nhật quyền." });
                        }
                    }
                }
                return Json(new { success = true, message = "Thay đổi hình ảnh thành công !" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });

            }
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
                FileInfo template = new FileInfo(Server.MapPath(@"~\ExportExcelFile\Template_Food.xlsx"));
                template.CopyTo(errorFileLocation);
                FileInfo _fileInfo = new FileInfo(errorFileLocation);
                var _excelPkg = new ExcelPackage(_fileInfo);
                ExcelWorksheet oSheet = excelPkg.Workbook.Worksheets["Sheet1"];
                ExcelWorksheet eSheet = _excelPkg.Workbook.Worksheets["Sheet1"];
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
                            string ten_thuc_pham = oSheet.Cells[i, 1].Value != null ? oSheet.Cells[i, 1].Value.ToString().Trim() : "";
                            if (ten_thuc_pham == "")
                            {
                                continue;
                            }
                            double nuoc = double.Parse(oSheet.Cells[i, 2].Value != null ? oSheet.Cells[i, 2].Value.ToString().Trim() : "");
                            double nang_luong = double.Parse(oSheet.Cells[i, 3].Value != null ? oSheet.Cells[i, 3].Value.ToString().Trim() : "");
                            double protein = double.Parse(oSheet.Cells[i, 4].Value != null ? oSheet.Cells[i, 4].Value.ToString().Trim() : "");
                            double lipid = double.Parse(oSheet.Cells[i, 5].Value != null ? oSheet.Cells[i, 5].Value.ToString().Trim() : "");
                            double glucid = double.Parse(oSheet.Cells[i, 6].Value != null ? oSheet.Cells[i, 6].Value.ToString().Trim() : "");
                            double celluloza = double.Parse(oSheet.Cells[i, 7].Value != null ? oSheet.Cells[i, 7].Value.ToString().Trim() : "");
                            double tro = double.Parse(oSheet.Cells[i, 8].Value != null ? oSheet.Cells[i, 8].Value.ToString().Trim() : "");
                            double duong_tong_so = double.Parse(oSheet.Cells[i, 9].Value != null ? oSheet.Cells[i, 9].Value.ToString().Trim() : "");
                            double galactoza = double.Parse(oSheet.Cells[i, 10].Value != null ? oSheet.Cells[i, 10].Value.ToString().Trim() : "");
                            double maltoza = double.Parse(oSheet.Cells[i, 11].Value != null ? oSheet.Cells[i, 11].Value.ToString().Trim() : "");
                            double lactoza = double.Parse(oSheet.Cells[i, 12].Value != null ? oSheet.Cells[i, 11].Value.ToString().Trim() : "");
                            double Fructoza = double.Parse(oSheet.Cells[i, 13].Value != null ? oSheet.Cells[i, 12].Value.ToString().Trim() : "");
                            double Glucoza = double.Parse(oSheet.Cells[i, 14].Value != null ? oSheet.Cells[i, 13].Value.ToString().Trim() : "");
                            double Sacaroza = double.Parse(oSheet.Cells[i, 15].Value != null ? oSheet.Cells[i, 14].Value.ToString().Trim() : "");
                            double Calci = double.Parse(oSheet.Cells[i, 16].Value != null ? oSheet.Cells[i, 15].Value.ToString().Trim() : "");
                            double sat = double.Parse(oSheet.Cells[i, 17].Value != null ? oSheet.Cells[i, 16].Value.ToString().Trim() : "");
                            double magie = double.Parse(oSheet.Cells[i, 18].Value != null ? oSheet.Cells[i, 17].Value.ToString().Trim() : "");
                            double mangan = double.Parse(oSheet.Cells[i, 19].Value != null ? oSheet.Cells[i, 18].Value.ToString().Trim() : "");
                            double phospho = double.Parse(oSheet.Cells[i, 20].Value != null ? oSheet.Cells[i, 19].Value.ToString().Trim() : "");
                            double kali = double.Parse(oSheet.Cells[i, 21].Value != null ? oSheet.Cells[i, 21].Value.ToString().Trim() : "");
                            double natri = double.Parse(oSheet.Cells[i, 22].Value != null ? oSheet.Cells[i, 22].Value.ToString().Trim() : "");
                            double kem = double.Parse(oSheet.Cells[i, 23].Value != null ? oSheet.Cells[i, 23].Value.ToString().Trim() : "");
                            double dong = double.Parse(oSheet.Cells[i, 24].Value != null ? oSheet.Cells[i, 24].Value.ToString().Trim() : "");
                            double selen = double.Parse(oSheet.Cells[i, 25].Value != null ? oSheet.Cells[i, 25].Value.ToString().Trim() : "");
                            double vitaminc = double.Parse(oSheet.Cells[i, 26].Value != null ? oSheet.Cells[i, 26].Value.ToString().Trim() : "");
                            double vitaminb1 = double.Parse(oSheet.Cells[i, 27].Value != null ? oSheet.Cells[i, 27].Value.ToString().Trim() : "");
                            double vitaminb2 = double.Parse(oSheet.Cells[i, 28].Value != null ? oSheet.Cells[i, 28].Value.ToString().Trim() : "");
                            double vitaminpp = double.Parse(oSheet.Cells[i, 29].Value != null ? oSheet.Cells[i, 29].Value.ToString().Trim() : "");
                            double vitaminb5 = double.Parse(oSheet.Cells[i, 30].Value != null ? oSheet.Cells[i, 30].Value.ToString().Trim() : "");
                            double vitaminb6 = double.Parse(oSheet.Cells[i, 31].Value != null ? oSheet.Cells[i, 31].Value.ToString().Trim() : "");
                            double folat = double.Parse(oSheet.Cells[i, 32].Value != null ? oSheet.Cells[i, 32].Value.ToString().Trim() : "");
                            double vitaminb9 = double.Parse(oSheet.Cells[i, 33].Value != null ? oSheet.Cells[i, 33].Value.ToString().Trim() : "");
                            double vitaminh = double.Parse(oSheet.Cells[i, 34].Value != null ? oSheet.Cells[i, 34].Value.ToString().Trim() : "");
                            double vitaminb12 = double.Parse(oSheet.Cells[i, 35].Value != null ? oSheet.Cells[i, 35].Value.ToString().Trim() : "");
                            double vitamina = double.Parse(oSheet.Cells[i, 36].Value != null ? oSheet.Cells[i, 36].Value.ToString().Trim() : "");
                            double vitamind = double.Parse(oSheet.Cells[i, 37].Value != null ? oSheet.Cells[i, 37].Value.ToString().Trim() : "");
                            double vitamine = double.Parse(oSheet.Cells[i, 38].Value != null ? oSheet.Cells[i, 38].Value.ToString().Trim() : "");
                            double vitamink = double.Parse(oSheet.Cells[i, 39].Value != null ? oSheet.Cells[i, 39].Value.ToString().Trim() : "");
                            double betaocaroten = double.Parse(oSheet.Cells[i, 40].Value != null ? oSheet.Cells[i, 40].Value.ToString().Trim() : "");
                            double alphaocaroten = double.Parse(oSheet.Cells[i, 41].Value != null ? oSheet.Cells[i, 41].Value.ToString().Trim() : "");
                            double betaocryptoxanthin = double.Parse(oSheet.Cells[i, 42].Value != null ? oSheet.Cells[i, 42].Value.ToString().Trim() : "");
                            double lycopen = double.Parse(oSheet.Cells[i, 43].Value != null ? oSheet.Cells[i, 43].Value.ToString().Trim() : "");
                            double lutein_zeaxanthin = double.Parse(oSheet.Cells[i, 44].Value != null ? oSheet.Cells[i, 44].Value.ToString().Trim() : "");
                            double purin = double.Parse(oSheet.Cells[i, 45].Value != null ? oSheet.Cells[i, 45].Value.ToString().Trim() : "");
                            double tong_so_isoflavon = double.Parse(oSheet.Cells[i, 46].Value != null ? oSheet.Cells[i, 46].Value.ToString().Trim() : "");
                            double daidzein = double.Parse(oSheet.Cells[i, 47].Value != null ? oSheet.Cells[i, 47].Value.ToString().Trim() : "");
                            double genistein = double.Parse(oSheet.Cells[i, 48].Value != null ? oSheet.Cells[i, 48].Value.ToString().Trim() : "");
                            double glycetin = double.Parse(oSheet.Cells[i, 49].Value != null ? oSheet.Cells[i, 49].Value.ToString().Trim() : "");
                            double tong_so_acid_beo_no = double.Parse(oSheet.Cells[i, 50].Value != null ? oSheet.Cells[i, 50].Value.ToString().Trim() : "");
                            double palmitic_c16 = double.Parse(oSheet.Cells[i, 51].Value != null ? oSheet.Cells[i, 51].Value.ToString().Trim() : "");
                            double margaric_c17 = double.Parse(oSheet.Cells[i, 52].Value != null ? oSheet.Cells[i, 52].Value.ToString().Trim() : "");
                            double stearic_c18 = double.Parse(oSheet.Cells[i, 53].Value != null ? oSheet.Cells[i, 53].Value.ToString().Trim() : "");
                            double arachidic_c20 = double.Parse(oSheet.Cells[i, 54].Value != null ? oSheet.Cells[i, 54].Value.ToString().Trim() : "");
                            double behenic_c22 = double.Parse(oSheet.Cells[i, 55].Value != null ? oSheet.Cells[i, 55].Value.ToString().Trim() : "");
                            double lignoceric_c24 = double.Parse(oSheet.Cells[i, 56].Value != null ? oSheet.Cells[i, 56].Value.ToString().Trim() : "");
                            double tong_so_acid_beo_khong_no_mot_noi_doi = double.Parse(oSheet.Cells[i, 57].Value != null ? oSheet.Cells[i, 57].Value.ToString().Trim() : "");
                            double myrictoleic_c14 = double.Parse(oSheet.Cells[i, 58].Value != null ? oSheet.Cells[i, 58].Value.ToString().Trim() : "");
                            double palmitoleic_c16 = double.Parse(oSheet.Cells[i, 59].Value != null ? oSheet.Cells[i, 59].Value.ToString().Trim() : "");
                            double oleic_c18 = double.Parse(oSheet.Cells[i, 60].Value != null ? oSheet.Cells[i, 60].Value.ToString().Trim() : "");
                            double tong_so_acid_beo_khong_no_nhieu_noi_doi = double.Parse(oSheet.Cells[i, 61].Value != null ? oSheet.Cells[i, 61].Value.ToString().Trim() : "");
                            double linoleic_c18 = double.Parse(oSheet.Cells[i, 62].Value != null ? oSheet.Cells[i, 62].Value.ToString().Trim() : "");
                            double linoleic_c18_n3 = double.Parse(oSheet.Cells[i, 63].Value != null ? oSheet.Cells[i, 62].Value.ToString().Trim() : "");
                            double arachidonic_c20 = double.Parse(oSheet.Cells[i, 64].Value != null ? oSheet.Cells[i, 63].Value.ToString().Trim() : "");
                            double eicosapentaenoic_c20 = double.Parse(oSheet.Cells[i, 65].Value != null ? oSheet.Cells[i, 64].Value.ToString().Trim() : "");
                            double docosahexaenoic_c22 = double.Parse(oSheet.Cells[i, 66].Value != null ? oSheet.Cells[i, 65].Value.ToString().Trim() : "");
                            double tong_so_acid_beo_trans = double.Parse(oSheet.Cells[i, 67].Value != null ? oSheet.Cells[i, 66].Value.ToString().Trim() : "");
                            double cholesterol = double.Parse(oSheet.Cells[i, 68].Value != null ? oSheet.Cells[i, 67].Value.ToString().Trim() : "");
                            double phytosterol = double.Parse(oSheet.Cells[i, 69].Value != null ? oSheet.Cells[i, 68].Value.ToString().Trim() : "");
                            double lysin = double.Parse(oSheet.Cells[i, 70].Value != null ? oSheet.Cells[i, 69].Value.ToString().Trim() : "");
                            double methionin = double.Parse(oSheet.Cells[i, 71].Value != null ? oSheet.Cells[i, 70].Value.ToString().Trim() : "");
                            double tryptophan = double.Parse(oSheet.Cells[i, 72].Value != null ? oSheet.Cells[i, 71].Value.ToString().Trim() : "");
                            double phenylalanin = double.Parse(oSheet.Cells[i, 73].Value != null ? oSheet.Cells[i, 72].Value.ToString().Trim() : "");
                            double threonin = double.Parse(oSheet.Cells[i, 74].Value != null ? oSheet.Cells[i, 73].Value.ToString().Trim() : "");
                            double valin = double.Parse(oSheet.Cells[i, 75].Value != null ? oSheet.Cells[i, 74].Value.ToString().Trim() : "");
                            double leucin = double.Parse(oSheet.Cells[i, 76].Value != null ? oSheet.Cells[i, 75].Value.ToString().Trim() : "");
                            double isoleucin = double.Parse(oSheet.Cells[i, 77].Value != null ? oSheet.Cells[i, 76].Value.ToString().Trim() : "");
                            double arginin = double.Parse(oSheet.Cells[i, 78].Value != null ? oSheet.Cells[i, 77].Value.ToString().Trim() : "");
                            double histidin = double.Parse(oSheet.Cells[i, 79].Value != null ? oSheet.Cells[i, 78].Value.ToString().Trim() : "");
                            double cystin = double.Parse(oSheet.Cells[i, 80].Value != null ? oSheet.Cells[i, 79].Value.ToString().Trim() : "");
                            double tyrosin = double.Parse(oSheet.Cells[i, 81].Value != null ? oSheet.Cells[i, 80].Value.ToString().Trim() : "");
                            double alanin = double.Parse(oSheet.Cells[i, 82].Value != null ? oSheet.Cells[i, 81].Value.ToString().Trim() : "");
                            double acid_aspartic = double.Parse(oSheet.Cells[i, 83].Value != null ? oSheet.Cells[i, 82].Value.ToString().Trim() : "");
                            double acid_glutamic = double.Parse(oSheet.Cells[i, 84].Value != null ? oSheet.Cells[i, 83].Value.ToString().Trim() : "");
                            double glycin = double.Parse(oSheet.Cells[i, 85].Value != null ? oSheet.Cells[i, 84].Value.ToString().Trim() : "");
                            double prolin = double.Parse(oSheet.Cells[i, 86].Value != null ? oSheet.Cells[i, 85].Value.ToString().Trim() : "");
                            double serin = double.Parse(oSheet.Cells[i, 87].Value != null ? oSheet.Cells[i, 86].Value.ToString().Trim() : "");
                            string trang_thai = "true";
                            //if (oSheet.Cells[i, 20].Value.ToString() == "Hoạt động")
                            //{
                            //    trang_thai = "true";
                            //}
                            //else if (oSheet.Cells[i, 20].Value.ToString() == "Ngưng hoạt động")
                            //{
                            //    trang_thai = "false";
                            //}

                            try
                            {
                                
                                string ma_thuc_pham = "";
                                var ma = "NV";
                                var existLast = dbConn.SingleOrDefault<Food>("SELECT TOP 1 * FROM Food ORDER BY id DESC");
                                if (existLast != null)
                                {
                                    // Convert to int
                                    int now = Int32.Parse(Regex.Match(existLast.ma_thuc_pham, @"\d+").Value);
                                    // Increase
                                    now++;
                                    // Convert back to string
                                    ma_thuc_pham = ma + now.ToString("D7");
                                }
                                else
                                {
                                    ma_thuc_pham = ma + "0000001";
                                }
                                var item = new Food();
                                item.ma_thuc_pham = ma_thuc_pham;
                                item.ten_thuc_pham = ten_thuc_pham;
                                item.nuoc = nuoc;
                                item.nang_luong = nang_luong;
                                item.protein = protein;
                                item.lipid = lipid;
                                item.glucid = glucid;
                                item.celluloza = celluloza;
                                item.tro = tro;
                                item.duong_tong_so = duong_tong_so;
                                item.galactoza = galactoza;
                                item.maltoza = maltoza;
                                item.lactoza = lactoza;
                                item.fructoza = Fructoza;
                                item.glucoza = Glucoza;
                                item.sacaroza = Sacaroza;
                                item.calci = Calci;
                                item.sat = sat;
                                item.magie = magie;
                                item.mangan = mangan;
                                item.phospho = phospho;
                                item.kali = kali;
                                item.natri = natri;
                                item.kem = kem;
                                item.dong = dong;
                                item.selen = selen;
                                item.vitaminc = vitaminc;
                                item.vitaminb1 = vitaminb1;
                                item.vitaminb2 = vitaminb2;
                                item.vitaminpp = vitaminpp;
                                item.vitaminb5 = vitaminb5;
                                item.vitaminb6 = vitaminb6;
                                item.folat = folat;
                                item.vitaminb9 = vitaminb9;
                                item.vitaminh = vitaminh;
                                item.vitaminb12 = vitaminb12;
                                item.vitamina = vitamina;
                                item.vitamind = vitamind;
                                item.vitamine = vitamine;
                                item.vitamink = vitamink;
                                item.beta_caroten = betaocaroten;
                                item.alpha_caroten = alphaocaroten;
                                item.beta_cryptoxanthin = betaocryptoxanthin;
                                item.lycopen = lycopen;
                                item.lutein_zeaxanthin = lutein_zeaxanthin;
                                item.purin = purin;
                                item.tong_so_isoflavon = tong_so_isoflavon;
                                item.daidzein = daidzein;
                                item.genistein = genistein;
                                item.glycetin = glycetin;
                                item.tong_so_acid_beo_no = tong_so_acid_beo_no;
                                item.palmitic_c16 = palmitic_c16;
                                item.margaric_c17 = margaric_c17;
                                item.stearic_c18 = stearic_c18;
                                item.arachidic_c20 = arachidic_c20;
                                item.behenic_c22 = behenic_c22;
                                item.lignoceric_c24 = lignoceric_c24;
                                item.tong_so_acid_beo_khong_no_mot_noi_doi = tong_so_acid_beo_khong_no_mot_noi_doi;
                                item.myrictoleic_c14 = myrictoleic_c14;
                                item.palmitoleic_c16 = palmitoleic_c16;
                                item.oleic_c18 = oleic_c18;
                                item.tong_so_acid_beo_khong_no_nhieu_noi_doi = tong_so_acid_beo_khong_no_nhieu_noi_doi;
                                item.linoleic_c18 = linoleic_c18;
                                item.linoleic_c18_n3 = linoleic_c18_n3;
                                item.arachidonic_c20 = arachidonic_c20;
                                item.eicosapentaenoic_c20 = eicosapentaenoic_c20;
                                item.docosahexaenoic_c22 = docosahexaenoic_c22;
                                item.tong_so_acid_beo_trans = tong_so_acid_beo_trans;
                                item.cholesterol = cholesterol;
                                item.phytosterol = phytosterol;
                                item.lysin = lysin;
                                item.methionin = methionin;
                                item.tryptophan = tryptophan;
                                item.phenylalanin = phenylalanin;
                                item.threonin = threonin;
                                item.valin = valin;
                                item.leucin = leucin;
                                item.isoleucin = isoleucin;
                                item.arginin = arginin;
                                item.histidin = histidin;
                                item.cystin = cystin;
                                item.tyrosin = tyrosin;
                                item.alanin = alanin;
                                item.acid_aspartic = acid_aspartic;
                                item.acid_glutamic = acid_glutamic;
                                item.glycin = glycin;
                                item.prolin = prolin;
                                item.serin = serin;
                                item.url_anh = "";
                                item.trang_thai = string.IsNullOrEmpty(trang_thai) ? "true" : trang_thai;
                                item.ngay_tao = DateTime.Now;
                                item.nguoi_tao = currentUser.ma_nguoi_dung;
                                item.ngay_cap_nhat = DateTime.Now;
                                item.nguoi_cap_nhat = "";
                                item.ma_nhom_thuc_pham = "";
                                dbConn.Insert<Food>(item);

                                //    total++;
                                //    rownumber++;
                                //}
                                total++;
                                rownumber++;
                            }
                            catch (Exception e)
                            {
                               
                                eSheet.Cells[rownumber, 88].Value = e.Message;
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
        public ActionResult Delete(string data)
        {
            if (accessDetail.xoa)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    string[] separators = { "@@" };
                    var listItem = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in listItem)
                    {
                        dbConn.Delete<Core.Entities.Food>("id={0}", item);
                    }
                    return Json(new { success = true });
                }
            }
            else
            {
                return Json(new { success = false, error = "Bạn không có quyền xóa dữ liệu. Vui lòng liên hệ với ban quản trị để cập nhật quyền." });
            }
        }
    }
}