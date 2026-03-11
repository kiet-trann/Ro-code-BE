namespace Set_BE.Models
{
	public class Rating
	{
		public int Id { get; set; }
		public int Score { get; set; } // 1 đến 5 sao
		public DateTime RatedAt { get; set; } = DateTime.UtcNow;

		// Foreign Keys
		public int UserId { get; set; }
		public User? User { get; set; }

		public int MovieCodeId { get; set; }
		public MovieCode? MovieCode { get; set; }
	}
}
