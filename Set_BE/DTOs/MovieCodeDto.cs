namespace Set_BE.DTOs
{
	public class MovieCodeDto
	{
		public int Id { get; set; }
		public string CodeText { get; set; } = string.Empty;
		public string Author { get; set; } = string.Empty;
		public string TimeAgo { get; set; } = string.Empty;
		public double AvgRating { get; set; }
		public bool IsWatched { get; set; }
	}
}
