namespace PackageStack.Common.Services;

public class AzureBlobStorageService
{
    public async Task<string> UploadAsync(Stream stream, string containerName, string blobName)
    {
        var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new AzureBlobStorageException("AzureWebJobsStorage environment variable not defined", null);

        try
        {
            var blobServiceClient = new BlobServiceClient(connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await blobContainerClient.CreateIfNotExistsAsync();

            var blobClient = blobContainerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(stream, true);
            await stream.DisposeAsync();
        
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddYears(100)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }
        catch (Exception ex)
        {
            throw new AzureBlobStorageException("Error while uploading file to Azure Blob Storage", ex);
        }
    }
}

public class AzureBlobStorageException(string message, Exception? inner) : Exception(message, inner);