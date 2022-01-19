using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenu("Post-processing/Glitch")]
public class GlitchVolume : VolumeComponent, IPostProcessComponent
{
    public enum GlitchMode
    {
        None,
        _RGBSPLITGLITCH,
        _IMAGEBLOCKGLITCH,
        _LINEBLOCKGLITCH,
        _TILEJITTERGLITCH,
        _SCANLINEJITTERGLITCH,
        _DIGITALSTRIPEGLITCH,
        _ANALOGNOISEGLITCH,
        _SCREENJUMPGLITCH,
        _SCREENSHAKEGLITCH,
        _WAVEJITTERGLITCH
    }

    [Tooltip("选择一个转场模式")]
    public GlitchModeParameter mode = new GlitchModeParameter(GlitchMode.None);

    [Header("RGB颜色分离故障")]
    public TextureParameter _RGBSPLITGLITCH_NoiseTex = new TextureParameter(null, true);
    public MinFloatParameter _RGBSPLITGLITCH_Speed = new MinFloatParameter(0, 0, true);
    public MinFloatParameter _RGBSPLITGLITCH_Amplitude = new MinFloatParameter(0, 0, true);

    [Header("错位图块故障")]
    public MinFloatParameter _IMAGEBLOCKGLITCH_BlockSize = new MinFloatParameter(0, 0, true);
    public MinFloatParameter _IMAGEBLOCKGLITCH_Speed = new MinFloatParameter(0, 0, true);
    public Vector2Parameter _IMAGEBLOCKGLITCH_MaxRGBSplit = new Vector2Parameter(new Vector2(0, 0), true);
    public bool IsActive() => mode.value != GlitchMode.None;
    public bool IsTileCompatible() => true;

    [Serializable]
    public sealed class GlitchModeParameter : VolumeParameter<GlitchMode>
    {
        public GlitchModeParameter(GlitchMode value, bool overrideState = false) : base(value, overrideState) { }

        public override string ToString()
        {
            return value.ToString();
        }
    }

}
