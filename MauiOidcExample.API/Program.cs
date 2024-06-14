
namespace MauiOidcExample.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthentication().AddJwtBearer(options =>
            {
                if (builder.Environment.IsDevelopment()) options.IncludeErrorDetails = true;
                // These can help you find out possible issues.
                /*
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters.ValidateAudience = false;
                options.TokenValidationParameters.ValidateIssuer = false;
                options.TokenValidationParameters.ValidateLifetime = false;
                options.TokenValidationParameters.ValidateIssuerSigningKey = false;
                */
            });
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddSwaggerService();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseStaticFiles();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}
