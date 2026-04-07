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
			var approvedCodesCount = await _context.MovieCodes.CountAsync(c => c.AuthorId == authorId);
			if (approvedCodesCount >= 10) return true;

			// --- BƯỚC 2: TẠO LỆNH TÌM KIẾM MỞ RỘNG (KHÔNG ÉP SITE) ---
			string query = "";

			if (category == "Haiten")
			{
				// Thêm chữ nhentai, hitomi để Google tự hiểu ngữ cảnh, không khóa chết domain .net hay .la nữa
				query = $"\"{code}\" (nhentai OR hitomi OR doujinshi)";
			}
			else
			{
				// Tuyệt chiêu: Tìm thẳng mã code + chữ jav hoặc tên các web nổi tiếng
				query = $"\"{code}\" (jav OR missav OR jable OR javlibrary)";
			}

			// 3. Gọi SerpApi
			string url = $"https://serpapi.com/search.json?engine=google&q={Uri.EscapeDataString(query)}&api_key={_apiKey}";

			try
			{
				var response = await _httpClient.GetAsync(url);
				if (!response.IsSuccessStatusCode) return true;

				var content = await response.Content.ReadAsStringAsync();
				using var doc = JsonDocument.Parse(content);

				// Chỉ cần tìm thấy MỘT KẾT QUẢ BẤT KỲ trên quả đất này là duyệt!
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
