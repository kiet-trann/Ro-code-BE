using System.ComponentModel.DataAnnotations;

namespace Set_BE.Models
{
	public class Comment
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string Content { get; set; } = string.Empty;

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// Khóa ngoại liên kết với bài đăng Code
		public int MovieCodeId { get; set; }

		// Khóa ngoại liên kết với người bình luận (User)
		public int UserId { get; set; }
	}
}
