using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTunnelMesh : MonoBehaviour
{
    // see README.md file for more information about the following parameters
    public List<Vector3> points = new List<Vector3>();
    public float pipeRadius = 0.02f;
    [Range(3, 32)]
    public int pipeSegments = 8;
    [Range(3, 32)]
    public int elbowSegments = 6;
    public Material pipeMaterial;
    // Start is called before the first frame update
    void Start()
    {

    }

    public void AddPoint(Vector3 point)
    {
        points.Add(point);
    }

    public void RenderPipe()
    {
        if (points.Count < 2)
        {
            throw new System.Exception("Cannot render a pipe with fewer than 2 points");
        }


        // add mesh filter if not present
        MeshFilter currentMeshFilter = GetComponent<MeshFilter>();
        MeshFilter mf = currentMeshFilter != null ? currentMeshFilter : gameObject.AddComponent<MeshFilter>();
        Mesh mesh = GenerateMesh();

        mf.mesh = mesh;

        // add mesh renderer if not present
        MeshRenderer currentMeshRenderer = GetComponent<MeshRenderer>();
        MeshRenderer mr = currentMeshRenderer != null ? currentMeshRenderer : gameObject.AddComponent<MeshRenderer>();
        mr.materials = new Material[1] { pipeMaterial };
    }

    Mesh GenerateMesh()
    {
        Mesh m = new Mesh();
        m.name = "UnityPlumber Pipe";
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        // for each segment, generate a cylinder
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 initialPoint = points[i];
            Vector3 endPoint = points[i + 1];
            Vector3 direction = (points[i + 1] - points[i]).normalized;

            // generate two circles with "pipeSegments" sides each and then
            // connect them to make the cylinder
            GenerateCircleAtPoint(vertices, normals, initialPoint, direction);
            GenerateCircleAtPoint(vertices, normals, endPoint, direction);
            MakeCylinderTriangles(triangles, i);
        }

        m.SetVertices(vertices);
        m.SetTriangles(triangles, 0);
        m.SetNormals(normals);
        return m;
    }

    void GenerateCircleAtPoint(List<Vector3> vertices, List<Vector3> normals, Vector3 center, Vector3 direction)
    {
        // 'direction' is the normal to the plane that contains the circle

        // define a couple of utility variables to build circles
        float twoPi = Mathf.PI * 2;
        float radiansPerSegment = twoPi / pipeSegments;

        // generate two axes that define the plane with normal 'direction'
        // we use a plane to determine which direction we are moving in order
        // to ensure we are always using a left-hand coordinate system
        // otherwise, the triangles will be built in the wrong order and
        // all normals will end up inverted!
        Plane p = new Plane(Vector3.forward, Vector3.zero);
        Vector3 xAxis = Vector3.up;
        Vector3 yAxis = Vector3.right;
        if (p.GetSide(direction))
        {
            yAxis = Vector3.left;
        }

        // build left-hand coordinate system, with orthogonal and normalized axes
        Vector3.OrthoNormalize(ref direction, ref xAxis, ref yAxis);

        for (int i = 0; i < pipeSegments; i++)
        {
            Vector3 currentVertex =
                center +
                (pipeRadius * Mathf.Cos(radiansPerSegment * i) * xAxis) +
                (pipeRadius * Mathf.Sin(radiansPerSegment * i) * yAxis);
            vertices.Add(currentVertex);
            normals.Add((currentVertex - center).normalized);
        }
    }

    void MakeCylinderTriangles(List<int> triangles, int segmentIdx)
    {
        // connect the two circles corresponding to segment segmentIdx of the pipe
        int offset = segmentIdx * pipeSegments * 2;
        for (int i = 0; i < pipeSegments; i++)
        {
            triangles.Add(offset + (i + 1) % pipeSegments);
            triangles.Add(offset + i + pipeSegments);
            triangles.Add(offset + i);

            triangles.Add(offset + (i + 1) % pipeSegments);
            triangles.Add(offset + (i + 1) % pipeSegments + pipeSegments);
            triangles.Add(offset + i + pipeSegments);
        }
    }
}
