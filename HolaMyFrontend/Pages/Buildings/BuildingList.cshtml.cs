using HolaMyFrontend.Models.BuildingDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HolaMyFrontend.Models;
using Microsoft.Extensions.Options;

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
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalPages { get; set; } = 1;
        public string Search { get; set; } = string.Empty;
        public string SortBy { get; set; } = string.Empty;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? RoomTypeId { get; set; }
        public int? WardId { get; set; }

        public List<WardDTO> Wards { get; set; } = new List<WardDTO>();
        public async Task OnGetAsync(int page = 1, string search = null, string sortBy = null, decimal? minPrice = null, decimal? maxPrice = null, int? roomTypeId = null, int? wardId = null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");

                CurrentPage = page;
                Search = search;
                SortBy = sortBy;
                MinPrice = minPrice;
                MaxPrice = maxPrice;
                RoomTypeId = roomTypeId;
                WardId = wardId;

                // Tạo query string cho API
                var query = $"?page={CurrentPage}&pageSize={PageSize}";
                if (!string.IsNullOrEmpty(Search)) query += $"&search={Uri.EscapeDataString(Search)}";
                if (!string.IsNullOrEmpty(SortBy)) query += $"&sortBy={SortBy}";
                if (MinPrice.HasValue) query += $"&minPrice={MinPrice.Value}";
                if (MaxPrice.HasValue) query += $"&maxPrice={MaxPrice.Value}";
                if (RoomTypeId.HasValue) query += $"&roomTypeId={RoomTypeId.Value}";
                if (WardId.HasValue) query += $"&wardId={WardId.Value}";

                // Gọi API danh sách tòa nhà
                var response = await client.GetAsync($"api/Building/get-building-list{query}");
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ResponseDTO<IEnumerable<BuildingListDTO>>>();
                if (apiResponse != null && apiResponse.StatusCode == 200)
                {
                    Buildings = apiResponse.Data?.ToList() ?? new List<BuildingListDTO>();
                    // Giả định API trả về tổng số bản ghi trong header hoặc cần thêm logic phân trang
                    TotalPages = (int)Math.Ceiling((double)(Buildings.Count > 0 ? 100 : 0) / PageSize); // Thay 100 bằng tổng số bản ghi từ API
                }
                else
                {
                    Buildings = new List<BuildingListDTO>();
                    Console.WriteLine($"Lỗi API: {apiResponse?.Message}");
                }

                // Gọi API danh sách phường/xã
                var wardResponse = await client.GetAsync("api/ward");
                if (wardResponse.IsSuccessStatusCode)
                {
                    var wardApiResponse = await wardResponse.Content.ReadFromJsonAsync<ResponseDTO<IEnumerable<WardDTO>>>();
                    if (wardApiResponse?.StatusCode == 200)
                    {
                        Wards = wardApiResponse.Data?.ToList() ?? new List<WardDTO>();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gọi API: {ex.Message}");
                Buildings = new List<BuildingListDTO>();
            }
        }
    }
}
