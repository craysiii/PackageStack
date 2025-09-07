// ReSharper disable InconsistentNaming
namespace PackageStack.Common.Services;

public class PackageSerializerService(IHttpClientFactory httpClientFactory, ILogger<PackageSerializerService> logger)
{
    private ProvisioningPackageRequest? Request { get; set; }
    private XmlDocument? RunTimeDocument { get; set; }
    private string TempDirectory { get; } = Path.Combine(Path.GetTempPath(), "packagestack");
    private string? PackageDirectory { get; set; }
    private string? MultiVariantDirectory { get; set; }
    private string? MasterDatastoreDirectory { get; set; }
    private string? ProvisioningDirectory { get; set; }
    private string? RunTimeDirectory { get; set; }
    private string? DataAssetDirectory { get; set; }
    private string? CommandFileDirectory { get; set; }
    private string? DependencyDirectory { get; set; }
    private string? DownloadDirectory { get; set; }
    private int ConfigCount { get; set; }
    private int UserCount { get; set; } = -1;
    private int CommandCount { get; set; }
    private int CommandFileCount { get; set; } = -1;
    private int CommandLineCount { get; set; } = -1;
    private int ContinueInstallCount { get; set; } = -1;
    private int DependencyCount { get; set; } = -1;
    private int RestartRequiredCount { get; set; } = -1;
    private int ReturnCodeRestartCount { get; set; } = -1;
    private int ReturnCodeSuccessCount { get; set; } = -1;
    private int FileCount { get; set; }
    private int WlanCount { get; set; } = -1;
    
    // Dependencies
    private IHttpClientFactory HttpClientFactory { get; } = httpClientFactory;
    private ILogger<PackageSerializerService> Logger { get; } = logger;

    ~PackageSerializerService()
    {
        Directory.Delete(PackageDirectory!, true);
        File.Delete(Path.Join(TempDirectory, $"{Request!.RequestId}.ppkg"));
    }

    public async Task<string> GeneratePackage(ProvisioningPackageRequest request)
    {
        Request = request;

        if (Logger.IsEnabled(LogLevel.Information))
        {
            Logger.LogInformation(
                "{RequestId} New package request {PackageId} {PackageName}",
                Request.RequestId,
                Request.PackageConfig.Id,
                Request.PackageConfig.Name
            );
        }
        
        GenerateDirectoryPaths();

        // Create Multivariant.xml
        new XmlDocument()
            .CreateProvisioningMultivariantDocument()
            .Save(Path.Join(MultiVariantDirectory, "Multivariant.xml"));
        
        // Create MasterDatastore.xml
        new XmlDocument()
            .CreateProvisioningMasterDatastoreDocument()
            .Save(Path.Join(MasterDatastoreDirectory, "MasterDatastore.xml"));
        
        // Process Runtime configurations
        await ProcessRuntimeConfiguration();
        
        // Build Image
        var packagePath = WimBuilderService.BuildWim(request, TempDirectory);
        
        return packagePath;
    }

    private void GenerateDirectoryPaths()
    {
        if (Logger.IsEnabled(LogLevel.Information))
        {
            Logger.LogInformation("{RequestId} Generating directories", Request!.RequestId);
        }
        
        PackageDirectory =  Path.Join(TempDirectory, $"{Request!.RequestId}");
        MultiVariantDirectory = Path.Join(PackageDirectory, "Multivariant");
        MasterDatastoreDirectory = Path.Join(MultiVariantDirectory, ((int)ElementType.Multivariant).ToString());
        ProvisioningDirectory = Path.Join(MasterDatastoreDirectory, "Prov");
        RunTimeDirectory = Path.Join(ProvisioningDirectory, "RunTime");
        Directory.CreateDirectory(RunTimeDirectory);
        
        if (Request.ProvisioningCommands is null || !Request.ProvisioningCommands.Any(x =>
                x.CommandFile is not null || (x.Dependencies is not null && x.Dependencies.Count > 0))) return;
        
        DataAssetDirectory = Path.Join(PackageDirectory, "DataAsset");
        if (Request.ProvisioningCommands.Any(x => x.CommandFile is not null))
        {
            CommandFileDirectory = Path.Join(DataAssetDirectory, ((int)ElementType.CommandFile).ToString());
            Directory.CreateDirectory(CommandFileDirectory);
        }

        if (Request.ProvisioningCommands.Any(x => x.Dependencies is not null && x.Dependencies.Count > 0))
        {
            DependencyDirectory = Path.Join(DataAssetDirectory, ((int)ElementType.Dependency).ToString());
            Directory.CreateDirectory(DependencyDirectory);
        }
        
        DownloadDirectory = Path.Join(PackageDirectory, "Download");
        Directory.CreateDirectory(DownloadDirectory);
    }

    private async Task ProcessRuntimeConfiguration()
    {
        if (Logger.IsEnabled(LogLevel.Information))
        {
            Logger.LogInformation("{RequestId} Processing configuration", Request!.RequestId);
        }
        
        RunTimeDocument = new XmlDocument().CreateProvisioningRuntimeDocument();
        
        ProcessAzureConfiguration();
        ProcessComputerAccountConfiguration();
        ProcessOOBEConfiguration();
        ProcessUserConfigurations();
        await ProcessProvisioningCommandConfigurations();
        ProcessWLANSettingConfigurations();
        
        RunTimeDocument.Save(Path.Join(ProvisioningDirectory, "RunTime.xml"));
    }

    private void ProcessAzureConfiguration()
    {
        var azure = Request!.Azure;
        if (azure is null) return;

        if (Logger.IsEnabled(LogLevel.Information))
        {
            Logger.LogInformation("{RequestId} Processing Azure", Request!.RequestId);
        }

        var provFileName = $"{ConfigCount}__Accounts_Azure.provxml";
        
        // Write to RunTime.xml
        RunTimeDocument!
            .DocumentElement!
            .AddPackageConfigurationSet(SettingsGroup.Azure, @$"$(_prov)\RunTime\{provFileName}");
        
        // Write configuration to disk
        new XmlDocument()
            .CreateProvisioningRuntimeConfigurationDocument()
            .DocumentElement!
            .AddPackageCharacteristic("AADJ")
            .AddPackageParameter("Authority", azure.Authority, "string")
            .AddPackageParameter("BPRT", azure.BPRT, "string")
            .Save(Path.Join(RunTimeDirectory, provFileName));
        
        ConfigCount++;
    }

    private void ProcessComputerAccountConfiguration()
    {
        var computerAccount = Request!.ComputerAccount;
        if (computerAccount is null) return;

        if (Logger.IsEnabled(LogLevel.Information))
        {
            Logger.LogInformation("{RequestId} Processing computer account", Request!.RequestId);
        }
        
        var provFileName = $"{ConfigCount}__Accounts_ComputerAccount.provxml";
        
        // Write to RunTime.xml
        RunTimeDocument!
            .DocumentElement!
            .AddPackageConfigurationSet(SettingsGroup.ComputerAccount, @$"$(_prov)\RunTime\{provFileName}");
        
        // Write configuration to disk
        var baseDocument = new XmlDocument();
        var accounts = baseDocument
            .CreateProvisioningRuntimeConfigurationDocument()
            .DocumentElement!
            .AddPackageCharacteristic("Accounts");

        accounts.AddPackageCharacteristic("Domain")
            .AddPackageParameter("ComputerName", computerAccount.ComputerName, "string")
            .AddPackageParameterIfNotNull("Account", computerAccount.Account!, "string")
            .AddPackageParameterIfNotNull("Password", computerAccount.Password!, "string")
            .AddPackageParameterIfNotNull("AccountOU", computerAccount.AccountOU!, "string")
            .AddPackageParameterIfNotNull("DomainName", computerAccount.DomainName!, "string");
        
        baseDocument
            .DocumentElement!
            .AddPackageCharacteristic("Provisioning")
            .AddPackageCharacteristic("ForceSettingReload")
            .AddPackageParameter("ComputerName", "1", "integer")
            .Save(Path.Join(RunTimeDirectory, provFileName));
        
        ConfigCount++;
    }

    private void ProcessOOBEConfiguration()
    {
        var oobe = Request!.OOBE;
        if (oobe?.HideOOBE is not null)
        {
            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation("{RequestId} Processing OOBE.HideOOBE configuration", Request!.RequestId);
            }
            
            var provFileName = $"{ConfigCount}__OOBE_Desktop_HideOobe.provxml";
            
            // Write to RunTime.xml
            RunTimeDocument!
                .DocumentElement!
                .AddPackageConfigurationSet(SettingsGroup.OOBE.HideOOBE, @$"$(_prov)\RunTime\{provFileName}");
                
            new XmlDocument()
                .CreateProvisioningRuntimeConfigurationDocument()
                .DocumentElement!
                .AddPackageCharacteristic("MCSF")
                .AddPackageCharacteristic("OOBE")
                .AddPackageCharacteristic("Desktop")
                .AddPackageParameter("HideOobe", oobe.HideOOBE.ToString()!.ToLowerInvariant(), "boolean")
                .Save(Path.Join(RunTimeDirectory, provFileName));
            
            ConfigCount++;
        }

        if (oobe?.EnableCortanaVoice is not null)
        {
            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation("{RequestId} Processing OOBE.EnableCortanaVoice configuration", Request!.RequestId);
            }
            
            var provFileName = $"{ConfigCount}__OOBE_Desktop_EnableCortanaVoice.provxml";
            
            // Write to RunTime.xml
            RunTimeDocument!
                .DocumentElement!
                .AddPackageConfigurationSet(SettingsGroup.OOBE.EnableCortanaVoice, @$"$(_prov)\RunTime\{provFileName}");
            
            new XmlDocument()
                .CreateProvisioningRuntimeConfigurationDocument()
                .DocumentElement!
                .AddPackageCharacteristic("MCSF")
                .AddPackageCharacteristic("OOBE")
                .AddPackageCharacteristic("Desktop")
                .AddPackageParameter("EnableCortanaVoice", oobe.EnableCortanaVoice.ToString()!.ToLowerInvariant(), "boolean")
                .Save(Path.Join(RunTimeDirectory, provFileName));
            
            ConfigCount++;
        }
    }

    private void ProcessUserConfigurations()
    {
        foreach (var user in Request!.LocalUsers ?? [])
        {
            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation("{RequestId} Processing user {Username}", Request!.RequestId,  user.Username);
            }
            var provFileName = $"{ConfigCount}__Accounts_Users_{user.Username}.provxml";
            
            RunTimeDocument!
                .DocumentElement!
                .AddPackageConfigurationSet(
                    $"{SettingsGroup.User}{(UserCount < 0 ? "": $"_{UserCount}")}", 
                    @$"$(_prov)\RunTime\{provFileName}"
                );
            
            
            new XmlDocument()
                .CreateProvisioningRuntimeConfigurationDocument()
                .DocumentElement!
                .AddPackageCharacteristic("Accounts")
                .AddPackageCharacteristic("Users")
                .AddPackageCharacteristic(user.Username)
                .AddPackageParameter("Password", user.Password, "string")
                .AddPackageParameter("LocalUserGroup", ((int)(user.Group)).ToString(), "integer")
                .Save(Path.Join(RunTimeDirectory, provFileName));
            
            ConfigCount++;
            UserCount++;
        }
    }


    private async Task ProcessProvisioningCommandConfigurations()
    {
        var dataAssetDocument = new XmlDocument();
        var dataAssetElements = dataAssetDocument
            .CreateProvisioningDataAssetDocument();

        XmlElement? commandFileElement = null;
        XmlElement? commandDependencyElement = null;

        foreach (var provisioningCommand in Request!.ProvisioningCommands ?? [])
        {
            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation(
                    "{RequestId} Processing ProvisioningCommand {ProvisioningCommand} configuration",
                    Request.RequestId,
                    provisioningCommand.CommandFile?.Name
                );
            }
            
            // Handle CommandFile
            if (provisioningCommand.CommandFile is not null)
            {
                if (Logger.IsEnabled(LogLevel.Information))
                {
                    Logger.LogInformation(
                        "{RequestId} Processing ProvisioningCommand {ProvisioningCommand} CommandFile configuration",
                        Request.RequestId,
                        provisioningCommand.CommandFile.Name
                    );
                }

                // Download the file to the temp directory and get SHA256 hash
                var tempDownloadPath = Path.Join(DownloadDirectory, provisioningCommand.CommandFile.Name);
                var fileHash = await ProcessPackageFile(provisioningCommand.CommandFile, tempDownloadPath);
                
                // Move the file to the correct location
                var fileHashName = $"{FileCount}_{fileHash}";
                var finalAssetPath = Path.Join(CommandFileDirectory, fileHashName);
                Directory.CreateDirectory(finalAssetPath);
                File.Move(tempDownloadPath, Path.Join(finalAssetPath, provisioningCommand.CommandFile.Name));
                
                // Declare final paths
                var commandFilePath = $"{ConfigCount}__ProvisioningCommands_PrimaryContext_Command_{CommandCount}_CommandFile.provxml";
                var commandFileHashPath = @$"{fileHashName}\{provisioningCommand.CommandFile.Name}";
                
                // Add to RunTime.xml
                RunTimeDocument!
                    .DocumentElement!
                    .AddPackageConfigurationSet(
                        $"{SettingsGroup.ProvisioningCommand.CommandFile}{(CommandFileCount < 0 ? "": $"_{CommandFileCount}")}", 
                        @$"$(_prov)\RunTime\{commandFilePath}"
                    );
                
                // Write Runtime provxml
                new XmlDocument()
                    .CreateProvisioningRuntimeConfigurationDocument()
                    .DocumentElement!
                    .AddPackageCharacteristic("Provisioning")
                    .AddPackageCharacteristic("Assets")
                    .AddPackageCharacteristic("{" + Request.PackageConfig.Id + "}")
                    .AddPackageCharacteristic(
                        $"/ProvisioningCommands/PrimaryContext/Command/{CommandCount}/CommandFile")
                    .AddPackageParameter(
                        commandFileHashPath, provisioningCommand.CommandFile.Name, "string")
                    .Save(Path.Join(RunTimeDirectory, commandFilePath));

                // Add to DataAsset.xml
                commandFileElement ??= dataAssetElements
                    .AddPackageElement((int)ElementType.CommandFile)
                    .AddPackageElementMetadata("Name", "CommandFile");
                commandFileElement.AddPackageElementMetadata(commandFileHashPath,
                    $"/ProvisioningCommands/PrimaryContext/Command/{CommandCount}/CommandFile");
                
                FileCount++;
                CommandFileCount++;
                ConfigCount++;

            }
            
            // Handle CommandLine
            {
                if (Logger.IsEnabled(LogLevel.Information))
                {
                    Logger.LogInformation(
                        "{RequestId} Processing ProvisioningCommand {ProvisioningCommand} CommandLine configuration",
                        Request.RequestId,
                        provisioningCommand.CommandFile!.Name
                    );
                }
                
                var commandLineFileName = $"{ConfigCount}__ProvisioningCommands_PrimaryContext_Command_{CommandCount}_CommandLine.provxml";
            
                RunTimeDocument!
                    .DocumentElement!
                    .AddPackageConfigurationSet(
                        $"{SettingsGroup.ProvisioningCommand.CommandLine}{(CommandLineCount < 0 ? "": $"_{CommandLineCount}")}", 
                        @$"$(_prov)\RunTime\{commandLineFileName}"
                    );
            
                new XmlDocument()
                    .CreateProvisioningRuntimeConfigurationDocument()
                    .DocumentElement!
                    .AddPackageCharacteristic("ProvisioningCommands")
                    .AddPackageCharacteristic("PrimaryContext")
                    .AddPackageCharacteristic("$(__PackageID)")
                    .AddPackageCharacteristic(CommandCount.ToString())
                    .AddPackageParameter("CommandLine", provisioningCommand.CommandLine, "string")
                    .Save(Path.Join(RunTimeDirectory, commandLineFileName));
            
                CommandLineCount++;
                ConfigCount++;
            }
            
            
            // Handle ContinueInstall
            if (provisioningCommand.ContinueInstall is not null)
            {
                if (Logger.IsEnabled(LogLevel.Information))
                {
                    Logger.LogInformation(
                        "{RequestId} Processing ProvisioningCommand {ProvisioningCommand} ContinueInstall configuration",
                        Request.RequestId,
                        provisioningCommand.CommandFile!.Name
                    );
                }
                
                var continueInstallFileName = $"{ConfigCount}__ProvisioningCommands_PrimaryContext_Command_{CommandCount}_ContinueInstall.provxml";
                
                RunTimeDocument!
                    .DocumentElement!
                    .AddPackageConfigurationSet(
                        $"{SettingsGroup.ProvisioningCommand.ContinueInstall}{(ContinueInstallCount < 0 ? "": $"_{ContinueInstallCount}")}", 
                        @$"$(_prov)\RunTime\{continueInstallFileName}"
                    );
                
                new XmlDocument()
                    .CreateProvisioningRuntimeConfigurationDocument()
                    .DocumentElement!
                    .AddPackageCharacteristic("ProvisioningCommands")
                    .AddPackageCharacteristic("PrimaryContext")
                    .AddPackageCharacteristic("$(__PackageID)")
                    .AddPackageCharacteristic(CommandCount.ToString())
                    .AddPackageParameter("ContinueInstall", provisioningCommand.ContinueInstall.ToString()!.ToLowerInvariant(), "boolean")
                    .Save(Path.Join(RunTimeDirectory, continueInstallFileName));
                
                ContinueInstallCount++;
                ConfigCount++;
            }
            
            // Handle Dependencies
            if (provisioningCommand.Dependencies is not null && provisioningCommand.Dependencies.Count != 0)
            {
                var localDependencyCount = 0;
                foreach (var dependency in provisioningCommand.Dependencies)
                {
                    if (Logger.IsEnabled(LogLevel.Information))
                    {
                        Logger.LogInformation(
                            "{RequestId} Processing ProvisioningCommand {ProvisioningCommand} Dependency {Dependency} configuration",
                            Request.RequestId,
                            provisioningCommand.CommandFile!.Name,
                            dependency.Name
                        );
                    }
                    
                    // Download the file to the temp directory and get SHA256 hash
                    var tempDownloadPath = Path.Join(DownloadDirectory, dependency.Name);
                    var fileHash = await ProcessPackageFile(dependency, tempDownloadPath);
                    
                    // Move the file to the correct location
                    var fileHashName = $"{FileCount}_{fileHash}";
                    var finalDependencyPath = Path.Join(DependencyDirectory, fileHashName);
                    Directory.CreateDirectory(finalDependencyPath);
                    File.Move(tempDownloadPath, Path.Join(finalDependencyPath, dependency.Name));
                    
                    // Declare final paths
                    var dependencyFilePath = $"{ConfigCount}__ProvisioningCommands_PrimaryContext_Command_{CommandCount}_DependencyPackages_{localDependencyCount}.provxml";
                    var dependencyFileHashPath = @$"{fileHashName}\{dependency.Name}";
                    
                    // Add to RunTime.xml
                    RunTimeDocument!
                        .DocumentElement!
                        .AddPackageConfigurationSet(
                            $"{SettingsGroup.ProvisioningCommand.Dependency}{(DependencyCount < 0 ? "": $"_{DependencyCount}")}", 
                            @$"$(_prov)\RunTime\{dependencyFilePath}"
                        );
                    
                    // Write Runtime provxml
                    new XmlDocument()
                        .CreateProvisioningRuntimeConfigurationDocument()
                        .DocumentElement!
                        .AddPackageCharacteristic("Provisioning")
                        .AddPackageCharacteristic("Assets")
                        .AddPackageCharacteristic("{" + Request.PackageConfig.Id + "}")
                        .AddPackageCharacteristic(
                            $"/ProvisioningCommands/PrimaryContext/Command/{CommandCount}/DependencyPackages/Dependency")
                        .AddPackageParameter(dependencyFileHashPath, dependency.Name, "string")
                        .Save(Path.Join(RunTimeDirectory, dependencyFilePath));
                    
                    // Add to DataAsset.xml
                    commandDependencyElement ??= dataAssetElements
                        .AddPackageElement((int)ElementType.Dependency)
                        .AddPackageElementMetadata("Name", "Dependency");
                    commandDependencyElement.AddPackageElementMetadata(dependencyFileHashPath,
                        $"/ProvisioningCommands/PrimaryContext/Command/{CommandCount}/DependencyPackages/{localDependencyCount}");

                    DependencyCount++;
                    localDependencyCount++;
                    FileCount++;
                    ConfigCount++;
                }
            }
            
            // Handle RestartRequired
            if (provisioningCommand.RestartRequired is not null)
            {
                if (Logger.IsEnabled(LogLevel.Information))
                {
                    Logger.LogInformation(
                        "{RequestId} Processing ProvisioningCommand {ProvisioningCommand} RestartRequired configuration",
                        Request.RequestId,
                        provisioningCommand.CommandFile!.Name
                    );
                }
                
                var restartRequiredFileName = $"{ConfigCount}__ProvisioningCommands_PrimaryContext_Command_{CommandCount}_RestartRequired.provxml";
                
                RunTimeDocument!
                    .DocumentElement!
                    .AddPackageConfigurationSet(
                        $"{SettingsGroup.ProvisioningCommand.RestartRequired}{(RestartRequiredCount < 0 ? "": $"_{RestartRequiredCount}")}", 
                        @$"$(_prov)\RunTime\{restartRequiredFileName}"
                    );
                
                new XmlDocument()
                    .CreateProvisioningRuntimeConfigurationDocument()
                    .DocumentElement!
                    .AddPackageCharacteristic("ProvisioningCommands")
                    .AddPackageCharacteristic("PrimaryContext")
                    .AddPackageCharacteristic("$(__PackageID)")
                    .AddPackageCharacteristic(CommandCount.ToString())
                    .AddPackageParameter("RestartRequired", provisioningCommand.RestartRequired.ToString()!.ToLowerInvariant(), "boolean")
                    .Save(Path.Join(RunTimeDirectory, restartRequiredFileName));
                
                RestartRequiredCount++;
                ConfigCount++;
            }
            
            // Handle ReturnCodeRestart
            if (provisioningCommand.ReturnCodeRestart is not null)
            {
                if (Logger.IsEnabled(LogLevel.Information))
                {
                    Logger.LogInformation(
                        "{RequestId} Processing ProvisioningCommand {ProvisioningCommand} ReturnCodeRestart configuration",
                        Request.RequestId,
                        provisioningCommand.CommandFile!.Name
                    );
                }
                
                var returnCodeRestartFileName = $"{ConfigCount}__ProvisioningCommands_PrimaryContext_Command_{CommandCount}_ReturnCodeRestart.provxml";
                
                RunTimeDocument!
                    .DocumentElement!
                    .AddPackageConfigurationSet(
                        $"{SettingsGroup.ProvisioningCommand.ReturnCodeRestart}{(ReturnCodeRestartCount < 0 ? "": $"_{ReturnCodeRestartCount}")}", 
                        @$"$(_prov)\RunTime\{returnCodeRestartFileName}"
                    );
                
                new XmlDocument()
                    .CreateProvisioningRuntimeConfigurationDocument()
                    .DocumentElement!
                    .AddPackageCharacteristic("ProvisioningCommands")
                    .AddPackageCharacteristic("PrimaryContext")
                    .AddPackageCharacteristic("$(__PackageID)")
                    .AddPackageCharacteristic(CommandCount.ToString())
                    .AddPackageParameter("ReturnCodeRestart", provisioningCommand.ReturnCodeRestart.ToString()!, "integer")
                    .Save(Path.Join(RunTimeDirectory, returnCodeRestartFileName));
                
                ReturnCodeRestartCount++;
                ConfigCount++;
            }
            
            // Handle ReturnCodeSuccess
            if (provisioningCommand.ReturnCodeSuccess is not null)
            {
                if (Logger.IsEnabled(LogLevel.Information))
                {
                    Logger.LogInformation(
                        "{RequestId} Processing ProvisioningCommand {ProvisioningCommand} ReturnCodeSuccess configuration",
                        Request.RequestId,
                        provisioningCommand.CommandFile!.Name
                    );
                }
                
                var returnCodeSuccessFileName = 
                    $"{ConfigCount}__ProvisioningCommands_PrimaryContext_Command_{CommandCount}_ReturnCodeSuccess.provxml";
                
                RunTimeDocument!
                    .DocumentElement!
                    .AddPackageConfigurationSet(
                        $"{SettingsGroup.ProvisioningCommand.ReturnCodeSuccess}{(ReturnCodeSuccessCount < 0 ? "": $"_{ReturnCodeSuccessCount}")}", 
                        @$"$(_prov)\RunTime\{returnCodeSuccessFileName}"
                    );
                
                new XmlDocument()
                    .CreateProvisioningRuntimeConfigurationDocument()
                    .DocumentElement!
                    .AddPackageCharacteristic("ProvisioningCommands")
                    .AddPackageCharacteristic("PrimaryContext")
                    .AddPackageCharacteristic("$(__PackageID)")
                    .AddPackageCharacteristic(CommandCount.ToString())
                    .AddPackageParameter("ReturnCodeSuccess", provisioningCommand.ReturnCodeSuccess.ToString()!, "integer")
                    .Save(Path.Join(RunTimeDirectory, returnCodeSuccessFileName));
                
                ReturnCodeSuccessCount++;
                ConfigCount++;
            }
            
            CommandCount++;
        }

        if (FileCount <= 0) return;
        
        dataAssetDocument.Save(Path.Join(DataAssetDirectory, "DataAsset.xml"));
        Directory.Delete(DownloadDirectory!, true);
    }
    
    private void ProcessWLANSettingConfigurations()
    {
        foreach (var wlanSetting in Request!.WLANSettings ?? [])
        {
            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation(
                    "{RequestId} Processing WLANSetting {WLANSetting} configuration",
                    Request.RequestId,
                    wlanSetting.SSID
                );
            }
            
            var provFileName = $"{ConfigCount}__ConnectivityProfiles_WLAN_WLANSetting_{wlanSetting.SSID}.provxml";
            
            RunTimeDocument!
                .DocumentElement!
                .AddPackageConfigurationSet(
                    $"{SettingsGroup.WlanSetting}{(WlanCount < 0 ? "": $"_{WlanCount}")}", 
                    @$"$(_prov)\RunTime\{provFileName}"
                );

            // Create the embedded document, save to the temp file, and read again to avoid serialization issues
            new XmlDocument()
                .CreateEmbeddedWiFiDocument(wlanSetting)
                .Save(Path.Join(RunTimeDirectory, "temp_wifi.xml"));
            var finalEmbeddedXml = File.ReadAllText(Path.Join(RunTimeDirectory, "temp_wifi.xml"), Encoding.UTF8);
            File.Delete(Path.Join(RunTimeDirectory, "temp_wifi.xml"));
            
            // Write RunTime provxml
            new XmlDocument()
                .CreateProvisioningRuntimeConfigurationDocument()
                .DocumentElement!
                .AddPackageCharacteristic("WiFi")
                .AddPackageCharacteristic("Profile")
                .AddPackageCharacteristic(wlanSetting.SSID)
                .AddPackageParameter("WlanXml", finalEmbeddedXml, "string")
                .Save(Path.Join(RunTimeDirectory, provFileName));
            
            ConfigCount++;
            WlanCount++;
        }
    }

    private async Task<string> ProcessPackageFile(PackageFile packageFile, string filePath)
    {
        if (Logger.IsEnabled(LogLevel.Information))
        {
            Logger.LogInformation(
                "{RequestId} Processing PackageFile {PackageFile} download",
                Request!.RequestId,
                packageFile.Name
            );
        }

        if (string.IsNullOrWhiteSpace(packageFile.Url) && string.IsNullOrWhiteSpace(packageFile.Base64))
        {
            throw new PackageSerializerException("PackageFile must have either a Url or a Base64 value");
        }

        if (!string.IsNullOrWhiteSpace(packageFile.Url) && !string.IsNullOrWhiteSpace(packageFile.Base64))
        {
            throw new PackageSerializerException("PackageFile must have either a Url or a Base64 value, but not both");
        }

        if (!string.IsNullOrWhiteSpace(packageFile.Url))
        {
            // Download File to temp directory
            var httpClient = HttpClientFactory.CreateClient();
            await using var downloadStream = await httpClient.GetStreamAsync(packageFile.Url);
            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await downloadStream.CopyToAsync(fileStream);
            downloadStream.Close();
            fileStream.Close();
        }

        if (!string.IsNullOrWhiteSpace(packageFile.Base64))
        {
            await File.WriteAllBytesAsync(filePath, Convert.FromBase64String(packageFile.Base64));
        }

        if (Logger.IsEnabled(LogLevel.Information))
        {
            Logger.LogInformation(
                "{RequestId} Processing PackageFile {PackageFile} SHA256 calculation",
                Request!.RequestId,
                packageFile.Name
            );
        }
        
        // Calculate SHA256 hash
        await using var hashStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None,
            bufferSize: 4096, useAsync: true);
        using var sha256 = SHA256.Create();
        var fileHash = await sha256.ComputeHashAsync(hashStream);
        var hash64 = Convert.ToBase64String(fileHash).Replace('/', '.');
        hashStream.Close();

        return hash64;
    }
}

public class PackageSerializerException(string message, Exception? inner = null) : Exception(message, inner);