using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

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
                PropertyField(_IMAGEBLOCKGLITCH_BlockSize, new GUIContent("大小"));
                PropertyField(_IMAGEBLOCKGLITCH_Speed, new GUIContent("速度"));
                PropertyField(_IMAGEBLOCKGLITCH_MaxRGBSplit, new GUIContent("噪声分离"));
                break;
            case (int)GlitchVolume.GlitchMode._LINEBLOCKGLITCH:
                break;
            case (int)GlitchVolume.GlitchMode._TILEJITTERGLITCH:
                break;
            case (int)GlitchVolume.GlitchMode._SCANLINEJITTERGLITCH:
                break;
            case (int)GlitchVolume.GlitchMode._DIGITALSTRIPEGLITCH:
                break;
            case (int)GlitchVolume.GlitchMode._ANALOGNOISEGLITCH:
                break;
            case (int)GlitchVolume.GlitchMode._SCREENJUMPGLITCH:
                break;
            case (int)GlitchVolume.GlitchMode._SCREENSHAKEGLITCH:
                break;
            case (int)GlitchVolume.GlitchMode._WAVEJITTERGLITCH:
                break;
        }
    }
}
