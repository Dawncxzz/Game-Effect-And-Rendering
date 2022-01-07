using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RainManager : MonoBehaviour
{

    public ComputeShader computeShader;

    class Rain
    {
        public int layer;
        public Vector3 dir;
        public Vector3 birth_pos;
        public Vector3 end_pos;
        public Vector3 size_offset_pos1;
        public Vector3 size_offset_pos2;
        public Vector3 size_offset_pos3;
    }

    //public float RAIN_MESH_HEIGHT = 30;
    public const float RAIN_AREA = 100;
    public const float RAIN_AREA_HALF = RAIN_AREA / 2;
    public const float RAIN_HEIGHT = 30;

    //private Vector3 RAIN_OFFSET = new Vector3(0, -RAIN_HEIGHT/2 + 10, 0);
    private Vector3 RAIN_OFFSET = new Vector3(0, -10, 0);
    public WindZone windzone;

    public Vector2 horizontalSpeedRange = new Vector2(-0.1f, 0.1f);
    public Vector2 verticalSpeedRange = new Vector2(-1.0f, -2.0f);

    public Vector2 horizontalSpeedRange2 = new Vector2(-0.1f, 0.1f);
    public Vector2 verticalSpeedRange2 = new Vector2(-1.0f, -2.0f);

    public Vector2 horizontalSpeedRange3 = new Vector2(-0.1f, 0.1f);
    public Vector2 verticalSpeedRange3 = new Vector2(-1.0f, -2.0f);
    public RainMeshConfig meshConfig;
    public Color color = Color.white;
    MeshRenderer rainRender;
    private enum eRainRunState
    {
        None = 0,
        Load = 2,
        Update = 3,
        Release = 4,
    }
    private eRainRunState m_state = eRainRunState.None;

    private static GameObject GlobaRainManagerObject;

    //摄像机相关
    private Transform m_mainCam;            //主摄像机
    private Vector3 m_lastMainCamPos = Vector3.zero;

    [GButton("ChangeRotation")]
    public byte _button;
    class RainInfo  //雪丝相关参数 
    {
        public int size = 1;
        public float speed = 10.0f;
        public GameObject rainObject;
    }
    private RainInfo m_rain = new RainInfo();

    public float focusForwardDistance = 10;

    public class SputterInfo
    {
        public GameObject spray;
        public GameObject ripple;
        public float runTime;
        public float cooling;
    }
    List<SputterInfo> sputterInfos = new List<SputterInfo>();
    public GameObject sputter;
    public float maxSputterTime = 0.5f;
    public float coolingTime = 0.1f;
    public float scale = 1f;

    Mesh rain_mesh;
    Vector3[] verts;
    List<Rain> rain_list = new List<Rain>();
    Vector3[] normals;
    Material mat;
    /// <summary>
    /// 创建一个全局雪控制器对象
    /// </summary>
    /// <returns> 全局雪控制器对象</returns>
    public static RainManager GetInstance()
    {
        RainManager rm = null;
        if (GlobaRainManagerObject == null)
        {
            GlobaRainManagerObject = new GameObject("GlobaRainManagerObject");
            UnityEngine.Object.DontDestroyOnLoad(GlobaRainManagerObject);
            rm = GlobaRainManagerObject.AddComponent(typeof(RainManager)) as RainManager;
        }
        else
        {
            rm = GlobaRainManagerObject.GetComponent("RainManager") as RainManager;
        }
        return rm;
    }

    public GameObject mainRole;
    public Texture2D rainTex;
    public Mesh rainMesh;

    public void Start()  //开始下雪
    {
        Debug.Log("OpenRain");
        if (Camera.main != null) 
        {
            m_mainCam = Camera.main.transform;
        }

        if (m_state == eRainRunState.None && m_mainCam != null)
        {
            InitInfo();
        }

        return;
    }

    private void DestroyRainImmediately()
    {
        DestroyAllObject();
        SetState(eRainRunState.None);
    }
    private void DestroyAllObject()
    {
        Destroy(m_rain.rainObject);
    }

    public void CloseRain()
    {
        SetState(eRainRunState.Release);
    }
    public static void ForceCloseRain()
    {
        if (GlobaRainManagerObject != null)
        {
            RainManager rm = GlobaRainManagerObject.GetComponent("RainManager") as RainManager;
            if (rm != null)
            {
                rm.DestroyRainImmediately();
            }
        }
    }
    public void SetRainArg(int size, float speed)
    {
        m_rain.size = size;
        m_rain.speed = speed;
    }

    private void InitInfo()
    {
        if (m_state != eRainRunState.None)
        {
            return;
        }
        if (m_mainCam == null)
        {
            Debug.Log("main role no create");
            DestroyRainImmediately();
            return;
        }
        mBirthPos = m_mainCam.position + m_mainCam.forward * focusForwardDistance;
        mBirthPos = transform.InverseTransformPoint(mBirthPos);
        m_lastMainCamPos = mBirthPos;

        

        //创建雪的模板
        GameObject rainObject = new GameObject("rain");
        rainObject.layer = LayerMask.NameToLayer("Water");
        rainRender = rainObject.AddComponent<MeshRenderer>();
        //mat = new Material(Shader.Find("Q/Scene/Weather/SceneRain"));
        mat = new Material(Shader.Find("Art/Weather/SceneRain"));
        mat.mainTexture = rainTex;
        //mat.mainTexture = Resources.Load<Texture2D>("weather/rain/rain");
        rainRender.sharedMaterial = mat;
        rain_mesh = Object.Instantiate<Mesh>(rainMesh);
        //rain_mesh = Resources.Load<Mesh>("weather/rain/RainFx/rain_LQ0");
        rainObject.AddComponent<MeshFilter>().mesh = rain_mesh;
        m_rain.rainObject = rainObject;

        rain_mesh.bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(3000, 200, 3000));
        verts = rain_mesh.vertices;
        normals = new Vector3[verts.Length];
        Vector3 vert;
        for (int i = 0; i < verts.Length; i += 4)
        {
            Rain s = new Rain();
            //s.dir = new Vector3(Random.Range(0.0f, 0.5f), -Random.Range(1f, 1.5f), Random.Range(-0.1f, 0.1f));

            if (i / 4 < meshConfig.rainLayer1NumberOfParticles)
            {
                s.layer = 1;
                float horizontalRandom = Random.Range(horizontalSpeedRange.x, horizontalSpeedRange.y);
                s.dir = new Vector3(horizontalRandom, Random.Range(verticalSpeedRange.x, verticalSpeedRange.y), horizontalRandom);

            }
            else if (i / 4 < (meshConfig.rainLayer1NumberOfParticles + meshConfig.rainLayer2NumberOfParticles))
            {
                s.layer = 2;
                float horizontalRandom = Random.Range(horizontalSpeedRange2.x, horizontalSpeedRange2.y);
                s.dir = new Vector3(horizontalRandom, Random.Range(verticalSpeedRange2.x, verticalSpeedRange2.y), horizontalRandom);
            }
            else
            {
                s.layer = 3;
                float horizontalRandom = Random.Range(horizontalSpeedRange3.x, horizontalSpeedRange3.y);
                s.dir = new Vector3(horizontalRandom, Random.Range(verticalSpeedRange3.x, verticalSpeedRange3.y), horizontalRandom);
            }

            s.birth_pos = new Vector3(verts[i].x, meshConfig.rainAreaHeight, verts[i].z);
            s.end_pos = mBirthPos + RAIN_OFFSET;
            vert = verts[i];
            s.size_offset_pos1 = verts[i + 1] - vert;
            s.size_offset_pos2 = verts[i + 2] - vert;
            s.size_offset_pos3 = verts[i + 3] - vert;
            rain_list.Add(s);
        }

        //初始化水花和涟漪
        if (sputter != null)
        {
            sputter = Instantiate(sputter, transform.position + Vector3.up * 0.1f, transform.rotation);
            for (int i = 0; i < sputter.GetChildren().Length / 2; i++)
            {
                SputterInfo sputterInfo = new SputterInfo();
                sputterInfo.runTime = 0;
                sputterInfo.cooling = coolingTime;
                sputterInfo.ripple = sputter.GetChildren()[i];
                sputterInfo.spray = sputter.GetChildren()[i + sputter.GetChildren().Length / 2];
                sputterInfos.Add(sputterInfo);
                sputterInfo.ripple.SetActive(false);
                sputterInfo.spray.SetActive(false);
            }
        }

        SetState(eRainRunState.Load);
    }

    public void ChangeRotation() 
    {
        //Vector3[] vects = rainMesh.vertices;
        //Vector3[] normals = rainMesh.normals;

        //Transform windTsf = windzone.transform;
        //for (int i = 0; i < vects.Length; i+=4) 
        //{
        //    vects[i] = windTsf.TransformPoint(vects[i]);
        //    vects[i+1] = windTsf.TransformPoint(vects[i+1]);
        //    vects[i+2] = windTsf.TransformPoint(vects[i+2]);
        //    vects[i+3] = windTsf.TransformPoint(vects[i+3]);

        //    Vector3 position = (vects[i] + vects[i + 1] + vects[i + 2] + vects[i + 3]) / 4;
        //    normals[i + 0] = position;
        //    normals[i + 1] = position;
        //    normals[i + 2] = position;
        //    normals[i + 3] = position;
        //}

        //rainMesh.vertices = vects;
        //rainMesh.normals = normals;
        Vector3 forward = -windzone.transform.forward;
        //forward.y *= -1;
        GetComponent<MeshRenderer>().sharedMaterial.SetVector("_upDir", forward);
    }

    public void SpeedChange()
    {
        for (int i= 0; i< rain_list.Count; i++)
        {
            if (rain_list[i].layer == 1)
            {
                float horizontalRandom = Random.Range(horizontalSpeedRange.x, horizontalSpeedRange.y);
                rain_list[i].dir = new Vector3(horizontalRandom, Random.Range(verticalSpeedRange.x, verticalSpeedRange.y), horizontalRandom);
            }
            else if (rain_list[i].layer == 2)
            {
                float horizontalRandom = Random.Range(horizontalSpeedRange2.x, horizontalSpeedRange2.y);
                rain_list[i].dir = new Vector3(horizontalRandom, Random.Range(verticalSpeedRange2.x, verticalSpeedRange2.y), horizontalRandom);
            }
            else
            {
                float horizontalRandom = Random.Range(horizontalSpeedRange3.x, horizontalSpeedRange3.y);
                rain_list[i].dir = new Vector3(horizontalRandom, Random.Range(verticalSpeedRange3.x, verticalSpeedRange3.y), horizontalRandom);
            }
        }
    }

    public void Update()
    {
        if (m_state != eRainRunState.None)
        {
            switch (m_state)
            {
                case eRainRunState.Load: Load(); break;
                case eRainRunState.Update: ForUpdate(); break;
                case eRainRunState.Release: ReleaseRain(); break;
            }
        }
    }

    private void Load()
    {
        if (m_mainCam == null)
        {
            Debug.Log("main role no create");
            DestroyRainImmediately();
            return;
        }

        Transform tmpTran = null;

        tmpTran = m_rain.rainObject.transform;

        tmpTran.parent = transform;
        tmpTran.position = new Vector3(0, 0, 0);// m_rainCam.position + RAIN_OFFSET;
        tmpTran.localRotation = Quaternion.identity;

        SetState(eRainRunState.Update);
    }

    private Vector3 mBirthPos;
  

    private void ForUpdate()
    {

        //雨滴部分
        Vector3 dir;
        Vector3 vert;
        Vector3 windzoneDir = windzone.transform.forward;
        mBirthPos = m_mainCam.position + m_mainCam.forward * focusForwardDistance;
        mBirthPos = transform.InverseTransformPoint(mBirthPos);
        Debug.Log(m_mainCam.forward +  "  "+  mBirthPos);
        //forward.y *= -1;
        mat.SetVector("_upDir", -windzoneDir);
        mat.SetColor("_Color", color);

        //ComputeBuffer computeBuffer1 = new ComputeBuffer(rain_list.Count, 19 * 4);

        //computeBuffer1.SetData(rain_list);
        //ComputeBuffer computeBuffer2 = new ComputeBuffer(verts.Length, 3 * 4);
        //computeBuffer2.SetData(verts);
        //ComputeBuffer computeBuffer3 = new ComputeBuffer(normals.Length, 3 * 4);
        //computeBuffer3.SetData(normals);
        //int kernel = computeShader.FindKernel("CSMain");

        //computeShader.SetBuffer(kernel, "rain_list", computeBuffer1);
        //computeShader.SetBuffer(kernel, "verts", computeBuffer2);
        //computeShader.SetBuffer(kernel, "normals", computeBuffer3);
        //computeShader.SetFloat("RAIN_AREA_HALF", RAIN_AREA_HALF);
        //computeShader.SetVector("windzoneDir", windzoneDir);
        //computeShader.SetVector("mBirthPos", mBirthPos);
        //computeShader.SetVector("RAIN_OFFSET", RAIN_OFFSET);

        //computeShader.SetFloat("TimeInterval", Time.deltaTime);

        //computeShader.Dispatch(kernel, verts.Length / 4, 1, 1);

        //Graphics.DrawMeshInstancedIndirect(rainMesh, 0, mat, rainMesh.bounds, computeBuffer1);

        for (int i = 0; i < verts.Length; i += 4)
        {
            Rain rain = rain_list[i / 4];
            dir = rain.dir * Time.deltaTime * 7;
            dir.x *= windzoneDir.x;
            dir.z *= windzoneDir.z;
            vert = verts[i] + dir;
            if (vert.x < rain.end_pos.x + RAIN_AREA_HALF && vert.y > rain.end_pos.y)
            {
                verts[i] = vert;
                verts[i + 1] += dir;
                verts[i + 2] += dir;
                verts[i + 3] += dir;
            }
            else
            {
                //Vector3 birth_pos = m_mainCam.position;
                rain.end_pos = mBirthPos + RAIN_OFFSET;
                //vert = new Vector3(Random.Range(0.0f, RAIN_AREA) - RAIN_AREA_HALF, RAIN_HEIGHT, Random.Range(0.0f, RAIN_AREA) - RAIN_AREA_HALF) + birth_pos;
                vert = mBirthPos + rain.birth_pos;
                verts[i] = vert;
                verts[i + 1] = vert + rain.size_offset_pos1;
                verts[i + 2] = vert + rain.size_offset_pos2;
                verts[i + 3] = vert + rain.size_offset_pos3;

            }
            Vector3 center = (verts[i] + verts[i + 2]) / 2;
            normals[i] = center;
            normals[i + 1] = center;
            normals[i + 2] = center;
            normals[i + 3] = center;
            //Debug.DrawLine(center, center + Vector3.up,  Color.red);
        }

        //Debug.Log(verts.Length / 4 + " " + verts[4]);
        //computeBuffer2.GetData(verts);
        //computeBuffer3.GetData(normals);
        rain_mesh.vertices = verts;
        rain_mesh.normals = normals;
        //computeBuffer1.Dispose();
        //computeBuffer2.Dispose();
        //computeBuffer3.Dispose();

        //水花和涟漪部分
        if (sputter != null)
        {
            MaterialPropertyBlock matPropertyBlock = new MaterialPropertyBlock();
            int curNum = Random.Range(0, sputter.GetChildren().Length / 2);
            for (int i = 0; i < sputterInfos.Count; i++)
            {
                if (sputterInfos[i].cooling <= 0)
                {
                    if (sputterInfos[i].runTime == 0 && i == curNum)
                    {
                        sputterInfos[i].spray.SetActive(true);
                        sputterInfos[i].ripple.SetActive(true);
                        sputterInfos[i].runTime += Time.deltaTime;
                    }
                    else if (sputterInfos[i].runTime > 0 && sputterInfos[i].runTime < maxSputterTime)
                    {
                        sputterInfos[i].runTime += Time.deltaTime;
                    }
                    else
                    {
                        sputterInfos[i].runTime = 0;
                        sputterInfos[i].cooling = coolingTime;
                        sputterInfos[i].spray.SetActive(false);
                        sputterInfos[i].ripple.SetActive(false);
                    }
                }
                else
                {
                    sputterInfos[i].cooling -= Time.deltaTime;
                }
                matPropertyBlock.SetFloat("_TimeLine", sputterInfos[i].runTime / maxSputterTime);
                matPropertyBlock.SetFloat("_Scale", scale);

                sputterInfos[i].ripple.GetComponent<MeshRenderer>().SetPropertyBlock(matPropertyBlock);
                sputterInfos[i].spray.GetComponent<MeshRenderer>().SetPropertyBlock(matPropertyBlock);


                //sputterInfos[i].spray.GetComponent<MeshRenderer>().material.SetFloat("_TimeLine", sputterInfos[i].runTime / maxSputterTime);
                //sputterInfos[i].ripple.GetComponent<MeshRenderer>().material.SetFloat("_TimeLine", sputterInfos[i].runTime / maxSputterTime);

                //sputterInfos[i].spray.GetComponent<MeshRenderer>().material.SetFloat("_Scale", scale);
                //sputterInfos[i].ripple.GetComponent<MeshRenderer>().material.SetFloat("_Scale", scale);
            }
        }

    }

    private void ReleaseRain()
    {
        DestroyRainImmediately();
        return;
    }

    private void SetState(eRainRunState state)
    {
        m_state = state;
    }
    public void SetRainSpeed(float speed)
    {
        m_rain.speed = speed;
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(RainManager))]
public class RainManagerEditor : Editor
{
    private RainManager sm;
    public void OnEnable()
    {
        sm = (RainManager)target;
        Undo.RegisterCompleteObjectUndo(sm, "RainManagerEditor");
    }
    public override void OnInspectorGUI()
    {
        //    public GameObject mainRole;
        //public Texture2D rainTex;
        //public Mesh rainMesh;
        //public Vector2 horizontalSpeedRange = new Vector2(-0.1f, 0.1f);
        //public Vector2 verticalSpeedRange = new Vector2(-1.0f, -2.0f);

        //sm.mainRolemainRole = EditorGUILayout.ObjectField("mainRole", sm.mainRole, typeof(GameObject), true) as GameObject;
        sm.computeShader = EditorGUILayout.ObjectField("computeShader", sm.computeShader, typeof(ComputeShader), true) as ComputeShader;

        sm.focusForwardDistance = EditorGUILayout.FloatField("雨丝区域中心向前偏移距离:", sm.focusForwardDistance);
        sm.rainTex = EditorGUILayout.ObjectField("rainTex", sm.rainTex, typeof(Texture2D), true) as Texture2D;
        sm.rainMesh = EditorGUILayout.ObjectField("rainMesh", sm.rainMesh, typeof(Mesh), true) as Mesh;


        sm.windzone = EditorGUILayout.ObjectField("windzone", sm.windzone, typeof(WindZone), true) as WindZone;

        sm.meshConfig = EditorGUILayout.ObjectField("meshConfig", sm.meshConfig, typeof(RainMeshConfig), true) as RainMeshConfig;
        sm.color = EditorGUILayout.ColorField("Color", sm.color);
        EditorGUI.BeginChangeCheck();
        sm.verticalSpeedRange = EditorGUILayout.Vector2Field("verticalSpeedRange", sm.verticalSpeedRange);
        sm.horizontalSpeedRange = EditorGUILayout.Vector2Field("horizontalSpeedRange", sm.horizontalSpeedRange);
        GUILayout.Space(10);

        sm.verticalSpeedRange2 = EditorGUILayout.Vector2Field("verticalSpeedRange2", sm.verticalSpeedRange2);
        sm.horizontalSpeedRange2 = EditorGUILayout.Vector2Field("horizontalSpeedRange2", sm.horizontalSpeedRange2);
        GUILayout.Space(10);

        sm.verticalSpeedRange3 = EditorGUILayout.Vector2Field("verticalSpeedRange3", sm.verticalSpeedRange3);
        sm.horizontalSpeedRange3 = EditorGUILayout.Vector2Field("horizontalSpeedRange3", sm.horizontalSpeedRange3);

        sm.sputter = EditorGUILayout.ObjectField("sputter", sm.sputter, typeof(GameObject), true) as GameObject;
        sm.maxSputterTime = EditorGUILayout.FloatField("maxSputterTime", sm.maxSputterTime);
        sm.coolingTime = EditorGUILayout.FloatField("coolingTime", sm.coolingTime);
        sm.scale = EditorGUILayout.FloatField("scale", sm.scale);
        sm = (RainManager)target;
        Undo.RegisterCompleteObjectUndo(sm, "RainManagerEditor");

        if (EditorGUI.EndChangeCheck())
        {
            sm.SpeedChange();
        }

        //if (GUILayout.Button("ChangeRotation")) 
        //{
        //    sm.ChangeRotation();
        //}

    }
}
#endif