using HolaMy.Core.Common;
using HolaMy.Core.DTOs.BuildingDTOs;
using HolaMyFrontend.Models;
using HolaMyFrontend.Models.AmenityDTOs;
using HolaMyFrontend.Models.BuildingDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace HolaMyFrontend.Pages.Buildings
{
    public class BuildingDetailModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiSettings _apiSettings;
        private readonly ILogger<BuildingDetailModel> _logger;
        public BuildingDetailModel(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings, ILogger<BuildingDetailModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _apiSettings = apiSettings.Value;
            _logger = logger;
        }

        public BuildingDetailDTO Building { get; set; } = new BuildingDetailDTO();
        public List<AmenityListDTO> AmenityListDTO { get; set; } = new List<AmenityListDTO>();
        public string ErrorMessage { get; set; }
        public List<BuildingListDTO> FeaturedBuildings { get; set; } = new List<BuildingListDTO>();
        public int FeatureBuildingNumber { get; set; } = 5;
        public List<BuildingListDTO> RelatedBuildings { get; set; } = new List<BuildingListDTO>();
        public int RelatedBuildingNumber { get; set; } = 3;

        public async Task OnGetAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");

                var response = await client.GetAsync($"api/Building/get-building-detail/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ResponseDTO<BuildingDetailDTO>>(jsonString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (apiResponse.Data != null)
                    {
                        Building = apiResponse.Data;
                    }
                    else
                    {
                        ErrorMessage = apiResponse.Message ?? "Không tìm thấy tòa nhà.";
                    }
                }
                else
                {
                    ErrorMessage = $"Lỗi khi gọi API: {response.StatusCode}";
                }
                var amenityResponse = await client.GetAsync("/Admin/AmenityManagement/All");
                if (amenityResponse.IsSuccessStatusCode)
                {
                    var amenityApiResponse = await amenityResponse.Content.ReadFromJsonAsync<IEnumerable<AmenityListDTO>>();
                    AmenityListDTO = amenityApiResponse?.ToList() ?? new List<AmenityListDTO>();
                }

                // Lấy 5 tòa nhà mới nhất cho phần Nổi bật
                var featuredQuery = $"?page=1&pageSize={FeatureBuildingNumber}&status=Approved&sortBy=created-desc";
                var featuredResponse = await client.GetAsync($"api/Building/get-building-list{featuredQuery}");
                featuredResponse.EnsureSuccessStatusCode();

                var featuredApiResponse = await featuredResponse.Content.ReadFromJsonAsync<ResponseDTO<PagedResult<BuildingListDTO>>>();
                if (featuredApiResponse != null && featuredApiResponse.StatusCode == 200 && featuredApiResponse.Data != null)
                {
                    FeaturedBuildings = featuredApiResponse.Data.Items ?? new List<BuildingListDTO>();
                }

                // Lấy danh sách nhà trọ liên quan (cùng ProviderId, không bao gồm tòa nhà hiện tại)
                if (Building.Provider != null && Building.Provider.Id > 0)
                {
                    var relatedQuery = $"?page=1&pageSize={RelatedBuildingNumber}&status=Approved&providerId={Building.Provider.Id}";
                    var relatedResponse = await client.GetAsync($"api/Building/get-building-list{relatedQuery}");
                    relatedResponse.EnsureSuccessStatusCode();

                    var relatedApiResponse = await relatedResponse.Content.ReadFromJsonAsync<ResponseDTO<PagedResult<BuildingListDTO>>>();
                    if (relatedApiResponse != null && relatedApiResponse.StatusCode == 200 && relatedApiResponse.Data != null)
                    {
                        RelatedBuildings = relatedApiResponse.Data.Items
                            .Where(b => b.BuildingId != id) // Loại bỏ tòa nhà hiện tại
                            .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi xảy ra: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostReportLandlordAsync(int reporterUserId, int buildingId, string reason, string description, bool isAnonymous, IFormFileCollection evidenceFiles)
        {
            try
            {
                // Validate authentication
                if (!User.Identity.IsAuthenticated || string.IsNullOrEmpty(User.FindFirst("sub")?.Value))
                {
                    TempData["ErrorMessage"] = "Bạn cần đăng nhập để gửi báo cáo.";
                    return RedirectToPage(new { id = buildingId });
                }
                string reporterUserIdFromAuth = User.FindFirst("sub")?.Value;
                if (reporterUserId.ToString() != reporterUserIdFromAuth)
                {
                    TempData["ErrorMessage"] = "Thông tin người dùng không hợp lệ.";
                    return RedirectToPage(new { id = buildingId });
                }

                // Validate reason
                var validReasons = new List<string> { "Thông tin sai lệch", "Hành vi không phù hợp", "Lừa đảo" };
                if (!validReasons.Contains(reason))
                {
                    TempData["ErrorMessage"] = "Lý do báo xấu không hợp lệ.";
                    return RedirectToPage(new { id = buildingId });
                }

                // Fetch building to get provider ID
                var client = _httpClientFactory.CreateClient("ApiClient");
                var buildingResponse = await client.GetAsync($"{_apiSettings.BaseUrl}/api/Building/get-building-detail/{buildingId}");
                if (!buildingResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin tòa nhà.";
                    return RedirectToPage(new { id = buildingId });
                }
                var jsonString = await buildingResponse.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ResponseDTO<BuildingDetailDTO>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (apiResponse.Data?.Provider?.Id == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin chủ trọ.";
                    return RedirectToPage(new { id = buildingId });
                }
                int reportedOwnerId = apiResponse.Data.Provider.Id;

                // Validate file uploads
                if (evidenceFiles.Count > 10)
                {
                    TempData["ErrorMessage"] = "Chỉ được gửi tối đa 10 ảnh.";
                    return RedirectToPage(new { id = buildingId });
                }

                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(reporterUserId.ToString()), "reporterUserId");
                formData.Add(new StringContent(reportedOwnerId.ToString()), "reportedOwnerId");
                formData.Add(new StringContent(reason), "reason");
                formData.Add(new StringContent(description ?? ""), "description");
                formData.Add(new StringContent(isAnonymous.ToString()), "isAnonymous");

                var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                foreach (var file in evidenceFiles)
                {
                    if (file != null && file.Length > 0)
                    {
                        if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
                        {
                            TempData["ErrorMessage"] = "Chỉ hỗ trợ định dạng JPEG, PNG, GIF.";
                            return RedirectToPage(new { id = buildingId });
                        }
                        if (file.Length > 5 * 1024 * 1024)
                        {
                            TempData["ErrorMessage"] = "Ảnh không được vượt quá 5MB.";
                            return RedirectToPage(new { id = buildingId });
                        }

                        var streamContent = new StreamContent(file.OpenReadStream());
                        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                        formData.Add(streamContent, "evidenceFiles", file.FileName);
                    }
                }

                // Submit report
                var response = await client.PostAsync($"{_apiSettings.BaseUrl}/api/Report", formData);
                var result = await response.Content.ReadFromJsonAsync<ResponseDTO<object>>();

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = result.Message ?? "Báo cáo đã được gửi thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = result?.Message ?? "Có lỗi khi gửi báo cáo.";
                }

                return RedirectToPage(new { id = buildingId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing report for building ID {buildingId}", buildingId);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage(new { id = buildingId });
            }
        }
    }    
}
