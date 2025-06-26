namespace HolaMyFrontend.Models.AccountDTO
{
    public class UserProfileDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string AddressDetail { get; set; }
        public string Role { get; set; }
        public string Avatar { get; set; } // URL ảnh hiện tại
        public IFormFile AvatarFile { get; set; } // File ảnh mới tải lên
    }
}
