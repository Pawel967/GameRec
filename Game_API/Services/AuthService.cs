using Game_API.Data;
using Game_API.Dtos.Account;
using Game_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Game_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtSettings _jwtSettings;

        public AuthService(AppDbContext context, IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
        {
            // Additional custom validation
            if (!IsValidEmail(registerDto.Email))
            {
                throw new ArgumentException("Invalid email format");
            }

            if (!IsStrongPassword(registerDto.Password))
            {
                throw new ArgumentException("Password does not meet complexity requirements");
            }

            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email))
            {
                throw new Exception("Username or email already exists");
            }

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = HashPassword(registerDto.Password)
            };

            var isFirstUser = !await _context.Users.AnyAsync();
            var roleName = isFirstUser ? Role.RoleNames.Admin : Role.RoleNames.User;
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);

            if (role == null)
            {
                throw new Exception($"Role {roleName} not found");
            }

            user.Roles.Add(role);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = user.Roles.Select(r => r.Name).ToList()
            };
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                throw new Exception("Invalid username or password");
            }

            var token = GenerateJwtToken(user);

            return new LoginResponseDto
            {
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Roles = user.Roles.Select(r => r.Name).ToList()
                },
                Token = token
            };
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto)
        {
            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            if (updateUserDto.Username != null)
            {
                if (await _context.Users.AnyAsync(u => u.Username == updateUserDto.Username && u.Id != userId))
                {
                    throw new Exception("Username already exists");
                }
                user.Username = updateUserDto.Username;
            }

            if (updateUserDto.Email != null)
            {
                if (!IsValidEmail(updateUserDto.Email))
                {
                    throw new ArgumentException("Invalid email format");
                }
                if (await _context.Users.AnyAsync(u => u.Email == updateUserDto.Email && u.Id != userId))
                {
                    throw new Exception("Email already exists");
                }
                user.Email = updateUserDto.Email;
            }

            if (!string.IsNullOrEmpty(updateUserDto.Password))
            {
                if (!IsStrongPassword(updateUserDto.Password))
                {
                    throw new ArgumentException("Password does not meet complexity requirements");
                }
                user.PasswordHash = HashPassword(updateUserDto.Password);
            }

            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = user.Roles.Select(r => r.Name).ToList()
            };
        }

        public async Task<UserDto> GetUserByIdAsync(Guid userId)
        {
            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = user.Roles.Select(r => r.Name).ToList()
            };
        }

        public async Task<bool> AssignRoleAsync(Guid userId, string roleName)
        {
            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return false;
            }

            var newRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (newRole == null)
            {
                return false;
            }

            user.Roles.Clear();
            user.Roles.Add(newRole);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Include(u => u.Roles)
                .ToListAsync();

            return users.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Roles = u.Roles.Select(r => r.Name).ToList()
            });
        }

        public async Task<IEnumerable<string>> GetAllRolesAsync()
        {
            return await _context.Roles.Select(r => r.Name).ToListAsync();
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            foreach (var role in user.Roles)
            {
                tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, role.Name));
            }

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return storedHash == HashPassword(password);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsStrongPassword(string password)
        {
            // Password must be at least 6 characters long and contain at least one uppercase letter, 
            // one lowercase letter, one digit, and one special character
            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$");
            return regex.IsMatch(password);
        }
    }
}
