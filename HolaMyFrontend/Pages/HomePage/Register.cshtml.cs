using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HolaMyFrontend.Pages.HomePage
{
    public class RegisterModel : PageModel
    {
        public IActionResult OnGet()
        {
            var token = Request.Cookies["jwt"];
            if (!string.IsNullOrEmpty(token))
            {
              return  Redirect("/Buildings/BuildingList");
            }

            return Page();
        }
    }
}
