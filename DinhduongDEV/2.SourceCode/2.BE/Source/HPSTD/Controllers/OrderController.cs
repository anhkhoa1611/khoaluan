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


namespace HPSTD.Controllers
{
    [Authorize]
    public class OrderController : CustomController
    {
        // GET: Order
        public ActionResult Index()
        {
            if (accessDetail.xem)
            {
                using (var dbConn = Helpers.OrmliteConnection.openConn())
                {
                    ViewBag.listDonVi = dbConn.Select<DepartmentHeirarchy>("cap=3");
                    ViewBag.listProduct = dbConn.Select<Product>();
                    ViewBag.listDonViTinh = dbConn.Select<Parameters>("loai_tham_so ='DonVITinh'");
                    return View();
                }
                    
            }
            return RedirectToAction("NoAccessRights", "Error");
        }
        public ActionResult Read([DataSourceRequest]DataSourceRequest request)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new DataSourceResult();
                data = KendoApplyFilter.KendoData<OrderHeader>(request);
                return Json(data);
            }
        }
        public ActionResult ReadDetail([DataSourceRequest]DataSourceRequest request, int order_id_header)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var data = new DataSourceResult();
                data = KendoApplyFilter.KendoData<OrderDetail>(request, "order_id_header={0}".Params(order_id_header));
                return Json(data);
            }
        }
        public ActionResult CreateUpdate(OrderHeader data)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    int id = 0;
                    if (data.id > 0)
                    {
                        if (accessDetail.sua)
                        {
                            var exist = dbConn.SingleOrDefault<OrderHeader>("id={0} ", data.id);
                            exist.ngay_cap_nhat = DateTime.Now;
                            exist.nguoi_cap_nhat = currentUser.ma_nguoi_dung;
                            exist.trang_thai = !string.IsNullOrEmpty(data.trang_thai) ? data.trang_thai : exist.trang_thai;
                            exist.ten_don_hang = data.ten_don_hang;
                            exist.ma_don_vi = data.ma_don_vi;
                            data.ngay_to_trinh = !string.IsNullOrEmpty(Request["ngay_to_trinh"]) ? DateTime.Parse(DateTime.ParseExact(Request["ngay_to_trinh"], "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")) : DateTime.Parse("1900-01-01");
                            //exist.ma_nhom_san_pham = data.ma_nhom_san_pham;
                            dbConn.Update(exist);
                            id = data.id;
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
                            var exist = dbConn.SingleOrDefault<DepartmentHeirarchy>("id={0}", data.ma_don_vi);
                            var existLast = dbConn.SingleOrDefault<OrderHeader>("SELECT TOP 1 * FROM OrderHeader ORDER BY id DESC");
                            var ma_tang = "";
                            if (existLast != null)
                            {
                                var ma = int.Parse(existLast.so_don_hang.Substring(0, 2)) + 1;
                                ma_tang = ma < 9 ? "0" + ma : ma.ToString();
                            }
                            else
                            {
                                ma_tang = "01";
                            }
                            data.so_don_hang = ma_tang + "." + DateTime.Now.ToString("MM/yy/") + exist.ten_phan_cap;
                            data.ngay_to_trinh = !string.IsNullOrEmpty(Request["ngay_to_trinh"]) ? DateTime.Parse(DateTime.ParseExact(Request["ngay_to_trinh"], "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")) : DateTime.Parse("1900-01-01");
                            data.ngay_tao = DateTime.Now;
                            data.nguoi_tao = currentUser.ma_nguoi_dung;
                            data.ngay_cap_nhat = DateTime.Parse("1900-01-01");
                            data.nguoi_cap_nhat = "";
                            data.trang_thai = "MOI";
                            dbConn.Insert(data);
                            id = (int)dbConn.GetLastInsertId();
                        }
                        else
                        {
                            return Json(new { success = false, error = "Bạn không có quyền thêm dữ liệu" });
                        }
                    }
                    return Json(new { success = true, id = id });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = ex.Message });
                }
            }
        }
    }
}