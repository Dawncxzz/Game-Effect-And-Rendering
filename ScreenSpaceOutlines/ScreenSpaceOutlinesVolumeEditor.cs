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

        PropertyField(_OutlineScale, new GUIContent("��Ե���"));
        PropertyField(_RobertsCrossMultiplier, new GUIContent("Roberts�������"));
        PropertyField(_DepthThreshold, new GUIContent("�����ֵ"));
        PropertyField(_NormalThreshold, new GUIContent("������ֵ"));
        PropertyField(_SteepAngleThreshold, new GUIContent("���ͽǶ���ֵ"));
        PropertyField(_SteepAngleMultiplier, new GUIContent("���ͽǶ�����"));
        PropertyField(_OutlineColor, new GUIContent("��Ե��ɫ"));
    }
}
#endif