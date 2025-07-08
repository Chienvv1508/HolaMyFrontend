using HolaMy.Core.Common;
using HolaMy.Core.DTOs.BuildingDTOs;
using HolaMyFrontend.Models;
using HolaMyFrontend.Models.AmenityDTOs;
using HolaMyFrontend.Models.BuildingDTOs;
using HolaMyFrontend.Services;
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
        private readonly ApiClientService _apiClientService;
        public BuildingDetailModel(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings, ILogger<BuildingDetailModel> logger, ApiClientService apiClientService)
        {
            _httpClientFactory = httpClientFactory;
            _apiSettings = apiSettings.Value;
            _logger = logger;
            _apiClientService = apiClientService;
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

        public async Task<IActionResult> OnPostReportLandlordAsync(int reportedOwnerId, int buildingId, string reason, string description, bool isAnonymous, IFormFileCollection evidenceFiles)
        {
            try
            {
                var (client, errorResult) = _apiClientService.GetAuthorizedClient();
                if (errorResult != null)
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập lại.";
                    return RedirectToPage("/HomePage/Login");
                }

                // Validate reason
                var validReasons = new List<string> { "Thông tin sai lệch", "Hành vi không phù hợp", "Lừa đảo" };
                if (!validReasons.Contains(reason))
                {
                    TempData["ErrorMessage"] = "Lý do báo xấu không hợp lệ.";
                    return RedirectToPage(new { id = buildingId });
                }
                
                // Validate file uploads
                if (evidenceFiles.Count > 10)
                {
                    TempData["ErrorMessage"] = "Chỉ được gửi tối đa 10 ảnh.";
                    return RedirectToPage(new { id = buildingId });
                }

                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(reportedOwnerId.ToString()), "ReportedOwnerId");
                formData.Add(new StringContent(reason), "Reason");
                formData.Add(new StringContent(description ?? ""), "Description");
                formData.Add(new StringContent(isAnonymous.ToString()), "IsAnonymous");
                
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
                        formData.Add(streamContent, "EvidenceFiles", file.FileName);
                    }
                }

                var response = await client.PostAsync($"{_apiSettings.BaseUrl}/api/Report", formData);
                var unauthorizedResult = _apiClientService.HandleUnauthorizedResponse(response);
                if (unauthorizedResult != null)
                {
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToPage("/HomePage/Login");
                }

                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadFromJsonAsync<ResponseDTO<object>>();
                Console.WriteLine($"OnPOSTAsync: Raw API Response: {jsonResponse}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Báo cáo đã được gửi thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi khi gửi báo cáo.";
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
