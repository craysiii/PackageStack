// ReSharper disable InconsistentNaming
namespace PackageStack.Common.Enums;

public static class SettingsGroup
{
    public const string Azure           = "641c3728-5623-544a-9144-976a2dbf3ca9";
    public const string ComputerAccount = "f35dd173-1c3e-5f2b-a3a4-29b602842b09";
    public const string User            = "316e2a31-250a-5f09-a0e1-d2e5c6b03559";
    public const string WlanSetting     = "ac17da61-38b8-5bb9-bdaf-7436ac4eb133";

    public static class OOBE
    {
        public const string HideOOBE            = "b786dbf1-ec31-5be5-aa49-934e4489fae9";
        public const string EnableCortanaVoice  = "2f06d211-95c0-5efd-a257-d6085f324283";
    }

    public static class ProvisioningCommand
    {
        public const string CommandFile         = "1348d3d6-134b-5eb8-8f89-96c44bf9aa0b";
        public const string CommandLine         = "56ccb8ff-b5f2-5806-a504-ab9091876608";
        public const string ContinueInstall     = "290f8b72-c400-5dc4-8e56-b98df699071a";
        public const string Dependency          = "0fb42616-94a6-5da8-b711-fd15131232e9";
        public const string RestartRequired     = "0a0302fe-0203-5635-8005-2878eac632cd";
        public const string ReturnCodeRestart   = "0c1ffbe1-4777-5325-afb2-61bfeee42220";
        public const string ReturnCodeSuccess   = "370d2f73-b454-5319-b1b1-ad5dcb6c1e42";
    }
}