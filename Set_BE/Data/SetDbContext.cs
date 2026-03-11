using Microsoft.EntityFrameworkCore;
using Set_BE.Models;

namespace Set_BE.Data
{
	public class SetDbContext : DbContext
	{
		public SetDbContext(DbContextOptions<SetDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
		public DbSet<MovieCode> MovieCodes { get; set; }
		public DbSet<Rating> Ratings { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Ràng buộc: 1 User chỉ được rate 1 Code duy nhất 1 lần
			modelBuilder.Entity<Rating>()
				.HasIndex(r => new { r.UserId, r.MovieCodeId })
				.IsUnique();
		}
	}
}
