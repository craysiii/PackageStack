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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Package Stack API",
        Description = "Minimal API to generate packages"
    });
});

builder.Services.AddScoped<PackageSerializerService>();
builder.Services.AddSingleton<WimBuilderService>();
builder.Services.AddScoped<AzureBlobStorageService>();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = string.Empty;
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Package Stack API");
});

app.Services.GetService<WimBuilderService>();

app.MapPost("/api/NewProvisioningPackage", async (
    [FromBody] ProvisioningPackageRequest packageRequest,
    [FromServices] PackageSerializerService packageSerializer,
    [FromServices] AzureBlobStorageService azureBlobStorage
) =>
{
    try
    {
        switch (packageRequest.ReturnType)
        {
            case ReturnType.SasUrl when string.IsNullOrWhiteSpace(packageRequest.ContainerName):
                return Results.BadRequest(new { error = "Container Name is required for SasUrl return type" });
            case ReturnType.File or ReturnType.SasUrl when string.IsNullOrWhiteSpace(packageRequest.FileName):
                return Results.BadRequest(new { error = "File Name is required for File or SasUrl return type" });
        }

        var packagePath = await packageSerializer.GeneratePackage(request: packageRequest);
        var fileStream = new FileStream(packagePath, FileMode.Open, FileAccess.Read, FileShare.None,
            bufferSize: 1024 * 1024, useAsync: true);

        return packageRequest.ReturnType switch
        {
            ReturnType.Base64 => Results.Stream(new CryptoStream(fileStream, new ToBase64Transform(), CryptoStreamMode.Read, leaveOpen: false), "text/plain"),
            ReturnType.File => Results.Stream(fileStream, "application/octet-stream", packageRequest.FileName),
            ReturnType.SasUrl => Results.Ok(new { url = await azureBlobStorage.UploadAsync(fileStream, packageRequest.ContainerName!, packageRequest.FileName!) }),
            _ => Results.BadRequest()
        };
    }
    catch (Exception ex)
    {
        return ex is WimBuilderException or AzureBlobStorageException ?
            Results.Json(new { error = ex.Message, inner = ex.InnerException?.Message ?? "" }, statusCode: 500) :
            Results.BadRequest(new { error = ex.Message, inner = ex.InnerException?.Message ?? "" });
    }
});

app.Run();
