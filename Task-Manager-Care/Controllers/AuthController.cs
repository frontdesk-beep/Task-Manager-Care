using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;

using Task_Manager_Care.Data;
using Task_Manager_Care.DTOs;
using Task_Manager_Care.Models;
using Task_Manager_Care.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Task_Manager_Care.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _hasher = new PasswordHasher<User>();
        private readonly IEmailService _emailService;

        public AuthController(AppDbContext context,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

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

        //[HttpPost("register")]
        //public async Task<IActionResult> Register(RegisterRequestDto request) { ... }

        [HttpPost("login")]
        public IActionResult Login(LoginDto request)
        {
            var email = NormalizeEmail(request.Email);
            var user = _context.Users.FirstOrDefault(x => x.Email == email);

            // Same generic message whether the user doesn't exist or the password is
            // wrong — avoids leaking which emails are registered (account enumeration).
            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });

            if (!user.IsActive)
            {
                return StatusCode(403, new
                {
                    message = "User account has been deactivated. Please contact an administrator."
                });
            }

            bool passwordValid = false;
            try
            {
                var verify = _hasher.VerifyHashedPassword(user, user.Password, request.Password);

                // Treat both Success and SuccessRehashNeeded as valid logins — otherwise
                // correct passwords get rejected the moment hasher defaults change.
                passwordValid = verify != PasswordVerificationResult.Failed;

                if (verify == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    user.Password = _hasher.HashPassword(user, request.Password);
                    _context.SaveChanges();
                }
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
                return Unauthorized(new { message = "Invalid email or password." });

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
        public async Task<IActionResult> Password(ForgotPasswordRequestDto request)
        {
            var frontendUrl = _configuration["AppSettings:FrontendUrl"];
            var email = NormalizeEmail(request.Email);
            var user = _context.Users.FirstOrDefault(x => x.Email == email);

            // Generic response for both "no such user" and "deactivated user" —
            // don't let this endpoint reveal which emails are registered.
            if (user == null || !user.IsActive)
                return Ok(new { message = "If that account exists, a password reset link has been sent." });

            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            var resetLink = $"{frontendUrl}/reset-password?token={user.PasswordResetToken}";
            var body =
                $@"<p>
                Click the link below to reset your password:
                </p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>This link expires in one hour.</p>";

            await _emailService.SendEmailAsync(user.Email, "Reset Password", body);

            return Ok(new { message = "If that account exists, a password reset link has been sent." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto request)
        {
            var user = _context.Users.FirstOrDefault(x => x.PasswordResetToken == request.Token);
            if (user == null)
                return BadRequest(new { message = "Invalid token." });

            if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
                return BadRequest(new { message = "Token expired." });

            user.Password = _hasher.HashPassword(user, request.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Password reset successfully." });
        }

        [HttpGet("profile/{id}")]
        [Authorize]
        public IActionResult GetProfile(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (currentUserId != id && currentRole != "Admin" && currentRole != "SuperAdmin")
                return Forbid();

            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(new { id = user.Id, name = user.Name, email = user.Email, role = user.Role });
        }

        [HttpPut("profile/{id}")]
        [Authorize]
        public IActionResult UpdateProfile(int id, UpdateProfileRequestDto request)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (currentUserId != id && currentRole != "Admin" && currentRole != "SuperAdmin")
                return Forbid();

            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            user.Name = request.Name;

            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                bool currentMatches = false;
                try
                {
                    var verify = _hasher.VerifyHashedPassword(user, user.Password, request.CurrentPassword ?? "");
                    currentMatches = verify != PasswordVerificationResult.Failed;

                    if (verify == PasswordVerificationResult.SuccessRehashNeeded)
                    {
                        user.Password = _hasher.HashPassword(user, request.CurrentPassword ?? "");
                    }
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