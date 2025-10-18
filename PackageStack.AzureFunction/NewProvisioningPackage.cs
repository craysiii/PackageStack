namespace PackageStack.AzureFunction;

public class NewProvisioningPackage(PackageSerializerService packageSerializer, AzureBlobStorageService azureBlobStorage)
{
    [Function("NewProvisioningPackage")]
    public  async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        var packageRequest = await req.ReadFromJsonAsync<ProvisioningPackageRequest>();
        if (packageRequest is null) return new BadRequestObjectResult(new { error = "Invalid request body" });
        
        switch (packageRequest.ReturnType)
        {
            case ReturnType.SasUrl when string.IsNullOrWhiteSpace(packageRequest.ContainerName):
                return new BadRequestObjectResult(new { error = "Container Name is required for SasUrl return type" });
            case ReturnType.File or ReturnType.SasUrl when string.IsNullOrWhiteSpace(packageRequest.FileName):
                return new BadRequestObjectResult(new { error = "File Name is required for File or SasUrl return type" });
        }

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
                        FileDownloadName = packageRequest.FileName
                    };
                case ReturnType.SasUrl:
                    var sasUrl = await azureBlobStorage.UploadAsync(fileStream, packageRequest.ContainerName!, packageRequest.FileName!);
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