using Set_BE.DTOs;
using Set_BE.Models;

namespace Set_BE.Interfaces
{
	public interface ICodesRepository
	{
		Task<(IEnumerable<MovieCode> Codes, int Total)> GetTrendingCodesAsync(int page, int pageSize);
		Task<(IEnumerable<MovieCode> Codes, int Total)> GetNewCodesAsync(int page, int pageSize);
		Task<(IEnumerable<MovieCode> Codes, int Total)> GetRecommendedCodesAsync(int userId, int page, int pageSize);
		Task<(IEnumerable<MovieCode> Codes, int Total)> GetWatchedCodesAsync(int userId, int page, int pageSize);
		Task<(IEnumerable<MovieCode> Codes, int Total)> GetDroppedCodesAsync(int userId, int page, int pageSize);
		Task<MovieCode?> GetCodeByIdAsync(int id);
		Task<Rating?> GetUserRatingAsync(int userId, int codeId);
		Task AddCodeAsync(MovieCode code);
		Task AddCodesAsync(IEnumerable<MovieCode> codes);
		Task AddRatingAsync(Rating rating);
		Task SaveChangesAsync();
		Task<(IEnumerable<MovieCode> Codes, int TotalCount)> SearchCodesAsync(string keyword, string category, int page, int pageSize);
		// 1. Hàm bật/tắt trạng thái lưu (Toggle)
		Task<bool> ToggleSaveCodeAsync(int userId, int codeId);

		// 2. Hàm lấy danh sách Tủ Đồ của user
		Task<(IEnumerable<MovieCode> Codes, int TotalCount)> GetSavedCodesAsync(int userId, int page, int pageSize);
		Task<UserProfileDto> GetUserProfileAsync(int targetUserId, int currentUserId);
	}
}
