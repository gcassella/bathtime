using Vintagestory.API.Common.Entities;

namespace BathTime;

public interface IStinkyRateModifier
{
    /// <summary>
    /// Is modifier active for entity?
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public bool StinkyRateModifierIsActive();

    /// <summary>
    /// Update rateMultiplier.
    /// </summary>
    /// <param name="rateMultplier"></param>
    /// <returns></returns>
    public double StinkyModifyRate(double rateMultplier);

    /// <summary>
    /// Priority of this rate multiplier. Higher priority multipliers are applied LAST, allowing them to potentially
    /// supersede the effects of lower priority multipliers.
    /// </summary>
    public double stinkyPriority { get; }
}


