var builder = FunctionsApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.Converters.Add(new JsonStringEnumConverter());
    options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddScoped<PackageSerializerService>();
builder.Services.AddSingleton<WimBuilderService>();
builder.Services.AddScoped<AzureBlobStorageService>();
builder.Services.AddHttpClient();
builder.Services.AddLogging();

var app = builder.Build();
// Force singleton initialization
app.Services.GetService<WimBuilderService>();
    
app.Run();