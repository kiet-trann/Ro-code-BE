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
		Task<MovieCodeDto> SpinRandomCodeAsync(int userId);

	}
}
