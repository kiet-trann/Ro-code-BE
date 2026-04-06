using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Set_BE.Data;
using Set_BE.DTOs;
using Set_BE.Interfaces;
using Set_BE.Repositories;
using System.Text.RegularExpressions;

namespace Set_BE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CodesController : ControllerBase
	{
		private readonly ICodesService _codesService;
		private readonly ICodeValidatorService _validatorService;
		private readonly SetDbContext _context;

		public CodesController(ICodesService codesService, ICodeValidatorService validatorService, SetDbContext context)
		{
			_codesService = codesService;
			_validatorService = validatorService;
			_context = context;
		}

		// GET: api/codes/trending?userId=1
		[HttpGet("trending")]
		public async Task<IActionResult> GetTrending([FromQuery] int userId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
		{
			var result = await _codesService.GetTrendingAsync(userId, page, limit);
			return Ok(result);
		}

		// POST: api/codes/drop
		[HttpPost("drop")]
		public async Task<IActionResult> DropCode([FromBody] CreateCodeDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.CodeText))
				return BadRequest(new { message = "Mã code không được để trống." });

			try
			{
				// Giao toàn quyền sinh sát cho Service
				var result = await _codesService.DropCodeAsync(dto);
				return Ok(new
				{
					message = "Thả code thành công! Đã cộng điểm nhân phẩm.",
					data = result
				});
			}
			catch (ArgumentException ex)
			{
				// Bắt các lỗi (Trùng lặp, Fake, Regex) và trả về Frontend
				return BadRequest(new { message = ex.Message });
			}
			catch (Exception)
			{
				return StatusCode(500, new { message = "Server đang bận rèn vũ khí, thử lại sau nhé!" });
			}
		}

		// POST: api/codes/{id}/rate
		[HttpPost("{id}/rate")]
		public async Task<IActionResult> RateCode(int id, [FromBody] RateCodeDto dto)
		{
			if (dto.Score < 1 || dto.Score > 5)
				return BadRequest("Điểm rate phải từ 1 đến 5 sao.");

			var success = await _codesService.RateCodeAsync(id, dto);
			if (!success)
				return BadRequest("Không tìm thấy mã code hoặc bạn đã thẩm định mã này rồi.");

			return Ok(new { message = "Đã lưu trạng thái Đã Xem & Cập nhật điểm thành công." });
		}
		// GET: api/codes/watched/{userId}
		[HttpGet("watched/{userId}")]
		public async Task<IActionResult> GetWatchedCodes(int userId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
		{
			return Ok(await _codesService.GetWatchedAsync(userId, page, limit));
		}

		// GET: api/codes/dropped/{userId}
		[HttpGet("dropped/{userId}")]
		public async Task<IActionResult> GetDroppedCodes(int userId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
		{
			return Ok(await _codesService.GetDroppedAsync(userId, page, limit));
		}
		// GET: api/codes/new?userId=1
		[HttpGet("new")]
		public async Task<IActionResult> GetNewCodes([FromQuery] int userId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
		{
			return Ok(await _codesService.GetNewAsync(userId, page, limit));
		}

		// GET: api/codes/foryou/1
		[HttpGet("foryou/{userId}")]
		public async Task<IActionResult> GetForYouCodes(int userId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
		{
			return Ok(await _codesService.GetRecommendedAsync(userId, page, limit));
		}

		[HttpPost("{id}/view")]
		public async Task<IActionResult> IncreaseView(int id)
		{
			var success = await _codesService.IncreaseViewCountAsync(id);
			if (!success)
				return NotFound(new { message = "Không tìm thấy code này!" });

			return Ok(new { message = "Đã tăng lượt xem thành công!" });
		}

		[HttpPost("spin/{userId}")]
		public async Task<IActionResult> SpinGacha(int userId, CancellationToken cancellationToken)
		{
			try
			{
				// Truyền nó xuống Service
				var result = await _codesService.SpinRandomCodeAsync(userId, cancellationToken);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}
		[HttpGet("search")]
		public async Task<IActionResult> SearchCodes(
	[FromQuery] int currentUserId,
	[FromQuery] string keyword = "",
	[FromQuery] string category = "All",
	[FromQuery] int page = 1,
	[FromQuery] int pageSize = 10)
		{
			try
			{
				var result = await _codesService.SearchCodesAsync(currentUserId, keyword, category, page, pageSize);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = ex.Message });
			}
		}

		[HttpPost("{codeId}/save")]
		public async Task<IActionResult> ToggleSaveCode(int codeId, [FromBody] int userId)
		{
			try
			{
				var isSaved = await _codesService.ToggleSaveCodeAsync(userId, codeId);
				return Ok(new { isSaved });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpGet("saved/{userId}")]
		public async Task<IActionResult> GetSavedCodes(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
		{
			try
			{
				var result = await _codesService.GetSavedCodesAsync(userId, page, pageSize);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = ex.Message });
			}
		}

		// GET: api/codes/profile/5?currentUserId=5
		[HttpGet("profile/{targetUserId}")]
		public async Task<IActionResult> GetUserProfile(int targetUserId, [FromQuery] int currentUserId)
		{
			try
			{
				var profile = await _codesService.GetUserProfileAsync(targetUserId, currentUserId);
				return Ok(profile);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}
		// GET: api/codes/leaderboard
		[HttpGet("leaderboard")]
		public async Task<IActionResult> GetLeaderboard()
		{
			try
			{
				var result = await _codesService.GetLeaderboardAsync();
				return Ok(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = ex.Message });
			}
		}
		// API Ẩn: Chỉ dùng 1 lần rồi thôi, hoặc giấu đi để Admin xài
		[HttpPost("admin/normalize-legacy-codes")]
		public async Task<IActionResult> NormalizeLegacyCodes([FromQuery] string secretKey)
		{
			// 1. CHỐNG NGƯỜI LẠ (Bảo mật thô sơ nhưng hiệu quả)
			if (secretKey != "roset-tuyet-mat-2026")
			{
				return Unauthorized(new { message = "Lượn đi cho nước nó trong! Khu vực cấm." });
			}

			try
			{
				// 2. KÉO ĐỒ CỔ LÊN (Chỉ lấy những mã mà NormalizedCode đang bị bỏ trống)
				var legacyCodes = await _context.MovieCodes
					.Where(c => string.IsNullOrEmpty(c.NormalizedCode))
					.ToListAsync();

				if (!legacyCodes.Any())
				{
					return Ok(new { message = "Kho dữ liệu đã sạch bóng, không có mã đồ cổ nào cần tẩy trần!" });
				}

				// 3. ĐƯA VÀO "MÁY GIẶT"
				int count = 0;
				foreach (var code in legacyCodes)
				{
					// Tẩy sạch mọi thứ, chỉ giữ lại A-Z và 0-9
					code.NormalizedCode = Regex.Replace(code.CodeText.ToUpper(), @"[^A-Z0-9]", "");
					count++;
				}

				// 4. LƯU LẠI VÀO DB
				await _context.SaveChangesAsync();

				return Ok(new
				{
					message = "Tẩy trần hoàn tất!",
					totalCleaned = count
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Máy giặt hỏng: " + ex.Message });
			}
		}
		// API Ẩn: Truy lĩnh lương hưu cho anh em up code từ thời tiền sử
		[HttpPost("admin/grant-legacy-points")]
		public async Task<IActionResult> GrantLegacyPoints([FromQuery] string secretKey)
		{
			if (secretKey != "roset-tuyet-mat-2026") return Unauthorized(new { message = "Cấm cửa!" });

			try
			{
				var users = await _context.Users.ToListAsync();
				int updatedCount = 0;

				foreach (var user in users)
				{
					// Đếm số code người này đã up
					int uploadCount = await _context.MovieCodes.CountAsync(c => c.AuthorId == user.Id);

					// Nếu có up code mà điểm = 0 thì truy lĩnh (+10đ mỗi code)
					if (uploadCount > 0 && user.ActionPoints == 0)
					{
						user.ActionPoints = uploadCount * 10;
						updatedCount++;
					}
				}

				await _context.SaveChangesAsync();

				return Ok(new
				{
					message = "Phát lương hưu hoàn tất!",
					usersUpdated = updatedCount
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Lỗi kho bạc: " + ex.Message });
			}
		}
		[HttpPost("{codeId}/report")]
		public async Task<IActionResult> ReportCode(int codeId, [FromBody] int reporterId)
		{
			try
			{
				var message = await _codesService.ReportCodeAsync(codeId, reporterId);
				return Ok(new { message = message });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}
		[HttpGet("ping")]
		public IActionResult Ping()
		{
			return Ok(new { message = "Rổ code đang thức!" });
		}
	}
}
