using Microsoft.AspNetCore.Mvc;
using Set_BE.DTOs;
using Set_BE.Interfaces;

namespace Set_BE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CodesController : ControllerBase
	{
		private readonly ICodesService _codesService;

		public CodesController(ICodesService codesService)
		{
			_codesService = codesService;
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
				return BadRequest("Mã code không được để trống.");

			var result = await _codesService.DropCodeAsync(dto);
			return Ok(result);
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
		public async Task<IActionResult> SpinGacha(int userId)
		{
			try
			{
				var result = await _codesService.SpinRandomCodeAsync(userId);
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
	}
}
