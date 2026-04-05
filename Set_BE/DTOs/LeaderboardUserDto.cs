namespace Set_BE.DTOs
{
	public class LeaderboardUserDto
	{
		public int Rank { get; set; }
		public int UserId { get; set; }
		public string Username { get; set; }
		public string AvatarUrl { get; set; }
		public int TotalScore { get; set; }
		public int TotalUploads { get; set; }
		public int TotalViews { get; set; }
	}
}
