namespace Set_BE.Models
{
	public class MovieCode
	{
		public int Id { get; set; }
		public string CodeText { get; set; } = string.Empty; // Mã ẩn danh
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// Caching điểm trung bình để Query Top Tier cho nhanh, không cần tính toán lại mỗi lần load
		public double AverageRating { get; set; } = 0.0;
		public int TotalRatings { get; set; } = 0;

		// Foreign Key
		public int AuthorId { get; set; }
		public User? Author { get; set; }

		public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
	}
}
