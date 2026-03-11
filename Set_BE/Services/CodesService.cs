using Set_BE.DTOs;
using Set_BE.Interfaces;
using Set_BE.Models;

namespace Set_BE.Services
{
	public class CodesService : ICodesService
	{
		private readonly ICodesRepository _repository;

		public CodesService(ICodesRepository repository)
		{
			_repository = repository;
		}

		public async Task<PagedResponse<MovieCodeDto>> GetTrendingAsync(int currentUserId, int page, int pageSize)
		{
			var (codes, total) = await _repository.GetTrendingCodesAsync(page, pageSize);

			var dtos = codes.Select(code => new MovieCodeDto
			{
				Id = code.Id,
				CodeText = code.CodeText,
				Author = code.Author?.Username ?? "ẩn_danh",
				TimeAgo = GetTimeAgo(code.CreatedAt),
				AvgRating = Math.Round(code.AverageRating, 1),
				IsWatched = code.Ratings.Any(r => r.UserId == currentUserId)
			});

			return new PagedResponse<MovieCodeDto>
			{
				Items = dtos,
				CurrentPage = page,
				TotalPages = (int)Math.Ceiling(total / (double)pageSize) // Tính tổng số trang
			};
		}
		public async Task<PagedResponse<MovieCodeDto>> GetNewAsync(int currentUserId, int page, int pageSize)
		{
			var (codes, total) = await _repository.GetNewCodesAsync(page, pageSize);
			var dtos = codes.Select(code => new MovieCodeDto
			{ /* Map giống trending */
				Id = code.Id,
				CodeText = code.CodeText,
				Author = code.Author?.Username ?? "ẩn_danh",
				TimeAgo = GetTimeAgo(code.CreatedAt),
				AvgRating = Math.Round(code.AverageRating, 1),
				IsWatched = code.Ratings.Any(r => r.UserId == currentUserId)
			});
			return new PagedResponse<MovieCodeDto> { Items = dtos, CurrentPage = page, TotalPages = (int)Math.Ceiling(total / (double)pageSize) };
		}

		public async Task<PagedResponse<MovieCodeDto>> GetRecommendedAsync(int currentUserId, int page, int pageSize)
		{
			var (codes, total) = await _repository.GetRecommendedCodesAsync(currentUserId, page, pageSize);
			var dtos = codes.Select(code => new MovieCodeDto
			{
				Id = code.Id,
				CodeText = code.CodeText,
				Author = code.Author?.Username ?? "ẩn_danh",
				TimeAgo = GetTimeAgo(code.CreatedAt),
				AvgRating = Math.Round(code.AverageRating, 1),
				IsWatched = false
			});
			return new PagedResponse<MovieCodeDto> { Items = dtos, CurrentPage = page, TotalPages = (int)Math.Ceiling(total / (double)pageSize) };
		}

		public async Task<PagedResponse<MovieCodeDto>> GetWatchedAsync(int userId, int page, int pageSize)
		{
			var (codes, total) = await _repository.GetWatchedCodesAsync(userId, page, pageSize);
			var dtos = codes.Select(code => new MovieCodeDto
			{
				Id = code.Id,
				CodeText = code.CodeText,
				Author = code.Author?.Username ?? "ẩn_danh",
				TimeAgo = GetTimeAgo(code.CreatedAt),
				AvgRating = Math.Round(code.AverageRating, 1),
				IsWatched = true
			});
			return new PagedResponse<MovieCodeDto> { Items = dtos, CurrentPage = page, TotalPages = (int)Math.Ceiling(total / (double)pageSize) };
		}

		public async Task<PagedResponse<MovieCodeDto>> GetDroppedAsync(int userId, int page, int pageSize)
		{
			var (codes, total) = await _repository.GetDroppedCodesAsync(userId, page, pageSize);
			var dtos = codes.Select(code => new MovieCodeDto
			{
				Id = code.Id,
				CodeText = code.CodeText,
				Author = code.Author?.Username ?? "ẩn_danh",
				TimeAgo = GetTimeAgo(code.CreatedAt),
				AvgRating = Math.Round(code.AverageRating, 1),
				IsWatched = code.Ratings.Any(r => r.UserId == userId)
			});
			return new PagedResponse<MovieCodeDto> { Items = dtos, CurrentPage = page, TotalPages = (int)Math.Ceiling(total / (double)pageSize) };
		}

		public async Task<IEnumerable<MovieCodeDto>> DropCodeAsync(CreateCodeDto dto)
		{
			// Cắt chuỗi dựa trên dấu phẩy, chấm phẩy hoặc khoảng trắng/xuống dòng
			var rawCodes = dto.CodeText.Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

			var newCodes = new List<MovieCode>();
			var resultDtos = new List<MovieCodeDto>();

			foreach (var raw in rawCodes)
			{
				var cleanedCode = raw.Trim().ToUpper();
				if (string.IsNullOrWhiteSpace(cleanedCode)) continue; // Bỏ qua nếu bị rỗng

				newCodes.Add(new MovieCode
				{
					CodeText = cleanedCode,
					AuthorId = dto.AuthorId,
					CreatedAt = DateTime.UtcNow,
					AverageRating = 0,
					TotalRatings = 0
				});
			}

			// Bulk Insert toàn bộ xuống DB 1 lần duy nhất
			if (newCodes.Any())
			{
				await _repository.AddCodesAsync(newCodes);
				await _repository.SaveChangesAsync();
			}

			// Map dữ liệu trả về
			foreach (var code in newCodes)
			{
				resultDtos.Add(new MovieCodeDto
				{
					Id = code.Id,
					CodeText = code.CodeText,
					Author = "bạn",
					TimeAgo = "Vừa xong",
					AvgRating = 0,
					IsWatched = false
				});
			}

			return resultDtos;
		}

		public async Task<bool> RateCodeAsync(int codeId, RateCodeDto dto)
		{
			var code = await _repository.GetCodeByIdAsync(codeId);
			if (code == null) return false;

			// Kiểm tra xem user đã rate chưa
			var existingRating = await _repository.GetUserRatingAsync(dto.UserId, codeId);
			if (existingRating != null) return false; // Không cho rate lại

			// Thêm rating mới
			var rating = new Rating
			{
				Score = dto.Score,
				UserId = dto.UserId,
				MovieCodeId = codeId,
				RatedAt = DateTime.UtcNow
			};
			await _repository.AddRatingAsync(rating);

			// Cập nhật điểm trung bình của MovieCode
			double totalScore = (code.AverageRating * code.TotalRatings) + dto.Score;
			code.TotalRatings += 1;
			code.AverageRating = totalScore / code.TotalRatings;

			await _repository.SaveChangesAsync();
			return true;
		}

		// Hàm Helper tạo chuỗi "Time Ago"
		private string GetTimeAgo(DateTime createdAt)
		{
			var span = DateTime.UtcNow - createdAt;
			if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} phút trước";
			if (span.TotalHours < 24) return $"{(int)span.TotalHours} giờ trước";
			return $"{(int)span.TotalDays} ngày trước";
		}
		
	}
}
