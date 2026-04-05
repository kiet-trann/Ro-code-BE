using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Set_BE.Data;
using Set_BE.Interfaces;
using Set_BE.Models;

namespace Set_BE.Repositories
{
	public class CodesRepository : ICodesRepository
	{
		private readonly SetDbContext _context;

		public CodesRepository(SetDbContext context)
		{
			_context = context;
		}

		public async Task<(IEnumerable<MovieCode> Codes, int Total)> GetTrendingCodesAsync(int page, int pageSize)
		{
			var query = _context.MovieCodes
				.Include(c => c.Author)
				.Include(c => c.Ratings);

			// Đếm tổng số lượng record để tính tổng số trang
			int total = await query.CountAsync();

			// Bỏ qua các record của trang trước, và lấy số lượng của trang hiện tại
			var codes = await query
				.OrderByDescending(c => c.AverageRating)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (codes, total);
		}
		public async Task<(IEnumerable<MovieCode> Codes, int Total)> GetNewCodesAsync(int page, int pageSize)
		{
			var query = _context.MovieCodes.Include(c => c.Author).Include(c => c.Ratings);
			int total = await query.CountAsync();
			var codes = await query.OrderByDescending(c => c.CreatedAt)
				.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
			return (codes, total);
		}

		public async Task<(IEnumerable<MovieCode> Codes, int Total)> GetRecommendedCodesAsync(int userId, int page, int pageSize)
		{
			var watchedCodeIds = await _context.Ratings.Where(r => r.UserId == userId).Select(r => r.MovieCodeId).ToListAsync();
			var query = _context.MovieCodes.Include(c => c.Author).Include(c => c.Ratings).Where(c => !watchedCodeIds.Contains(c.Id));

			int total = await query.CountAsync();
			var codes = await query.OrderByDescending(c => c.AverageRating)
				.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
			return (codes, total);
		}
		public async Task<(IEnumerable<MovieCode> Codes, int Total)> GetWatchedCodesAsync(int userId, int page, int pageSize)
		{
			var query = _context.Ratings.Where(r => r.UserId == userId).Include(r => r.MovieCode).ThenInclude(mc => mc.Author);
			int total = await query.CountAsync();
			var codes = await query.OrderByDescending(r => r.RatedAt)
				.Skip((page - 1) * pageSize).Take(pageSize).Select(r => r.MovieCode!).ToListAsync();
			return (codes, total);
		}

		public async Task<(IEnumerable<MovieCode> Codes, int Total)> GetDroppedCodesAsync(int userId, int page, int pageSize)
		{
			var query = _context.MovieCodes.Where(c => c.AuthorId == userId).Include(c => c.Author).Include(c => c.Ratings);
			int total = await query.CountAsync();
			var codes = await query.OrderByDescending(c => c.CreatedAt)
				.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
			return (codes, total);
		}

		public async Task<MovieCode?> GetCodeByIdAsync(int id)
		{
			return await _context.MovieCodes.FindAsync(id);
		}

		public async Task<Rating?> GetUserRatingAsync(int userId, int codeId)
		{
			return await _context.Ratings
				.FirstOrDefaultAsync(r => r.UserId == userId && r.MovieCodeId == codeId);
		}

		public async Task AddCodeAsync(MovieCode code)
		{
			await _context.MovieCodes.AddAsync(code);
		}
		public async Task AddCodesAsync(IEnumerable<MovieCode> codes)
		{
			await _context.MovieCodes.AddRangeAsync(codes);
		}

		public async Task AddRatingAsync(Rating rating)
		{
			await _context.Ratings.AddAsync(rating);
		}
		public async Task<(IEnumerable<MovieCode> Codes, int TotalCount)> SearchCodesAsync(string keyword, string category, int page, int pageSize)
		{
			var query = _context.MovieCodes
				.Include(c => c.Author)
				.Include(c => c.Ratings)
				.AsQueryable();

			// 1. Lọc theo thể loại (Nếu có chọn khác "All")
			if (!string.IsNullOrWhiteSpace(category) && category != "All")
			{
				if (category == "Movie")
				{
					// Ép những code đời cũ (null hoặc rỗng) vào chung mâm với Movie
					query = query.Where(c => c.Category == "Movie" || c.Category == null || c.Category == "");
				}
				else
				{
					query = query.Where(c => c.Category == category);
				}
			}

			// 2. Tìm kiếm theo từ khóa (Tìm cả trong Mã Code lẫn Tên Diễn Viên)
			if (!string.IsNullOrWhiteSpace(keyword))
			{
				// Sử dụng ILike để không phân biệt hoa/thường trên PostgreSQL
				var searchPattern = $"%{keyword}%";
				query = query.Where(c =>
					EF.Functions.ILike(c.CodeText, searchPattern) ||
					(c.ActorName != null && EF.Functions.ILike(c.ActorName, searchPattern))
				);
			}

			var total = await query.CountAsync();
			var codes = await query.OrderByDescending(c => c.CreatedAt)
								   .Skip((page - 1) * pageSize)
								   .Take(pageSize)
								   .ToListAsync();

			return (codes, total);
		}
		public async Task<bool> ToggleSaveCodeAsync(int userId, int codeId)
		{
			// Kiểm tra xem user này đã lưu code này chưa
			var existingSave = await _context.SavedCodes
				.FirstOrDefaultAsync(sc => sc.UserId == userId && sc.MovieCodeId == codeId);

			if (existingSave != null)
			{
				// Đã lưu rồi -> Xóa đi (Bỏ lưu)
				_context.SavedCodes.Remove(existingSave);
				await _context.SaveChangesAsync();
				return false;
			}
			else
			{
				// Chưa lưu -> Thêm mới vào (Lưu)
				_context.SavedCodes.Add(new SavedCode { UserId = userId, MovieCodeId = codeId });
				await _context.SaveChangesAsync();
				return true;
			}
		}

		public async Task<(IEnumerable<MovieCode> Codes, int TotalCount)> GetSavedCodesAsync(int userId, int page, int pageSize)
		{
			// Bước 1: Tìm trong tủ đồ của user này
			var query = _context.SavedCodes.Where(sc => sc.UserId == userId);
			var total = await query.CountAsync();

			// Bước 2: CHỈ LẤY DANH SÁCH ID (Không Include lằng nhằng ở đây)
			var pagedSavedCodes = await query
				.OrderByDescending(sc => sc.SavedAt)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Select(sc => new { sc.MovieCodeId })
				.ToListAsync();

			var savedCodeIds = pagedSavedCodes.Select(sc => sc.MovieCodeId).ToList();

			if (!savedCodeIds.Any())
			{
				return (new List<MovieCode>(), 0); // Nếu tủ đồ trống thì trả về rỗng luôn cho lẹ
			}

			// Bước 3: Dùng danh sách ID đó để kéo MovieCode và các bảng liên quan về
			var codes = await _context.MovieCodes
				.Include(c => c.Author)
				.Include(c => c.Ratings)
				.Where(c => savedCodeIds.Contains(c.Id))
				.ToListAsync();

			// Bước 4: Sắp xếp lại thẻ Code hiển thị theo đúng thứ tự: Mới lưu nằm ở trên cùng
			var sortedCodes = pagedSavedCodes
				.Select(sc => codes.First(c => c.Id == sc.MovieCodeId))
				.ToList();

			return (sortedCodes, total);
		}
		public async Task SaveChangesAsync()
		{
			await _context.SaveChangesAsync();
		}


	}
}
