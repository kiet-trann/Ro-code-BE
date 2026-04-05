using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Set_BE.Data;
using Set_BE.Interfaces;

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
			string site = (category == "Haiten") ? "nhentai.net" : "missav.ws";
			string url = $"https://serpapi.com/search.json?engine=google&q=site:{site}+\"{code}\"&api_key={_apiKey}";

			try
			{
				var response = await _httpClient.GetAsync(url);
				if (!response.IsSuccessStatusCode) return true; // Nếu SerpApi lỗi quota, cho qua để tránh block oan

				var content = await response.Content.ReadAsStringAsync();
				var json = JObject.Parse(content);

				var organicResults = json["organic_results"] as JArray;
				return organicResults != null && organicResults.Count > 0;
			}
			catch
			{
				return true; // Lỗi mạng bất ngờ thì cho qua
			}
		}
	}
}
