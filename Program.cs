using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using OpenTelemetry.Exporter.Console;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.Logger.LogInformation("Adding Routes");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Logger.LogInformation("Starting the app");

var serviceProvider = new ServiceCollection()
    .AddLogging((loggingBuilder) => loggingBuilder
        .SetMinimumLevel(LogLevel.Debug)
        .AddOpenTelemetry(options =>
            options.AddOtlpExporter(otlpOptions =>
            {
                // Use IConfiguration directly for Otlp exporter endpoint option.
                otlpOptions.Endpoint = new Uri(appBuilder.Configuration.GetValue<string>("Otlp:Endpoint"));
            })
        )
    .BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();

logger.LogDebug("This is a {severityLevel} message", LogLevel.Debug);
logger.LogInformation("{severityLevel} messages are used to provide contextual information", LogLevel.Information);
logger.LogError(new Exception("Application exception"), "These are usually accompanied by an exception");

app.Run();
