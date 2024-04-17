using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEditor.UIElements;
using UnityEngine;
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
class triangle
{
    Vector3 p1;
    Vector3 p2;
    Vector3 p3;
    public triangle(Vector3 cp1, Vector3 cp2, Vector3 cp3)
    {
        p1 = cp1;
        p2 = cp2;
        p3 = cp3;
    }
}
[ExecuteInEditMode]
public class proceduralPlanet : MonoBehaviour
{

    [SerializeField]
    Gradient planetColors;
    [SerializeField]
    float[] xyzDispacement = new float[3];
    [SerializeField]
    float[] fractalDisplacement = new float[5];
    [SerializeField]
    float[] frequency = new float[5] { 1, 1, 1, 1, 1 };
    [SerializeField]
    float[] amplitude = new float[5] { 0, 0, 0, 0, 0 };
    [SerializeField]
    float mountainAmplitude = 1;
    [SerializeField]
    float mountainFrequency = 1;
    [SerializeField]
    float mountainDisplacement = 1;
    [SerializeField]
    int moutainRoughness = 1;
    [SerializeField]
    int radius = 1;
    [SerializeField]
    int subdivisions = 5;
    MeshFilter meshf;
    MeshRenderer meshr;
    void OnValidate()
    {
        GameObject planet = this.gameObject;
        meshf = GetComponent<MeshFilter>();
        meshr = GetComponent<MeshRenderer>();
        Mesh mesh = newPlanet();
        meshf.mesh = mesh;
    }
    void Start()
    {
        //!!!PROTECTION FROM TOO MANY TRIANGLES!!!
        //if (subdivisions > 150) { subdivisions = 150; Debug.LogError("Too many triangles, changed subdivisions to 150 for consistency"); }

    }
    int findVector3InArray(Vector3[] a, Vector3 element)
    {
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] == element)
            {
                return i;
            }
        }
        return -1;
    }
    Mesh newPlanet()
    {
        var mesh = new Mesh
        {
            name = "Procedural Mesh"
        };

        //shape
        Vector3[] vertices = new Vector3[] {
        Vector3.left, Vector3.right, Vector3.up, Vector3.down, Vector3.back, Vector3.forward
    };
        List<Vector3> vertices1 = new List<Vector3>();
        int[] triangles = new int[] {
        0,5,2,1,2,5,0,3,5,1,5,3,0,2,4,1,4,2,0,4,3,1,3,4
    };
        List<int> triangles1 = new List<int>();
        //triangulate the shape to a sphere
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];
            for (int j = 0; j <= subdivisions; j++)
            {
                for (int k = 0; k <= subdivisions; k++)
                {
                    if (j + k > subdivisions) { continue; }
                    else
                    {
                        vertices1.Add(((p3 - p1) * k / subdivisions + (p2 - p1) * j / subdivisions + p1).normalized);
                    }
                }
            }
            int newVerticesCount = (subdivisions + 1) * (subdivisions + 2) / 2;
            //initialize triangles facing the same direction as parent triangles
            int trianglescount = (subdivisions * subdivisions) * 3;
            int[] trianglesdispositioned = new int[trianglescount];
            int currentPoint = 0;
            int currentTriangle = 0;
            for (int j = 0; j <= subdivisions; j++)
            {
                for (int k = 0; k <= subdivisions; k++)
                {
                    if (j + k > subdivisions)
                    {
                        continue;
                    }
                    if (j + k == subdivisions)
                    {
                        currentPoint += 1;
                        continue;
                    }
                    else
                    {
                        trianglesdispositioned[currentTriangle * 3] = currentPoint;
                        trianglesdispositioned[currentTriangle * 3 + 1] = currentPoint + subdivisions - j + 1;
                        trianglesdispositioned[currentTriangle * 3 + 2] = currentPoint + 1;
                        currentPoint += 1;
                        currentTriangle += 1;
                    }
                }
            }
            //initialize triangles facing the opposite direction of parent triangle
            int startingPoint = currentTriangle * 3;
            currentTriangle = 0;
            currentPoint = 0;
            for (int j = 0; j <= subdivisions; j++)
            {
                for (int k = 0; k <= subdivisions; k++)
                {
                    if (j + k > subdivisions)
                    {
                        continue;
                    }
                    if (k == 0 || j + k == subdivisions)
                    {
                        currentPoint += 1;
                        continue;
                    }
                    trianglesdispositioned[startingPoint + currentTriangle * 3] = currentPoint;
                    trianglesdispositioned[startingPoint + currentTriangle * 3 + 1] = currentPoint + subdivisions - j;
                    trianglesdispositioned[startingPoint + currentTriangle * 3 + 2] = currentPoint + subdivisions - j + 1;
                    currentPoint += 1;
                    currentTriangle += 1;
                }
            }
            for (int j = 0; j < trianglesdispositioned.Length; j++) { trianglesdispositioned[j] += vertices1.Count - newVerticesCount; }
            triangles1.AddRange(trianglesdispositioned);
        }








        vertices = vertices1.ToArray();
        triangles = triangles1.ToArray();
        //creating hills and seas
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] *= radius;
            for (int j = 0; j < 5; j++)
            {
                float vx = vertices[i].normalized.x;
                float vy = vertices[i].normalized.y;
                float vz = vertices[i].normalized.z;
                float sx;
                float sy;
                float sz;
                if (vx == 0) { sx = 1; } else { sx = Mathf.Sign(vx); }
                if (vy == 0) { sy = 1; } else { sy = Mathf.Sign(vy); }
                if (vz == 0) { sz = 1; } else { sz = Mathf.Sign(vz); }
                float dvx = fractalDisplacement[j] + xyzDispacement[0];
                float dvy = fractalDisplacement[j] + xyzDispacement[1];
                float dvz = fractalDisplacement[j] + xyzDispacement[2];
                vertices[i] += vertices[i].normalized * (1 + (0
                + (Mathf.PerlinNoise(vx * frequency[j] + dvx, vy * sz * frequency[j] + dvy) - 0.5f) * Mathf.Abs(vz)
                + (Mathf.PerlinNoise(vz * frequency[j] + dvz, vx * sy * frequency[j] + dvx) - 0.5f) * Mathf.Abs(vy)
                + (Mathf.PerlinNoise(vy * frequency[j] + dvy, vz * sx * frequency[j] + dvz) - 0.5f) * Mathf.Abs(vx)
                ) * amplitude[j]);
            }
        }


        //creating mountains

        for (int i = 0; i < vertices.Length; i++)
        {
            float vx = vertices[i].normalized.x;
            float vy = vertices[i].normalized.y;
            float vz = vertices[i].normalized.z;
            float sx;
            float sy;
            float sz;
            if (vx == 0) { sx = 1; } else { sx = Mathf.Sign(vx); }
            if (vy == 0) { sy = 1; } else { sy = Mathf.Sign(vy); }
            if (vz == 0) { sz = 1; } else { sz = Mathf.Sign(vz); }
            vertices[i] += vertices[i].normalized * (1 + (0
            + Mathf.Pow(Mathf.PerlinNoise(vx * mountainFrequency + mountainDisplacement, vy * sz * mountainFrequency + mountainDisplacement),moutainRoughness) * Mathf.Abs(vz)
            + Mathf.Pow(Mathf.PerlinNoise(vz * mountainFrequency + mountainDisplacement, vx * sy * mountainFrequency + mountainDisplacement),moutainRoughness) * Mathf.Abs(vy)
            + Mathf.Pow(Mathf.PerlinNoise(vy * mountainFrequency + mountainDisplacement, vz * sx * mountainFrequency + mountainDisplacement),moutainRoughness) * Mathf.Abs(vx)
            ) * mountainAmplitude);
        }


        //assigning colors.
        Color[] colors = new Color[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i] = planetColors.Evaluate(vertices[i].magnitude / 2 / radius);
        }

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        // assign the array of colors to the Mesh.
        mesh.colors = colors;
        mesh.RecalculateNormals();
        return mesh;
    }

    void Update()
    {

    }
}