using Mechanics.Api.Conventions;
using Mechanics.Api.Extensions;
using Mechanics.Infra.CrossCutting.IoC.Extensions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Mechanics.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers(options =>
            options.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseParameterTransformer())));
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
        app.MapControllers()
            .RequireAuthorization();

        await app.ApplyMigrations();
        app.UseHealthChecks("/health");

        await app.RunAsync();
    }
}
