using Set_BE.DTOs;
using Set_BE.Interfaces;
using Set_BE.Models;
using System.Text.RegularExpressions; // Thêm thư viện này để dùng Regex kiểm tra số

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
				ActorName = code.ActorName, // Thêm mới
				Category = code.Category,   // Thêm mới
				ViewCount = code.ViewCount, // Thêm mới
				TimeAgo = GetTimeAgo(code.CreatedAt),
				AvgRating = Math.Round(code.AverageRating, 1),
				IsWatched = code.Ratings.Any(r => r.UserId == currentUserId)
			});

			return new PagedResponse<MovieCodeDto>
			{
				Items = dtos,
				CurrentPage = page,
				TotalPages = (int)Math.Ceiling(total / (double)pageSize)
			};
		}
		public async Task<PagedResponse<MovieCodeDto>> GetNewAsync(int currentUserId, int page, int pageSize)
		{
			var (codes, total) = await _repository.GetNewCodesAsync(page, pageSize);
			var dtos = codes.Select(code => new MovieCodeDto
			{
				Id = code.Id,
				CodeText = code.CodeText,
				Author = code.Author?.Username ?? "ẩn_danh",
				ActorName = code.ActorName, // Thêm mới
				Category = code.Category,   // Thêm mới
				ViewCount = code.ViewCount, // Thêm mới
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
				ActorName = code.ActorName, // Thêm mới
				Category = code.Category,   // Thêm mới
				ViewCount = code.ViewCount, // Thêm mới
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
				ActorName = code.ActorName, // Thêm mới
				Category = code.Category,   // Thêm mới
				ViewCount = code.ViewCount, // Thêm mới
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
				ActorName = code.ActorName, // Thêm mới
				Category = code.Category,   // Thêm mới
				ViewCount = code.ViewCount, // Thêm mới
				TimeAgo = GetTimeAgo(code.CreatedAt),
				AvgRating = Math.Round(code.AverageRating, 1),
				IsWatched = code.Ratings.Any(r => r.UserId == userId)
			});
			return new PagedResponse<MovieCodeDto> { Items = dtos, CurrentPage = page, TotalPages = (int)Math.Ceiling(total / (double)pageSize) };
		}

		// --- CẬP NHẬT HÀM NÀY ĐỂ CHECK LUẬT HAITEN ---
		public async Task<IEnumerable<MovieCodeDto>> DropCodeAsync(CreateCodeDto dto)
		{
			var rawCodes = dto.CodeText.Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

			var newCodes = new List<MovieCode>();
			var resultDtos = new List<MovieCodeDto>();

			foreach (var raw in rawCodes)
			{
				var cleanedCode = raw.Trim().ToUpper();
				if (string.IsNullOrWhiteSpace(cleanedCode)) continue;

				// 🚨 KIỂM TRA LUẬT HAITEN: Chỉ cho phép 1 đến 6 chữ số
				if (dto.Category == "Haiten")
				{
					bool isOnlyDigits = Regex.IsMatch(cleanedCode, @"^\d{1,6}$");
					if (!isOnlyDigits)
					{
						// Quăng lỗi ngay lập tức để báo về cho React
						throw new ArgumentException($"Code '{cleanedCode}' không hợp lệ! Haiten chỉ được chứa 1 đến 6 chữ số.");
					}
				}

				newCodes.Add(new MovieCode
				{
					CodeText = cleanedCode,
					AuthorId = dto.AuthorId,
					ActorName = dto.ActorName, // Thêm diễn viên
					Category = dto.Category,   // Thêm phân loại
					ViewCount = 0,             // Khởi tạo lượt xem bằng 0
					CreatedAt = DateTime.UtcNow,
					AverageRating = 0,
					TotalRatings = 0
				});
			}

			if (newCodes.Any())
			{
				await _repository.AddCodesAsync(newCodes);
				await _repository.SaveChangesAsync();
			}

			foreach (var code in newCodes)
			{
				resultDtos.Add(new MovieCodeDto
				{
					Id = code.Id,
					CodeText = code.CodeText,
					ActorName = code.ActorName,
					Category = code.Category,
					ViewCount = code.ViewCount,
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

			var existingRating = await _repository.GetUserRatingAsync(dto.UserId, codeId);
			if (existingRating != null) return false;

			var rating = new Rating
			{
				Score = dto.Score,
				UserId = dto.UserId,
				MovieCodeId = codeId,
				RatedAt = DateTime.UtcNow
			};
			await _repository.AddRatingAsync(rating);

			double totalScore = (code.AverageRating * code.TotalRatings) + dto.Score;
			code.TotalRatings += 1;
			code.AverageRating = totalScore / code.TotalRatings;

			await _repository.SaveChangesAsync();
			return true;
		}

		// --- THÊM HÀM NÀY ĐỂ TĂNG LƯỢT XEM KHI CLICK VÀO CODE ---
		public async Task<bool> IncreaseViewCountAsync(int codeId)
		{
			var code = await _repository.GetCodeByIdAsync(codeId);
			if (code == null) return false;

			code.ViewCount += 1; // Tăng view lên 1
			await _repository.SaveChangesAsync(); // Lưu xuống DB
			return true;
		}

		private string GetTimeAgo(DateTime createdAt)
		{
			var span = DateTime.UtcNow - createdAt;
			if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} phút trước";
			if (span.TotalHours < 24) return $"{(int)span.TotalHours} giờ trước";
			return $"{(int)span.TotalDays} ngày trước";
		}
	}
}