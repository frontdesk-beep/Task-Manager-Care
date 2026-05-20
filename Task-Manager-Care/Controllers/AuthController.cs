using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Task_Manager_Care.Data;
using Task_Manager_Care.DTOs;
using Task_Manager_Care.Models;

using MailKit.Net.Smtp;
using MimeKit;

namespace Task_Manager_Care.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        private readonly IConfiguration _configuration;

        public AuthController(
            AppDbContext context,
            IConfiguration configuration
        )
        {
            _context = context;

            _configuration = configuration;
        }

        private string GenerateToken(
            User user
        )
        {
            var claims =
            new[]
            {
                new Claim(
                    ClaimTypes.Name,
                    user.Name
                ),

                new Claim(
                    ClaimTypes.Email,
                    user.Email
                ),

                new Claim(
                    ClaimTypes.Role,
                    user.Role
                )
            };

            var key =
            new SymmetricSecurityKey(

            Encoding.UTF8.GetBytes(
            _configuration["JWT:Key"]
            ));

            var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token =
            new JwtSecurityToken(

            issuer:
            _configuration["JWT:Issuer"],

            audience:
            _configuration["JWT:Audience"],

            claims:
            claims,

            expires:
            DateTime.UtcNow.AddHours(4),

            signingCredentials:
            credentials
            );

            return
            new JwtSecurityTokenHandler()
            .WriteToken(token);
        }

        [HttpPost("register")]

        public async Task<IActionResult>
        Register(
        RegisterRequest request
        )
        {
            var exists =
            _context.Users
            .Any(
            x =>
            x.Email ==
            request.Email
            );

            if (exists)
            {
                return BadRequest(
                new
                {
                    message =
                    "Email already registered."
                });
            }

            var user =
            new User
            {
                Name =
                request.Name,

                Email =
                request.Email,

                Password =
                request.Password,

                Role =
                request.Role,

                CreatedAt =
                DateTime.UtcNow
            };

            _context.Users
            .Add(user);

            await
            _context
            .SaveChangesAsync();

            return Ok(
            new
            {
                message =
                "User registered successfully."
            });
        }

        [HttpPost("login")]

        public IActionResult Login(
        LoginRequest request
        )
        {
            var user =
            _context.Users
            .FirstOrDefault(
            x =>
            x.Email ==
            request.Email
            );

            if (user == null)
            {
                return Unauthorized(
                new
                {
                    message =
                    "User not found."
                });
            }

            if (
            user.Password
            !=
            request.Password
            )
            {
                return Unauthorized(
                new
                {
                    message =
                    "Invalid password."
                });
            }

            var token =
            GenerateToken(
            user
            );

            return Ok(
            new
            {
                token,

                name =
                user.Name,

                role =
                user.Role
            });
        }

    }
}