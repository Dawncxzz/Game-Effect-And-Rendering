using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif
using UnityEngine;
#if UNITY_EDITOR
[VolumeComponentEditor(typeof(GlitchVolume))]
public class GlitchVolumeEditor : VolumeComponentEditor
{
    SerializedDataParameter m_Mode;
    PropertyFetcher<GlitchVolume> o;

    SerializedDataParameter _RGBSPLITGLITCH_NoiseTex;
    SerializedDataParameter _RGBSPLITGLITCH_Speed;
    SerializedDataParameter _RGBSPLITGLITCH_Amplitude;

    SerializedDataParameter _IMAGEBLOCKGLITCH_BlockSize;
    SerializedDataParameter _IMAGEBLOCKGLITCH_Speed;
    SerializedDataParameter _IMAGEBLOCKGLITCH_MaxRGBSplit;

    SerializedDataParameter _LINEBLOCKGLITCH_Frequency;
    SerializedDataParameter _LINEBLOCKGLITCH_Amount;
    SerializedDataParameter _LINEBLOCKGLITCH_Offset;
    SerializedDataParameter _LINEBLOCKGLITCH_LinesWidth;
    SerializedDataParameter _LINEBLOCKGLITCH_Alpha;

    SerializedDataParameter _TILEJITTERGLITCH_SplittingNumber;
    SerializedDataParameter _TILEJITTERGLITCH_JitterAmount;
    SerializedDataParameter _TILEJITTERGLITCH_JitterSpeed;
    SerializedDataParameter _TILEJITTERGLITCH_Frequency;

    SerializedDataParameter _SCANLINEJITTERGLITCH_Amount;
    SerializedDataParameter _SCANLINEJITTERGLITCH_Threshold;
    SerializedDataParameter _SCANLINEJITTERGLITCH_Frequency;

    SerializedDataParameter _DIGITALSTRIPEGLITCH_Indensity;
    SerializedDataParameter _DIGITALSTRIPEGLITCH_TexSize;
    SerializedDataParameter _DIGITALSTRIPEGLITCH_Frequncy;
    SerializedDataParameter _DIGITALSTRIPEGLITCH_StripeLength;
    SerializedDataParameter _DIGITALSTRIPEGLITCH_StripColorAdjustColor;
    SerializedDataParameter _DIGITALSTRIPEGLITCH_StripColorAdjustIndensity;

    SerializedDataParameter _SCREENJUMPGLITCH_JumpIndensity;

    SerializedDataParameter _SCREENSHAKEGLITCH_ScreenShake;

    public override void OnEnable()
    {
        o = new PropertyFetcher<GlitchVolume>(serializedObject);

        m_Mode = Unpack(o.Find(x => x.mode));

        _RGBSPLITGLITCH_NoiseTex = Unpack(o.Find(x => x._RGBSPLITGLITCH_NoiseTex));
        _RGBSPLITGLITCH_Speed = Unpack(o.Find(x => x._RGBSPLITGLITCH_Speed));
        _RGBSPLITGLITCH_Amplitude = Unpack(o.Find(x => x._RGBSPLITGLITCH_Amplitude));

        _IMAGEBLOCKGLITCH_BlockSize = Unpack(o.Find(x => x._IMAGEBLOCKGLITCH_BlockSize));
        _IMAGEBLOCKGLITCH_Speed = Unpack(o.Find(x => x._IMAGEBLOCKGLITCH_Speed));
        _IMAGEBLOCKGLITCH_MaxRGBSplit = Unpack(o.Find(x => x._IMAGEBLOCKGLITCH_MaxRGBSplit));

        _LINEBLOCKGLITCH_Frequency = Unpack(o.Find(x => x._LINEBLOCKGLITCH_Frequency));
        _LINEBLOCKGLITCH_Amount = Unpack(o.Find(x => x._LINEBLOCKGLITCH_Amount));
        _LINEBLOCKGLITCH_Offset = Unpack(o.Find(x => x._LINEBLOCKGLITCH_Offset));
        _LINEBLOCKGLITCH_LinesWidth = Unpack(o.Find(x => x._LINEBLOCKGLITCH_LinesWidth));
        _LINEBLOCKGLITCH_Alpha = Unpack(o.Find(x => x._LINEBLOCKGLITCH_Alpha));

        _TILEJITTERGLITCH_SplittingNumber = Unpack(o.Find(x => x._TILEJITTERGLITCH_SplittingNumber));
        _TILEJITTERGLITCH_JitterAmount = Unpack(o.Find(x => x._TILEJITTERGLITCH_JitterAmount));
        _TILEJITTERGLITCH_JitterSpeed = Unpack(o.Find(x => x._TILEJITTERGLITCH_JitterSpeed));
        _TILEJITTERGLITCH_Frequency = Unpack(o.Find(x => x._TILEJITTERGLITCH_Frequency));

        _SCANLINEJITTERGLITCH_Amount = Unpack(o.Find(x => x._SCANLINEJITTERGLITCH_Amount));
        _SCANLINEJITTERGLITCH_Threshold = Unpack(o.Find(x => x._SCANLINEJITTERGLITCH_Threshold));
        _SCANLINEJITTERGLITCH_Frequency = Unpack(o.Find(x => x._SCANLINEJITTERGLITCH_Frequency));

        _DIGITALSTRIPEGLITCH_Indensity = Unpack(o.Find(x => x._DIGITALSTRIPEGLITCH_Indensity));
        _DIGITALSTRIPEGLITCH_TexSize = Unpack(o.Find(x => x._DIGITALSTRIPEGLITCH_TexSize));
        _DIGITALSTRIPEGLITCH_Frequncy = Unpack(o.Find(x => x._DIGITALSTRIPEGLITCH_Frequncy));
        _DIGITALSTRIPEGLITCH_StripeLength = Unpack(o.Find(x => x._DIGITALSTRIPEGLITCH_StripeLength));
        _DIGITALSTRIPEGLITCH_StripColorAdjustColor = Unpack(o.Find(x => x._DIGITALSTRIPEGLITCH_StripColorAdjustColor));
        _DIGITALSTRIPEGLITCH_StripColorAdjustIndensity = Unpack(o.Find(x => x._DIGITALSTRIPEGLITCH_StripColorAdjustIndensity));

        _SCREENJUMPGLITCH_JumpIndensity = Unpack(o.Find(x => x._SCREENJUMPGLITCH_JumpIndensity));

        _SCREENSHAKEGLITCH_ScreenShake = Unpack(o.Find(x => x._SCREENSHAKEGLITCH_ScreenShake));
    }

    public override void OnInspectorGUI()
    {
        PropertyField(m_Mode);
        switch (m_Mode.value.intValue)
        {
            case (int)GlitchVolume.GlitchMode.None:
                break;
            case (int)GlitchVolume.GlitchMode._RGBSPLITGLITCH:
                PropertyField(_RGBSPLITGLITCH_NoiseTex, new GUIContent("噪声图"));
                PropertyField(_RGBSPLITGLITCH_Speed, new GUIContent("速度"));
                PropertyField(_RGBSPLITGLITCH_Amplitude, new GUIContent("幅度"));
                break;
            case (int)GlitchVolume.GlitchMode._IMAGEBLOCKGLITCH:
                PropertyField(_IMAGEBLOCKGLITCH_BlockSize, new GUIContent("数量"));
                PropertyField(_IMAGEBLOCKGLITCH_Speed, new GUIContent("速度"));
                PropertyField(_IMAGEBLOCKGLITCH_MaxRGBSplit, new GUIContent("最大噪声分离"));
                break;
            case (int)GlitchVolume.GlitchMode._LINEBLOCKGLITCH:
                PropertyField(_LINEBLOCKGLITCH_Frequency, new GUIContent("频率"));
                PropertyField(_LINEBLOCKGLITCH_Amount, new GUIContent("数量"));
                PropertyField(_LINEBLOCKGLITCH_Offset, new GUIContent("间隔"));
                PropertyField(_LINEBLOCKGLITCH_LinesWidth, new GUIContent("线条宽度"));
                PropertyField(_LINEBLOCKGLITCH_Alpha, new GUIContent("透明度"));
                break;
            case (int)GlitchVolume.GlitchMode._TILEJITTERGLITCH:
                PropertyField(_TILEJITTERGLITCH_SplittingNumber, new GUIContent("分离数量"));
                PropertyField(_TILEJITTERGLITCH_JitterAmount, new GUIContent("抖动数量"));
                PropertyField(_TILEJITTERGLITCH_JitterSpeed, new GUIContent("抖动速度"));
                PropertyField(_TILEJITTERGLITCH_Frequency, new GUIContent("频率"));
                break;
            case (int)GlitchVolume.GlitchMode._SCANLINEJITTERGLITCH:
                PropertyField(_SCANLINEJITTERGLITCH_Amount, new GUIContent("数量"));
                PropertyField(_SCANLINEJITTERGLITCH_Threshold, new GUIContent("阈值"));
                PropertyField(_SCANLINEJITTERGLITCH_Frequency, new GUIContent("频率"));
                break;
            case (int)GlitchVolume.GlitchMode._DIGITALSTRIPEGLITCH:
                PropertyField(_DIGITALSTRIPEGLITCH_Indensity, new GUIContent("强度"));
                PropertyField(_DIGITALSTRIPEGLITCH_TexSize, new GUIContent("像素数量"));
                PropertyField(_DIGITALSTRIPEGLITCH_Frequncy, new GUIContent("频率"));
                PropertyField(_DIGITALSTRIPEGLITCH_StripeLength, new GUIContent("条纹长度"));
                PropertyField(_DIGITALSTRIPEGLITCH_StripColorAdjustColor, new GUIContent("条纹颜色"));
                PropertyField(_DIGITALSTRIPEGLITCH_StripColorAdjustIndensity, new GUIContent("条纹强度"));
                break;
            case (int)GlitchVolume.GlitchMode._ANALOGNOISEGLITCH:
                break;
            case (int)GlitchVolume.GlitchMode._SCREENJUMPGLITCH:
                PropertyField(_SCREENJUMPGLITCH_JumpIndensity, new GUIContent("强度"));
                break;
            case (int)GlitchVolume.GlitchMode._SCREENSHAKEGLITCH:
                PropertyField(_SCREENSHAKEGLITCH_ScreenShake, new GUIContent("强度"));
                break;
            case (int)GlitchVolume.GlitchMode._WAVEJITTERGLITCH:
                break;
        }
    }
}
#endif