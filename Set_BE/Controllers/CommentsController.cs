using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Set_BE.Data;
using Set_BE.DTOs;
using Set_BE.Models;

namespace Set_BE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CommentsController : ControllerBase
	{
		private readonly SetDbContext _context;

		// Gọi thẳng Database vào đây để xử lý bình luận cho lẹ
		public CommentsController(SetDbContext context)
		{
			_context = context;
		}

		// 1. API Lấy danh sách bình luận của 1 Code (GET: /api/comments/code/5)
		[HttpGet("code/{codeId}")]
		public async Task<IActionResult> GetComments(int codeId)
		{
			var comments = await _context.Comments
				.Where(c => c.MovieCodeId == codeId)
				.OrderByDescending(c => c.CreatedAt) // Bình luận mới nhất lên đầu
				.Select(c => new
				{
					c.Id,
					c.Content,
					c.CreatedAt,
					c.UserId
					// Nếu sau này bạn muốn hiện Tên User, có thể join bảng Users vào đây
				})
				.ToListAsync();

			return Ok(comments);
		}

		// 2. API Đăng bình luận mới (POST: /api/comments/code/5)
		[HttpPost("code/{codeId}")]
		public async Task<IActionResult> AddComment(int codeId, [FromBody] CreateCommentDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Content))
			{
				return BadRequest(new { message = "Bình luận không được để trống!" });
			}

			var newComment = new Comment
			{
				Content = dto.Content,
				MovieCodeId = codeId,
				CreatedAt = DateTime.UtcNow,
				UserId = dto.UserId
			};

			_context.Comments.Add(newComment);
			await _context.SaveChangesAsync();

			return Ok(newComment);
		}
	}
}
