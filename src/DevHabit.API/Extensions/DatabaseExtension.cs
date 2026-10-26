
using DevHabit.API.Database;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.API.Extensions;

public static class DatabaseExtension
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            await dbContext.Database.MigrateAsync();
            app.Logger.LogInformation("Database migrations applied successfully");
        }
        catch (Exception e)
        {
            app.Logger.LogError(e, "An error occurred while applying database migrations");
            throw;
        }
    }
}
