using HolaMyFrontend.Models;
using HolaMyFrontend.Models.AmenityDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace HolaMyFrontend.Pages.Buildings
{
    public class CreateBuildingModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiSettings _apiSettings;

        public CreateBuildingModel(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiSettings = apiSettings.Value;
        }

        public List<AmenityDTO> Amenities { get; set; } = new List<AmenityDTO>();

        public async Task OnGetAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");

                // Gọi API lấy danh sách amenity
                var response = await client.GetAsync("admin/AmenityManagement/All");
                response.EnsureSuccessStatusCode();

                // Giả sử API trả về kiểu ResponseDTO<IEnumerable<AmenityDTO>>
                var apiResponse = await response.Content.ReadFromJsonAsync<ResponseDTO<IEnumerable<AmenityDTO>>>();
                if (apiResponse != null && apiResponse.StatusCode == 200)
                {
                    Amenities = apiResponse.Data?.ToList() ?? new List<AmenityDTO>();
                }
                else
                {
                    Amenities = new List<AmenityDTO>();
                    Console.WriteLine($"Lỗi API: {apiResponse?.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gọi API Amenity: {ex.Message}");
                Amenities = new List<AmenityDTO>();
            }
        }
    }
}
