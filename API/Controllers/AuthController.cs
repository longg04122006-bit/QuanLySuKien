using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserDAL _userDAL;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserDAL userDAL,
            IConfiguration configuration)
        {
            _userDAL = userDAL;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "Username và Password không được để trống."
                });
            }

            var user = _userDAL.Login(
                request.Username,
                request.Password
            );

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Sai tài khoản hoặc mật khẩu."
                });
            }

            // =====================================
            // Xác định quyền
            // =====================================

            string roleName = user.RoleId switch
            {
                1 => "Admin",
                2 => "Manager",
                3 => "User",
                _ => "User"
            };

            // =====================================
            // Tạo Claims
            // =====================================

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.UserId.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    user.Username
                ),

                new Claim(
                    ClaimTypes.Email,
                    user.Email
                ),

                new Claim(
                    ClaimTypes.Role,
                    roleName
                )
            };

            // =====================================
            // Tạo JWT
            // =====================================

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!
                )
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var expireMinutes = Convert.ToDouble(
                _configuration["Jwt:ExpireMinutes"]
            );

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials
            );

            var tokenString =
                new JwtSecurityTokenHandler().WriteToken(token);

            // =====================================
            // Trả kết quả
            // =====================================

            return Ok(new
            {
                message = "Đăng nhập thành công",

                token = tokenString,

                user = new
                {
                    user.UserId,
                    user.Username,
                    user.FullName,
                    user.Email,
                    user.Phone,
                    user.RoleId,
                    role = roleName
                }
            });
        }
    }
}