namespace Set_BE.DTOs
{
	public class CreateCommentDto
	{
		public string Content { get; set; } = string.Empty;
		public int UserId { get; set; }
	}
}
