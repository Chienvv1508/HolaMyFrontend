using HolaMy.Core.DTOs.BuildingDTOs;
using HolaMyFrontend.Models;
using HolaMyFrontend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace HolaMyFrontend.Pages.Reports
{
    public class CreateModel : PageModel
    {
        private readonly ToastService _toastService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiClientService _apiClientService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(ToastService toastService, IHttpClientFactory httpClientFactory, ApiClientService apiClientService, ILogger<CreateModel> logger)
        {
            _toastService = toastService;
            _httpClientFactory = httpClientFactory;
            _apiClientService = apiClientService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string phoneNumber { get; set; }

        [BindProperty(SupportsGet = true)]
        public string UserId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int BuildingId { get; set; }
        [BindProperty]
        public string Reason { get; set; }

        [BindProperty]
        public string Description { get; set; }

        [BindProperty]
        public bool IsAnonymous { get; set; }

        [BindProperty]
        public string ProviderPhone { get; set; }

        public ProviderDTO Provider { get; set; }

        public List<string> ReportReasons { get; } = new()
            {
                "Thông tin sai lệch",
                "Hành vi không phù hợp",
                "Lừa đảo"
            };

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var (client, errorResult) = _apiClientService.GetAuthorizedClient();
                if (errorResult != null)
                {
                    _toastService.ShowError("Lỗi", "Vui lòng đăng nhập lại.");
                    return RedirectToPage("/HomePage/Login");
                }

                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    var response = await client.GetAsync($"api/User/get-by-phone?phone={phoneNumber}");
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        var apiResponse = JsonSerializer.Deserialize<ResponseDTO<ProviderDTO>>(jsonString,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (apiResponse.Data != null)
                        {
                            Provider = apiResponse.Data;
                        }
                        else
                        {
                            _logger.LogWarning("Provider not found for ID {UserId}: {Message}", UserId, apiResponse.Message);
                            _toastService.ShowError("Lỗi", apiResponse.Message ?? "Không tìm thấy chủ trọ.");
                            return RedirectToPage("/Buildings/BuildingDetail", new { id = BuildingId });
                        }
                    }
                    else
                    {
                        _logger.LogError("API call failed for provider ID {UserId}: Status {StatusCode}", UserId, response.StatusCode);
                        _toastService.ShowError("Lỗi", $"Lỗi khi gọi API: {response.StatusCode}");
                        return RedirectToPage("/Buildings/BuildingDetail", new { id = BuildingId });
                    }
                }
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching provider details for ID {UserId}", UserId);
                _toastService.ShowError("Lỗi", $"Lỗi xảy ra: {ex.Message}");
                return RedirectToPage("/Buildings/BuildingDetail", new { id = BuildingId });
            }
        }

        public async Task<IActionResult> OnPostReportLandlordAsync(string reportedOwnerId, int buildingId, string reason, string description, bool isAnonymous, IFormFileCollection evidenceFiles, string providerPhone)
        {
            try
            {
                var (client, errorResult) = _apiClientService.GetAuthorizedClient();

                if (errorResult != null)
                {
                    _toastService.ShowError("Lỗi", "Vui lòng đăng nhập lại.");
                    return RedirectToPage("/HomePage/Login");
                }

                var validReasons = new List<string> { "Thông tin sai lệch", "Hành vi không phù hợp", "Lừa đảo" };
                if (!validReasons.Contains(reason))
                {
                    _toastService.ShowError("Lỗi", "Lý do báo xấu không hợp lệ.");
                    return Page();
                }

                if (string.IsNullOrEmpty(reportedOwnerId) && !string.IsNullOrEmpty(providerPhone))
                {
                    var phoneResponse = await client.GetAsync($"api/User/get-by-phone?phone={Uri.EscapeDataString(providerPhone)}");
                    if (phoneResponse.IsSuccessStatusCode)
                    {
                        var phoneApiResponse = await phoneResponse.Content.ReadFromJsonAsync<ResponseDTO<ProviderDTO>>();
                        if (phoneApiResponse?.Data != null)
                        {
                            Provider = phoneApiResponse.Data;
                            reportedOwnerId = phoneApiResponse.Data.Id.ToString();
                        }
                        else
                        {
                            _toastService.ShowError("Lỗi", "Không tìm thấy chủ trọ với số điện thoại này.");
                            return Page();
                        }
                    }
                    else
                    {
                        _toastService.ShowError("Lỗi", "Lỗi khi tìm kiếm chủ trọ.");
                        return Page();
                    }
                }

                if (string.IsNullOrEmpty(reportedOwnerId))
                {
                    _toastService.ShowError("Lỗi", "Vui lòng cung cấp thông tin chủ trọ.");
                    return Page();
                }
                if (evidenceFiles.Count == 0)
                {
                    _toastService.ShowError("Lỗi", "Vui lòng tải lên ít nhất một ảnh bằng chứng.");
                    return Page();
                }
                if (evidenceFiles.Count > 10)
                {
                    _toastService.ShowError("Lỗi", "Chỉ được gửi tối đa 10 ảnh.");
                    return Page();
                }

                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(reportedOwnerId), "ReportedOwnerId");
                formData.Add(new StringContent(reason), "Reason");
                formData.Add(new StringContent(description ?? ""), "Description");
                formData.Add(new StringContent(isAnonymous.ToString()), "IsAnonymous");
                if (buildingId > 0)
                {
                    formData.Add(new StringContent(buildingId.ToString()), "BuildingId");
                }

                var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                foreach (var file in evidenceFiles)
                {
                    if (file != null && file.Length > 0)
                    {
                        if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
                        {
                            _toastService.ShowError("Lỗi", "Chỉ hỗ trợ định dạng JPEG, PNG, GIF.");
                            return Page();
                        }
                        if (file.Length > 5 * 1024 * 1024)
                        {
                            _toastService.ShowError("Lỗi", "Ảnh không được vượt quá 5MB.");
                            return Page();
                        }

                        var streamContent = new StreamContent(file.OpenReadStream());
                        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                        formData.Add(streamContent, "EvidenceFiles", file.FileName);
                    }
                }

                var response = await client.PostAsync("api/Report/customer", formData);
                var unauthorizedResult = _apiClientService.HandleUnauthorizedResponse(response);
                if (unauthorizedResult != null)
                {
                    _toastService.ShowError("Lỗi", "Phiên đăng nhập đã hết hạn.");
                    return RedirectToPage("/HomePage/Login");
                }

                if (response.IsSuccessStatusCode)
                {
                    _toastService.ShowSuccess("Thành công", "Báo cáo đã được gửi thành công.");
                    return RedirectToPage("/Reports/CustomerReports");
                }
                else
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ResponseDTO<object>>();
                    _toastService.ShowError("Lỗi", errorResponse?.Message ?? "Có lỗi khi gửi báo cáo.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing report for provider ID {ReportedOwnerId}, building ID {BuildingId}", reportedOwnerId, buildingId);
                _toastService.ShowError("Lỗi", $"Lỗi xảy ra: {ex.Message}");
                return Page();
            }
        }

    }
}