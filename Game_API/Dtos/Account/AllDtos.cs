using System.ComponentModel.DataAnnotations;

namespace Game_API.Dtos.Account
{
    public class AllDtos
    {
        public class LoginDto
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class LoginResponseDto
        {
            public UserDto User { get; set; }
            public string Token { get; set; }
        }

        public class RegisterDto
        {
            [Required(ErrorMessage = "Username is required")]
            [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
            public string Username { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$",
                ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character")]
            public string Password { get; set; }
        }

        public class UpdateUserDto
        {
            public string? Username { get; set; }
            public string? Email { get; set; }
            public string? Password { get; set; }
        }

        public class UserDto
        {
            public Guid Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public List<string> Roles { get; set; }
        }
    }
}
