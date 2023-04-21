using FrooxEngine;
using NeosModLoader;

namespace Peffin;

public class Peffin : NeosMod
{
    public override string Name => "Peffin";
    public override string Author => "dfgHiatus";
    public override string Version => "1.0.0";
    public override string Link => "https://github.com/dfgHiatus/Peffin";

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