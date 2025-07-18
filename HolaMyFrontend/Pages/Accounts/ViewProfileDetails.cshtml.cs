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

        public async Task<IActionResult> OnPostLoadProfileAsync([FromBody] TokenRequest request)
        {
            if (string.IsNullOrEmpty(request?.Token))
                return BadRequest("Token is required.");

            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.Token);

            try
            {
                var response = await client.GetAsync("api/User/profile");
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // 1. Cấu hình options để không phân biệt chữ hoa/thường
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // 2. Đọc toàn bộ phản hồi vào lớp ApiResponse<JsonElement>
                // Dùng JsonElement để linh hoạt, không cần định nghĩa lại ViewModel ở đây
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(options);

                // 3. Kiểm tra và chỉ trả về phần "data" cho JavaScript
                if (apiResponse != null && apiResponse.Data.ValueKind != JsonValueKind.Undefined)
                {
                    return new ContentResult { Content = apiResponse.Data.GetRawText(), ContentType = "application/json" };
                }

                return new NotFoundObjectResult("User data not found in API response.");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, $"Service Unavailable: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return new UnauthorizedObjectResult("Authorization header is missing.");
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader.ToString());

            // Tạo FormData để gửi đi
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
                // Gọi API cập nhật
                var response = await client.PutAsync("api/User/updateProfile", multiPartContent);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Đọc phản hồi từ API cập nhật (cũng có thể là cấu trúc lồng nhau)
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>(options);

                // Trả về thông báo thành công từ API
                return new OkObjectResult(apiResponse?.Message ?? "Cập nhật thành công!");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, $"Service Unavailable: {ex.Message}");
            }
        }
    }

    // Các lớp DTO cần thiết
    public class TokenRequest
    {
        public string? Token { get; set; }
    }

    public class ApiResponse<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}