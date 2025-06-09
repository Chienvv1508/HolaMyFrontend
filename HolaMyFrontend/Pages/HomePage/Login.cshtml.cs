using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.Json;
using HolaMyFrontend.Models;

namespace HolaMyFrontend.Pages.HomePage
{
    public class LoginModel : PageModel
    {
        private readonly IConfiguration _configuration;
        [BindProperty]
        public LoginPhone loginPhone { get; set; }

        private readonly HttpClient _httpClient;
        public LoginModel(IConfiguration configuration, IHttpClientFactory httpClientFactory )
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
        }

        
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
            var backendurl = _configuration["ApiUrl:Google-Login"];
            return Redirect(backendurl);
        }
        public async Task<IActionResult> OnPostPhone()
        {
           
            var phoneLoginURL = _configuration["ApiUrl:Phone-Login"];
            var response = await _httpClient.PostAsJsonAsync(phoneLoginURL, loginPhone);
            if (response.IsSuccessStatusCode)
            {
                var data1 = await response.Content.ReadAsStringAsync();
              var  result = JsonSerializer.Deserialize<ResponseData<string>>(data1);
                return RedirectToPage(new { token = result.data });
            }
            var data = await response.Content.ReadAsStringAsync();
            var rs = JsonSerializer.Deserialize<ResponseData<string>>(data);
            ModelState.AddModelError(string.Empty, rs.message);
            return Page();
        }
    }
}
