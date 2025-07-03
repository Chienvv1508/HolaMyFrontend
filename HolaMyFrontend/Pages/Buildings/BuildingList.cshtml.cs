using HolaMyFrontend.Models.BuildingDTOs;
using HolaMyFrontend.Models.RoomTypeDTOs;
using HolaMyFrontend.Models.AmenityDTOs;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using HolaMyFrontend.Models;
using HolaMy.Core.Common;

namespace HolaMyFrontend.Pages.Buildings
{
    public class BuildingListModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiSettings _apiSettings;

        public BuildingListModel(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiSettings = apiSettings.Value;
        }

        public List<BuildingListDTO> Buildings { get; set; } = new List<BuildingListDTO>();
        public List<BuildingListDTO> FeaturedBuildings { get; set; } = new List<BuildingListDTO>();
        public int FeatureBuildingNumber { get; set; } = 5;
        public List<RoomTypeListDTO> RoomTypeListDTO { get; set; } = new List<RoomTypeListDTO>();
        public List<AmenityListDTO> AmenityListDTO { get; set; } = new List<AmenityListDTO>();
        public List<WardDTO> Wards { get; set; } = new List<WardDTO>();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalPages { get; set; } = 1;
        public string Search { get; set; } = string.Empty;
        public string SortBy { get; set; } = string.Empty;
        public List<int>? WardIds { get; set; }
        public List<int>? RoomTypeIds { get; set; }
        public List<string>? PriceRanges { get; set; }
        public List<int>? AmenityIds { get; set; }
        public bool WardFilterOpen { get; set; }

        public async Task OnGetAsync(int page = 1, string search = null, string sortBy = null,
            List<int> wardIds = null, List<int> roomTypeIds = null, List<string> priceRanges = null,
            List<int> amenityIds = null, bool wardFilterOpen = false, string status = "Approved")
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");

                CurrentPage = page;
                Search = search ?? string.Empty;
                SortBy = sortBy ?? string.Empty;
                WardIds = wardIds ?? new List<int>();
                RoomTypeIds = roomTypeIds ?? new List<int>();
                PriceRanges = priceRanges ?? new List<string>();
                AmenityIds = amenityIds ?? new List<int>();
                WardFilterOpen = wardFilterOpen;

                // Lấy danh sách tòa nhà chính
                var queryParams = new List<string> { $"page={CurrentPage}", $"pageSize={PageSize}" };
                if (!string.IsNullOrEmpty(status))
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");
                if (!string.IsNullOrEmpty(Search))
                    queryParams.Add($"search={Uri.EscapeDataString(Search)}");
                if (!string.IsNullOrEmpty(SortBy))
                    queryParams.Add($"sortBy={SortBy}");
                if (WardIds.Any())
                    queryParams.AddRange(WardIds.Select(id => $"wardIds={id}"));
                if (RoomTypeIds.Any())
                    queryParams.AddRange(RoomTypeIds.Select(id => $"roomTypeIds={id}"));
                if (PriceRanges.Any())
                    queryParams.AddRange(PriceRanges.Select(pr => $"priceRanges={Uri.EscapeDataString(pr)}"));
                if (AmenityIds.Any())
                    queryParams.AddRange(AmenityIds.Select(id => $"amenityIds={id}"));

                var query = $"?{string.Join("&", queryParams)}";
                var response = await client.GetAsync($"api/Building/get-building-list{query}");
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ResponseDTO<PagedResult<BuildingListDTO>>>();
                if (apiResponse != null && apiResponse.StatusCode == 200 && apiResponse.Data != null)
                {
                    Buildings = apiResponse.Data.Items ?? new List<BuildingListDTO>();
                    TotalPages = apiResponse.Data.TotalPages;
                    CurrentPage = apiResponse.Data.PageNumber;
                    PageSize = apiResponse.Data.PageSize;
                }
                else
                {
                    Buildings = new List<BuildingListDTO>();
                    TotalPages = 1;
                    Console.WriteLine($"Lỗi API: {apiResponse?.Message}");
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

                // Lấy danh sách Ward
                var wardResponse = await client.GetAsync("api/ward");
                if (wardResponse.IsSuccessStatusCode)
                {
                    var wardApiResponse = await wardResponse.Content.ReadFromJsonAsync<ResponseDTO<IEnumerable<WardDTO>>>();
                    if (wardApiResponse?.StatusCode == 200)
                    {
                        Wards = wardApiResponse.Data?.ToList() ?? new List<WardDTO>();
                    }
                }

                // Lấy danh sách RoomType
                var roomTypeResponse = await client.GetAsync("/Admin/RoomTypeManagement/All");
                if (roomTypeResponse.IsSuccessStatusCode)
                {
                    var roomTypeApiResponse = await roomTypeResponse.Content.ReadFromJsonAsync<IEnumerable<RoomTypeListDTO>>();
                    RoomTypeListDTO = roomTypeApiResponse?.ToList() ?? new List<RoomTypeListDTO>();
                }

                // Lấy danh sách Amenity
                var amenityResponse = await client.GetAsync("/Admin/AmenityManagement/All");
                if (amenityResponse.IsSuccessStatusCode)
                {
                    var amenityApiResponse = await amenityResponse.Content.ReadFromJsonAsync<IEnumerable<AmenityListDTO>>();
                    AmenityListDTO = amenityApiResponse?.ToList() ?? new List<AmenityListDTO>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gọi API: {ex.Message}");
                Buildings = new List<BuildingListDTO>();
                FeaturedBuildings = new List<BuildingListDTO>();
                Wards = new List<WardDTO>();
                RoomTypeListDTO = new List<RoomTypeListDTO>();
                AmenityListDTO = new List<AmenityListDTO>();
            }
        }
    }
}
