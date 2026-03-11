namespace Set_BE.DTOs
{
	public class PagedResponse<T>
	{
		public IEnumerable<T> Items { get; set; } = new List<T>();
		public int CurrentPage { get; set; }
		public int TotalPages { get; set; }
	}
}
