using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DynamicForms.Editor.Data;

/// <summary>
/// Design-time factory for creating ApplicationDbContext instances during migrations.
/// This allows EF Core tools to create the DbContext without a running application.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    /// <summary>
    /// Creates a new instance of ApplicationDbContext for design-time operations (migrations).
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    /// <returns>Configured ApplicationDbContext instance</returns>
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use LocalDB by default for development
        // This can be overridden by setting the EDITOR_DB_CONNECTION_STRING environment variable
        var connectionString = Environment.GetEnvironmentVariable("EDITOR_DB_CONNECTION_STRING")
            ?? "Server=(localdb)\\mssqllocaldb;Database=DynamicFormsEditor;Trusted_Connection=True;TrustServerCertificate=True;";

        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
        {
            // Enable retry on failure for transient errors
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });

        // Enable detailed errors in development
#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
#endif

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
