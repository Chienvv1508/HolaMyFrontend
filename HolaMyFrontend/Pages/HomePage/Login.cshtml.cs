using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.Json;
using HolaMyFrontend.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

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

        
        public async  Task<IActionResult> OnGet(string token = null)
        {
            if (!string.IsNullOrEmpty(token))
            {
                ViewData["Token"] = token;
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var claims = jwt.Claims.ToList();
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
                Response.Cookies.Append("jwt", token, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(7)
                });
                
                Response.Cookies.Append("userId",claims.FirstOrDefault(x => x.Type == "UserId").Value, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(7)
                });
                return role switch
                {
                    "Admin" => Redirect("/AdminPage"),
                    "Provider" => Redirect("/ProviderPage"),
                    "Customer" => Redirect("/Buildings/BuildingList"),
                    _ => RedirectToPage("/AccessDenied")
                };



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
