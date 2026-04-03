namespace Set_BE.Models
{
	public class SavedCode
	{
		public int UserId { get; set; }
		public User User { get; set; }

		public int MovieCodeId { get; set; }
		public MovieCode MovieCode { get; set; }

		// Lưu lại thời điểm bấm lưu, để sau này có thể sắp xếp code mới lưu lên đầu
		public DateTime SavedAt { get; set; } = DateTime.UtcNow;
	}
}
