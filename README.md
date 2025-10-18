# PackageStack
[![PowerShell Gallery Version](https://img.shields.io/powershellgallery/v/PackageStack?label=PowerShell%20Gallery)](https://www.powershellgallery.com/packages/PackageStack)
[![PowerShell Gallery Downloads](https://img.shields.io/powershellgallery/dt/PackageStack?label=Module%20Downloads)](https://www.powershellgallery.com/packages/PackageStack/0.1.5)
[![Docker Pulls](https://img.shields.io/docker/pulls/craysiii/packagestack?label=Docker%20Pulls)](https://hub.docker.com/r/craysiii/packagestack)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

PackageStack is a collection of projects for generating Windows Provisioning Packages (`.ppkg`). These packages are a quick and easy way to configure devices running Windows without deploying a new image.

## Why PackageStack?
*   **Truly Cross-Platform:** Generate Windows provisioning packages from any major OS, including macOS and Linux. You are no longer tied to a Windows-only workstation.
*   **No Windows ADK Required:** Avoid the large and complex installation of the Windows Assessment and Deployment Kit (ADK). PackageStack is self-contained and lightweight.
*   **Flexible Usage:** Consume it the way you want. Use it as a local PowerShell module for simple scripting, or deploy it as a Docker container or serverless Azure Function for a robust, scalable web API.
*   **Automation-First Design:** With both a PowerShell module and a full web API, PackageStack is designed to be easily integrated into your CI/CD pipelines and automated infrastructure workflows.
*   **Modern and Open Source:** Built on .NET 8 and licensed under the permissive MIT license, PackageStack is a free, modern, and transparent solution that you can trust and extend.

## Table of Contents
- [Supported Features](#supported-features)
- [Usage](#usage)
- [Output Options](#output-options)
- [Deployment Options](#deployment-options)
  - [Deploying with Docker](#deploying-with-docker)
  - [Deploying to Azure](#deploying-to-azure)
- [Installing the PowerShell Module](#installing-the-powershell-module)
- [OpenAPI Specification](#openapi-specification)
- [API Examples](#api-examples)
- [PowerShell Cmdlet Reference](#powershell-cmdlet-reference)
  - [`New-PackageStackAzureConfiguration`](#new-packagestackazureconfiguration)
  - [`New-PackageStackComputerConfiguration`](#new-packagestackcomputerconfiguration)
  - [`New-PackageStackOOBEConfiguration`](#new-packagestackoobeconfiguration)
  - [`New-PackageStackPackageConfiguration`](#new-packagestackpackageconfiguration)
  - [`New-PackageStackPackageFileConfiguration`](#new-packagestackpackagefileconfiguration)
  - [`New-PackageStackProvisioningCommandConfiguration`](#new-packagestackprovisioningcommandconfiguration)
  - [`New-PackageStackUserConfiguration`](#new-packagestackuserconfiguration)
  - [`New-PackageStackWLANSettingConfiguration`](#new-packagestackwlansettingconfiguration)
  - [`New-PackageStackProvisioningPackage`](#new-packagestackprovisioningpackage)
- [Applying the Provisioning Package](#applying-the-provisioning-package)
- [Contributing](#contributing)
- [License](#license)
- [AI and Large Language Model Usage](#ai-and-large-language-model-usage)

## Supported Features
PackageStack currently supports a base set of provisioning settings to streamline your device setup process.

### User and Domain Management
- **Local User Creation**: Create local user accounts with specified usernames and passwords.
- **Azure AD Join**: Seamlessly join devices to Azure Active Directory.
- **Active Directory Join**: Join devices to a traditional Active Directory domain.

### Device Configuration
- **Set Hostname**: Customize the device's computer name.
- **Wireless Networks**: Pre-configure Wi-Fi network profiles for automatic connection.

### Setup and Provisioning
- **Out-of-Box Experience (OOBE)**: Customize the OOBE by skipping setup pages to create a faster, more streamlined first-time user experience.
- **Provisioning Commands**: Execute custom commands and scripts during the provisioning process for advanced configurations.

## Usage
PackageStack offers flexible usage options to fit your workflow.

### PowerShell Module
The core of PackageStack is a PowerShell module that runs on PowerShell 7.4 or newer. It can be imported into your existing scripts to programmatically build provisioning packages.

### Web API
To integrate with cloud-native workflows, you can deploy PackageStack as an Azure Function or Docker Container. This approach exposes a web API, allowing you to generate provisioning packages by sending HTTP requests.

## Output Options
Both the PowerShell module and the API provide flexible options for how the final provisioning package is delivered. You can choose the format that best suits your workflow.

### Raw File
This option provides the generated `.ppkg` as a direct file.
- **When to use it:** This is the most straightforward option and is ideal for local use. If you are running a script on your machine and want to save the package directly to the filesystem, or if you are using an API client to download the file, this is the best choice.

### Base64 String
This option returns the content of the `.ppkg` file encoded as a single Base64 text string.
- **When to use it:** This is useful for automation scenarios where you need to embed the package data within a larger script, JSON payload, or another text-based format. It avoids the need to handle binary file transfers, which can simplify integration with certain systems.

### Azure Blob Storage SAS URL
This option uploads the generated `.ppkg` to an Azure Blob Storage container and returns a temporary, secure Shared Access Signature (SAS) URL.
- **When to use it:** This is the most powerful option for cloud-based or distributed workflows. Instead of transferring the entire file, you receive a lightweight URL that can be easily passed to other services, devices, or users.

**Requirements:** To use this feature, you must configure an environment variable named `AzureWebJobsStorage` set to the connection string of your Azure Storage account.

#### Docker Container
To use this option with the Docker container, you must set the `AzureWebJobsStorage` environment variable in the `docker-compose.yaml` file.
``` yaml
# ...
    environment:
      AzureWebJobsStorage: "DefaultEndpointsProtocol=https...your-connection-string"
# ...
```

#### Azure Function
If deploying the API as an Azure Function, you should not need to make any changes. The function will automatically detect the environment variable associated with the Azure Function and use it to upload the package to Azure Blob Storage.

## Deployment Options
PackageStack can be deployed in a variety of ways. The following sections provide more information about each option.

### Deploying with Docker
The project includes a file to make it easy to run the PackageStack API using Docker Compose. To start the service, download the `compose.yaml` file to a local directory and run the following command:

``` bash
docker-compose up -d
```

This will pull the latest `craysiii/packagestack` image from Docker Hub and start the container in detached mode. The API will be available on port 8080. You can change the port by updating the `ports` section of the `compose.yaml` file.

### Deploying to Azure
The project is designed for easy deployment to Microsoft Azure as a serverless Function App. Before you can publish the code, you need to create the necessary resources in your Azure subscription.

### Setting up Azure Resources
You can create the required resources using the Azure Portal:
1. **Create a Resource Group**: Start by creating a new Resource Group to hold all the related resources for this service.
2. **Create a Storage Account**: Azure Functions requires a storage account to operate. Create a general-purpose storage account within your new resource group. This account will also be used to store the provisioning packages if you use the SasUrl output option.
3. **Create a Function App**: This is the core resource that will host your code. When creating the Function App, make sure you configure the following:
    * **Publish**: Select `Code`.
    * **Runtime stack**: Select `.NET`.
    * **Version**: Choose the appropriate .NET version for the project.
    * **Operating System**: Select "Linux".
    * **Plan type**: A "Consumption (Serverless)" plan is recommended to start, as you only pay for what you use.
    * Link the Function App to the Storage Account you created earlier.

### Publishing the Function
Once the infrastructure is deployed, you will need to publish the PackageStack.AzureFunction project to the newly created Function App. This can be done in several ways:

* **From an IDE**: Use the built-in publishing features in Visual Studio or JetBrains Rider.
* **Azure CLI**: Use the az functionapp deployment source config-zip command to deploy the published project.
* **CI/CD**: Set up a CI/CD pipeline using GitHub Actions or Azure DevOps to automatically build and deploy the function whenever changes are pushed to your repository.

## Installing the PowerShell Module
The PackageStack module can be installed on any system that meets the minimum requirements, allowing you to generate provisioning packages from your preferred operating system.

### System Requirements
*   **Operating System:** Windows, macOS, or Linux
*   **PowerShell:** Version 7.4 or newer

### Installation
The module can be installed directly from the PowerShell Gallery.

1.  Open a PowerShell 7.4 (or newer) terminal.
2.  Run the following command:

```powershell
Install-Module -Name PackageStack
```

Once installed, you can import the module into your session using `Import-Module PackageStack` to begin using the cmdlets.

## OpenAPI Specification

The full OpenAPI specification for the PackageStack API is available for interactive viewing. This can be helpful for understanding the available endpoints, request bodies, and response schemas in detail.

You can view the latest specification from the `main` branch by visiting the following link:

[View OpenAPI Specification](https://rest.wiki/?https://raw.githubusercontent.com/craysiii/PackageStack/refs/heads/main/openapi.json)

## API Examples
The following examples demonstrate how to send requests to the web API exposed by the Docker container or Azure Function. The endpoint for creating a package is `/api/NewProvisioningPackage`.

### Set Computer Name
This example sets the computer name of the device.

**Request Body**
```json
{
  "return_type": "Base64",
  "package_config": {
    "id": "a2a58817-6916-4d69-92d6-2358eed33c82",
    "name": "Set-Hostname",
    "version": "1.0",
    "owner_type": "ITAdmin"
  },
  "computer_account": {
    "computer_name": "MyNewPC-%RAND:5%"
  }
}
```

### Join Azure AD
This example joins the device to Azure Active Directory using a Bulk Primary Refresh Token (BPRT).

**Request Body**
```json
{
  "return_type": "Base64",
  "package_config": {
    "id": "f8c859c3-722c-4ad8-a3f2-8e6f51c72a15",
    "name": "AzureAD-Join",
    "version": "1.0",
    "owner_type": "ITAdmin"
  },
  "azure": {
    "bprt": "your-bulk-primary-refresh-token"
  }
}
```

### Create a Local Administrator
This example creates a new local user account and adds it to the Administrators group.

**Request Body**
```json
{
  "return_type": "Base64",
  "package_config": {
    "id": "9b1d9bc4-3d92-4f3d-8e4b-9d413e1e6b2c",
    "name": "Create-Local-Admin",
    "version": "1.0",
    "owner_type": "ITAdmin"
  },
  "local_users": [
    {
      "username": "localadmin",
      "password": "AComplexPassword!",
      "group": "Administrators"
    }
  ]
}
```

### Install Zoom
This example downloads the Zoom installer and executes it silently.

**Request Body**
```json
{
    "return_type": "Base64",
    "package_config": {
        "id": "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
        "name": "Install-Zoom",
        "version": "1.0",
        "owner_type": "ITAdmin"
    },
    "provisioning_commands": [
        {
            "command_line": "msiexec.exe /i \"ZoomInstaller.msi\" /quiet /qn /norestart",
            "command_file": {
                "name": "ZoomInstaller.msi",
                "url": "https://zoom.us/client/latest/ZoomInstallerFull.msi?archType=x64"
            }
        }
    ]
}
```

## PowerShell Cmdlet Reference

This section provides a detailed reference for each cmdlet included in the `PackageStack` module.

### `New-PackageStackAzureConfiguration`
Creates a configuration object for joining a device to Azure Active Directory.

**Parameters**
*   `-BPRT <String>`: **(Required)** The Bulk Primary Refresh Token for Azure AD.
*   `-Authority <String>`: (Optional) The authority server URL. Defaults to `https://login.microsoftonline.com/common`.

**Example**
```powershell
$azureConfig = New-PackageStackAzureConfiguration -BPRT "your-bulk-primary-refresh-token"
```

### `New-PackageStackComputerConfiguration`
Creates a configuration object for setting the device hostname or joining it to an Active Directory domain.

**Parameters**
*   `-LocalComputerName <String>`: Sets the device's local hostname. Supports the use of `%RAND:x%` for a random string of `x` digits.
*   `-DomainComputerName <String>`: Sets the hostname for a domain-joined device.
*   `-DomainName <String>`: The fully qualified domain name to join (e.g., `corp.contoso.com`).
*   `-Account <String>`: The username of a domain account with permissions to join devices to the domain.
*   `-Password <String>`: The password for the domain join account.
*   `-OrganizationalUnit <String>`: (Optional) The distinguished name of the OU to place the computer object in (e.g., `OU=Workstations,DC=corp,DC=contoso,DC=com`).

**Examples**

*Set a local hostname:*

```powershell
$computerConfig = New-PackageStackComputerConfiguration -LocalComputerName "DESKTOP-%RAND:5%"
```

*Join a device to a domain:*

```powershell
$computerConfig = New-PackageStackComputerConfiguration -DomainComputerName "CORP-%RAND:5%" -DomainName "corp.contoso.com" -Account "corp\join-account" -Password "AComplexPassword!"
```

### `New-PackageStackOOBEConfiguration`
Creates a configuration object for customizing the Windows Out-of-Box Experience (OOBE).

**Parameters**
*   `-EnableCortanaVoice <Boolean>`: (Optional) Enables or disables Cortana's voice during OOBE.
*   `-HideOOBE <Boolean>`: (Optional) Skips most of the OOBE setup screens for a streamlined experience.

**Example**

```powershell
$oobeConfig = New-PackageStackOOBEConfiguration -HideOOBE $true
```

### `New-PackageStackPackageConfiguration`
Creates the main metadata object for the provisioning package. This is a required component for any package.

**Parameters**
*   `-Name <String>`: **(Required)** The display name of the package.
*   `-Version <String>`: **(Required)** The package version (e.g., "1.0").
*   `-OwnerType <OwnerType>`: **(Required)** The package creator type. Valid options are `Microsoft`, `SiliconVendor`, `OEM`, `SystemIntegrator`, `MobileOperator`, `ITAdmin`.
*   `-Rank <UInt32>`: **(Required)** The installation priority (0-99). A lower value means higher priority.
*   `-Id <Guid>`: (Optional) A unique ID for the package. If omitted, a new GUID is generated.

**Example**

```powershell
$packageConfig = New-PackageStackPackageConfiguration -Name "Base-Config" -Version "1.2" -OwnerType "ITAdmin" -Rank 10
```

### `New-PackageStackPackageFileConfiguration`
Creates a configuration object for a file that needs to be included in the package, used with `New-PackageStackProvisioningCommandConfiguration`.

**Parameters**
*   `-Name <String>`: **(Required)** The destination name of the file within the package.
*   `-Url <String>`: A URL from which to download the file during package creation.
*   `-Path <String>`: A local file path from which to include the file.
*   `-Base64 <String>`: A Base64-encoded string representing the file content.

**Examples**

*Reference a file by URL:*

```powershell
$fileConfig = New-PackageStackPackageFileConfiguration -Name "setup.exe" -Url "https://example.com/installers/setup.exe"
```

*Reference a file from a local path:*

```powershell
$fileConfig = New-PackageStackPackageFileConfiguration -Name "my-script.ps1" -Path "C:\temp\scripts\my-script.ps1"
```

### `New-PackageStackProvisioningCommandConfiguration`
Creates a configuration for a command-line instruction to be executed during provisioning.

**Parameters**
*   `-CommandLine <String>`: **(Required)** The command to execute (e.g., `msiexec.exe /i "installer.msi" /quiet`).
*   `-CommandFile <PackageFile>`: (Optional) The primary executable file, created using `New-PackageStackPackageFileConfiguration`.
*   `-Dependencies <PackageFile[]>`: (Optional) An array of supporting files.
*   `-ContinueInstall <Boolean>`: (Optional) If set to `$true`, the provisioning process will continue even if this command fails.
*   `-RestartRequired <Boolean>`: (Optional) If set to `$true`, the device will restart after this command completes.
*   `-ReturnCodeRestart <Int32>`: (Optional) A specific process exit code that should trigger a device restart.
*   `-ReturnCodeSuccess <Int32>`: (Optional) A specific process exit code that signals success.

**Example**

```powershell
$commandFile = New-PackageStackPackageFileConfiguration -Name "Zoom.msi" -Url "https://zoom.us/client/latest/ZoomInstallerFull.msi"
$provCommand = New-PackageStackProvisioningCommandConfiguration -CommandLine 'msiexec.exe /i "Zoom.msi" /quiet' -CommandFile $commandFile
```

### `New-PackageStackUserConfiguration`
Creates a configuration object for a local user account.

**Parameters**
*   `-Username <String>`: **(Required)** The name for the local user.
*   `-Password <String>`: **(Required)** The password for the local user.
*   `-Group <UserGroup>`: (Optional) The group to add the user to. Options are `Administrators` or `StandardUsers`. Defaults to `StandardUsers`.

**Example**

```powershell
$userConfig = New-PackageStackUserConfiguration -Username "localadmin" -Password "AComplexPassword!" -Group "Administrators"
```

### `New-PackageStackWLANSettingConfiguration`
Creates a configuration object for a Wi-Fi network profile.

**Parameters**
*   `-SSID <String>`: **(Required)** The SSID (name) of the Wi-Fi network.
*   `-SecurityType <SecurityType>`: **(Required)** The network security type. Options are `Open`, `WEP`, `WPA2PSK`.
*   `-SecurityKey <String>`: The network password (required for `WPA2PSK`).
*   `-AutoConnect <Boolean>`: (Optional) If set to `$true`, the device will connect automatically. Defaults to `$true`.
*   `-HiddenNetwork <Boolean>`: (Optional) Set to `$true` if the network does not broadcast its SSID.

**Example**

```powershell
$wifiConfig = New-PackageStackWLANSettingConfiguration -SSID "MyCorpWiFi" -SecurityType "WPA2PSK" -SecurityKey "SuperSecretPassword"
```

### `New-PackageStackProvisioningPackage`
This is the main cmdlet that assembles all the configuration objects and generates the final `.ppkg` provisioning package.

**Parameters**
*   `-PackageConfiguration <PackageConfig>`: **(Required)** The main package configuration object created with `New-PackageStackPackageConfiguration`.
*   `-AzureConfiguration <Azure>`: (Optional) An Azure AD join configuration.
*   `-ComputerConfiguration <ComputerAccount>`: (Optional) A computer name or AD domain join configuration.
*   `-OOBEConfiguration <OOBE>`: (Optional) An OOBE customization configuration.
*   `-UserConfigurations <User[]>`: (Optional) An array of one or more local user configurations.
*   `-ProvisioningCommands <ProvisioningCommand[]>`: (Optional) An array of one or more provisioning command configurations.
*   `-WLANSettings <WLANSetting[]>`: (Optional) An array of one or more Wi-Fi configurations.
*   `-OutputPath <String>`: Saves the generated `.ppkg` file to the specified path.
*   `-AsBase64`: Returns the package content as a Base64-encoded string.
*   `-ContainerName <String>`: The target container name in Azure Blob Storage. Used to return a SAS URL.
*   `-BlobName <String>`: The target blob name in Azure Blob Storage. Used to return a SAS URL.

**Examples**

*Create a package and save it as a file:*

```powershell
 $packageConfig = New-PackageStackPackageConfiguration -Name "My-First-Package" -Version "1.0" -OwnerType ITAdmin -Rank 0
 $user = New-PackageStackUserConfiguration -Username "testuser" -Password "Password123"
 New-PackageStackProvisioningPackage -PackageConfiguration $packageConfig -UserConfigurations $user -OutputPath "C:\temp\My-First-Package.ppkg"
```

*Create a package and get its Base64 string:*

```powershell
# (Prerequisite: $packageConfig and $user from the previous example)
$base64Package = New-PackageStackProvisioningPackage -PackageConfiguration $packageConfig -UserConfigurations $user -AsBase64
$base64Package.Encoded
```
*Create a package and upload it to Azure for a SAS URL:*

```powershell
# (Prerequisite: $packageConfig and $user from the first example)
# (Prerequisite: $env:AzureWebJobsStorage must be set to your storage connection string)
$sasUrl = New-PackageStackProvisioningPackage -PackageConfiguration $packageConfig -UserConfigurations $user -ContainerName "packages" -BlobName "My-First-Package.ppkg"
$sasUrl.Url
```

## Applying the Provisioning Package
Once you have generated a `.ppkg` file, there are several ways to apply it to a Windows device to configure its settings.

### During Initial Setup (OOBE)
For new devices that have not yet gone through the initial Windows setup, this is the most seamless method.
1.  Copy your `.ppkg` file to the root directory of a USB flash drive.
2.  When the device is on the first screen of the Out-of-Box Experience (OOBE), insert the USB drive.
3.  Windows will automatically detect the provisioning package and apply its settings. If multiple packages are present, you may be prompted to choose one.

### On a Running Windows System
For devices that are already set up and running Windows, you can apply a package directly.
1.  Transfer the `.ppkg` file to the target device.
2.  Double-click the `.ppkg` file.
3.  A security prompt will appear asking if you trust the source of the package. Click **Yes** to approve and install the package. The settings will be applied immediately.

### Using PowerShell
On a running system, you can also use PowerShell to install a package, which is useful for scripting and automation.
1.  Open a PowerShell terminal with administrator privileges.
2.  Run the `Install-ProvisioningPackage` cmdlet, providing the path to your file.

```powershell
Install-ProvisioningPackage -PackagePath "C:\path\to\your\package.ppkg" -ForceInstall
```

### With Enterprise Management Tools
In a corporate environment, you can deploy provisioning packages to multiple devices at scale using management tools.
*   **Microsoft Intune:** You can upload the `.ppkg` file to Intune and assign it to device groups. Intune will handle the deployment and ensure the settings are applied to all targeted devices.
*   **Microsoft Configuration Manager (SCCM):** You can create an application or package in Configuration Manager to distribute and install the `.ppkg` file on managed clients.

## Contributing
Contributions are welcome and greatly appreciated! Whether it's reporting a bug, suggesting a new feature, or submitting code changes, your help is valuable.

### Reporting Issues and Feature Requests
If you encounter a bug or have an idea for a new feature, please open an issue on the GitHub repository.

When filing an issue, please provide as much detail as possible:

* **For Bugs**: Include steps to reproduce the issue, the version you are using, and any relevant error messages.
* **For Feature Requests**: Clearly describe the new functionality you would like to see and explain why it would be a valuable addition.

### Submitting Pull Requests
If you would like to contribute code, please follow these steps

1. **Fork the repository** on GitHub.
2. **Create a new branch** for your feature or bug fix (e.g., `feature/add-new-setting` or `fix/hostname-bug`).
3. **Make your changes** in your new branch. Please adhere to the existing coding style.
4. **Write clear commit messages** that explain the "what" and "why" of your changes.
5. **Submit a pull request** from your branch to the main branch of the main repository.
6. In the pull request description, clearly explain the changes you have made and reference any related issues.

## License
PackageStack is licensed under the MIT License. You are free to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the software. For more details, see the `LICENSE.txt` file included in the repository.

## AI and Large Language Model Usage
This project has utilized AI and Large Language Models (LLMs) for the sole purpose of generating and refining the `README.md` documentation file. The source code for the project itself has been written by human developers.