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
	}
}
