using Mechanics.Application.Utils;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;

namespace Mechanics.Api.Extensions;

public static class SwaggerSetupExtensions
{
    public static void AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Mechanics API",
                Version = "v1",
                Description = "API para gestão de ordens de serviço.",
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Digite o token JWT desta forma: Bearer {seu token}",
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        },
                    },
                    Array.Empty<string>()
                },
            });

            var apiXmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, apiXmlFile), includeControllerXmlComments: true);
            var applicationXmlFile = new FileInfo(typeof(IAppService).Assembly.Location);
            c.IncludeXmlComments(Path.Combine(applicationXmlFile.DirectoryName!,
                applicationXmlFile.Name.Replace("dll", "xml")));

            c.EnableAnnotations();
            c.ExampleFilters();
        });

        services.AddSwaggerExamplesFromAssemblies(Assembly.GetExecutingAssembly());
    }

    public static void UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mechanics API v1");
            c.RoutePrefix = "swagger";
        });
    }
}
