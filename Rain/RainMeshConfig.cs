using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[System.Serializable]
public class RainMeshConfig : ScriptableObject
{
    public static Vector3 cameraRight = new Vector3(1, 0, 0);
    public static Vector3 cameraZ = new Vector3(0, 0, 1);
    public static Vector3 cameraUp = new Vector3(0, 1f, 0f);
    public float rainAreaHeight = 30.0f;

    [Header("Layer1")]
    public Vector2 rainLayer1Area = new Vector2(0, 30);
    public int rainLayer1NumberOfParticles = 600;
    public Vector2 rainLayer1ParticleSize = new Vector2(0.1f, 0.5f);
    public Vector2 rainLayer1FlakeRandom = new Vector2(0.1f, 0.5f);
    public Vector2 layer1UVOffset = new Vector2(0, 0);
    public Vector2 layer1UVScale = new Vector2(0.2f, 1);


    [Header("Layer2")]
    public Vector2 rainLayer2Area = new Vector2(25, 40);
    public int rainLayer2NumberOfParticles = 200;
    public Vector2 rainLayer2ParticleSize = new Vector2(0.4f, 1f);
    public Vector2 rainLayer2FlakeRandom = new Vector2(0.4f, 1f);
    public Vector2 layer2UVOffset = new Vector2(0.2f, 0);
    public Vector2 layer2UVScale = new Vector2(0.4f, 1);

    [Header("Layer3")]
    public Vector2 rainLayer3Area = new Vector2(35, 45);
    public int rainLayer3NumberOfParticles = 100;
    public Vector2 rainLayer3ParticleSize = new Vector2(1f, 2f);
    public Vector2 rainLayer3FlakeRandom = new Vector2(1f, 2f);
    public Vector2 layer3UVOffset = new Vector2(0.6f, 0);
    public Vector2 layer3UVScale = new Vector2(0.4f, 1);

    public string meshName = "rain_LQ0";
    [GButton("GenRainMesh")]
    public byte _button1;


    public void GenRainMesh()
    {
#if UNITY_EDITOR
        Mesh m1 = CreateRainMesh();
        AssetDatabase.CreateAsset(m1, "Assets/ABAssets/scenes/common/texture/rain/"+ meshName + ".asset");
#endif
    }
#if UNITY_EDITOR

    public void CreateRainMeshLayer(int startIndex, int EndIndex, ref Vector3[] verts, ref Vector2[] uvs, ref Vector3[] normal, ref int[] tris, Vector2 areaSize, Vector2 particleSize, Vector2 flakeRandom, Vector2 uvScale, Vector2 uvOffset)
    {
        Vector3 position;
        bool hitlayer = false;
        float rangeRandom = areaSize.x / areaSize.y;
        for (int i = startIndex; i < EndIndex; i++)
        {
            int i4 = i * 4;
            int i6 = i * 6;
            float randomX = 0;
            float randomZ = 0;
            hitlayer = false;
            while (!hitlayer)
            {
                randomX = (Random.value * 2 - 1f);
                randomZ = (Random.value * 2 - 1f);

                if (Mathf.Abs(randomX) > rangeRandom || Mathf.Abs(randomZ) > rangeRandom)
                {
                    hitlayer = true;
                }
            }

            position.x = areaSize.y * randomX;
            position.y = rainAreaHeight * Random.value;

            position.z = areaSize.y * randomZ;

            //float rand = Random.value;
            Vector2 sizeRandom = particleSize +new Vector2(Random.Range(0, flakeRandom.x), Random.Range(0, flakeRandom.y)); ;

            verts[i4 + 0] = position - cameraRight * sizeRandom.x - cameraUp * sizeRandom.y;
            verts[i4 + 1] = position + cameraRight * sizeRandom.x - cameraUp * sizeRandom.y;
            verts[i4 + 2] = position + cameraRight * sizeRandom.x + cameraUp * sizeRandom.y;
            verts[i4 + 3] = position - cameraRight * sizeRandom.x + cameraUp * sizeRandom.y;


            uvs[i4 + 0] = new Vector2(0.0f, 0.0f) * uvScale + uvOffset;
            uvs[i4 + 1] = new Vector2(1.0f, 0.0f) * uvScale + uvOffset;
            uvs[i4 + 2] = new Vector2(1.0f, 1.0f) * uvScale + uvOffset;
            uvs[i4 + 3] = new Vector2(0.0f, 1.0f) * uvScale + uvOffset;

            //uvs[i4 + 0] = uvs[i4 + 0] * uvScale + uvOffset;
            //uvs[i4 + 1] = uvs[i4 + 1] * uvScale + uvOffset;
            //uvs[i4 + 2] = uvs[i4 + 2] * uvScale + uvOffset;
            //uvs[i4 + 3] = uvs[i4 + 3] * uvScale + uvOffset;

            normal[i4 + 0] = position;
            normal[i4 + 1] = position;
            normal[i4 + 2] = position;
            normal[i4 + 3] = position;

            tris[i6 + 0] = i4 + 0;
            tris[i6 + 1] = i4 + 1;
            tris[i6 + 2] = i4 + 2;
            tris[i6 + 3] = i4 + 0;
            tris[i6 + 4] = i4 + 2;
            tris[i6 + 5] = i4 + 3;
        }
    }
    public Mesh CreateRainMesh()
    {

        Mesh mesh = new Mesh();
        int particleNum = rainLayer1NumberOfParticles + rainLayer2NumberOfParticles + rainLayer3NumberOfParticles;
        Vector3[] verts = new Vector3[4 * particleNum];
        Vector2[] uvs = new Vector2[4 * particleNum];
        Vector3[] normal = new Vector3[4 * particleNum];

        int[] tris = new int[2 * 3 * particleNum];
        int total = 0;
        CreateRainMeshLayer(total, rainLayer1NumberOfParticles, ref verts, ref uvs, ref normal, ref tris, rainLayer1Area, rainLayer1ParticleSize, rainLayer1FlakeRandom, layer1UVScale,layer1UVOffset);
        total += rainLayer1NumberOfParticles;
        CreateRainMeshLayer(total, total + rainLayer2NumberOfParticles, ref verts, ref uvs, ref normal, ref tris, rainLayer2Area, rainLayer2ParticleSize, rainLayer2FlakeRandom, layer2UVScale, layer2UVOffset);
        total += rainLayer2NumberOfParticles;
        CreateRainMeshLayer(total, total + rainLayer3NumberOfParticles, ref verts, ref uvs, ref normal, ref tris, rainLayer3Area, rainLayer3ParticleSize, rainLayer3FlakeRandom, layer3UVScale, layer3UVOffset);

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.normals = normal;
        mesh.RecalculateBounds();

        return mesh;
    }
    [MenuItem("GameEditor/Weather/CreateRainMeshConfig")]
    public static void CreateRainMeshConfig() 
    {
        RainMeshConfig newConfig = ScriptableObject.CreateInstance<RainMeshConfig>();
        AssetDatabase.CreateAsset(newConfig, "Assets/ABAssets/scenes/common/texture/rain/newRainConfig.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif

}