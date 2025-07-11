using HolaMy.Core.Common;
using HolaMyFrontend.Models;
using HolaMyFrontend.Models.BuildingDTOs;
using HolaMyFrontend.Models.RoomTypeDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HolaMyFrontend.Pages.HomePage
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<BuildingDto> Buildings { get; set; } = new();
        public List<RoomTypeListDTO> RoomTypeListDTO { get; set; } = new List<RoomTypeListDTO>();
        public Dictionary<string, List<BuildingDto>> TopWardBuildings { get; set; } = new();
        public string Search { get; set; }
        public List<BuildingDto> TopVipBuildings { get; set; } = new();


        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"api/Building/get-building-list?page=1&pageSize=1000");
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content.ReadFromJsonAsync<ResponseDTO<PagedResult<BuildingDto>>>();

            if (apiResponse != null && apiResponse.StatusCode == 200 && apiResponse.Data != null)
            {
                Buildings = apiResponse.Data.Items ?? new List<BuildingDto>();
            }

            // Nhóm theo ward và chọn 4 ward có nhiều nhà nhất
            TopWardBuildings = Buildings
                .GroupBy(b => b.WardName)
                .OrderByDescending(g => g.Count())
                .Take(4)
                .ToDictionary(g => g.Key, g => g.Take(8).ToList());

            var roomTypeResponse = await client.GetAsync("/Admin/RoomTypeManagement/All");
            if (roomTypeResponse.IsSuccessStatusCode)
            {
                var roomTypeApiResponse = await roomTypeResponse.Content.ReadFromJsonAsync<IEnumerable<RoomTypeListDTO>>();
                RoomTypeListDTO = roomTypeApiResponse?.ToList() ?? new List<RoomTypeListDTO>();
            }

            var vipResponse = await client.GetAsync("api/Building/top-vip-buildings");
            if (vipResponse.IsSuccessStatusCode)
            {
                var vipApiResponse = await vipResponse.Content.ReadFromJsonAsync<ResponseDTO<List<BuildingDto>>>();
                if (vipApiResponse != null && vipApiResponse.StatusCode == 200)
                {
                    TopVipBuildings = vipApiResponse.Data ?? new List<BuildingDto>();
                }
            }


        }

        public class BuildingListResponse
        {
            public BuildingListData? Data { get; set; }
        }

        public class BuildingListData
        {
            public List<BuildingDto> Items { get; set; } = new();
        }

        public class BuildingDto
        {
            public int BuildingId { get; set; }
            public string BuildingName { get; set; } = string.Empty;
            public string ThumbnailUrl { get; set; } = string.Empty;
            public decimal? DisplayPrice { get; set; }
            public string WardName { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
        }
    }
}
