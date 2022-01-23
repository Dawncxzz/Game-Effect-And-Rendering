using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif
using UnityEngine;

#if UNITY_EDITOR
[VolumeComponentEditor(typeof(GhostEffectVolume))]
public class GhostEffectVolumeEditor : VolumeComponentEditor
{
    SerializedDataParameter m_Mode;
    PropertyFetcher<GhostEffectVolume> o;

    SerializedDataParameter _PLAYER_Width;
    SerializedDataParameter _PLAYER_Progress;

    public override void OnEnable()
    {
        o = new PropertyFetcher<GhostEffectVolume>(serializedObject);

        m_Mode = Unpack(o.Find(x => x.mode));

        _PLAYER_Width = Unpack(o.Find(x => x._PLAYER_Width));
        _PLAYER_Progress = Unpack(o.Find(x => x._PLAYER_Progress));

    }

    public override void OnInspectorGUI()
    {
        PropertyField(m_Mode);
        switch (m_Mode.value.intValue)
        {
            case (int)GhostEffectVolume.GhostEffectMode.None:
                break;
            case (int)GhostEffectVolume.GhostEffectMode._PLAYER:
                PropertyField(_PLAYER_Width, new GUIContent("¿í¶È"));
                PropertyField(_PLAYER_Progress, new GUIContent("½ø¶È"));
                break;
            default:
                break;

        }
    }
}
#endif
