namespace PackageStack.AzureFunction;

public class NewProvisioningPackage(PackageSerializerService packageSerializer, AzureBlobStorageService azureBlobStorage)
{
    [Function("NewProvisioningPackage")]
    public  async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        var packageRequest = await req.ReadFromJsonAsync<ProvisioningPackageRequest>();
        if (packageRequest is null) return new BadRequestObjectResult(new { error = "Invalid request body" });

        try
        {
            var packagePath = await packageSerializer.GeneratePackage(packageRequest);
            var fileStream = new FileStream(packagePath, FileMode.Open, FileAccess.Read, FileShare.None,
                bufferSize: 1024 * 1024, useAsync: true);
            switch (packageRequest.ReturnType)
            {
                case ReturnType.Base64:
                    var cryptoStream = new CryptoStream(fileStream, new ToBase64Transform(), CryptoStreamMode.Read,
                        leaveOpen: false);
                    return new FileStreamResult(cryptoStream, "text/plain");
                case ReturnType.File:
                    return new FileStreamResult(fileStream, "application/octet-stream")
                    {
                        FileDownloadName = $"{packageRequest.PackageConfig.Name}.ppkg"
                    };
                case ReturnType.SasUrl:
                    var sasUrl = await azureBlobStorage.UploadAsync(fileStream, "packages", $"{packageRequest.PackageConfig.Name}.ppkg");
                    return new OkObjectResult(new { url = sasUrl });
                default:
                    return new BadRequestResult();
            }
        }
        catch (Exception ex)
        {
            return ex is WimBuilderException or AzureBlobStorageException ?
                new ObjectResult(new { error = ex.Message, inner = ex.InnerException?.Message ?? "" })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                } : 
                new BadRequestObjectResult(new { error = ex.Message, inner = ex.InnerException?.Message ?? "" });
        }
    }
}