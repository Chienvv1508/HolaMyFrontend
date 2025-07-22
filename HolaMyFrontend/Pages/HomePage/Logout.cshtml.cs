using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HolaMyFrontend.Pages.HomePage
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGet()
        {
            Response.Cookies.Delete("jwt");
            Response.Cookies.Delete("userId");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Buildings/BuildingList");
        }
    }
}
