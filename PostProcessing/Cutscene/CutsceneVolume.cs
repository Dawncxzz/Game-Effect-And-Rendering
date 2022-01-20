using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenu("Post-processing/Cut scene")]
public class CutsceneVolume : VolumeComponent, IPostProcessComponent
{
    public enum CutsceneMode
    {
        None,
        _FLIPOVER,
        _CLOCKWIPE,
        _DOUBLECLOCKWIPE,
        _WEDEWIPE,
        _INKFADE,
        _SLIDINGBANDS,
        _CHECKERWIPE,
        _DISSOLVE,
        _DIAMONDDISSOLVE,
        _TRIANGLEDISSOLVE,
        _DOOR,
        _SPIN,
        _CENTERMERGE,
        _CENTERSPLIT,
        _BANDSLIDE,
        _IRISROUND,
        _RANDOMBLOCKS,
        _RANDOMBWIPE,
        _GRAYSCALE,
    }

    [Tooltip("选择一个转场模式")]
    public CutsceneModeParameter mode = new CutsceneModeParameter(CutsceneMode.None);

    [Header("百叶窗")]
    public MinFloatParameter _FLIPOVER_Width = new MinFloatParameter(0, 0, true);
    public ClampedFloatParameter _FLIPOVER_Progress = new ClampedFloatParameter(0, 0, 1, true);

    [Header("时钟擦除")]
    public ClampedFloatParameter _CLOCKWIPE_Width = new ClampedFloatParameter(0, 0, 1, true);
    public ClampedFloatParameter _CLOCKWIPE_Blend = new ClampedFloatParameter(0, 0, 7, true); 

    [Header("灰度")]
    public ClampedFloatParameter _GRAYSCALE_Value = new ClampedFloatParameter(0, 0, 1, true);
    public bool IsActive() => mode.value != CutsceneMode.None;
    public bool IsTileCompatible() => true;

    [Serializable]
    public sealed class CutsceneModeParameter : VolumeParameter<CutsceneMode> 
    { 
        public CutsceneModeParameter(CutsceneMode value, bool overrideState = false) : base(value, overrideState) { }

        public override string ToString()
        {
            return value.ToString();
        }
    }

}
