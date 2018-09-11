using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HPSTD.Areas.frontend.Controllers
{
    public class HomeController : Controller
    {
        // GET: frontend/Home
        public ActionResult Index()
        {
            return View("Home");
        }
    }
}