namespace PackageStack.Common.Services;

public class WimBuilderService
{
    public WimBuilderService(string? baseDir = null)
    {
        var libBaseDir = baseDir ?? AppDomain.CurrentDomain.BaseDirectory;
        var libDir = "runtimes";
        string? libPath = null;

        var arch = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm => "arm",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException(
                $"Unsupported architecture: {RuntimeInformation.ProcessArchitecture}")
        };
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            libDir = Path.Combine(libDir, $"win-{arch}", "native");
            libPath = Path.Combine(libBaseDir, libDir, "libwim-15.dll");
        }
        
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            libDir = Path.Combine(libDir, $"linux-{arch}", "native");
            libPath = Path.Combine(libBaseDir, libDir, "libwim.so");
        }
        
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            libDir = Path.Combine(libDir, $"osx-{arch}", "native");
            libPath = Path.Combine(libBaseDir, libDir, "libwim.dylib");
        }
    
        if (libPath == null)
            throw new PlatformNotSupportedException($"Unable to find native library.");
        if (!File.Exists(libPath))
            throw new PlatformNotSupportedException($"Unable to find native library [{libPath}].");

        Wim.GlobalInit(libPath, InitFlags.None);
    }
    public static string BuildWim(ProvisioningPackageRequest request, string tempDirectory)
    {
        using var wim = Wim.CreateNewWim(CompressionType.XPRESS);
        
        var imagePath = Path.Join(tempDirectory, request.RequestId.ToString());
        var outputPath = Path.Join(tempDirectory, $"{request.RequestId}.ppkg");

        wim.AddImage(imagePath, request.PackageConfig.Name, null, AddFlags.RpFix);
        var notes = new StringBuilder();
        notes.Append("VERSION=10.0.26100.1;");
        notes.Append("Source=Classic;;");
        notes.Append("TargetSkus=WindowsCommon,Desktop;");
        notes.Append("EncryptPackage=False;");
        notes.Append("SignPackage=False;");
        notes.Append("PackageID=" + request.PackageConfig.Id + ";");
        wim.SetImageProperty(1, "NOTES", notes.ToString());
        wim.SetImageProperty(1, "VERSION", request.PackageConfig.Version);
        wim.SetImageProperty(1, "ALTITUDE", ((int)request.PackageConfig.OwnerType).ToString());
        wim.SetImageProperty(1, "RESETCLEAR", "0");
        wim.SetImageProperty(1, "PACKAGEID", "{" + request.PackageConfig.Id + "}");

        try
        {
            wim.Write(outputPath, Wim.AllImages, WriteFlags.None, Wim.DefaultThreads);
        }
        catch (Exception ex)
        {
            throw new WimBuilderException("Error while building ppkg file", ex);
        }
        
        return outputPath;
    }
    
    public static void Unload()
    {
        Wim.GlobalCleanup();
    }
}

public class WimBuilderException(string message, Exception inner) : Exception(message, inner);