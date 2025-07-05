using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace HolaMyFrontend.Pages.Accounts
{
    [IgnoreAntiforgeryToken(Order = 1001)]
    public class ViewProfileDetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ViewProfileDetailsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public void OnGet() { }

        // Handler này được JavaScript gọi để TẢI DỮ LIỆU.
        public async Task<IActionResult> OnPostLoadProfileAsync([FromBody] TokenRequest request)
        {
            if (string.IsNullOrEmpty(request?.Token)) return BadRequest("Token is required.");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.Token);

            try
            {
                var response = await client.GetAsync("http://localhost:8888/api/User/profile");
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }
                var userProfileJson = await response.Content.ReadAsStringAsync();
                return new ContentResult { Content = userProfileJson, ContentType = "application/json" };
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, $"Service Unavailable: {ex.Message}");
            }
        }

        // Handler này được JavaScript gọi để CẬP NHẬT DỮ LIỆU.
        [HttpPost]
        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                // SỬA LỖI Ở ĐÂY
                return new UnauthorizedObjectResult("Authorization header is missing.");
            }
            
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader.ToString());
            
            using var multiPartContent = new MultipartFormDataContent();
            
            foreach (var item in Request.Form)
            {
                multiPartContent.Add(new StringContent(item.Value), item.Key);
            }
            if (Request.Form.Files.Any())
            {
                var file = Request.Form.Files[0];
                var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                multiPartContent.Add(streamContent, file.Name, file.FileName);
            }

            try
            {
                var response = await client.PutAsync("http://localhost:8888/api/User/updateProfile", multiPartContent);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }
                // SỬA LỖI Ở ĐÂY
                return new OkObjectResult("Cập nhật thành công!");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, $"Service Unavailable: {ex.Message}");
            }
        }
    }

    public class TokenRequest
    {
        // Thêm dấu ? để cho phép giá trị null, giải quyết các cảnh báo CS8618
        public string? Token { get; set; }
    }
}