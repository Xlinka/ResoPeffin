using BaseX;
using PicoBridge;
using PicoBridge.Types;
using System;
using System.Threading;

namespace Peffin;

internal class PicoDevice
{
    public static PicoFaceTrackingDatagram PicoExpressionData { get; private set; }

    private readonly PicoBridgeServer server = new();
    private PicoFaceTrackingDatagram datagram = new();
    private bool isDeviceConnected;
    private DateTime lastLogTime = DateTime.Now;
    private long lastUpdateTimestamp;
    private Thread thread;
    private CancellationTokenSource token = new();

    public bool Initialize()
    {
        UniLog.Log("Starting PICO server");

        try
        {
            server.DatagramChange += OnDatagram;
            server.ConnectivityChange += (_, state) =>
            {
                isDeviceConnected = state;
                UniLog.Log(state ? "PICO connected" : "PICO disconnected");
            };
            server.Start();
            UniLog.Log("PICO server started, waiting for connection");
        }
        catch (Exception)
        {
            UniLog.Error("Port already in use. Run PICOBridgeHelper.exe first!");
            return false;
        }

        return true;
    }

    public void StartDataPolling()
    {
        thread = new Thread(new ThreadStart(UpdateWrapper));
        thread.Start();
    }

    private void UpdateWrapper()
    {
        token = new CancellationTokenSource();
        var now = DateTime.Now;
        while (!token.IsCancellationRequested)
        {
            Update();
            Thread.Sleep(10);
            if (!isDeviceConnected || now - lastLogTime < TimeSpan.FromSeconds(5)) continue;

            lastLogTime = DateTime.Now;
        }
    }

    private void Update()
    {
        if (!isDeviceConnected)
        {
            return;
        }

        if (datagram.Timestamp == lastUpdateTimestamp)
        {
            return;
        }

        PicoExpressionData = datagram;
        lastUpdateTimestamp = datagram.Timestamp;
    }

    public void Teardown()
    {
        thread.Abort(); // thread.Join() ?
        server.Stop();
        server.Join();
    }

    private void OnDatagram(object sender, PicoFaceTrackingDatagram data)
    {
        datagram = data;
    }
}
