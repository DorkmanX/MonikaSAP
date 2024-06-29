using MonikaSAP.Services;
using MonikaSAP.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddScoped<IPreprocessingService,PreprocessingService>();
builder.Services.AddScoped<ICalculatingService, CalculatingService>();

var app = builder.Build();
// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
