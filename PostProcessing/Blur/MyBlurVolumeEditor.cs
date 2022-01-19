using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

[VolumeComponentEditor(typeof(MyBlurVolume))]
public class MyBlurVolumeEditor : VolumeComponentEditor
{
    SerializedDataParameter m_Mode;
    PropertyFetcher<MyBlurVolume> o;

    SerializedDataParameter _GAUSSIANBLUR_Iterations;
    SerializedDataParameter _GAUSSIANBLUR_Downscale;
    SerializedDataParameter _GAUSSIANBLUR_Offsets;
    public override void OnEnable()
    {
        o = new PropertyFetcher<MyBlurVolume>(serializedObject);

        m_Mode = Unpack(o.Find(x => x.mode));

        _GAUSSIANBLUR_Iterations = Unpack(o.Find(x => x._GAUSSIANBLUR_Iterations));
        _GAUSSIANBLUR_Downscale = Unpack(o.Find(x => x._GAUSSIANBLUR_Downscale));
        _GAUSSIANBLUR_Offsets = Unpack(o.Find(x => x._GAUSSIANBLUR_Offsets));
    }

    public override void OnInspectorGUI()
    {
        PropertyField(m_Mode);
        switch (m_Mode.value.intValue)
        {
            case (int)MyBlurVolume.BlurMode.None:
                break;
            case (int)MyBlurVolume.BlurMode._GAUSSIANBLUR:
                PropertyField(_GAUSSIANBLUR_Iterations, new GUIContent("迭代次数"));
                PropertyField(_GAUSSIANBLUR_Downscale, new GUIContent("下采样"));
                PropertyField(_GAUSSIANBLUR_Offsets, new GUIContent("模板大小"));
                break;
            case (int)MyBlurVolume.BlurMode._BOXBLUR:
                break;
            case (int)MyBlurVolume.BlurMode._KAWASEBLUR:
                break;
            case (int)MyBlurVolume.BlurMode._DUALBLUR:
                break;
            case (int)MyBlurVolume.BlurMode._BOKEHBLUR:
                break;
            case (int)MyBlurVolume.BlurMode._TILTSHIFTBLUR:
                break;
            case (int)MyBlurVolume.BlurMode._LRISBLUR:
                break;
            case (int)MyBlurVolume.BlurMode._GRAINYBLUR:
                break;
            case (int)MyBlurVolume.BlurMode._RADIALBLUR:
                break;
            case (int)MyBlurVolume.BlurMode._DIRECTIONALBLUR:
                break;
        }
    }
}
