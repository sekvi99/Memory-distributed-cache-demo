namespace CacheDemo.Extensions;

public static class WebApplicationExtension
{
    public static WebApplication UseSwaggerUI(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "CacheDemo API v1");
            options.RoutePrefix = string.Empty; // Swagger UI at root (https://localhost:5001/)
        });
        return app;
    }
}