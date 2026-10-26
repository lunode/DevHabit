using DevHabit.API;
using DevHabit.API.DTOs.Habits;
using DevHabit.API.Entities;
using DevHabit.API.Extensions;
using DevHabit.API.Services;
using DevHabit.API.Services.Sort;
using FluentValidation;
var builder = WebApplication.CreateBuilder(args);

Console.WriteLine(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
Console.WriteLine(builder.Configuration["OTEL_EXPORTER_OTLP_PROTOCOL"]);

// Add services to the container.
builder.AddApiServices()
    .AddErrorHandling()
    .AddDatabase()
    .AddObservability();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddSingleton<SortMappingProvider>();
builder.Services.AddSingleton<ISortMappingDefinition, SortMappingDefinition<HabitDto, Habit>>(_ => HabitMappings.SortMapping);
builder.Services.AddSingleton<DataShapingService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<LinkService>();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.ApplyMigrationsAsync();
}


app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseAuthorization();

app.MapControllers();

app.Run();
