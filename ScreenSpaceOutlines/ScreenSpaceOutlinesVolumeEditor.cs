#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScreenSpaceOutlinesVolume))]
public class ScreenSpaceOutlinesVolumeEditor : VolumeComponentEditor
{
    SerializedDataParameter m_Mode;
    PropertyFetcher<ScreenSpaceOutlinesVolume> o;

    SerializedDataParameter _OutlineScale;
    SerializedDataParameter _RobertsCrossMultiplier;
    SerializedDataParameter _DepthThreshold;
    SerializedDataParameter _NormalThreshold;
    SerializedDataParameter _SteepAngleThreshold;
    SerializedDataParameter _SteepAngleMultiplier;
    SerializedDataParameter _OutlineColor;
    public override void OnEnable()
    {
        o = new PropertyFetcher<ScreenSpaceOutlinesVolume>(serializedObject);
        _OutlineScale = Unpack(o.Find(x => x._OutlineScale));
        _RobertsCrossMultiplier = Unpack(o.Find(x => x._RobertsCrossMultiplier));
        _DepthThreshold = Unpack(o.Find(x => x._DepthThreshold));
        _NormalThreshold = Unpack(o.Find(x => x._NormalThreshold));
        _SteepAngleThreshold = Unpack(o.Find(x => x._SteepAngleThreshold));
        _SteepAngleMultiplier = Unpack(o.Find(x => x._SteepAngleMultiplier));
        _OutlineColor = Unpack(o.Find(x => x._OutlineColor));

    }

    public override void OnInspectorGUI()
    {

        PropertyField(_OutlineScale, new GUIContent("边缘宽度"));
        PropertyField(_RobertsCrossMultiplier, new GUIContent("Roberts叉乘因子"));
        PropertyField(_DepthThreshold, new GUIContent("深度阈值"));
        PropertyField(_NormalThreshold, new GUIContent("法线阈值"));
        PropertyField(_SteepAngleThreshold, new GUIContent("陡峭角度阈值"));
        PropertyField(_SteepAngleMultiplier, new GUIContent("陡峭角度因子"));
        PropertyField(_OutlineColor, new GUIContent("边缘颜色"));
    }
}
#endif