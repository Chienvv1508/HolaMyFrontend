using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HolaMyFrontend.Pages.ProviderPage
{
    [Authorize(Roles = "Provider")]
    public class IndexModel : PageModel
    {
        
        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
