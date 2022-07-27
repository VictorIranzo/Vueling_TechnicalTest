namespace Vueling.OTD.Services
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Vueling.OTD.Persistence;

    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services
                .AddLogicServices(builder.Configuration)
                .AddPersistenceServices(builder.Configuration);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddLogging();

            WebApplication app = builder.Build();

            // TODO: Move.
            Context context = app.Services.CreateScope().ServiceProvider.GetService<Context>();

            if (context.Database.GetService<IDatabaseCreator>() is RelationalDatabaseCreator relationalDatabaseCreator
                && !relationalDatabaseCreator.Exists())
            {
                context.Database.EnsureCreated();
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}