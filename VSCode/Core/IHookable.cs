using FortRise;

namespace TFModFortRiseWinCounters;

public interface IHookable
{
    abstract static void Load(IHarmony harmony);
}