using System;
using System.ComponentModel;
using System.IO;
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

    public static TSelfReferenceType LoadStoredConfig(ICoreAPI api)
    {
        // Don't catch this, points to a fundamental code error in the mode.
        string configName = GetConfigName(new()) ?? throw new MissingMemberException("Tried loading config class with no name! Mod is borked.");
        try
        {
            var maybe_config = api.LoadModConfig<TSelfReferenceType?>(configName);
            return maybe_config ?? throw new FileNotFoundException("Could not find " + configName + ".");
        }
        catch (Exception exc)
        {
            api.Logger.Error(Constants.LOGGING_PREFIX + exc);
            TSelfReferenceType config = new();

            // Always return a valid default config on a loading exception, but only write default to disk if the
            // exception is FileNotFoundException.
            if (exc is FileNotFoundException)
            {
                api.Logger.Warning("Writing default config.");
                api.StoreModConfig(config, configName);
            }

            return config;
        }
    }

    public static bool UpdateStoredConfig(ICoreAPI api, string valueName, string value)
    {
        // Don't catch this, points to a fundamental code error in the mode.
        string configName = GetConfigName(new()) ?? throw new MissingMemberException("Tried updating config class with no name! Mod is borked.");
        try
        {
            TSelfReferenceType config = LoadStoredConfig(api);

            var valueProperty = GetType().GetProperty(valueName);
            Type valueType = valueProperty?.GetValue(config)?.GetType() ?? throw new ArgumentException("Could not find " + valueName + " in the config.");

            var typeConverter = TypeDescriptor.GetConverter(valueType);
            if (!typeConverter.IsValid(value))
            {
                throw new InvalidCastException("Value " + value + " could not be converted to type of " + valueName + ": " + valueType);
            }

            valueProperty.SetValue(config, typeConverter.ConvertFromString(value));
            api.StoreModConfig(config, configName);
            api.Event.PushEvent(Constants.RELOAD_COMMAND);
        }
        catch (Exception exc)
        {
            api.Logger.Error(Constants.LOGGING_PREFIX + exc);
            return false;
        }

        return true;
    }
}
