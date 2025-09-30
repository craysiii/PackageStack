using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using PackageStack.Common.Enums;
using PackageStack.Common.Models;
using PackageStack.Common.Services;

// ReSharper disable InconsistentNaming

namespace PackageStack.Module;

[Cmdlet(VerbsCommon.New, "PackageStackProvisioningPackage")]
public class NewPackageStackProvisioningPackage : PSCmdlet
{
    [Parameter(
        Mandatory = true
    )]
    public required PackageConfig PackageConfiguration { get; set; }
    
    [Parameter(
        Mandatory = false
    )]
    public Common.Models.Azure? AzureConfiguration { get; set; }
    
    [Parameter(
        Mandatory = false
    )]
    public ComputerAccount? ComputerConfiguration { get; set; }
    
    [Parameter(
        Mandatory = false
    )]
    public OOBE? OOBEConfiguration { get; set; }
    
    [Parameter(
        Mandatory = false
    )]
    public User[]? UserConfigurations { get; set; }
    
    [Parameter(
        Mandatory = false
    )]
    public ProvisioningCommand[]? ProvisioningCommands { get; set; }
    
    [Parameter(
        Mandatory = false
    )]
    public WLANSetting[]? WLANSettings { get; set; }
    
    [Parameter(
        ParameterSetName = "File",
        Mandatory = false
    )]
    public string? OutputPath { get; set; }
    
    [Parameter(
        ParameterSetName = "Base64",
        Mandatory = false
    )]
    public SwitchParameter AsBase64 { get; set; }
    
    [Parameter(
        ParameterSetName = "BlobStorage",
        Mandatory = false
    )]
    public string? ContainerName { get; set; }
    
    [Parameter(
        ParameterSetName = "BlobStorage",
        Mandatory = false    
    )]
    public string? BlobName { get; set; }
    
    

    private PackageSerializerService? _packageSerializerService;
    
    private WimBuilderService? _wimBuilderService;
    
    private AzureBlobStorageService? _azureBlobStorageService;
    
    private readonly Guid _requestId = Guid.NewGuid();
    
    private string? _tempPackagePath;

    private class InternalHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient();
        }
    }

    protected override void BeginProcessing()
    {
        _packageSerializerService = new PackageSerializerService(
            new InternalHttpClientFactory(),
            new Logger<PackageSerializerService>(new LoggerFactory())
        );

        // Unload the WIM builder service if it is already loaded
        try
        {
            WimBuilderService.Unload();
        }
        catch (Exception)
        {
            // Ignore
        }
        _wimBuilderService = new WimBuilderService(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

        _azureBlobStorageService = new AzureBlobStorageService();
    }
    
    protected override void ProcessRecord()
    {
        var package = new ProvisioningPackageRequest
        {
            RequestId = _requestId,
            ReturnType = ReturnType.File, // Doesn't matter for this command
            PackageConfig = PackageConfiguration,
            Azure = AzureConfiguration,
            ComputerAccount = ComputerConfiguration,
            OOBE = OOBEConfiguration,
            LocalUsers = UserConfigurations?.ToList(),
            ProvisioningCommands = ProvisioningCommands?.ToList(),
            WLANSettings = WLANSettings?.ToList()
        };
        
        _tempPackagePath = _packageSerializerService!.GeneratePackage(package).GetAwaiter().GetResult();
        
        var fileStream = new FileStream(_tempPackagePath, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize: 1024 * 1024, useAsync: true);

        switch (ParameterSetName)
        {
            case "File":
                fileStream.Close();
                File.Copy(_tempPackagePath, OutputPath!, true);
                WriteObject(OutputPath);
                break;
            case "Base64":
                var cryptoStream = new CryptoStream(fileStream, new ToBase64Transform(), CryptoStreamMode.Read, leaveOpen: false);
                WriteObject(new StreamReader(cryptoStream).ReadToEnd());
                fileStream.Close();
                break;
            case "BlobStorage":
                var url = _azureBlobStorageService!.UploadAsync(fileStream, ContainerName!, BlobName!).GetAwaiter().GetResult();
                fileStream.Close();
                WriteObject(url);
                break;
        }
        
    }
    
    protected override void EndProcessing()
    {
        WimBuilderService.Unload();
        
        // Clean up build files since we cannot rely on the module to be unloaded
        Directory.Delete(Path.Combine(Path.GetTempPath(), "packagestack", _requestId.ToString()), true);
        if (!string.IsNullOrWhiteSpace(_tempPackagePath))
            File.Delete(_tempPackagePath!);
    }
    
}