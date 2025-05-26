using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HolaMyFrontend.Pages.HomePage
{
    public class LoginModel : PageModel
    {
        public IActionResult OnGet(string token = null)
        {
            if (!string.IsNullOrEmpty(token))
            {
                ViewData["Token"] = token;
            }
            return Page();
        }
        public IActionResult OnPostGoogle()
        {
            return Redirect("http://localhost:9999/api/User/google-login");
        }
    }
}
