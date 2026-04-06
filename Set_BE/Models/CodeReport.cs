namespace Set_BE.Models
{
	public class CodeReport
	{
		public int Id { get; set; }
		public int MovieCodeId { get; set; }
		public MovieCode MovieCode { get; set; }

		public int ReporterId { get; set; }
		public User Reporter { get; set; }

		public string Reason { get; set; } = "Hàng pha ke / Sai link";
		public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
	}
}
