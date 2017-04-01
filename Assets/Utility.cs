using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

public class Utility
{
    public static Mesh GenerateMeshForPolygon(Vector2[] polygon)
    {
        Mesh output = new Mesh();

        //create triangle fan
        Vector3[] vertices = new Vector3[polygon.Length];
        for (int i = 0; i < polygon.Length; i++)
        {
            vertices[i] = polygon[i];
        }

        int[] triangles = new int[(polygon.Length - 2) * 3];
        int indexIndex = 0;
        for (int i = 0; i < polygon.Length - 2; i++)
        {
            triangles[indexIndex++] = 0;
            triangles[indexIndex++] = i + 2;
            triangles[indexIndex++] = i + 1;
        }

        output.vertices = vertices;
        output.uv = polygon;
        output.triangles = triangles;

        return output;
    }
}
