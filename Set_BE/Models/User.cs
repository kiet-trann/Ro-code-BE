namespace Set_BE.Models
{
	public class User
	{
		public int Id { get; set; }
		public string Username { get; set; } = string.Empty;
		public string Passcode { get; set; } = string.Empty;	
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// Navigation properties
		public ICollection<MovieCode> PostedCodes { get; set; } = new List<MovieCode>();
		public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
	}
}
