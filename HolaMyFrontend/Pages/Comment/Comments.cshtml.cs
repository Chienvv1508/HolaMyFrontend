using HolaMyFrontend.Models;
using HolaMyFrontend.Models.CommentDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace HolaMyFrontend.Pages.Comment
{
    public class CommentsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiSettings _apiSettings;

        public CommentsModel(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiSettings = apiSettings.Value;
        }

        [BindProperty]
        public List<CommentDTO> BuildingComments { get; set; } = new List<CommentDTO>();

        [BindProperty]
        public CreateCommentDTO NewComment { get; set; } = new CreateCommentDTO();

        public async Task OnGetAsync(int buildingId)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = $"{_apiSettings.BaseUrl}/api/comment/building/{buildingId}";
            var response = await httpClient.GetFromJsonAsync<List<CommentDTO>>(apiUrl);

            if (response != null)
            {
                BuildingComments = response;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = $"{_apiSettings.BaseUrl}/api/comment";

            // For form with file upload
            var content = new MultipartFormDataContent();

            if (NewComment.File != null)
            {
                var fileContent = new StreamContent(NewComment.File.OpenReadStream());
                content.Add(fileContent, "File", NewComment.File.FileName);
            }

            content.Add(new StringContent(NewComment.BuildingId.ToString() ?? ""), "BuildingId");
            content.Add(new StringContent(NewComment.UserId.ToString() ?? ""), "UserId");
            content.Add(new StringContent(NewComment.Content), "Content");
            content.Add(new StringContent(NewComment.ContentType.ToString()), "ContentType");
            if (NewComment.Rating.HasValue)
            {
                content.Add(new StringContent(NewComment.Rating.Value.ToString()), "Rating");
            }

            var response = await httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage(new { buildingId = NewComment.BuildingId });
            }

            ModelState.AddModelError(string.Empty, "Error creating comment");
            return Page();
        }
    }
}
