using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenu("Post-processing/ScreenSpaceOutlines")]
public class ScreenSpaceOutlinesVolume : VolumeComponent, IPostProcessComponent
{

    public MinFloatParameter _OutlineScale = new MinFloatParameter(1, 0, true);
    public MinFloatParameter _RobertsCrossMultiplier = new MinFloatParameter(1, 0, true);
    public MinFloatParameter _DepthThreshold = new MinFloatParameter(1, 0, true);
    public MinFloatParameter _NormalThreshold = new MinFloatParameter(1, 0, true);
    public MinFloatParameter _SteepAngleThreshold = new MinFloatParameter(1, 0, true);
    public MinFloatParameter _SteepAngleMultiplier = new MinFloatParameter(1, 0, true);
    public ColorParameter _OutlineColor = new ColorParameter(Color.black);
    public bool IsActive() => true;
    public bool IsTileCompatible() => true;

}
