using System.Web.Mvc;

namespace FrogFoot.Areas.FFUser
{
    public class FFUserAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "FFUser";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "FFUser_default",
                "FFUser/{controller}/{action}/{id}",
                new { controller = "FFUser", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}