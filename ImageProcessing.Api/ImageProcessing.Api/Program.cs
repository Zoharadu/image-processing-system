using ImageProcessing.Core.Interfaces;
using ImageProcessing.Services.Services;
using ImageProcessing.Services.Stores;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Application services
builder.Services.AddSingleton<IImageStore, InMemoryImageStore>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
