using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using HolaMyFrontend.Models;

namespace HolaMyFrontend.Services
{
    public class ApiClientService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApiSettings _apiSettings;

        public ApiClientService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _apiSettings = apiSettings.Value;
        }

        public (HttpClient Client, IActionResult? ErrorResult) GetAuthorizedClient()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt"];

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Không tìm thấy token xác thực trong cookie 'jwt'.");
                return (client, new JsonResult(new { success = false, message = "Vui lòng đăng nhập lại.", redirect = "/HomePage/Login" }));
            }

            Console.WriteLine($"Token từ cookie: {token.Substring(0, Math.Min(token.Length, 20))}...");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return (client, null);
        }

        public IActionResult HandleUnauthorizedResponse(HttpResponseMessage response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("Lỗi 401: Token không hợp lệ hoặc người dùng chưa đăng nhập.");
                return new JsonResult(new { success = false, message = "Phiên đăng nhập đã hết hạn.", redirect = "/HomePage/Login" });
            }
            return null;
        }
    }
}