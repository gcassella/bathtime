using System;
using System.ComponentModel;
using System.Linq;
using Vintagestory.API.Common;

namespace BathTime.Config;


public interface IHasConfigName
{
    public static string configName { get; } = "";
}


public class BathtimeBaseConfig<TSelfReferenceType> where TSelfReferenceType : IHasConfigName, new()
{
    public new static Type GetType()
    {
        return typeof(TSelfReferenceType);
    }

    public static string? GetConfigName(TSelfReferenceType config)
    {
        var configNameField = GetType().GetProperty("configName");
        if (configNameField is null)
        {
            return null;
        }
        var configNameValue = configNameField.GetValue(config);
        if (configNameValue is null)
        {
            return null;
        }
        return (string)configNameValue;
    }

    public static string[] ValueNames
    {
        get
        {
            return [.. GetType().GetProperties().Select(p => p.Name)];
        }
    }

    public static bool UpdateStoredConfig(ICoreAPI api, string valueName, string value)
    {
        TSelfReferenceType? config = new();
        string? configName = GetConfigName(config);

        if (configName is null)
        {
            api.Logger.Error("Tried updating config class with no name! Mod is borked.");
            return false;
        }

        try
        {
            config = api.LoadModConfig<TSelfReferenceType?>(configName);
        }
        catch
        {
            api.Logger.Error("Could not load " + configName + ", is there a typo?");
            return false;
        }


        if (config is null)
        {
            api.Logger.Warning(Constants.LOGGING_PREFIX + "Could not find " + configName + ". Writing default.");
            config = new TSelfReferenceType();
        }

        var valueProperty = GetType().GetProperty(valueName);
        Type? valueType = valueProperty?.GetValue(config)?.GetType();

        if (valueProperty is null || valueType is null)
        {
            api.Logger.Warning(Constants.LOGGING_PREFIX + "Could not find " + valueName + " in the config.");
            return false;
        }

        var typeConverter = TypeDescriptor.GetConverter(valueType);
        if (!typeConverter.IsValid(value))
        {
            api.Logger.Warning(Constants.LOGGING_PREFIX + "Value " + value + " could not be converted to type of " + valueName + ": " + valueType);
            return false;
        }

        valueProperty.SetValue(config, typeConverter.ConvertFromString(value));
        api.StoreModConfig(config, configName);
        api.Event.PushEvent(Constants.RELOAD_COMMAND);

        return true;
    }
}
