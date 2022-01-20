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

    [Tooltip("Ñ¡ÔñÒ»¸ö×ª³¡Ä£Ê½")]
    public GlitchModeParameter mode = new GlitchModeParameter(GlitchMode.None);

    [Header("RGBÑÕÉ«·ÖÀë¹ÊÕÏ")]
    public TextureParameter _RGBSPLITGLITCH_NoiseTex = new TextureParameter(null, true);
    public MinFloatParameter _RGBSPLITGLITCH_Speed = new MinFloatParameter(0, 0, true);
    public MinFloatParameter _RGBSPLITGLITCH_Amplitude = new MinFloatParameter(0, 0, true);

    [Header("´íÎ»Í¼¿é¹ÊÕÏ")]
    public MinFloatParameter _IMAGEBLOCKGLITCH_BlockSize = new MinFloatParameter(0, 0, true);
    public MinFloatParameter _IMAGEBLOCKGLITCH_Speed = new MinFloatParameter(0, 0, true);
    public Vector2Parameter _IMAGEBLOCKGLITCH_MaxRGBSplit = new Vector2Parameter(new Vector2(0, 0), true);

    [Header("´íÎ»ÏßÌõ¹ÊÕÏ")]
    public MinFloatParameter _LINEBLOCKGLITCH_Frequency = new MinFloatParameter(0, 0, true);
    public ClampedFloatParameter _LINEBLOCKGLITCH_Amount = new ClampedFloatParameter(0, 0, 1, true);
    public MinFloatParameter _LINEBLOCKGLITCH_Offset = new MinFloatParameter(0, 0, true);
    public MinFloatParameter _LINEBLOCKGLITCH_LinesWidth = new MinFloatParameter(0, 0, true);
    public ClampedFloatParameter _LINEBLOCKGLITCH_Alpha = new ClampedFloatParameter(0, 0, 1, true);

    [Header("Í¼¿é¶¶¶¯¹ÊÕÏ")]
    public MinFloatParameter _TILEJITTERGLITCH_SplittingNumber = new MinFloatParameter(0, 0, true);
    public MinFloatParameter _TILEJITTERGLITCH_JitterAmount = new MinFloatParameter(0, 0, true);
    public MinFloatParameter _TILEJITTERGLITCH_JitterSpeed = new MinFloatParameter(0, 0, true);
    public MinFloatParameter _TILEJITTERGLITCH_Frequency = new MinFloatParameter(0, 0, true);

    [Header("´íÎ»ÏßÌõ¹ÊÕÏ")]
    public MinFloatParameter _SCANLINEJITTERGLITCH_Amount = new MinFloatParameter(0, 0, true);
    public MinFloatParameter _SCANLINEJITTERGLITCH_Threshold = new MinFloatParameter(0, 0, true);
    public MinFloatParameter _SCANLINEJITTERGLITCH_Frequency = new MinFloatParameter(0, 0, true);

    [Header("Êý×ÖÌõÎÆ¹ÊÕÏ")]
    public MinFloatParameter _DIGITALSTRIPEGLITCH_Indensity = new MinFloatParameter(0, 0, true);
    public Vector2Parameter _DIGITALSTRIPEGLITCH_TexSize = new Vector2Parameter(new Vector2(1, 1), true);
    public MinIntParameter _DIGITALSTRIPEGLITCH_Frequncy = new MinIntParameter(1, 1, true);
    public ClampedFloatParameter _DIGITALSTRIPEGLITCH_StripeLength = new ClampedFloatParameter(0.01f, 0, 1, true);
    public ColorParameter _DIGITALSTRIPEGLITCH_StripColorAdjustColor = new ColorParameter(Color.white, true);
    public MinFloatParameter _DIGITALSTRIPEGLITCH_StripColorAdjustIndensity = new MinFloatParameter(0, 0, true);

    [Header("ÆÁÄ»ÌøÔ¾¹ÊÕÏ")]
    public MinFloatParameter _SCREENJUMPGLITCH_JumpIndensity = new MinFloatParameter(0, 0, true);

    [Header("ÆÁÄ»¶¶¶¯¹ÊÕÏ")]
    public MinFloatParameter _SCREENSHAKEGLITCH_ScreenShake = new MinFloatParameter(0, 0, true);
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
