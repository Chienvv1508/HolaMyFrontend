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

        public BuildingDetailModel(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiSettings = apiSettings.Value;
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
    }
}
