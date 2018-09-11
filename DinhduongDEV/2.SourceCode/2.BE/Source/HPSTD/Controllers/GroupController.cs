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
using DevTrends.MvcDonutCaching;
using HPSTD.Core.Entities;

namespace HPSTD.Controllers
{
    [Authorize]
    public class GroupController : CustomController
    {
        //
        // GET: /Groups/
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listCNPBPGD = dbConn.Select<Branch>();
                    ViewBag.listMenu = dbConn.Select<Core.Entities.Menu>("Select * From Menu Where cap = 0 Order by thu_tu");
                    //ViewBag.listNguoiDung = dbConn.Select<User>();
                    return View("Group");
                }
            }
            return RedirectToAction("NoAccessRights", "Error");
        }
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new List<Group>();
                if (accessDetail.xem)
                {
                    if (request.Filters.Any())
                    {
                        var where = KendoApplyFilter.ApplyFilter(request.Filters[0], "");
                        data = dbConn.Query<Group>(where).ToList();
                    }
                    else
                    {
                        data = dbConn.Select<Group>().ToList();
                    }
                }
                return Json(data.ToDataSourceResult(request));
            }
        }

        private List<AccessRightDetail> GetMenuCon(List<AccessRightDetail> lstCha, int ma_nhom)
        {
            var data = new List<AccessRightDetail>();
            string where = " and ma_nhom =" + ma_nhom;
            if (lstCha != null && lstCha.Count > 0)
            {
                using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
                {
                    foreach (var item in lstCha)
                    {
                        var lstcon = dbConn.Select<AccessRightDetail>(@"
                            select * from(
                                    select 
	                                    d.id
	                                    ,d.ma_nhom
	                                    ,d.xem
	                                    ,d.them
	                                    ,d.xoa
	                                    ,d.sua
	                                    ,d.xuat
	                                    ,d.xuat_pdf
	                                    ,d.nhap
	                                    ,d.phan_quyen
	                                    ,ISNULL(m.ten_chuc_nang, N'Trang chủ') as ma_man_hinh
                                        ,ISNULL(m.thu_tu,0) thu_tu
                                        ,m.id_cha
                                        ,m.id as id_menu
                                    from AccessRightDetail d
                                    right join Menu m on m.ma_man_hinh = d.ma_man_hinh
                                ) data  where id_cha={0}".Params(item.id_menu) + where);

                        data.AddRange(lstcon);
                    }

                    if (data.Count > 0)
                        data.AddRange(GetMenuCon(data, ma_nhom));
                }
            }

            return data;
        }

        public ActionResult ReadAccessRightDetail([DataSourceRequest]DataSourceRequest request, int ma_nhom)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                if (accessDetail.xem)
                {
                    var data = new List<AccessRightDetail>();
                    if (request.Filters.Any())
                    {
                        var where = KendoApplyFilter.ApplyFilter(request.Filters[0], "") + " and (ma_nhom = " + ma_nhom + "or ma_nhom is null )";
                        data = dbConn.Select<AccessRightDetail>(@"
                            select * from(
                                    select 
	                                    d.id
	                                    ,d.ma_nhom
	                                    ,d.xem
	                                    ,d.them
	                                    ,d.xoa
	                                    ,d.sua
	                                    ,d.xuat
	                                    ,d.xuat_pdf
	                                    ,d.nhap
	                                    ,d.phan_quyen
	                                    ,ISNULL(m.ten_chuc_nang, N'Trang chủ') as ma_man_hinh
                                        ,ISNULL(m.thu_tu,0) thu_tu
                                        ,m.id_cha
                                        ,m.id as id_menu
                                    from AccessRightDetail d
                                    right join Menu m on m.ma_man_hinh = d.ma_man_hinh
                                ) data  where " + where);
                        data.AddRange(GetMenuCon(data, ma_nhom));
                        data = data.Where(s => s.id > 0).OrderBy(s => s.thu_tu).ToList();
                    }
                    else
                    {
                        data = dbConn.Select<AccessRightDetail>(@" select * from(
                                    select 
	                                    d.id
	                                    ,d.ma_nhom
	                                    ,d.xem
	                                    ,d.them
	                                    ,d.xoa
	                                    ,d.sua
	                                    ,d.xuat
	                                    ,d.xuat_pdf
	                                    ,d.nhap
	                                    ,d.phan_quyen
	                                    ,ISNULL(m.ten_chuc_nang, N'Trang chủ') as ma_man_hinh
                                        ,ISNULL(m.thu_tu,0) thu_tu
                                        ,m.id_cha	
                                    from AccessRightDetail d
                                    left join Menu m on m.ma_man_hinh = d.ma_man_hinh
                                ) data where ma_nhom =" + ma_nhom + " order by thu_tu ");
                    }
                    request = new DataSourceRequest();
                    return Json(data.ToDataSourceResult(request));
                }
                else
                {
                    return RedirectToAction("NoAccessRights", "Error");
                }
            }
        }

        public ActionResult ReadUserInGroup([DataSourceRequest]DataSourceRequest request, int ma_nhom)
        {
            using (IDbConnection dbConn = Helpers.OrmliteConnection.openConn())
            {
                if (accessDetail.xem)
                {
                    var data = new List<UserInGroup>();
                    if (request.Filters.Any())
                    {
                        var where = KendoApplyFilter.ApplyFilter(request.Filters[0], "") + " and ma_nhom =" + ma_nhom;
                        data = dbConn.Select<UserInGroup>(@"
                            select * from(
                                    select 
	                                    u.id,
	                                    u.ma_nguoi_dung,
	                                    u.ten_nguoi_dung, " +
                                        ma_nhom + @" as ma_nhom,                                        
	                                    case when uig.id_nhom_nguoi_dung is null then 0 else 1 end as thuoc_nhom, 
                                        u.email,
                                        ISNULL(STUFF ((
                                                Select DISTINCT ',' + a.ma_chi_nhanh
                                                From UsersBranch a
                                                    Inner Join Branch b ON a.ma_chi_nhanh = b.ma_chi_nhanh
                                                Where a.ma_nguoi_dung = u.ma_nguoi_dung
                                                For XML Path('') 
                                                ), 1, 1, ''),'') AS ma_chi_nhanh,
                                        ISNULL(STUFF ((
                                                Select DISTINCT ',' + b.ten_chi_nhanh
                                                From UsersBranch a
                                                    Inner Join Branch b ON a.ma_chi_nhanh = b.ma_chi_nhanh
                                                Where a.ma_nguoi_dung = u.ma_nguoi_dung
                                                For XML Path('') 
                                                ), 1, 1, ''),'') AS ten_chi_nhanh
                                    from [User] u
                                    left join ( Select * From UserInGroup where id_nhom_nguoi_dung = " + ma_nhom + @") uig on uig.ma_nguoi_dung = u.ma_nguoi_dung 
                                ) data  where " + where);
                    }
                    else
                    {
                        data = dbConn.Select<UserInGroup>(@" select * from(
                                    select 
	                                    u.id,
	                                    u.ma_nguoi_dung,
	                                    u.ten_nguoi_dung, " +
                                        ma_nhom + @" as ma_nhom,
	                                   	case when uig.id_nhom_nguoi_dung is null then 0 else 1 end as thuoc_nhom, 
                                        u.email,
                                        ISNULL(STUFF ((
                                                Select DISTINCT ',' + a.ma_chi_nhanh
                                                From UsersBranch a
                                                    Inner Join Branch b ON a.ma_chi_nhanh = b.ma_chi_nhanh
                                                Where a.ma_nguoi_dung = u.ma_nguoi_dung
                                                For XML Path('') 
                                                ), 1, 1, ''),'') AS ma_chi_nhanh,
                                        ISNULL(STUFF ((
                                                Select DISTINCT ',' + b.ten_chi_nhanh
                                                From UsersBranch a
                                                    Inner Join Branch b ON a.ma_chi_nhanh = b.ma_chi_nhanh
                                                Where a.ma_nguoi_dung = u.ma_nguoi_dung
                                                For XML Path('') 
                                                ), 1, 1, ''),'') AS ten_chi_nhanh
                                    from [User] u
                                    left join ( Select * From UserInGroup where id_nhom_nguoi_dung = " + ma_nhom + @") uig on uig.ma_nguoi_dung = u.ma_nguoi_dung 
                                ) data  where  ma_nhom = " + ma_nhom);
                    }
                    return Json(data.ToDataSourceResult(request));
                }
                else
                {
                    return RedirectToAction("NoAccessRights", "Error");
                }
            }
        }
        public ActionResult InsertUserInGroup(string data, string ma_nhom_nguoi_dung)
        {
            if (accessDetail.them || accessDetail.sua)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    dbConn.Delete<Core.Entities.UserInGroup>("id_nhom_nguoi_dung={0}", ma_nhom_nguoi_dung);

                    string[] separators = { "@@" };
                    var listUserId = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var uid in listUserId)
                    {
                        var usr = dbConn.SingleOrDefault<User>("id = " + uid);
                        UserInGroup newdata = new UserInGroup();
                        newdata.ma_nguoi_dung = usr.ma_nguoi_dung;
                        newdata.id_nhom_nguoi_dung = int.Parse(ma_nhom_nguoi_dung);
                        newdata.ngay_tao = DateTime.Now;
                        newdata.nguoi_tao = User.Identity.Name;
                        dbConn.Insert(newdata);

                    }
                    return Json(new { success = true });
                }
            }
            else
            {
                return Json(new { success = false, error = "Bạn không có quyền cập nhật dữ liệu" });
            }
        }
        public ActionResult DeleteListGroup(string data)
        {
            if (accessDetail.xoa)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    string[] separators = { "@@" };
                    var listItem = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in listItem)
                    {
                        //Co UserInGroup thi ko dc xoa
                        var checkAccessRight = dbConn.Select<UserInGroup>(s => s.id_nhom_nguoi_dung == int.Parse(item));
                        if (checkAccessRight != null && checkAccessRight.Count > 0)
                        {
                            return Json(new { success = false, error = "Bạn không có thể xóa nhóm đang được sử dụng" });
                        }
                        else
                        {

                            dbConn.Delete<Core.Entities.Group>("id={0}", item);
                            dbConn.Delete<Core.Entities.AccessRightDetail>("ma_nhom={0}", item);
                        }
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
        public ActionResult Create([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.Group> items)
        {
            if (accessDetail.them)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            var exist = dbConn.SingleOrDefault<Group>("ma_nhom={0}", item.ma_nhom);
                            if (exist != null)
                            {
                                ModelState.AddModelError("", "Mã nhóm này đã tồn tại.");
                                return Json(items.ToDataSourceResult(request, ModelState));
                            }
                            item.ngay_tao = DateTime.Now;
                            item.nguoi_tao = currentUser.ma_nguoi_dung;
                            item.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            item.nguoi_cap_nhat = "";
                            dbConn.Insert(item);

                            var idnhom = (int)dbConn.GetLastInsertId();
                            var accessright = dbConn.Select<Core.Entities.AccessRightDetail>("ma_nhom={0}", 1);
                            accessright.ForEach(s => s.ma_nhom = idnhom);
                            foreach (var accr in accessright)
                            {

                                accr.them = false;
                                accr.xoa = false;
                                accr.sua = false;
                                accr.xem = false;
                                accr.phan_quyen = false;
                                accr.xuat = false;
                                accr.xuat_pdf = false;
                                accr.nhap = false;
                                dbConn.Insert(accr);
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
        public ActionResult Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.Group> items)
        {
            if (accessDetail.sua)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            var exist = dbConn.SingleOrDefault<Group>("id={0}", item.id);
                            if (item.ma_nhom != exist.ma_nhom)
                            {
                                var checkDup = dbConn.SingleOrDefault<Group>("ma_nhom={0}", item.ma_nhom);
                                if (checkDup != null)
                                {
                                    ModelState.AddModelError("", "Mã nhóm này đã tồn tại.");
                                    return Json(items.ToDataSourceResult(request, ModelState));
                                }
                                else
                                {
                                    exist.ma_nhom = item.ma_nhom;
                                }
                            }

                            exist.ten_nhom = item.ten_nhom;
                            exist.ghi_chu = item.ghi_chu;
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
        [HttpPost]
        public ActionResult Excel_Export(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);

            return File(fileContents, contentType, fileName);
        }
        //[DonutOutputCache(CacheProfile = "5Secs")]
        public ActionResult GetListAssets(int Id)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new List<AccessRightDetail>();
                data = dbConn.Select<Core.Entities.AccessRightDetail>("ma_nhom={0}", Id);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getListUser()
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = dbConn.Select<User>("trang_thai='true'").Select(s => new SelectListItem { Value = s.ma_nguoi_dung.ToString(), Text = s.ten_nguoi_dung });
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetAssetsByMenu(int Id, int Id_menu)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new List<Core.Entities.AccessRightDetail>();
                if (Id_menu > 0)
                {
                    data = dbConn.Select<Core.Entities.AccessRightDetail>(@" select d.* from AccessRightDetail d  
                        left join Menu m on m.ma_man_hinh = d.ma_man_hinh where d.ma_nhom={0} and m.id_cha={1}", Id, Id_menu);
                }
                else
                {
                    data = dbConn.Select<Core.Entities.AccessRightDetail>("ma_nhom={0}", Id);
                }
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult UpdateAccessRight([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Core.Entities.AccessRightDetail> items)
        {
            if (accessDetail.sua)
            {
                if (items != null && ModelState.IsValid)
                {
                    using (var dbConn = Helpers.OrmliteConnection.openConn())
                    {
                        foreach (var item in items)
                        {
                            var exist = dbConn.SingleOrDefault<AccessRightDetail>("id={0}", item.id);

                            exist.xem = item.xem;
                            exist.them = item.them;
                            exist.xoa = item.xoa;
                            exist.sua = item.sua;
                            exist.xuat = item.xuat;
                            exist.nhap = item.nhap;
                            exist.phan_quyen = item.phan_quyen;
                            exist.xuat_pdf = item.xuat_pdf;
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