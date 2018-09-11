using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HDBank.Controllers
{
    public class PricePolicyDetailController : Controller
    {
        // GET: PricePolicyDetail
        public ActionResult Index()
        {
            return View("PricePolicyDetail");
        }
    }
}