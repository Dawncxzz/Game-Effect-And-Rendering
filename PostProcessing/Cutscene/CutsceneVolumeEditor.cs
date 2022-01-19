using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

[VolumeComponentEditor(typeof(CutsceneVolume))]
public class CutsceneVolumeEditor : VolumeComponentEditor
{
    SerializedDataParameter m_Mode;
    PropertyFetcher<CutsceneVolume> o;

    SerializedDataParameter _FLIPOVER_Width;
    SerializedDataParameter _FLIPOVER_Progress;

    SerializedDataParameter _GRAYSCALE_Value;

    public override void OnEnable()
    {
        o = new PropertyFetcher<CutsceneVolume>(serializedObject);

        m_Mode = Unpack(o.Find(x => x.mode));

        _FLIPOVER_Width = Unpack(o.Find(x => x._FLIPOVER_Width));
        _FLIPOVER_Progress = Unpack(o.Find(x => x._FLIPOVER_Progress));

        _GRAYSCALE_Value = Unpack(o.Find(x => x._GRAYSCALE_Value));
    }

    public override void OnInspectorGUI()
    {
        PropertyField(m_Mode);
        switch (m_Mode.value.intValue)
        {
            case (int)CutsceneVolume.CutsceneMode.None:
                break;
            case (int)CutsceneVolume.CutsceneMode._FLIPOVER:
                PropertyField(_FLIPOVER_Width, new GUIContent("宽度"));
                PropertyField(_FLIPOVER_Progress, new GUIContent("进度"));
                break;
            case (int)CutsceneVolume.CutsceneMode._CLOCKWIPE:
                break;
            case (int)CutsceneVolume.CutsceneMode._DOUBLECLOCKWIPE:
                break;
            case (int)CutsceneVolume.CutsceneMode._WEDEWIPE:
                break;
            case (int)CutsceneVolume.CutsceneMode._INKFADE:
                break;
            case (int)CutsceneVolume.CutsceneMode._SLIDINGBANDS:
                break;
            case (int)CutsceneVolume.CutsceneMode._CHECKERWIPE:
                break;
            case (int)CutsceneVolume.CutsceneMode._DISSOLVE:
                break;
            case (int)CutsceneVolume.CutsceneMode._DIAMONDDISSOLVE:
                break;
            case (int)CutsceneVolume.CutsceneMode._TRIANGLEDISSOLVE:
                break;
            case (int)CutsceneVolume.CutsceneMode._DOOR:
                break;
            case (int)CutsceneVolume.CutsceneMode._SPIN:
                break;
            case (int)CutsceneVolume.CutsceneMode._CENTERMERGE:
                break;
            case (int)CutsceneVolume.CutsceneMode._CENTERSPLIT:
                break;
            case (int)CutsceneVolume.CutsceneMode._BANDSLIDE:
                break;
            case (int)CutsceneVolume.CutsceneMode._IRISROUND:
                break;
            case (int)CutsceneVolume.CutsceneMode._RANDOMBLOCKS:
                break;
            case (int)CutsceneVolume.CutsceneMode._RANDOMBWIPE:
                break;
            case (int)CutsceneVolume.CutsceneMode._GRAYSCALE:
                PropertyField(_GRAYSCALE_Value, new GUIContent("灰度"));
                break;

        }
    }
}
