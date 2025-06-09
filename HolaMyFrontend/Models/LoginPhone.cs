using System.ComponentModel.DataAnnotations;

namespace HolaMyFrontend
{
    public class LoginPhone
    {
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
