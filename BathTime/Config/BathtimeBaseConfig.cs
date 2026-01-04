using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using HarmonyLib;
using Vintagestory.API.Common;

namespace BathTime;


public interface IConfig
{
    public static string configName { get; } = "";

    public static EnumAppSide Side { get; }
}

public static class BathtimeBaseConfig<TSelfReferenceType> where TSelfReferenceType : IConfig, new()
{
    // Thread safe static boolean, see https://stackoverflow.com/a/49233660.
    private static int _cacheIsDirtyBackValue = 1;
    private static bool cacheIsDirty
    {
        get
        {
            return Interlocked.CompareExchange(ref _cacheIsDirtyBackValue, 1, 1) == 1;
        }
        set
        {
            if (value) Interlocked.CompareExchange(ref _cacheIsDirtyBackValue, 1, 0);
            else Interlocked.CompareExchange(ref _cacheIsDirtyBackValue, 0, 1);
        }
    }

    private static TSelfReferenceType cached = new();


    private new static Type GetType()
    {
        return typeof(TSelfReferenceType);
    }

    private static string GetConfigName(TSelfReferenceType config)
    {
        var configNameValue = (GetType().GetProperty("configName")?.GetValue(config)) ?? throw new UnreachableException("Tried loading config class with no name! Mod is borked.");
        return (string)configNameValue;
    }

    private static EnumAppSide GetConfigSide(TSelfReferenceType config)
    {
        var configNameValue = (GetType().GetProperty("Side")?.GetValue(config)) ?? throw new UnreachableException("Tried loading config class with no side! Mod is borked.");
        return (EnumAppSide)configNameValue;
    }

    public static string[] ValueNames
    {
        get
        {
            return [.. GetType().GetProperties().Select(p => p.Name).Except(typeof(IConfig).GetPropertyNames())];
        }
    }

    public static void GloballyReloadStoredConfig(ICoreAPI api)
    {
        cacheIsDirty = true;
        api.Event.PushEvent(Constants.RELOAD_COMMAND);
    }

    private static TSelfReferenceType LoadInner(ICoreAPI api, string configName)
    {
        TSelfReferenceType? maybe_config;
        if (cacheIsDirty)
        {
            maybe_config = api.LoadModConfig<TSelfReferenceType?>(configName);
            cached = maybe_config ?? throw new FileNotFoundException("Could not find " + configName + ".");
            // Store after load to propagate any new defaults to the file.
            api.StoreModConfig(maybe_config, configName);
            cacheIsDirty = false;
        }
        else
        {
            maybe_config = cached;
        }
        return maybe_config;
    }

    public static TSelfReferenceType LoadStoredConfig(ICoreAPI api)
    {
        var configSide = GetConfigSide(cached);
        if (api.Side != configSide) throw new UnreachableException("Tried to read config with side: " + nameof(configSide) + " on side: " + nameof(api.Side));
        string configName = GetConfigName(cached);
        try
        {
            return LoadInner(api, configName);
        }
        catch (Exception exc)
        {
            TSelfReferenceType config = new();
            cached = config;
            cacheIsDirty = false;

            // Always return a valid default config on a loading exception, but only write default to disk if the
            // exception is FileNotFoundException.
            if (exc is FileNotFoundException)
            {
                api.Logger.Warning(Constants.LOGGING_PREFIX + "Writing default config.");
                api.StoreModConfig(config, configName);
            }
            else
            {
                api.Logger.Error(Constants.LOGGING_PREFIX + exc);
            }

            return config;
        }
    }

    public static bool UpdateStoredConfig(ICoreAPI api, string valueName, string value)
    {

        var configSide = GetConfigSide(cached);
        if (api.Side != configSide) throw new UnreachableException("Tried to read config with side: " + nameof(configSide) + " on side: " + nameof(api.Side));
        string configName = GetConfigName(cached);
        try
        {
            // Use LoadInner over LoadStoredConfig here to avoid loading and storing a modification of the default
            // config on a loading error, which would clobber user changes.
            TSelfReferenceType config = LoadInner(api, configName);

            var valueProperty = GetType().GetProperty(valueName);
            Type valueType = valueProperty?.GetValue(config)?.GetType() ?? throw new ArgumentException("Could not find " + valueName + " in the config.");

            var typeConverter = TypeDescriptor.GetConverter(valueType);
            if (!typeConverter.IsValid(value))
            {
                throw new InvalidCastException("Value " + value + " could not be converted to type of " + valueName + ": " + valueType);
            }

            valueProperty.SetValue(config, typeConverter.ConvertFromString(value));
            api.StoreModConfig(config, configName);
            GloballyReloadStoredConfig(api);
        }
        catch (Exception exc)
        {
            api.Logger.Error(Constants.LOGGING_PREFIX + exc);
            return false;
        }

        return true;
    }
}
