using IconFilers.Api.IServices;
using IconFilers.Application.DTOs;
using IconFilers.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using IconFilers.Api.Controllers.Requests;

namespace IconFilers.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly AppDbContext _db;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthController(IJwtService jwtService, AppDbContext db, IPasswordHasher<User> passwordHasher)
        {
            _jwtService = jwtService;
            _db = db;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Email and password are required" });

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower());

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            var verify = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);
            if (verify == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Invalid email or password" });

            var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role);
            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.Role
                }
            });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Email and password are required" });

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName ?? string.Empty,
                LastName = request.LastName ?? string.Empty,
                Email = request.Email.Trim(),
                Phone = request.Phone ?? string.Empty,
                Role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role,
                CreatedAt = DateTime.UtcNow
            };

            user.Password = _passwordHasher.HashPassword(user, request.Password);
          
            var result = await _db.Database
                .ExecuteSqlRawAsync(
                    "EXEC sp_RegisterUser @p0, @p1, @p2, @p3, @p4, @p5, @p6",
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.Phone,
                    user.Password,
                    user.Role
                );

            if (result == 0)
                return Conflict(new { message = "Email already registered" });

            var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role);

            return CreatedAtAction(nameof(Login), new { id = user.Id }, new { token, user = new { user.Id, user.Email, user.Role } });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest(new { message = "Email and new password are required" });

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower());
            if (user == null)
                return NotFound(new { message = "User not found" });

            user.Password = _passwordHasher.HashPassword(user, request.NewPassword);
            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Password has been reset" });
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest(new { message = "Old and new passwords are required" });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Invalid user" });

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var verify = _passwordHasher.VerifyHashedPassword(user, user.Password, request.OldPassword);
            if (verify == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Old password is incorrect" });

            user.Password = _passwordHasher.HashPassword(user, request.NewPassword);
            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully" });
        }
    }
}
