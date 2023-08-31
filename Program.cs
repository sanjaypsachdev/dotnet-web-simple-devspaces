using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//var app = builder.Build();

// Configure the HTTP request pipeline.
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Home/Error");
//     // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//     app.UseHsts();
// }

// app.UseHttpsRedirection();
// app.UseStaticFiles();

// app.UseRouting();

// app.UseAuthorization();

// app.Logger.LogInformation("Adding Routes");

// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}");


builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddControllers();

///Nlog configuration
// builder.Services.AddSingleton<INavClient, NavClient>();

// builder.Services.AddHttpClient<INavClient, NavClient>()
//     .ConfigureHttpClient((serviceProvider, httpClient) =>
//     {
//         httpClient.Timeout = TimeSpan.FromSeconds(120);
//         httpClient.DefaultRequestHeaders.Add("TargetApiVersion", "1001");
//         httpClient.DefaultRequestHeaders.Add("targetapiplatform", "web");
//         httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
//     })
//     .SetHandlerLifetime(TimeSpan.FromMinutes(5))    /// Default is 2 mins
//     .ConfigurePrimaryHttpMessageHandler(x =>
//         new HttpClientHandler
//         {
//             AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
//             UseCookies = false,
//             AllowAutoRedirect = false,
//             UseDefaultCredentials = true,
//         });

//Health Check Configuration
builder.Services.AddHealthChecksUI().AddInMemoryStorage();
//builder.Services.AddHealthChecks()
//.AddCheck<SessionCheck>("Session API", tags: new[] { "POD Readiness"})
//.AddCheck<SRPCheck>("Flight Search API", tags: new[] { "POD Readiness" })
//.AddCheck<GetPassengerCheck>("Get Passenger API", tags: new[] { "POD Readiness" })
//.AddCheck<PostPassengerCheck>("Post Passenger API", tags: new[] { "POD Readiness" });



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecksUI();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// app.UseEndpoints(endpoints =>
// {
//     endpoints.MapHealthChecks("/health",
//      new HealthCheckOptions()
//      {

//          ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
//      }
//     );
// });

app.Logger.LogInformation("Starting the app");

var appResourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("dotnet-web-simple", "1.0");

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddOpenTelemetry(options =>
    {
        options.SetResourceBuilder(appResourceBuilder);
        options.AddOtlpExporter(option =>
        {
            var otel_collector_endpoint = System.Environment.GetEnvironmentVariable("OTEL_COLLECTOR_ENDPOINT");
            option.Endpoint = new Uri(otel_collector_endpoint);
        });
    });
});

var logger = loggerFactory.CreateLogger<Program>();

logger.LogDebug("This is a debug message from dotnet-web-simple-2", LogLevel.Debug);
logger.LogInformation("Information messages from dotnet-web-simple are used to provide contextual information", LogLevel.Information);
logger.LogError(new Exception("Application exception"), "dotnet-web-simple ==> These are usually accompanied by an exception");

app.Run();
