using FrooxEngine;
using ResoniteModLoader;

namespace Peffin;

public class Peffin : ResoniteMod
{
    public override string Name => "Peffin";
    public override string Author => "xLinka";
    public override string Version => "1.0.0";
    public override string Link => "https://github.com/xLinka/ResoPeffin";

    private PicoDevice picoDevice;

    public override void OnEngineInit()
    {
        if (!picoDevice.Initialize())
        {
            return;
        }

        picoDevice.StartDataPolling();
        Engine.Current.OnShutdown += () => picoDevice.Teardown();
    }
}