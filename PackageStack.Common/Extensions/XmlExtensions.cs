namespace PackageStack.Common.Extensions;

public static class XmlExtensions
{
    public static XmlDocument CreateProvisioningMultivariantDocument(this XmlDocument document)
    {
        var xmlDeclaration = document.CreateXmlDeclaration("1.0", "UTF-8", null);
        document.AppendChild(xmlDeclaration);
        
        var root = document.CreateElement("Elements");
        root.SetAttribute("Type", "Multivariant");
        document.AppendChild(root);
        
        var element = document.CreateElement("Element");
        element.SetAttribute("Index", "0");
        element.IsEmpty = false;
        root.AppendChild(element);
        
        return document;
    }

    public static XmlDocument CreateProvisioningMasterDatastoreDocument(this XmlDocument document)
    {
        var xmlDeclaration = document.CreateXmlDeclaration("1.0", "utf-8", null);
        document.AppendChild(xmlDeclaration);
        
        var root = document.CreateElement("ConfigurationSourceList");
        root.SetAttribute("Version", "1.0");
        document.AppendChild(root);
        
        var configurationSource = document.CreateElement("ConfigurationSource");
        configurationSource.SetAttribute("Name", "RunTime");
        configurationSource.SetAttribute("Path", @"$(_prov)\RunTime.xml");
        root.AppendChild(configurationSource);
        
        var eventList = document.CreateElement("EventList");
        configurationSource.AppendChild(eventList);
        
        var singleEvent = document.CreateElement("Event");
        singleEvent.SetAttribute("Name", "RunTime");
        eventList.AppendChild(singleEvent);
        
        return document;
    }

    public static XmlDocument CreateProvisioningRuntimeDocument(this XmlDocument document)
    {
        var xmlDeclaration = document.CreateXmlDeclaration("1.0", "utf-8", null);
        document.AppendChild(xmlDeclaration);
        
        var root = document.CreateElement("ConfigurationSource");
        root.SetAttribute("Version", "1.0");
        document.AppendChild(root);
        
        return document;
    }

    public static XmlElement CreateBaseElement(this XmlDocument document, string name)
    {
        var element = document.CreateElement(name);
        document.AppendChild(element);
        
        return element;
    }

    public static XmlElement CreateAndAppendElement(this XmlElement element, string name)
    {
        var child = element.OwnerDocument.CreateElement(name);
        element.AppendChild(child);
        
        return child;
    }

    public static XmlElement CreateAndAppendElementWithText(this XmlElement element, string name,
        string text)
    {
        var child = element.OwnerDocument.CreateElement(name);
        child.InnerText = text;
        element.AppendChild(child);
        
        return element;
    }

    public static XmlElement AddPackageConfigurationSet(this XmlElement element, string settingsGroup,
        string data)
    {
        var configurationSet = element.OwnerDocument.CreateElement("ConfigurationSet");
        configurationSet.SetAttribute("Type", "provxml");
        configurationSet.SetAttribute("SettingsGroup", settingsGroup);
        configurationSet.SetAttribute("Data", data);
        element.AppendChild(configurationSet);
        
        return configurationSet;
    }
    
    public static XmlDocument CreateProvisioningRuntimeConfigurationDocument(this XmlDocument document)
    {
        var xmlDeclaration = document.CreateXmlDeclaration("1.0", "utf-8", "yes");
        document.AppendChild(xmlDeclaration);
        
        var root = document.CreateElement("wap-provisioningdoc");
        document.AppendChild(root);
        
        return document;
    }
    
    public static XmlElement AddPackageCharacteristic(this XmlElement element, string type)
    {
        var characteristic = element.OwnerDocument.CreateElement("characteristic");
        characteristic.SetAttribute("type", type);
        element.AppendChild(characteristic);
        
        return characteristic;
    }
    
    public static XmlElement AddPackageParameter(this XmlElement element, string name, string value, string datatype)
    {
        var parm = element.OwnerDocument.CreateElement("parm");
        parm.SetAttribute("name", name);
        parm.SetAttribute("value", value);
        parm.SetAttribute("datatype", datatype);
        element.AppendChild(parm);
        
        return element;
    }

    public static XmlElement AddPackageParameterIfNotNull(this XmlElement element, string name, string value,
        string datatype)
    {
        return string.IsNullOrWhiteSpace(value) ? element : AddPackageParameter(element, name, value, datatype);
    }

    public static XmlElement CreateProvisioningDataAssetDocument(this XmlDocument document)
    {
        var xmlDeclaration = document.CreateXmlDeclaration("1.0", "utf-8", null);
        document.AppendChild(xmlDeclaration);
        
        var root = document.CreateElement("Elements");
        root.SetAttribute("Type", "DataAsset");
        document.AppendChild(root);

        return root;
    }

    public static XmlElement AddPackageElement(this XmlElement rootElement, int index)
    {
        var element = rootElement.OwnerDocument.CreateElement("Element");
        element.SetAttribute("Index", index.ToString());
        rootElement.AppendChild(element);
        
        return element;
    }

    public static XmlElement AddPackageElementMetadata(this XmlElement element, string key, string value)
    {
        var  metadata = element.OwnerDocument.CreateElement("Metadata");
        metadata.SetAttribute("Key", key);
        metadata.SetAttribute("Value", value);
        element.AppendChild(metadata);
        
        return element;
    }

    public static XmlDocument CreateEmbeddedWiFiDocument(this XmlDocument document, WLANSetting wlanSetting)
    {
        document.PreserveWhitespace = true;
        
        var wlanProfile = document
            .CreateBaseElement("WLANProfile");
        wlanProfile.SetAttribute("xmlns", "http://www.microsoft.com/networking/WLAN/profile/v1");
        
        wlanProfile.CreateAndAppendElementWithText("name", wlanSetting.SSID);

        var ssidConfig = wlanProfile.CreateAndAppendElement("SSIDConfig");
        ssidConfig
            .CreateAndAppendElement("SSID")
            .CreateAndAppendElementWithText("name", wlanSetting.SSID);

        ssidConfig.CreateAndAppendElementWithText("nonBroadcast", wlanSetting.HiddenNetwork ? "true" : "false");

        wlanProfile
            .CreateAndAppendElementWithText("connectionType", "ESS")
            .CreateAndAppendElementWithText("connectionMode", wlanSetting.AutoConnect ? "auto" : "manual");
        
        
        var security = wlanProfile
            .CreateAndAppendElement("MSM")
            .CreateAndAppendElement("security");

        security
            .CreateAndAppendElement("authEncryption")
            .CreateAndAppendElementWithText(
                "authentication",
                wlanSetting.SecurityType == SecurityType.WPA2PSK ? "WPA2PSK" : "open")
            .CreateAndAppendElementWithText("encryption", wlanSetting.SecurityType switch
            {
                SecurityType.Open => "none",
                SecurityType.WEP => "WEP",
                SecurityType.WPA2PSK => "AES",
                _ => throw new InvalidEnumArgumentException()
            });

        if (wlanSetting.SecurityType != SecurityType.Open)
        {
            security
                .CreateAndAppendElement("sharedKey")
                .CreateAndAppendElementWithText(
                    "keyType",
                    wlanSetting.SecurityType == SecurityType.WPA2PSK ? "passPhrase" : "networkKey")
                .CreateAndAppendElementWithText("protected", "false")
                .CreateAndAppendElementWithText("keyMaterial", wlanSetting.SecurityKey!);
        }

        return document;
    }

    public static void Save(this XmlElement element, string path)
    {
        element.OwnerDocument.Save(path);
    }
}