namespace Set_BE.DTOs
{
	public class CreateCodeDto
	{
		public string CodeText { get; set; } = string.Empty;
		public int AuthorId { get; set; }
	}
}
