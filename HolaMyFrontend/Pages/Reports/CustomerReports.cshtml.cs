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
        private readonly ToastService _toastService;
        private readonly string[] _validSortOptions = { "created-desc", "created-asc", "status" };

        public CustomerReportsModel(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings, ILogger<CustomerReportsModel> logger, ApiClientService apiClientService, ToastService toastService)
        {
            _httpClientFactory = httpClientFactory;
            _apiSettings = apiSettings.Value;
            _logger = logger;
            _apiClientService = apiClientService;
            _toastService = toastService;
        }

        public PagedResult<ReportResponseDto> Reports { get; set; } = new PagedResult<ReportResponseDto>();
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
                    _toastService.ShowError("Lỗi", "Vui lòng đăng nhập lại.");
                    return;
                }

                Page = Math.Max(1, page);
                PageSize = Math.Max(5, Math.Min(20, pageSize));
                SortBy = _validSortOptions.Contains(sortBy) ? sortBy : "created-desc";

                var query = $"?page={Page}&pageSize={PageSize}";
                if (!string.IsNullOrEmpty(SortBy))
                {
                    query += $"&sortBy={Uri.EscapeDataString(SortBy)}";
                }

                var response = await client.GetAsync($"{_apiSettings.BaseUrl}/api/Report/customer{query}");
                var unauthorizedResult = _apiClientService.HandleUnauthorizedResponse(response);
                if (unauthorizedResult != null)
                {
                    _toastService.ShowError("Lỗi", "Phiên đăng nhập đã hết hạn.");
                    return;
                }

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<PagedResult<ReportResponseDto>>(jsonString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    Reports = apiResponse ?? new PagedResult<ReportResponseDto>();
                    if (!Reports.Items.Any())
                    {
                        _toastService.ShowError("Thông báo", "Không tìm thấy báo cáo.");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("API call failed: Status {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    _toastService.ShowError("Lỗi", $"Lỗi khi gọi API: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching customer reports for page {Page}, pageSize {PageSize}", page, pageSize);
                _toastService.ShowError("Lỗi", "Lỗi xảy ra khi tải danh sách báo cáo.");
            }
        }
    }
}