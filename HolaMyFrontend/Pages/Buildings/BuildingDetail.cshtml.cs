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
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace HolaMyFrontend.Pages.Buildings
{
    public class BuildingDetailModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiSettings _apiSettings;
        private readonly ILogger<BuildingDetailModel> _logger;
        private readonly ApiClientService _apiClientService;
        private readonly ToastService _toastService;

        public BuildingDetailModel(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings, ILogger<BuildingDetailModel> logger, ApiClientService apiClientService, ToastService toastService)
        {
            _httpClientFactory = httpClientFactory;
            _apiSettings = apiSettings.Value;
            _logger = logger;
            _apiClientService = apiClientService;
            _toastService = toastService;
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
                    string token = GetCookie("jwt"); // Hàm GetCookie cần được định nghĩa
                    if (!string.IsNullOrEmpty(token))
                    {
                        // Gọi API để lấy thông tin profile của người dùng hiện tại
                        var request = new HttpRequestMessage(HttpMethod.Post, "?handler=LoadProfile");
                        request.Headers.Add("Authorization", $"Bearer {token}"); // Gửi token trong header
                        request.Content = new StringContent(JsonSerializer.Serialize(new { token }), Encoding.UTF8, "application/json");

                        var profileResponse = await client.SendAsync(request);
                        if (profileResponse.IsSuccessStatusCode)
                        {
                            var profileData = await profileResponse.Content.ReadFromJsonAsync<dynamic>();
                            if (profileData != null && (profileData.avatarUrl != null || profileData.avatar != null))
                            {
                                ViewData["UserAvatar"] = profileData.avatarUrl ?? profileData.avatar; // Lưu URL avatar
                            }
                        }
                    }
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
                        _logger.LogWarning("Building not found for ID {id}: {message}", id, apiResponse.Message);
                        _toastService.ShowError("Lỗi", apiResponse.Message ?? "Không tìm thấy tòa nhà.");
                    }
                }
                else
                {
                    _logger.LogError("API call failed for building ID {id}: Status {statusCode}", id, response.StatusCode);
                    _toastService.ShowError("Lỗi", $"Lỗi khi gọi API: {response.StatusCode}");
                }

                var amenityResponse = await client.GetAsync("/Admin/AmenityManagement/All");
                if (amenityResponse.IsSuccessStatusCode)
                {
                    var amenityApiResponse = await amenityResponse.Content.ReadFromJsonAsync<IEnumerable<AmenityListDTO>>();
                    AmenityListDTO = amenityApiResponse?.ToList() ?? new List<AmenityListDTO>();
                }

                var featuredQuery = $"?page=1&pageSize={FeatureBuildingNumber}&status=Approved&sortBy=created-desc";
                var featuredResponse = await client.GetAsync($"api/Building/get-building-list{featuredQuery}");
                featuredResponse.EnsureSuccessStatusCode();

                var featuredApiResponse = await featuredResponse.Content.ReadFromJsonAsync<ResponseDTO<PagedResult<BuildingListDTO>>>();
                if (featuredApiResponse != null && featuredApiResponse.StatusCode == 200 && featuredApiResponse.Data != null)
                {
                    FeaturedBuildings = featuredApiResponse.Data.Items ?? new List<BuildingListDTO>();
                }

                if (Building.Provider != null && Building.Provider.Id > 0)
                {
                    var relatedQuery = $"?page=1&pageSize={RelatedBuildingNumber}&status=Approved&providerId={Building.Provider.Id}";
                    var relatedResponse = await client.GetAsync($"api/Building/get-building-list{relatedQuery}");
                    relatedResponse.EnsureSuccessStatusCode();

                    var relatedApiResponse = await relatedResponse.Content.ReadFromJsonAsync<ResponseDTO<PagedResult<BuildingListDTO>>>();
                    if (relatedApiResponse != null && relatedApiResponse.StatusCode == 200 && relatedApiResponse.Data != null)
                    {
                        RelatedBuildings = relatedApiResponse.Data.Items
                            .Where(b => b.BuildingId != id)
                            .ToList();
                    }
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching building details for ID {id}", id);
                _toastService.ShowError("Lỗi", $"Lỗi xảy ra: {ex.Message}");
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
               
                // Validate inputs
                if (NewComment.BuildingId == null || NewComment.BuildingId == 0)
                {
                    TempData["CommentErrorMessage"] = "Mã tòa nhà không hợp lệ.";
                    await OnGetAsync(NewComment.BuildingId ?? 0);
                    return Page();
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

        public string GetCookie(string name)
        {
            if (Request.Cookies[name] != null)
            {
                return Request.Cookies[name];
            }
            return null;
        }
    }
}