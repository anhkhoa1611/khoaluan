using System.Web.Mvc;

namespace HPSTD.Areas.frontend
{
    public class frontendAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "frontend";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            //context.MapRoute(
            //    "frontend_default",
            //    "frontend/{controller}/{action}/{id}",
            //    new { action = "Index", id = UrlParameter.Optional }
            //);
        }
    }
}