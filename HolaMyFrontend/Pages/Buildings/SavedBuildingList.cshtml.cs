using HolaMyFrontend.Models;
using HolaMyFrontend.Models.BuildingDTOs;
using HolaMyFrontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text.Json;

namespace HolaMyFrontend.Pages.Buildings
{
    public class SavedBuildingListModel : PageModel
    {
        private readonly ApiClientService _apiClientService;
        private readonly ApiSettings _apiSettings;

        public SavedBuildingListModel(ApiClientService apiClientService, IOptions<ApiSettings> apiSettings)
        {
            _apiClientService = apiClientService;
            _apiSettings = apiSettings.Value;
        }

        public List<SavedBuildingResponseDTO> SavedBuildings { get; set; } = new List<SavedBuildingResponseDTO>();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalPages { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync(int page = 1)
        {
            try
            {
                var (client, errorResult) = _apiClientService.GetAuthorizedClient();
                if (errorResult != null)
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập lại.";
                    return RedirectToPage("/HomePage/Login");
                }

                CurrentPage = page;
                var query = $"?page={CurrentPage}&pageSize={PageSize}";
                var response = await client.GetAsync($"{_apiSettings.BaseUrl}/api/Building/save-building{query}");
                Console.WriteLine($"OnGetAsync: API Request - {_apiSettings.BaseUrl}/api/Building/save-building{query}");

                var unauthorizedResult = _apiClientService.HandleUnauthorizedResponse(response);
                if (unauthorizedResult != null)
                {
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToPage("/HomePage/Login");
                }

                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"OnGetAsync: Raw API Response: {jsonResponse}");

                var apiResponse = await response.Content.ReadFromJsonAsync<ResponseDTO<IEnumerable<SavedBuildingResponseDTO>>>();
                if (apiResponse != null && apiResponse.StatusCode == 200)
                {
                    SavedBuildings = apiResponse.Data?.ToList() ?? new List<SavedBuildingResponseDTO>();
                    TotalPages = (int)Math.Ceiling((double)(SavedBuildings.Count > 0 ? SavedBuildings.Count : 0) / PageSize);
                }
                else
                {
                    SavedBuildings = new List<SavedBuildingResponseDTO>();
                    Console.WriteLine($"OnGetAsync: Lỗi API: {apiResponse?.Message}");
                    TempData["ErrorMessage"] = apiResponse?.Message ?? "Lỗi khi tải danh sách nhà trọ.";
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"OnGetAsync: Lỗi deserialization JSON: {ex.Message}");
                SavedBuildings = new List<SavedBuildingResponseDTO>();
                TempData["ErrorMessage"] = "Lỗi định dạng dữ liệu từ server.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnGetAsync: Lỗi khi gọi API: {ex.Message}");
                SavedBuildings = new List<SavedBuildingResponseDTO>();
                TempData["ErrorMessage"] = "Lỗi khi tải danh sách nhà trọ: " + ex.Message;
            }

            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync(int buildingId, int? roomId)
        {
            try
            {
                var (client, errorResult) = _apiClientService.GetAuthorizedClient();
                if (errorResult != null)
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập lại.";
                    return RedirectToPage("/HomePage/Login");
                }

                var request = new SavedBuildingRequestDTO
                {
                    BuildingId = buildingId,
                    RoomId = roomId
                };
                var response = await client.PostAsJsonAsync($"{_apiSettings.BaseUrl}/api/Building/save-building", request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Đã lưu vào danh sách yêu thích!";
                    return RedirectToPage(new { id = buildingId });
                }
                else
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ResponseDTO<object>>();
                    TempData["ErrorMessage"] = errorResponse?.Message ?? "Lỗi khi lưu vào danh sách.";
                    return RedirectToPage();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToPage();
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteAsync(int savedId)
        {
            try
            {
                var (client, errorResult) = _apiClientService.GetAuthorizedClient();
                if (errorResult != null)
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập lại.";
                    return RedirectToPage("/HomePage/Login");
                }

                var response = await client.DeleteAsync($"{_apiSettings.BaseUrl}/api/Building/save-building/{savedId}");
                Console.WriteLine($"OnPostDeleteAsync: API Response Status - {response.StatusCode}");
                var rawResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"OnPostDeleteAsync: Raw API Response - {rawResponse}");

                var unauthorizedResult = _apiClientService.HandleUnauthorizedResponse(response);
                if (unauthorizedResult != null)
                {
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToPage("/HomePage/Login");
                }

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Xóa nhà trọ yêu thích thành công.";
                    return RedirectToPage();
                }

                try
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ResponseDTO<object>>();
                    Console.WriteLine($"OnPostDeleteAsync: API Error - {errorResponse?.Message}");
                    TempData["ErrorMessage"] = errorResponse?.Message ?? "Lỗi khi xóa nhà trọ.";
                }
                catch (JsonException)
                {
                    Console.WriteLine($"OnPostDeleteAsync: Invalid JSON response - {rawResponse}");
                    TempData["ErrorMessage"] = "Lỗi khi xóa nhà trọ: Phản hồi từ server không hợp lệ.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnPostDeleteAsync: Exception - {ex.Message}");
                TempData["ErrorMessage"] = "Lỗi khi xóa nhà trọ: " + ex.Message;
            }

            return RedirectToPage();
        }
    }
}