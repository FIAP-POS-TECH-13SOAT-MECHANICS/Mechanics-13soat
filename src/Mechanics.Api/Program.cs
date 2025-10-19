using Mechanics.Api.Extensions;
using Mechanics.Api.Middlewares;
using Mechanics.Infra.CrossCutting.IoC.Extensions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Text.Json;
using System.Text.Json.Serialization;
using KebabCaseParameterTransformer = Mechanics.Api.Extensions.KebabCaseParameterTransformer;

namespace Mechanics.Api;

public class Program
{
    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers(options =>
        {
            options.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseParameterTransformer()));
            options.Filters.Add<RequestValidationFilter>();
        }).AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerDocumentation();

        builder.Services.AddDbContext(builder.Configuration)
            .AddCustomAuthentication(builder.Configuration)
            .AddAppServices(builder.Configuration)
            .AddRequestValidators()
            .AddEmailSender(builder.Configuration);

        builder.Services.AddHealthChecks()
            .AddDbHealthCheck();

        builder.Services.AddGlobalCorsPolicy();

        var app = builder.Build();

        app.UseRouting();
        app.UseCors("AllowAllOrigins");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers()
            .RequireAuthorization();

        app.UseMiddleware<DomainValidationMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerDocumentation();
            await app.ApplyMigrations();
        }

        app.UseHealthChecks("/health");

        await app.RunAsync();
    }
}
