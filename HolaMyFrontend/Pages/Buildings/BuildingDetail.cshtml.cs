using HolaMy.Core.Common;
using HolaMy.Core.DTOs.BuildingDTOs;
using HolaMyFrontend.Models;
using HolaMyFrontend.Models.AmenityDTOs;
using HolaMyFrontend.Models.BuildingDTOs;
using HolaMyFrontend.Models.CommentDTOs;
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

        [BindProperty]
        public List<CommentDTO> BuildingComments { get; set; } = new List<CommentDTO>();

        [BindProperty]
        public CreateCommentDTO NewComment { get; set; } = new CreateCommentDTO();

        public async Task OnGetAsync(int id)
        {
            try
            {

                var client = _httpClientFactory.CreateClient("ApiClient");
                var (authClient, errorResult, userId) = _apiClientService.GetAuthorizedClientWithUserId();
                if (userId.HasValue)
                {
                    ViewData["UserId"] = userId.Value;
                }
                else
                {
                    ViewData["UserId"] = 0; // Default for anonymous users
                }
                ViewData["UserId"] = userId;

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
                // Lấy danh sách bình luận
                var commentApiUrl = $"{_apiSettings.BaseUrl}/api/comment/building/{id}";
                var commentResponse = await client.GetFromJsonAsync<List<CommentDTO>>(commentApiUrl);
                if (commentResponse != null)
                {
                    BuildingComments = commentResponse;
                }
                else
                {
                    BuildingComments = new List<CommentDTO>();
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
        
        public async Task<IActionResult> OnPostCreateCommentAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["CommentErrorMessage"] = "Dữ liệu nhập không hợp lệ.";
                    await OnGetAsync(NewComment.BuildingId ?? 0);
                    return Page();
                }

                var (client, errorResult) = _apiClientService.GetAuthorizedClient();
                if (errorResult != null)
                {
                    TempData["CommentErrorMessage"] = "Vui lòng đăng nhập để gửi bình luận.";
                    return RedirectToPage("/HomePage/Login");
                }

                // Validate inputs
                if (NewComment.BuildingId == null || NewComment.BuildingId == 0)
                {
                    TempData["CommentErrorMessage"] = "Mã tòa nhà không hợp lệ.";
                    await OnGetAsync(NewComment.BuildingId ?? 0);
                    return Page();
                }

                if (NewComment.UserId == null || NewComment.UserId == 0)
                {
                    TempData["CommentErrorMessage"] = "Vui lòng đăng nhập để gửi bình luận.";
                    return RedirectToPage("/HomePage/Login");
                }

                if (string.IsNullOrWhiteSpace(NewComment.Content) && NewComment.ContentType == CommentContentType.text)
                {
                    TempData["CommentErrorMessage"] = "Nội dung bình luận không được để trống.";
                    await OnGetAsync(NewComment.BuildingId ?? 0);
                    return Page();
                }

                if ((NewComment.ContentType == CommentContentType.image || NewComment.ContentType == CommentContentType.text_image) && NewComment.File == null)
                {
                    TempData["CommentErrorMessage"] = "Vui lòng chọn tệp hình ảnh cho loại nội dung này.";
                    await OnGetAsync(NewComment.BuildingId ?? 0);
                    return Page();
                }

                if (NewComment.Rating.HasValue && (NewComment.Rating < 1 || NewComment.Rating > 5))
                {
                    TempData["CommentErrorMessage"] = "Điểm đánh giá phải từ 1 đến 5.";
                    await OnGetAsync(NewComment.BuildingId ?? 0);
                    return Page();
                }

                var apiUrl = $"{_apiSettings.BaseUrl}/api/comment";
                var content = new MultipartFormDataContent();

                if (NewComment.File != null)
                {
                    var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                    if (!allowedContentTypes.Contains(NewComment.File.ContentType.ToLower()))
                    {
                        TempData["CommentErrorMessage"] = "Chỉ hỗ trợ định dạng JPEG, PNG, GIF.";
                        await OnGetAsync(NewComment.BuildingId ?? 0);
                        return Page();
                    }
                    if (NewComment.File.Length > 5 * 1024 * 1024)
                    {
                        TempData["CommentErrorMessage"] = "Tệp không được vượt quá 5MB.";
                        await OnGetAsync(NewComment.BuildingId ?? 0);
                        return Page();
                    }

                    var fileContent = new StreamContent(NewComment.File.OpenReadStream());
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(NewComment.File.ContentType);
                    content.Add(fileContent, "File", NewComment.File.FileName);
                }

                content.Add(new StringContent(NewComment.BuildingId.ToString()), "BuildingId");
                content.Add(new StringContent(NewComment.UserId.ToString()), "UserId");
                content.Add(new StringContent(NewComment.Content ?? ""), "Content");
                content.Add(new StringContent(NewComment.ContentType.ToString()), "ContentType");
                if (NewComment.Rating.HasValue)
                {
                    content.Add(new StringContent(NewComment.Rating.Value.ToString()), "Rating");
                }

                var response = await client.PostAsync(apiUrl, content);
                var unauthorizedResult = _apiClientService.HandleUnauthorizedResponse(response);
                if (unauthorizedResult != null)
                {
                    TempData["CommentErrorMessage"] = "Phiên đăng nhập đã hết hạn.";
                    return RedirectToPage("/HomePage/Login");
                }

                if (response.IsSuccessStatusCode)
                {
                    TempData["CommentSuccessMessage"] = "Bình luận đã được gửi thành công.";
                    return RedirectToPage(new { id = NewComment.BuildingId });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["CommentErrorMessage"] = $"Lỗi khi gửi bình luận: {errorContent}";
                    await OnGetAsync(NewComment.BuildingId ?? 0);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating comment for building ID {BuildingId}", NewComment.BuildingId);
                TempData["CommentErrorMessage"] = $"Lỗi xảy ra: {ex.Message}";
                await OnGetAsync(NewComment.BuildingId ?? 0);
                return Page();
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
