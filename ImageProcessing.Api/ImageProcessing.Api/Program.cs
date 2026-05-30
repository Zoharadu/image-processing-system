using ImageProcessing.Api.Middleware;
using ImageProcessing.Core.Interfaces;
using ImageProcessing.Services.Services;
using ImageProcessing.Services.Stores;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<IImageStore, InMemoryImageStore>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
