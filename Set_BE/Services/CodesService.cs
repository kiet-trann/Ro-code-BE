using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
		private readonly IMemoryCache _cache;
		private readonly ICodeValidatorService _validatorService;
		public CodesService(ICodesRepository repository, SetDbContext context, IMemoryCache cache, ICodeValidatorService validatorService)
		{
			_repository = repository;
			_context = context;
			_cache = cache;
			_validatorService = validatorService;
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

			// Lấy thông tin User ra để tí nữa cộng điểm và check Rank
			var author = await _context.Users.FindAsync(dto.AuthorId);
			if (author == null) throw new Exception("Không tìm thấy giang hồ này trong Hầm Ngầm!");

			int addedCount = 0; // Đếm số code đăng thành công để tính lương

			foreach (var raw in rawCodes)
			{
				var cleanedCode = raw.Trim().ToUpper();
				if (string.IsNullOrWhiteSpace(cleanedCode)) continue;

				// 🥊 1. TẨY TRẦN DỮ LIỆU (Giữ lại chữ và số)
				string normalized = Regex.Replace(cleanedCode, @"[^A-Z0-9]", "");

				// 🥊 2. KIỂM TRA HÀNG NHAI LẠI (Duplicate Check)
				bool isDuplicate = await _context.MovieCodes.AnyAsync(c => c.NormalizedCode == normalized);
				if (isDuplicate)
				{
					// Báo lỗi luôn, hoặc bạn có thể dùng 'continue' để bỏ qua mã trùng và lưu các mã khác
					throw new ArgumentException($"Mã '{cleanedCode}' anh em đã thẩm nát rồi! Múa phím kiểu gì cũng bị lộ nhé.");
				}

				// 🚨 3. KIỂM TRA LUẬT HAITEN
				if (dto.Category == "Haiten")
				{
					bool isOnlyDigits = Regex.IsMatch(normalized, @"^\d{1,6}$"); // Dùng normalized check cho chuẩn
					if (!isOnlyDigits)
						throw new ArgumentException($"Code '{cleanedCode}' không hợp lệ! Haiten chỉ được chứa số.");
				}

				// 🥊 4. THẨM ĐỊNH GOOGLE SERPAPI (Chỉ soi lũ Sét Nhựa, Sét Đồng)
				if (author.ActionPoints < 50)
				{
					// Gọi Service thẩm định ngay tại đây, cho từng mã một
					bool isReal = await _validatorService.IsCodeRealAsync(cleanedCode, dto.Category, dto.AuthorId);
					if (!isReal)
						throw new ArgumentException($"Mã '{cleanedCode}' là hàng pha ke! Lính mới đừng định lùa gà anh em.");
				}

				// Đạt mọi tiêu chuẩn -> Thêm vào hàng đợi
				newCodes.Add(new MovieCode
				{
					CodeText = cleanedCode,
					NormalizedCode = normalized, // Gán bảo bối vào đây
					AuthorId = dto.AuthorId,
					ActorName = dto.ActorName,
					Category = dto.Category,
					ViewCount = 0,
					CreatedAt = DateTime.UtcNow,
					AverageRating = 0,
					TotalRatings = 0
				});

				addedCount++;
			}

			// 🥊 5. LƯU DATABASE VÀ PHÁT LƯƠNG
			if (newCodes.Any())
			{
				await _repository.AddCodesAsync(newCodes);

				// Thưởng 10 điểm nhân phẩm cho mỗi siêu phẩm
				author.ActionPoints += (addedCount * 10);

				await _repository.SaveChangesAsync();
			}

			// 6. TRẢ VỀ DTO
			foreach (var code in newCodes)
			{
				resultDtos.Add(new MovieCodeDto
				{
					Id = code.Id,
					CodeText = code.CodeText,
					ActorName = code.ActorName,
					Category = code.Category,
					ViewCount = code.ViewCount,
					Author = author.Username, // Hiển thị tên thật luôn cho ngầu
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

		public async Task<UserProfileDto> GetUserProfileAsync(int targetUserId, int currentUserId)
		{
			return await _repository.GetUserProfileAsync(targetUserId, currentUserId);
		}

		public async Task<List<LeaderboardUserDto>> GetLeaderboardAsync()
		{
			const string cacheKey = "LeaderboardTop10";

			// Kiểm tra xem trong Cache đã có bản lưu tạm nào chưa?
			if (!_cache.TryGetValue(cacheKey, out List<LeaderboardUserDto> top10))
			{
				// Nếu chưa có (hoặc đã quá 5 phút bị xóa), thì gọi xuống DB để tính toán lại
				top10 = await _repository.GetTop10LeaderboardAsync();

				// Setup thời gian sống cho Cache là 5 phút
				var cacheOptions = new MemoryCacheEntryOptions()
					.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

				// Lưu kết quả vào Cache
				_cache.Set(cacheKey, top10, cacheOptions);
			}

			return top10;
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