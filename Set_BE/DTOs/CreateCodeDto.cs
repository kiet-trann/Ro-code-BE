namespace Set_BE.DTOs
{
	public class CreateCodeDto
	{
		public string CodeText { get; set; } = string.Empty;
		public int AuthorId { get; set; }
		public string ActorName { get; set; } = string.Empty; // Bắt buộc nhập tên diễn viên
		public string Category { get; set; } = "Movie";
	}
}
