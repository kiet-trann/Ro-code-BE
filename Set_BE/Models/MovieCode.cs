namespace Set_BE.Models
{
	public class MovieCode
	{
		public int Id { get; set; }

		public string CodeText { get; set; } = string.Empty; // Mã ẩn danh
		public string NormalizedCode { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// Caching điểm trung bình để Query Top Tier cho nhanh, không cần tính toán lại mỗi lần load
		public double AverageRating { get; set; } = 0.0;
		public int TotalRatings { get; set; } = 0;

		// Foreign Key
		public int AuthorId { get; set; }
		public User? Author { get; set; }

		public ICollection<Rating> Ratings { get; set; } = new List<Rating>();

		// Tên diễn viên (bắt buộc nhập)
		public string ActorName { get; set; } = string.Empty;

		// Đếm số lượt xem (Mặc định là 0)
		public int ViewCount { get; set; } = 0;

		// Phân loại: "Movie" hoặc "Haiten"
		public string Category { get; set; } = "Movie";
		public ICollection<CodeReport> Reports { get; set; } = new List<CodeReport>();
	}
}
