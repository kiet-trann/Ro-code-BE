namespace Set_BE.Models
{
	public class User
	{
		public int Id { get; set; }
		public string Username { get; set; } = string.Empty;
		public string Passcode { get; set; } = string.Empty;
		public int ActionPoints { get; set; } = 0;
		public string RankTier => ActionPoints switch
		{
			>= 500 => "Thách Đấu Sét",
			>= 200 => "Sét Kim Cương",
			>= 50 => "Sét Vàng",
			>= 10 => "Sét Đồng",
			_ => "Sét Nhựa" // Default cho lính mới
		};
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// Navigation properties
		public ICollection<MovieCode> PostedCodes { get; set; } = new List<MovieCode>();
		public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
		public DateTime? LastSpinAt { get; set; }
	}
}
