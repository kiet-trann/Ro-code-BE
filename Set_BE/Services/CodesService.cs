using Microsoft.EntityFrameworkCore;
using Set_BE.Data;
using Set_BE.DTOs;
using Set_BE.Interfaces;
using Set_BE.Models;
using System.Text.RegularExpressions; // Thêm thư viện này để dùng Regex kiểm tra số

namespace Set_BE.Services
{
	public class CodesService : ICodesService
	{
		private readonly ICodesRepository _repository;
		private readonly SetDbContext _context;

		public CodesService(ICodesRepository repository, SetDbContext context)
		{
			_repository = repository;
			_context = context;
		}
		private async Task<List<int>> GetUserSavedIdsAsync(int userId)
		{
			if (userId <= 0) return new List<int>();
			return await _context.SavedCodes
				.Where(sc => sc.UserId == userId)
				.Select(sc => sc.MovieCodeId)
				.ToListAsync();
		}
		public async Task<PagedResponse<MovieCodeDto>> GetTrendingAsync(int currentUserId, int page, int pageSize)
		{
			var (codes, total) = await _repository.GetTrendingCodesAsync(page, pageSize);
			var savedIds = await GetUserSavedIdsAsync(currentUserId); // Lấy tủ đồ

			var dtos = codes.Select(code => new MovieCodeDto
			{
				Id = code.Id,
				CodeText = code.CodeText,
				Author = code.Author?.Username ?? "ẩn_danh",
				ActorName = code.ActorName,
				Category = code.Category,
				ViewCount = code.ViewCount,
				TimeAgo = GetTimeAgo(code.CreatedAt),
				AvgRating = Math.Round(code.AverageRating, 1),
				IsWatched = code.Ratings.Any(r => r.UserId == currentUserId),
				IsSaved = savedIds.Contains(code.Id) // CHECK VÀNG NÚT
			});

			return new PagedResponse<MovieCodeDto> { Items = dtos, CurrentPage = page, TotalPages = (int)Math.Ceiling(total / (double)pageSize) };
		}
		public async Task<PagedResponse<MovieCodeDto>> GetNewAsync(int currentUserId, int page, int pageSize)
		{
			var (codes, total) = await _repository.GetNewCodesAsync(page, pageSize);
			var savedIds = await GetUserSavedIdsAsync(currentUserId);

			var dtos = codes.Select(code => new MovieCodeDto
			{
				Id = code.Id,
				CodeText = code.CodeText,
				Author = code.Author?.Username ?? "ẩn_danh",
				ActorName = code.ActorName,
				Category = code.Category,
				ViewCount = code.ViewCount,
				TimeAgo = GetTimeAgo(code.CreatedAt),
				AvgRating = Math.Round(code.AverageRating, 1),
				IsWatched = code.Ratings.Any(r => r.UserId == currentUserId),
				IsSaved = savedIds.Contains(code.Id) // CHECK VÀNG NÚT
			});
			return new PagedResponse<MovieCodeDto> { Items = dtos, CurrentPage = page, TotalPages = (int)Math.Ceiling(total / (double)pageSize) };
		}

		public async Task<PagedResponse<MovieCodeDto>> GetRecommendedAsync(int currentUserId, int page, int pageSize)
		{
			var (codes, total) = await _repository.GetRecommendedCodesAsync(currentUserId, page, pageSize);
			var savedIds = await GetUserSavedIdsAsync(currentUserId);

			var dtos = codes.Select(code => new MovieCodeDto
			{
				Id = code.Id,
				CodeText = code.CodeText,
				Author = code.Author?.Username ?? "ẩn_danh",
				ActorName = code.ActorName,
				Category = code.Category,
				ViewCount = code.ViewCount,
				TimeAgo = GetTimeAgo(code.CreatedAt),
				AvgRating = Math.Round(code.AverageRating, 1),
				IsWatched = false,
				IsSaved = savedIds.Contains(code.Id) // CHECK VÀNG NÚT
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

		public async Task<MovieCodeDto> SpinRandomCodeAsync(int userId, CancellationToken cancellationToken)
		{
			var user = await _context.Users.FindAsync(userId);
			if (user == null) throw new Exception("Không tìm thấy User!");

			// 1. Tính toán ngày hiện tại theo chuẩn Giờ Việt Nam
			var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
			var nowInVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);

			// 2. Kiểm tra xem hôm nay đã quay chưa
			if (user.LastSpinAt.HasValue)
			{
				var lastSpinInVn = TimeZoneInfo.ConvertTimeFromUtc(user.LastSpinAt.Value, vnTimeZone);

				if (lastSpinInVn.Date == nowInVn.Date)
				{
					throw new Exception("Hôm nay nhân phẩm đã cạn! Hãy quay lại vào 0h00 đêm nay nhé.");
				}
			}

			// 3. PostgreSQL bốc thăm Random 1 mã Code (Rất nhẹ Server)
			var randomCode = await _context.MovieCodes
				.Include(c => c.Author) // Kéo theo tên tác giả
				.OrderBy(c => EF.Functions.Random())
				.FirstOrDefaultAsync();

			if (randomCode == null) throw new Exception("Kho code đang trống rỗng!");

			// 4. Lưu lại lịch sử quay (Lưu giờ UTC chuẩn quốc tế)
			user.LastSpinAt = DateTime.UtcNow;
			await _context.SaveChangesAsync(cancellationToken);

			// 5. Trả kết quả về
			return new MovieCodeDto
			{
				Id = randomCode.Id,
				CodeText = randomCode.CodeText,
				ActorName = randomCode.ActorName,
				Category = randomCode.Category,
				ViewCount = randomCode.ViewCount,
				Author = randomCode.Author?.Username ?? "ẩn_danh",
				TimeAgo = GetTimeAgo(randomCode.CreatedAt),
				AvgRating = Math.Round(randomCode.AverageRating, 1),
				IsWatched = false
			};
		}

		public async Task<PagedResponse<MovieCodeDto>> SearchCodesAsync(int currentUserId, string keyword, string category, int page, int pageSize)
		{
			var (codes, total) = await _repository.SearchCodesAsync(keyword, category, page, pageSize);
			var savedIds = await GetUserSavedIdsAsync(currentUserId);

			var dtos = codes.Select(code => new MovieCodeDto
			{
				Id = code.Id,
				CodeText = code.CodeText,
				Author = code.Author?.Username ?? "ẩn_danh",
				ActorName = code.ActorName,
				Category = code.Category,
				ViewCount = code.ViewCount,
				TimeAgo = GetTimeAgo(code.CreatedAt),
				AvgRating = Math.Round(code.AverageRating, 1),
				IsWatched = currentUserId > 0 && code.Ratings.Any(r => r.UserId == currentUserId),
				IsSaved = savedIds.Contains(code.Id) // CHECK VÀNG NÚT
			});

			return new PagedResponse<MovieCodeDto> { Items = dtos, CurrentPage = page, TotalPages = (int)Math.Ceiling(total / (double)pageSize) };
		}

		public async Task<bool> ToggleSaveCodeAsync(int userId, int codeId)
		{
			return await _repository.ToggleSaveCodeAsync(userId, codeId);
		}

		public async Task<PagedResponse<MovieCodeDto>> GetSavedCodesAsync(int userId, int page, int pageSize)
		{
			var (codes, total) = await _repository.GetSavedCodesAsync(userId, page, pageSize);

			var dtos = codes.Select(code => new MovieCodeDto
			{
				Id = code.Id,
				CodeText = code.CodeText,
				Author = code.Author?.Username ?? "ẩn_danh",
				ActorName = code.ActorName,
				Category = code.Category,
				ViewCount = code.ViewCount,
				TimeAgo = GetTimeAgo(code.CreatedAt),
				AvgRating = Math.Round(code.AverageRating, 1),
				IsWatched = true,
				IsSaved = true // Đã nằm trong Tủ đồ thì 100% nút phải Vàng
			});

			return new PagedResponse<MovieCodeDto> { Items = dtos, CurrentPage = page, TotalPages = (int)Math.Ceiling(total / (double)pageSize) };
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