using Microsoft.EntityFrameworkCore;
using Set_BE.Data;

namespace Set_BE.Services
{
	public class SeasonResetService : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<SeasonResetService> _logger;

		// TIÊM IServiceProvider chứ không tiêm thẳng DbContext
		public SeasonResetService(IServiceProvider serviceProvider, ILogger<SeasonResetService> logger)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation(" Cỗ Máy Mùa Giải đã khởi động ngầm...");

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					// Đòn né Timezone: Lấy giờ UTC + 7 tiếng để ra đúng giờ Việt Nam
					var vnTime = DateTime.UtcNow.AddHours(7);

					// CHUÔNG BÁO THỨC: Chạy vào ngày 1 của đầu mỗi Quý (Tháng 1, 4, 7, 10) lúc 00:xx giờ sáng
					if ((vnTime.Month - 1) % 3 == 0 && vnTime.Day == 1 && vnTime.Hour == 0)
					{
						await ProcessSeasonResetAsync();
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Lỗi khi chốt sổ mùa giải!");
				}

				// Ngủ 1 tiếng rồi dậy kiểm tra thời gian 1 lần
				await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
			}
		}

		private async Task ProcessSeasonResetAsync()
		{
			// GIẢI QUYẾT GÓC KHUẤT DB: Mở một Vùng không gian mới (Scope) để gọi DbContext
			using var scope = _serviceProvider.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<SetDbContext>();

			// 1. CHỐNG DOUBLE-KILL: Kiểm tra xem đã reset chưa 
			// (Lỡ trong 1 tiếng nó lặp lại thì sao? Nếu thằng Top 1 điểm = 0 nghĩa là đã reset rồi)
			var isAlreadyReset = !await context.Users.AnyAsync(u => u.ActionPoints > 0);
			if (isAlreadyReset) return;

			_logger.LogInformation(" BẮT ĐẦU CHỐT SỔ MÙA GIẢI...");

			// 2. Tìm Top 3 Lão Đại hiện tại
			var topUsers = await context.Users
				.Where(u => u.ActionPoints > 0)
				.OrderByDescending(u => u.ActionPoints)
				.Take(3)
				.ToListAsync();

			if (topUsers.Any())
			{
				// Mùa giải tự động sinh tên (Ví dụ: "Q2/2026")
				int quarter = (DateTime.UtcNow.AddHours(7).Month - 1) / 3 + 1;
				string seasonName = $"Q{quarter}/{DateTime.UtcNow.AddHours(7).Year}";
				string[] ranks = { "Quán Quân", "Á Quân", "Quý Quân" };

				// 3. Trao Danh Hiệu
				for (int i = 0; i < topUsers.Count; i++)
				{
					var title = $"{ranks[i]} {seasonName}";
					var user = topUsers[i];

					user.LegacyTitles = string.IsNullOrEmpty(user.LegacyTitles)
						? title
						: user.LegacyTitles + ", " + title;
				}

				// 4. THANOS BÚNG TAY: Đưa điểm toàn server về 0 (Sử dụng EF Core 7+ ExecuteUpdate siêu tốc)
				await context.Users.Where(u => u.ActionPoints > 0)
								   .ExecuteUpdateAsync(s => s.SetProperty(u => u.ActionPoints, 0));

				await context.SaveChangesAsync();
				_logger.LogInformation($"✅ ĐÃ CHỐT SỔ {seasonName}. ĐẾ CHẾ MỚI BẮT ĐẦU!");
			}
		}
	}
}
