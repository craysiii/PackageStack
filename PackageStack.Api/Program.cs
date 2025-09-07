var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<PackageSerializerService>();
builder.Services.AddSingleton<WimBuilderService>();
builder.Services.AddScoped<AzureBlobStorageService>();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Services.GetService<WimBuilderService>();

app.MapPost("/api/NewProvisioningPackage", async (
    [FromBody] ProvisioningPackageRequest packageRequest,
    [FromServices] PackageSerializerService packageSerializer,
    [FromServices] AzureBlobStorageService azureBlobStorage
) =>
{
    try
    {
        var packagePath = await packageSerializer.GeneratePackage(request: packageRequest);
        var fileStream = new FileStream(packagePath, FileMode.Open, FileAccess.Read, FileShare.None,
            bufferSize: 1024 * 1024, useAsync: true);
            
        switch (packageRequest.ReturnType)
        {
            case ReturnType.Base64:
                var cryptoStream = new CryptoStream(fileStream, new ToBase64Transform(), CryptoStreamMode.Read,
                    leaveOpen: false);
                return Results.Stream(cryptoStream, "text/plain");
            case ReturnType.File:
                return Results.Stream(fileStream, "application/octet-stream", $"{packageRequest.PackageConfig.Name}.ppkg");
            case ReturnType.SasUrl:
                var sasUrl = await azureBlobStorage.UploadAsync(fileStream, "packages", $"{packageRequest.PackageConfig.Name}.ppkg");
                return Results.Ok(new { url = sasUrl });
            default:
                return Results.BadRequest();
        }
    }
    catch (Exception ex)
    {
        return ex is WimBuilderException or AzureBlobStorageException ?
            Results.InternalServerError(new { error = ex.Message, inner = ex.InnerException?.Message ?? "" }) :
            Results.BadRequest(new { error = ex.Message, inner = ex.InnerException?.Message ?? "" });
    }
});

app.Run();
