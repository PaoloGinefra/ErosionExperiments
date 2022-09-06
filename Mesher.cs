using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mesher : MonoBehaviour
{
    [Header("Mesh parameters")]
    //The length of a side
    public float size;

    //The number of vertices per side
    public uint res;

    public float heightScale = 1f;

    [Header("Noise parameters")]
    public Vector2 noiseOrigin;
    public float noiseScale;
    public uint harmonics;


    public Transform dropTransform;
    public LineRenderer lr;

    public float wait = 0.5f;


    private MeshFilter meshFilter;
    private Mesh mesh;
    private List<float> map = new List<float>();

    private Simulator simulator;

    private Drop drop;


    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();

        simulator = new Simulator(
            p_inertia: 0.1f,
            p_maxStep: 50,
            p_radius: 1
            );

        mesh = new Mesh { name = "Procedural Terrain" };
        meshFilter.mesh = mesh;

        map = new List<float>();

        drop = new Drop((int)res);

        NoiseGenerator.populateMap(map, res, res, noiseOrigin, noiseScale, harmonics);

        mapMesh(map, mesh, size, res);
    }

    void mapMesh(List<float> map, Mesh mesh, float size, uint res)
    {
        float cellSize = size / (res - 1);

        Vector3[] vertices = new Vector3[res * res];

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                int i = (int)(x + y * res);
                vertices[i] = new Vector3(x * cellSize - size / 2, map[i] * heightScale, y * cellSize - size / 2);
            }
        }

        int[] triangles = new int[6 * (res - 1) * (res - 1)];
        for (int y = 0, i = 0; y < res - 1; y++)
        {
            for (int x = 0; x < res - 1; x++, i++)
            {
                int vertIndex = (int)(x + y * res);

                triangles[6 * i + 0] = vertIndex;
                triangles[6 * i + 1] = vertIndex + (int)res;
                triangles[6 * i + 2] = vertIndex + 1;

                triangles[6 * i + 3] = vertIndex + 1;
                triangles[6 * i + 4] = vertIndex + (int)res;
                triangles[6 * i + 5] = vertIndex + 1 + (int)res;
            }
        }

        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }

    float t = 0;

    void Update()
    {
        if (t > wait)
        {
            simulator.simulateDrop(map, (int)res);

            /*
            Debug.Log(drop.sediment);

            int index = Mathf.RoundToInt(drop.pos.x) + Mathf.RoundToInt(drop.pos.y) * (int)res;

            dropTransform.position = new Vector3((drop.pos.x / res - 0.5f) * size, map[index] * heightScale + 0.1f, (drop.pos.y / res - 0.5f) * size);

            lr.positionCount++;
            lr.SetPosition(lr.positionCount - 1, dropTransform.position);*/

            mapMesh(map, mesh, size, res);


            t = 0;
        }

        t += Time.deltaTime;
    }
}
