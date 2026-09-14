using System.Text.Json.Serialization;
using SmartX.Api.Repositories;
using SmartX.Api.Repositories;
using SmartX.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<
    ISensorRepository,
    SensorRepository>();

builder.Services.AddSingleton<
    ITelemetryRepository,
    TelemetryRepository>();

builder.Services.AddSingleton<
    TelemetryValidationService>();

builder.Services.AddSingleton<
    TelemetryHistoryService>();

builder.Services.AddSingleton<
    IAttachmentRepository,
    AttachmentRepository>();

builder.Services.AddSingleton<
    TypedTelemetryStore>();

// Add services to the container.

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

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
