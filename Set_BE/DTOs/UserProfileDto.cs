namespace Set_BE.DTOs
{
	public class UserProfileDto
	{
		public int UserId { get; set; }
		public string Username { get; set; }
		public string AvatarUrl { get; set; }
		public int TotalUploaded { get; set; }
		public int TotalViews { get; set; }
		public int TotalSaved { get; set; }

		// Cờ báo hiệu cho Frontend biết có được hiện Tab "Tủ Đồ" hay không
		public bool IsOwner { get; set; }
	}
}
