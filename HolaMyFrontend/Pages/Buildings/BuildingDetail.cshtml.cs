using HolaMy.Core.DTOs.BuildingDTOs;
using HolaMyFrontend.Models;
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
        public string ErrorMessage { get; set; }

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
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi xảy ra: {ex.Message}";
            }
        }
    }
}
