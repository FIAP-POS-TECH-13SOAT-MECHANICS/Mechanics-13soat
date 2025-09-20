using Mechanics.Api.Extensions;
using Mechanics.Infra.CrossCutting.IoC.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerDocumentation();

builder.Services.AddDbContext(builder.Configuration)
    .AddCustomAuthentication(builder.Configuration)
    .AddApplicationServices(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbHealthCheck();

builder.Services.AddGlobalCorsPolicy();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
}

app.UseRouting();
app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.ApplyMigrations();
app.UseHealthChecks("/health");

await app.RunAsync();
