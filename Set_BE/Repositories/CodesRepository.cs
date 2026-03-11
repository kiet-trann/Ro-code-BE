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

		public async Task SaveChangesAsync()
		{
			await _context.SaveChangesAsync();
		}


	}
}
