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
            Response.Cookies.Delete("abc");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/HomePage/Login");
        }
    }
}
