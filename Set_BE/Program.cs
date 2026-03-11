
using Microsoft.EntityFrameworkCore;
using Set_BE.Data;
using Set_BE.Interfaces;
using Set_BE.Repositories;
using Set_BE.Services;

namespace Set_BE
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

			builder.Services.AddDbContext<SetDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

			builder.Services.AddScoped<ICodesRepository, CodesRepository>();
			builder.Services.AddScoped<ICodesService, CodesService>();

			builder.Services.AddCors(options =>
			{
				options.AddPolicy("AllowReactApp",
					policy =>
					{
						policy.WithOrigins("http://localhost:5173",
						                   "https://ro-code.me"
                                           ) 
							  .AllowAnyHeader()
							  .AllowAnyMethod();
					});
			});

			var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
			app.UseCors("AllowReactApp");
			app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
