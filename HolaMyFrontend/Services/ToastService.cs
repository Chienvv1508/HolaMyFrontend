using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace HolaMyFrontend.Services
{
    public class ToastService
    {
        private readonly ITempDataDictionaryFactory _tempDataFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ToastService(ITempDataDictionaryFactory tempDataFactory, IHttpContextAccessor httpContextAccessor)
        {
            _tempDataFactory = tempDataFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private ITempDataDictionary TempData =>
            _tempDataFactory.GetTempData(_httpContextAccessor.HttpContext!);

        public void ShowSuccess(string title, string message)
        {
            TempData["ToastType"] = "success";
            TempData["ToastTitle"] = title;
            TempData["ToastMessage"] = message;
        }

        public void ShowError(string title, string message)
        {
            TempData["ToastType"] = "error";
            TempData["ToastTitle"] = title;
            TempData["ToastMessage"] = message;
        }
    }
}
