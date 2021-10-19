using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class DayAndNight : MonoBehaviour
{

    public Light light;
    public Color colorDay;
    public Color colorNight;
    [Range(0, 24)]
    public float Time;

    Color curColor;

    // Start is called before the first frame update
    void Start()
    {
        curColor.a = 1;
    }

    private void OnEnable()
    {
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        curColor.r = Mathf.Lerp(colorDay.r, colorNight.r, Math.Abs(Time - 12) / 12);
        curColor.g = Mathf.Lerp(colorDay.g, colorNight.g, Math.Abs(Time - 12) / 12);
        curColor.b = Mathf.Lerp(colorDay.b, colorNight.b, Math.Abs(Time - 12) / 12);
        Shader.SetGlobalColor("_DayAndNightColor", curColor);

        light.transform.rotation = Quaternion.Euler(90 * Vector3.left + Vector3.left * 360 * Time / 24);

    }

}
