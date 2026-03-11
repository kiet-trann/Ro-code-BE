using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Set_BE.Data;
using Set_BE.Models;

namespace Set_BE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly SetDbContext _context;

		public AuthController(SetDbContext context)
		{
			_context = context;
		}

		// Thêm trường Passcode vào DTO
		public class EnterVaultDto
		{
			public string Username { get; set; } = string.Empty;
			public string Passcode { get; set; } = string.Empty;
		}

		[HttpPost("enter")]
		public async Task<IActionResult> EnterVault([FromBody] EnterVaultDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Passcode))
				return BadRequest("Bí danh và Mã khóa không được để trống.");

			var username = dto.Username.Trim().ToLower();

			var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username);

			if (user == null)
			{
				// TẠO MỚI: Bí danh chưa tồn tại -> Đăng ký luôn với Mã khóa này
				user = new User
				{
					Username = dto.Username.Trim(),
					Passcode = dto.Passcode, // Ở dự án thực tế, bạn sẽ dùng BCrypt để mã hóa (Hash) chỗ này
					CreatedAt = DateTime.UtcNow
				};
				_context.Users.Add(user);
				await _context.SaveChangesAsync();
			}
			else
			{
				// ĐĂNG NHẬP: Bí danh đã tồn tại -> Phải kiểm tra xem Mã khóa có khớp không
				if (user.Passcode != dto.Passcode)
				{
					// Trả về lỗi 401 Unauthorized nếu sai mã khóa
					return Unauthorized("Bí danh này đã có chủ. Mã khóa không chính xác!");
				}
			}

			return Ok(new { id = user.Id, username = user.Username });
		}
	}
}
