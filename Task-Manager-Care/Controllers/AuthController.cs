using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;

using Task_Manager_Care.Data;
using Task_Manager_Care.DTOs;
using Task_Manager_Care.Models;

namespace Task_Manager_Care.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _hasher = new PasswordHasher<User>();

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (_context.Users.Any(x => x.Email == request.Email))
                return BadRequest(new { message = "Email already exists" });

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Role = "Employee",
                CreatedAt = DateTime.UtcNow
            };

            user.Password = _hasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Registration successful" });
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == request.Email 
            && x.IsActive);
            if (user == null)
                return Unauthorized(new { message = "User not found." });

            // Verify password. Handle legacy/plain-text stored passwords by catching
            // FormatException thrown when the stored value isn't in the expected hashed format.
            bool passwordValid = false;
            try
            {
                var verify = _hasher.VerifyHashedPassword(user, user.Password, request.Password);
                passwordValid = verify == PasswordVerificationResult.Success;
            }
            catch (FormatException)
            {
                // Legacy entry: compare raw values and, if valid, upgrade to a hashed password.
                if (user.Password == request.Password)
                {
                    user.Password = _hasher.HashPassword(user, request.Password);
                    _context.SaveChanges();
                    passwordValid = true;
                }
            }

            if (!passwordValid)
                return Unauthorized(new { message = "Invalid password." });

            var token = GenerateToken(user);
            return Ok(new
            {
                id = user.Id,
                token,
                name = user.Name,
                email = user.Email,
                role = user.Role
            });
        }

        [HttpPost("forgot-password")]
        public IActionResult Password(ForgotPasswordRequest request)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == request.Email);
            if (user == null) return NotFound(new { message = "User not found." });

            bool isSameAsOld = false;
            try
            {
                var check = _hasher.VerifyHashedPassword(user, user.Password, request.NewPassword);
                isSameAsOld = check == PasswordVerificationResult.Success;
            }
            catch (FormatException)
            {
                // Legacy stored password: compare raw values
                isSameAsOld = user.Password == request.NewPassword;
            }

            if (isSameAsOld)
                return BadRequest(new { message = "New password must be different from old password." });

            user.Password = _hasher.HashPassword(user, request.NewPassword);
            _context.SaveChanges();
            return Ok(new { message = "Password updated" });
        }

        [HttpGet("profile/{id}")]
        public IActionResult GetProfile(int id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null) 
                return NotFound(new 
                { message = "User not found." });
            return Ok(new { id = user.Id, name = user.Name, email = user.Email, role = user.Role });
        }

        [HttpPut("profile/{id}")]
        public IActionResult UpdateProfile(int id, UpdateProfileRequest request)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null) return NotFound(new { message = "User not found." });

            user.Name = request.Name;
            
            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                bool currentMatches = false;
                try
                {
                    var verify = _hasher.VerifyHashedPassword(user, user.Password, request.CurrentPassword ?? "");
                    currentMatches = verify == PasswordVerificationResult.Success;
                }
                catch (FormatException)
                {
                    currentMatches = user.Password == (request.CurrentPassword ?? "");
                }

                if (!currentMatches)
                    return BadRequest(new { message = "Current password is incorrect." });

                user.Password = _hasher.HashPassword(user, request.NewPassword);
            }
        _context.SaveChanges();
            return Ok(new { message = "Profile updated successfully." });
        }
    }
}