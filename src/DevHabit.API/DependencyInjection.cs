using Asp.Versioning;
using DevHabit.API.Database;
using DevHabit.API.Middlewares;
using DevHabit.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json.Serialization;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace DevHabit.API;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(options =>
        {
            // Will Reponse 406 Not Acceptable
            options.ReturnHttpNotAcceptable = true;
        })
        .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver =
            new CamelCasePropertyNamesContractResolver())
        .AddXmlSerializerFormatters();

        builder.Services.Configure<MvcOptions>(options =>
        {
            var formatter = options.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>().First();
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.JsonV1);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.JsonV2);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJson);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJsonV1);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJsonV2);
        });

        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1.0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionSelector = new DefaultApiVersionSelector(options);
            options.ApiVersionReader = ApiVersionReader.Combine(
                new MediaTypeApiVersionReader(), // for Accept: application/json;v=1.0
                new MediaTypeApiVersionReaderBuilder()
                    .Template(CustomMediaTypeNames.Application.HateoasJsonTemplate)
                    .Build()
                );
        })
        .AddMvc();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        // URL: /openapi/v1.json
        builder.Services.AddOpenApi();
        return builder;
    }
    public static WebApplicationBuilder AddErrorHandling(this WebApplicationBuilder builder)
    {
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions.Add("requestId", context.HttpContext.TraceIdentifier);
            };
        });
        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        return builder;
    }
    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options
            .UseNpgsql(
                builder.Configuration.GetConnectionString("Database"),
                npgOptions => npgOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Application))
            .UseSnakeCaseNamingConvention();
        });
        return builder;
    }
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(builder.Environment.ApplicationName)
                .AddAttributes(new Dictionary<string, object>
                {
                    {"service.name", builder.Environment.ApplicationName},
                    {"service.version", "1.0.0"},
                    {"service.instance.id", "172.0.0.1"},
                });
            })
            .WithTracing(trace => trace
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddNpgsql())
            .WithMetrics(metric => metric
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation())
            .UseOtlpExporter();

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeScopes = true; // default to false
            options.IncludeFormattedMessage = true;
        });
        return builder;
    }
}