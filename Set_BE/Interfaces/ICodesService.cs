using Set_BE.DTOs;

namespace Set_BE.Interfaces
{
	public interface ICodesService
	{
		Task<PagedResponse<MovieCodeDto>> GetTrendingAsync(int currentUserId, int page, int pageSize);
		Task<PagedResponse<MovieCodeDto>> GetNewAsync(int currentUserId, int page, int pageSize);
		Task<PagedResponse<MovieCodeDto>> GetRecommendedAsync(int currentUserId, int page, int pageSize);
		Task<PagedResponse<MovieCodeDto>> GetWatchedAsync(int userId, int page, int pageSize);
		Task<PagedResponse<MovieCodeDto>> GetDroppedAsync(int userId, int page, int pageSize);
		Task<IEnumerable<MovieCodeDto>> DropCodeAsync(CreateCodeDto dto);
		Task<bool> RateCodeAsync(int codeId, RateCodeDto dto);
		Task<bool> IncreaseViewCountAsync(int codeId);
		Task<MovieCodeDto> SpinRandomCodeAsync(int userId, CancellationToken cancellationToken);
		Task<PagedResponse<MovieCodeDto>> SearchCodesAsync(int currentUserId, string keyword, string category, int page, int pageSize);
		// 1. Hàm bật/tắt trạng thái lưu (Toggle)
		Task<bool> ToggleSaveCodeAsync(int userId, int codeId);

		// 2. Hàm lấy danh sách Tủ Đồ của user
		Task<PagedResponse<MovieCodeDto>> GetSavedCodesAsync(int userId, int page, int pageSize);
		Task<UserProfileDto> GetUserProfileAsync(int targetUserId, int currentUserId);
		Task<List<LeaderboardUserDto>> GetLeaderboardAsync();
	}
}
