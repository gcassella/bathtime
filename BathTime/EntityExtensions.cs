using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;

namespace BathTime;

public static class EntityExtensions
{
    public static ITreeAttribute GetTreeAttribute(this Entity entity)
    {
        ITreeAttribute treeAttribute = entity.WatchedAttributes.GetTreeAttribute(Constants.MOD_ID);
        if (treeAttribute is null)
        {
            entity.WatchedAttributes.SetAttribute(Constants.MOD_ID, treeAttribute = new TreeAttribute());
        }
        return treeAttribute;
    }

    public static double GetDoubleAttribute(this Entity entity, string key, double defaultValue = 0.0)
    {
        ITreeAttribute treeAttribute = GetTreeAttribute(entity);
        return treeAttribute.GetDouble(key, defaultValue);
    }

    public static void SetDoubleAttribute(this Entity entity, string key, double value)
    {
        ITreeAttribute treeAttribute = GetTreeAttribute(entity);
        treeAttribute.SetDouble(key, value);
        entity.WatchedAttributes.MarkPathDirty(Constants.MOD_ID);
    }

    public static bool GetBoolAttribute(this Entity entity, string key, bool defaultValue = false)
    {
        ITreeAttribute treeAttribute = GetTreeAttribute(entity);
        return treeAttribute.GetBool(key, defaultValue);
    }

    public static void SetBoolAttribute(this Entity entity, string key, bool value)
    {
        ITreeAttribute treeAttribute = GetTreeAttribute(entity);
        treeAttribute.SetBool(key, value);
        entity.WatchedAttributes.MarkPathDirty(Constants.MOD_ID);
    }
}