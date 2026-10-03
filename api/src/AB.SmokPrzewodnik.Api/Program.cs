using AB.SmokPrzewodnik.Api.Extensions;
using AB.SmokPrzewodnik.Api.Errors;
using AB.SmokPrzewodnik.Application;
using AB.SmokPrzewodnik.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddExceptionHandler<ApplicationExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddSupabaseAuthentication(builder.Configuration);
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

app.UsePathBase("/api/v1");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapHealthEndpoints();
app.MapControllers();

app.Run();

public partial class Program;
