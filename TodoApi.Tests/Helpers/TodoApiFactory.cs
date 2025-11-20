using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Data;

namespace TodoApi.Tests.Helpers;

public class TodoApiFactory : WebApplicationFactory<Program>
{
    private static int _databaseCounter = 0;
    private readonly string _dbName;

    public TodoApiFactory()
    {
        _dbName = $"TestDb_{Interlocked.Increment(ref _databaseCounter)}_{Guid.NewGuid()}";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TodoDb>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext using an in-memory database for testing with unique name
            services.AddDbContext<TodoDb>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<TodoDb>();

            // Ensure the database is created
            db.Database.EnsureCreated();
        });
    }
}
