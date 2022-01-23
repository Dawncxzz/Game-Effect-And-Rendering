using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenu("Post-processing/Ghost Effect")]
public class GhostEffectVolume : VolumeComponent, IPostProcessComponent
{
    public enum GhostEffectMode
    {
        None,
        _PLAYER,
    }

    [Tooltip("选择一个角色残影")]
    public GhostEffectModeParameter mode = new GhostEffectModeParameter(GhostEffectMode.None);

    [Header("角色残影参数")]
    public MinFloatParameter _PLAYER_Width = new MinFloatParameter(0, 0, true);
    public ClampedFloatParameter _PLAYER_Progress = new ClampedFloatParameter(0, 0, 1, true);

    public bool IsActive() => mode.value != GhostEffectMode.None;
    public bool IsTileCompatible() => true;

    [Serializable]
    public sealed class GhostEffectModeParameter : VolumeParameter<GhostEffectMode> 
    { 
        public GhostEffectModeParameter(GhostEffectMode value, bool overrideState = false) : base(value, overrideState) { }

        public override string ToString()
        {
            return value.ToString();
        }
    }

}
