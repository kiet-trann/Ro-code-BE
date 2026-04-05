using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Set_BE.Data;
using Set_BE.Interfaces;
using System.Text.Json;

namespace Set_BE.Services
{
	public class CodeValidatorService : ICodeValidatorService
	{
		private readonly HttpClient _httpClient;
		private readonly string _apiKey;
		private readonly SetDbContext _context;

		public CodeValidatorService(HttpClient httpClient, IConfiguration config, SetDbContext context)
		{
			_httpClient = httpClient;
			_apiKey = config["SerpApi:ApiKey"];
			_context = context;
		}

		public async Task<bool> IsCodeRealAsync(string code, string category, int authorId)
		{
			// --- BƯỚC 1: KIỂM TRA NHÂN PHẨM (BYPASS LOGIC) ---
			// Đếm xem user này đã đóng góp bao nhiêu code "sạch" rồi
			var approvedCodesCount = await _context.MovieCodes
				.CountAsync(c => c.AuthorId == authorId);

			// NẾU LÀ LÃO LÀNG (Ví dụ: Đã share > 10 code): CHO QUA LUÔN (Tiết kiệm API)
			if (approvedCodesCount >= 10)
			{
				return true;
			}

			// --- BƯỚC 2: CHỈ CHECK GOOGLE CHO LÍNH MỚI (< 10 CODE) ---
			string siteQuery = "";

			if (category == "Haiten")
			{
				// Thêm bao nhiêu tùy thích cho Haiten
				siteQuery = "(site:nhentai.net OR site:hitomi.la)";
			}
			else
			{
				siteQuery = "(site:missav.ws OR site:jable.tv OR site:javlibrary.com OR site:missav123.com)";
			}

			// 3. Tạo câu lệnh Search tổng hợp
			// Ví dụ: (site:missav.ws OR site:jable.tv) "ABCD-123"
			string query = $"{siteQuery} \"{code}\"";
			string url = $"https://serpapi.com/search.json?engine=google&q={Uri.EscapeDataString(query)}&api_key={_apiKey}";

			try
			{
				var response = await _httpClient.GetAsync(url);
				if (!response.IsSuccessStatusCode) return true;

				var content = await response.Content.ReadAsStringAsync();
				using var doc = JsonDocument.Parse(content);

				// Chỉ cần 1 trong các site trên có kết quả là duyệt!
				if (doc.RootElement.TryGetProperty("organic_results", out var results))
				{
					return results.GetArrayLength() > 0;
				}

				return false;
			}
			catch
			{
				return true;
			}
		}
	}
}
