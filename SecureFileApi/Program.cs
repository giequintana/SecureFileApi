using SecureFileApi.Services;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton<IFileProcessingService, FileProcessingService>();

var app = builder.Build();


var apiKey =
    builder.Configuration["FILE_API_KEY"]
    ?? builder.Configuration["ApiKey"]
    ?? Environment.GetEnvironmentVariable("FILE_API_KEY");

if (string.IsNullOrWhiteSpace(apiKey))
{
    throw new InvalidOperationException(
        "Configure FILE_API_KEY before starting the API.");
}


app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        var suppliedKey =
            context.Request.Headers["X-API-Key"].FirstOrDefault();

        var expectedBytes = Encoding.UTF8.GetBytes(apiKey);
        var suppliedBytes = Encoding.UTF8.GetBytes(
            suppliedKey ?? string.Empty);

        var authorized =
            expectedBytes.Length == suppliedBytes.Length &&
            CryptographicOperations.FixedTimeEquals(
                expectedBytes, suppliedBytes);

        if (!authorized)
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Missing or invalid API key."
            });

            return;
        }
    }

    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthorization();

app.MapControllers();

app.Run();
