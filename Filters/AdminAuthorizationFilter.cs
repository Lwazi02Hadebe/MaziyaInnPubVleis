using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MaziyaInnPubVleis.Filters
{
    public class AdminAuthorizationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("AdminUserId");
            var role = context.HttpContext.Session.GetString("AdminRole");

            if (userId == null || string.IsNullOrEmpty(role))
            {
                context.Result = new RedirectToActionResult("AdminLogin", "Auth", new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            // Check for specific role requirements if needed
            var controllerName = context.RouteData.Values["controller"]?.ToString();
            if (controllerName == "Manager" && role != "Manager" && role != "Admin")
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }
    }
}