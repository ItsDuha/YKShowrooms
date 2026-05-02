using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace YKShowroomSystem.Controllers
{
    public class LanguageController : Controller
    {
        public IActionResult Set(string culture, string returnUrl = "/")
        {
            // Write a cookie with the selected culture (e.g. "en" or "ar")
            // This cookie is read on every request to know which language to use
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,

                // Format the culture value correctly for the cookie
                CookieRequestCultureProvider.MakeCookieValue(
                    new RequestCulture(culture)),

                // Keep the cookie for 1 year so the user's choice is remembered
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            // Redirect the user back to the page they were on
            return LocalRedirect(returnUrl);
        }
    }
}