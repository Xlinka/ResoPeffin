using BaseX;
using FrooxEngine;
using PicoBridge.Types;
using static Peffin.PicoDevice;

namespace Peffin;

internal class PicoMouthDevice : IInputDriver
{
    private Mouth mouth;
    public int UpdateOrder => 100;

    public void CollectDeviceInfos(DataTreeList list)
    {
        var eyeDataTreeDictionary = new DataTreeDictionary();
        eyeDataTreeDictionary.Add("Name", "Pico Face Tracking");
        eyeDataTreeDictionary.Add("Type", "Face Tracking");
        eyeDataTreeDictionary.Add("Model", "Pico");
        list.Add(eyeDataTreeDictionary);
    }

    public void RegisterInputs(InputInterface inputInterface)
    {
        mouth = new Mouth(inputInterface, "Pico Face Tracking");
    }

    public void UpdateInputs(float deltaTime)
    {
#if DEBUG
        mouth.IsTracking = true;
#else
        mouth.IsTracking = Engine.Current.InputInterface.VR_Active;
#endif

        mouth.Jaw = new float3
        (
            PicoExpressionData[PicoBlendShapeWeight.JawLeft] - PicoExpressionData[PicoBlendShapeWeight.JawRight],
            -Ape(), // This feels dirty but it should work
            PicoExpressionData[PicoBlendShapeWeight.JawForward]
        );
        mouth.JawOpen = PicoExpressionData[PicoBlendShapeWeight.JawOpen];
        
        mouth.Tongue = new float3(0f, PicoExpressionData[PicoBlendShapeWeight.TongueOut], 0f);
        mouth.TongueRoll = 0.0f;
        
        mouth.LipUpperLeftRaise = PicoExpressionData[PicoBlendShapeWeight.MouthUpperUpLeft];
        mouth.LipUpperRightRaise = PicoExpressionData[PicoBlendShapeWeight.MouthUpperUpRight];
        mouth.LipLowerLeftRaise = PicoExpressionData[PicoBlendShapeWeight.MouthLowerDownLeft];
        mouth.LipLowerRightRaise = PicoExpressionData[PicoBlendShapeWeight.MouthLowerDownRight];

        mouth.LipUpperHorizontal = PicoExpressionData[PicoBlendShapeWeight.MouthRight] - PicoExpressionData[PicoBlendShapeWeight.MouthLeft];
        mouth.LipLowerHorizontal = PicoExpressionData[PicoBlendShapeWeight.MouthRight] - PicoExpressionData[PicoBlendShapeWeight.MouthLeft];

        mouth.MouthLeftSmileFrown = PicoExpressionData[PicoBlendShapeWeight.MouthSmileLeft] - PicoExpressionData[PicoBlendShapeWeight.MouthFrownLeft];
        mouth.MouthRightSmileFrown = PicoExpressionData[PicoBlendShapeWeight.MouthSmileRight] - PicoExpressionData[PicoBlendShapeWeight.MouthFrownRight];
        
        mouth.MouthPout = (PicoExpressionData[PicoBlendShapeWeight.MouthFunnel] + PicoExpressionData[PicoBlendShapeWeight.MouthPucker]) / 2;

        mouth.LipTopOverturn = PicoExpressionData[PicoBlendShapeWeight.MouthShrugUpper];
        mouth.LipBottomOverturn = PicoExpressionData[PicoBlendShapeWeight.MouthShrugLower];

        // Do these need to be negative?
        mouth.LipTopOverUnder = PicoExpressionData[PicoBlendShapeWeight.MouthRollUpper];
        mouth.LipBottomOverUnder = PicoExpressionData[PicoBlendShapeWeight.MouthRollLower];

        mouth.CheekLeftPuffSuck = PicoExpressionData[PicoBlendShapeWeight.CheekPuff];
        mouth.CheekRightPuffSuck = PicoExpressionData[PicoBlendShapeWeight.CheekPuff];
    }

    private float Ape()
    {
        return (0.05f + PicoExpressionData[PicoBlendShapeWeight.JawOpen]) * 
               (0.05f + PicoExpressionData[PicoBlendShapeWeight.MouthClose]) * 
               (0.05f + PicoExpressionData[PicoBlendShapeWeight.MouthClose]);
    }
}
