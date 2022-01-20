using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenu("Post-processing/My blur")]
public class MyBlurVolume : VolumeComponent, IPostProcessComponent
{
    public enum BlurMode
    {
        None,
        _GAUSSIANBLUR,
        _BOXBLUR,
        _KAWASEBLUR,
        _DUALBLUR,
        _BOKEHBLUR,
        _TILTSHIFTBLUR,
        _IRISBLUR,
        _GRAINYBLUR,
        _RADIALBLUR,
        _DIRECTIONALBLUR
    }

    [Tooltip("选择一个模糊模式")]
    public BlurModeParameter mode = new BlurModeParameter(BlurMode.None);

    [Header("高斯模糊")]
    public MinIntParameter _GAUSSIANBLUR_Iterations = new MinIntParameter(1, 1, true);
    public MinFloatParameter _GAUSSIANBLUR_Downscale = new MinFloatParameter(1, 1, true);
    public MinFloatParameter _GAUSSIANBLUR_Offsets = new MinFloatParameter(0, 0, true);

    [Header("方框模糊")]
    public MinIntParameter _BOXBLUR_Iterations = new MinIntParameter(1, 1, true);
    public MinFloatParameter _BOXBLUR_Downscale = new MinFloatParameter(1, 1, true);
    public MinFloatParameter _BOXBLUR_BlurOffset = new MinFloatParameter(0, 0, true); 

    [Header("散景模糊")]
    public MinIntParameter _BOKEHBLUR_Iteration = new MinIntParameter(2, 2, true);
    public MinFloatParameter _BOKEHBLUR_Downscale = new MinFloatParameter(1, 1, true);
    public ClampedFloatParameter _BOKEHBLUR_Radius = new ClampedFloatParameter(0, 0.01f, 5, true);
    public bool IsActive() => mode.value != BlurMode.None;
    public bool IsTileCompatible() => true;

    [Serializable]
    public sealed class BlurModeParameter : VolumeParameter<BlurMode>
    {
        public BlurModeParameter(BlurMode value, bool overrideState = false) : base(value, overrideState) { }

        public override string ToString()
        {
            return value.ToString();
        }
    }

}
