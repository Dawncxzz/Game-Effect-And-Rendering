using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif
using UnityEngine;
#if UNITY_EDITOR
[VolumeComponentEditor(typeof(CutsceneVolume))]
public class CutsceneVolumeEditor : VolumeComponentEditor
{
    SerializedDataParameter m_Mode;
    PropertyFetcher<CutsceneVolume> o;

    SerializedDataParameter _FLIPOVER_Width;
    SerializedDataParameter _FLIPOVER_Progress;

    SerializedDataParameter _CLOCKWIPE_Width;
    SerializedDataParameter _CLOCKWIPE_Blend;

    SerializedDataParameter _GRAYSCALE_Value;

    SerializedDataParameter _DENSEFOG1_MainTex;
    SerializedDataParameter _DENSEFOG1_FlowMapTex;
    SerializedDataParameter _DENSEFOG1_Offset;
    SerializedDataParameter _DENSEFOG1_Speed;
    SerializedDataParameter _DENSEFOG1_Intensity;

    SerializedDataParameter _DENSEFOG2_Mask;
    SerializedDataParameter _DENSEFOG2_Noise;
    SerializedDataParameter _DENSEFOG2_Noise1Params;
    SerializedDataParameter _DENSEFOG2_Noise2Params;
    SerializedDataParameter _DENSEFOG2_Color1;
    SerializedDataParameter _DENSEFOG2_Color2;

    public override void OnEnable()
    {
        o = new PropertyFetcher<CutsceneVolume>(serializedObject);

        m_Mode = Unpack(o.Find(x => x.mode));

        _FLIPOVER_Width = Unpack(o.Find(x => x._FLIPOVER_Width));
        _FLIPOVER_Progress = Unpack(o.Find(x => x._FLIPOVER_Progress));

        _CLOCKWIPE_Width = Unpack(o.Find(x => x._CLOCKWIPE_Width));
        _CLOCKWIPE_Blend = Unpack(o.Find(x => x._CLOCKWIPE_Blend));

        _GRAYSCALE_Value = Unpack(o.Find(x => x._GRAYSCALE_Value));

        _DENSEFOG1_MainTex = Unpack(o.Find(x => x._DENSEFOG1_MainTex));
        _DENSEFOG1_FlowMapTex = Unpack(o.Find(x => x._DENSEFOG1_FlowMapTex));
        _DENSEFOG1_Offset = Unpack(o.Find(x => x._DENSEFOG1_Offset));
        _DENSEFOG1_Speed = Unpack(o.Find(x => x._DENSEFOG1_Speed));
        _DENSEFOG1_Intensity = Unpack(o.Find(x => x._DENSEFOG1_Intensity));



        _DENSEFOG2_Mask = Unpack(o.Find(x => x._DENSEFOG2_Mask));
        _DENSEFOG2_Noise = Unpack(o.Find(x => x._DENSEFOG2_Noise));
        _DENSEFOG2_Noise1Params = Unpack(o.Find(x => x._DENSEFOG2_Noise1Params));
        _DENSEFOG2_Noise2Params = Unpack(o.Find(x => x._DENSEFOG2_Noise2Params));
        _DENSEFOG2_Color1 = Unpack(o.Find(x => x._DENSEFOG2_Color1));
        _DENSEFOG2_Color2 = Unpack(o.Find(x => x._DENSEFOG2_Color2));


    }

    public override void OnInspectorGUI()
    {
        PropertyField(m_Mode);
        switch (m_Mode.value.intValue)
        {
            case (int)CutsceneVolume.CutsceneMode.None:
                break;
            case (int)CutsceneVolume.CutsceneMode._FLIPOVER:
                PropertyField(_FLIPOVER_Width, new GUIContent("���"));
                PropertyField(_FLIPOVER_Progress, new GUIContent("����"));
                break;
            case (int)CutsceneVolume.CutsceneMode._CLOCKWIPE:
                PropertyField(_CLOCKWIPE_Width, new GUIContent("����"));
                PropertyField(_CLOCKWIPE_Blend, new GUIContent("����"));
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
                PropertyField(_GRAYSCALE_Value, new GUIContent("�Ҷ�"));
                break;
            case (int)CutsceneVolume.CutsceneMode._DENSEFOG1:
                PropertyField(_DENSEFOG1_MainTex, new GUIContent("��ɫͼ"));
                PropertyField(_DENSEFOG1_FlowMapTex, new GUIContent("����ͼ"));
                PropertyField(_DENSEFOG1_Offset, new GUIContent("ƫ��"));
                PropertyField(_DENSEFOG1_Speed, new GUIContent("�ٶ�"));
                PropertyField(_DENSEFOG1_Intensity, new GUIContent("�Ŷ�ǿ��"));
                break;
            case (int)CutsceneVolume.CutsceneMode._DENSEFOG2:
                PropertyField(_DENSEFOG2_Mask,  new GUIContent("����ͼ"));
                PropertyField(_DENSEFOG2_Noise,  new GUIContent("����ͼ"));
                PropertyField(_DENSEFOG2_Noise1Params, new GUIContent("����(X:����С Y:�ٶ� Z:ƫ��)"));
                PropertyField(_DENSEFOG2_Noise2Params, new GUIContent("����(X:����С Y:�ٶ� Z:ƫ��)"));
                PropertyField(_DENSEFOG2_Color1, new GUIContent("������ɫ"));
                PropertyField(_DENSEFOG2_Color2, new GUIContent("������ɫ"));
                break;

        }
    }
}
#endif