using BaseX;
using FrooxEngine;
using PicoBridge.Types;
using System;
using static Peffin.PicoDevice;

namespace Peffin;

internal class PicoEyeDevice : IInputDriver
{
    private Eyes eyes;
    private const float EyeCloseThreshold = 0.25f;
    private const float EyeWidenThreshold = 0.3f;
    private const float EyeHyperWidenThreshold = 0.5f;
    public int UpdateOrder => 100;

    public void CollectDeviceInfos(DataTreeList list)
    {
        var eyeDataTreeDictionary = new DataTreeDictionary();
        eyeDataTreeDictionary.Add("Name", "Pico Eye Tracking");
        eyeDataTreeDictionary.Add("Type", "Eye Tracking");
        eyeDataTreeDictionary.Add("Model", "Pico");
        list.Add(eyeDataTreeDictionary);
    }

    public void RegisterInputs(InputInterface inputInterface)
    {
        eyes = new Eyes(inputInterface, "Pico Pro Eye Tracking");
    }

    public void UpdateInputs(float deltaTime)
    {
#if DEBUG
        eyes.IsDeviceActive = true;
        eyes.IsTracking = true;
#else
        eyes.IsDeviceActive = Engine.Current.InputInterface.VR_Active;
        eyes.IsTracking = Engine.Current.InputInterface.VR_Active;
#endif

        var leftYaw = MathX.Clamp(PicoExpressionData[PicoBlendShapeWeight.EyeLookInLeft] -
                        PicoExpressionData[PicoBlendShapeWeight.EyeLookOutLeft], -0.8f, 0.8f);
        var leftPitch = PicoExpressionData[PicoBlendShapeWeight.EyeLookUpLeft] - PicoExpressionData[PicoBlendShapeWeight.EyeLookDownLeft];
        var rightYaw =
            MathX.Clamp(PicoExpressionData[PicoBlendShapeWeight.EyeLookOutRight] - PicoExpressionData[PicoBlendShapeWeight.EyeLookInRight], -0.8f,
                0.8f);
        var rightPitch = PicoExpressionData[PicoBlendShapeWeight.EyeLookUpRight] -
                         PicoExpressionData[PicoBlendShapeWeight.EyeLookDownRight];
        var averageYaw = MathX.Clamp((leftYaw + rightYaw) / 2, -0.8f, 0.8f);
        var averagePitch = MathX.Clamp((leftPitch + rightPitch) / 2, -0.8f, 0.8f);

        var leftOpenness = EyeOpenness(PicoExpressionData[PicoBlendShapeWeight.EyeBlinkLeft],
            PicoExpressionData[PicoBlendShapeWeight.EyeSquintLeft]);
        var rightOpenness = EyeOpenness(PicoExpressionData[PicoBlendShapeWeight.EyeBlinkRight],
            PicoExpressionData[PicoBlendShapeWeight.EyeSquintRight]);
        var averageOpenness = MathX.Clamp((leftOpenness + rightOpenness) / 2, 0, 1.0f);
        var leftWiden = PicoExpressionData[PicoBlendShapeWeight.EyeWideLeft];
        var rightWiden = PicoExpressionData[PicoBlendShapeWeight.EyeWideRight];
        var averageWiden = MathX.Clamp((leftWiden + rightWiden) / 2, 0, 1.0f);

        var leftSquint = PicoExpressionData[PicoBlendShapeWeight.EyeSquintLeft];
        var rightSquint = PicoExpressionData[PicoBlendShapeWeight.EyeSquintRight];
        var averageSquint = MathX.Clamp((leftSquint + rightSquint) / 2, 0, 1.0f);

        var dilation = averageWiden switch
        {
            >= EyeHyperWidenThreshold => 0.2f,
            >= EyeWidenThreshold => 0.4f,
            _ => 0.6f
        };

        UpdateEye(
            eyes.LeftEye,
            leftYaw,
            leftPitch,
            dilation,
            leftOpenness,
            leftWiden,
            leftSquint
        );

        UpdateEye(
            eyes.RightEye,
            rightYaw,
            rightPitch,
            dilation,
            rightOpenness,
            rightWiden,
            rightSquint
        );

        UpdateEye(
            eyes.CombinedEye,
            averageYaw,
            averagePitch,
            dilation,
            averageOpenness,
            averageWiden,
            averageSquint
        );
    }

    private void UpdateEye(Eye eye, float gazeX, float gazeY, float pupilSize, float openness, float widen, float squeeze)
    {
#if DEBUG
        eyes.IsDeviceActive = true;
        eyes.IsTracking = true;
#else
        eyes.IsDeviceActive = Engine.Current.InputInterface.VR_Active;
        eyes.IsTracking = Engine.Current.InputInterface.VR_Active;
#endif

        if (eye.IsTracking)
        {
            eye.UpdateWithDirection(ToFloat3(gazeX, gazeY));
            eye.RawPosition = float3.Zero;
            eye.PupilDiameter = pupilSize;
        }

        if (openness <= EyeCloseThreshold)
        {
            eye.Openness = 0.0f;
            eye.Squeeze = (openness / -EyeCloseThreshold) + 1;
            eye.Widen = 0;
        }
        else
        {
            if (widen >= EyeWidenThreshold)
            {
                eye.Openness = 1.0f;
                eye.Squeeze = 0;
                eye.Widen = (openness - EyeWidenThreshold) / (1 - EyeWidenThreshold);
            }
            else
            {
                eye.Openness = openness;
                eye.Squeeze = 0;
                eye.Widen = 0;
            }
        }

        eye.Frown = 0f;
    }

    private float3 ToFloat3(float x, float y)
    {
        return new float3(MathX.Tan(x), MathX.Tan(y), 1f).Normalized;
    }

    private float EyeOpenness(float blink, float squint) 
    {
        return 1.0f - Math.Max(0, Math.Min(1, blink)) + (float)(blink * (2f * squint) /
            Math.Pow(2f, 2f * squint));
    }
}
