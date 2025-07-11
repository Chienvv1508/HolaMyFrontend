using HolaMy.Core.Common;
using HolaMy.Core.DTOs.ReportDTOs;
using HolaMyFrontend.Models;
using HolaMyFrontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace HolaMyFrontend.Pages.Reports
{
    public class CustomerReportsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiSettings _apiSettings;
        private readonly ILogger<CustomerReportsModel> _logger;
        private readonly ApiClientService _apiClientService;

        public CustomerReportsModel(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings, ILogger<CustomerReportsModel> logger, ApiClientService apiClientService)
        {
            _httpClientFactory = httpClientFactory;
            _apiSettings = apiSettings.Value;
            _logger = logger;
            _apiClientService = apiClientService;
        }

        public PagedResult<ReportResponseDto> Reports { get; set; } = new PagedResult<ReportResponseDto>();
        public string ErrorMessage { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; }

        public async Task OnGetAsync(int page = 1, int pageSize = 10, string sortBy = null)
        {
            try
            {
                var (client, errorResult) = _apiClientService.GetAuthorizedClient();
                if (errorResult != null)
                {
                    ErrorMessage = "Vui lòng đăng nhập lại.";
                    TempData["ErrorMessage"] = ErrorMessage;
                    return;
                }

                Page = page;
                PageSize = pageSize;
                SortBy = sortBy;

                var query = $"?page={page}&pageSize={pageSize}";
                if (!string.IsNullOrEmpty(sortBy))
                {
                    query += $"&sortBy={sortBy}";
                }

                var response = await client.GetAsync($"{_apiSettings.BaseUrl}/api/Report/customer{query}");
                var unauthorizedResult = _apiClientService.HandleUnauthorizedResponse(response);
                if (unauthorizedResult != null)
                {
                    ErrorMessage = "Phiên đăng nhập đã hết hạn.";
                    TempData["ErrorMessage"] = ErrorMessage;
                    return;
                }

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<PagedResult<ReportResponseDto>>(jsonString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (apiResponse != null && apiResponse.Items != null)
                    {
                        Reports = apiResponse;
                    }
                    else
                    {
                        ErrorMessage = "Không tìm thấy báo cáo.";
                    }
                }
                else
                {
                    ErrorMessage = $"Lỗi khi gọi API: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching customer reports");
                ErrorMessage = $"Lỗi xảy ra: {ex.Message}";
            }

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                TempData["ErrorMessage"] = ErrorMessage;
            }
        }
    }
}