using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

public class PlayerControllerComponent : MonoBehaviour {

    public float minPolygonArea = 0.025f;
    public float moveForce = 1.0f;
    public float jumpForce = 800.0f;
    private Rigidbody2D rb;
    public Camera camera;

    private LayerMask environmentLayer;
    private LayerMask environmentPolygonLayer;

    private Vector2 cutStart;

	// Use this for initialization
	void Start () {
        rb = gameObject.GetComponent<Rigidbody2D>();
        environmentLayer = LayerMask.NameToLayer("environment");
        environmentPolygonLayer = LayerMask.NameToLayer("environmentPolygon");
    }
	
	// Update is called once per frame
	void Update () {

        Vector2 force = Vector2.zero;

        if(Input.GetKey(KeyCode.D))
        {
            if (!Input.GetKey(KeyCode.A))
            {
                force += Vector2.right;
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.A))
            {
                force += Vector2.left;
            }
        }

        force = force.normalized;
        force *= moveForce;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Collider2D floor = Physics2D.OverlapCircle((Vector2)transform.position + (Vector2.down * 0.2f), 0.7f, 1 << environmentLayer);
            if(floor != null)
            {
                force += Vector2.up * jumpForce;
            }
        }


        rb.AddForce(force);

        if(Input.GetMouseButtonDown(0))
        {
            cutStart = camera.ScreenToWorldPoint(Input.mousePosition);
        } else if (Input.GetMouseButtonUp(0))
        {
            Vector2 cutEnd = camera.ScreenToWorldPoint(Input.mousePosition);

            CutThroughSpace(cutStart, cutEnd);
        }

    }

    private Vector2 LineIntersectionPoint(Vector2 ps1, Vector2 pe1, Vector2 ps2, Vector2 pe2)
    {
        // Get A,B,C of first line - points : ps1 to pe1
        float A1 = pe1.y - ps1.y;
        float B1 = ps1.x - pe1.x;
        float C1 = A1 * ps1.x + B1 * ps1.y;

        // Get A,B,C of second line - points : ps2 to pe2
        float A2 = pe2.y - ps2.y;
        float B2 = ps2.x - pe2.x;
        float C2 = A2 * ps2.x + B2 * ps2.y;

        // Get delta and check if the lines are parallel
        float delta = A1 * B2 - A2 * B1;
        if (delta == 0)
            throw new System.Exception("Lines are parallel");

        // now return the Vector2 intersection point
        return new Vector2(
            (B2 * C1 - B1 * C2) / delta,
            (A1 * C2 - A2 * C1) / delta
        );
    }

    private bool RightOf(Vector2 rayOrigin, Vector2 rayDirection, Vector2 point)
    {
        return Vector2.Dot(point - rayOrigin, new Vector2(rayDirection.y, -rayDirection.x)) > 0.0f;
    }

    Vector2[] ClipPolygon(Vector2 clipOrigin, Vector2 clipDirection, Vector2[] subject)
    {
        List<Vector2> output = new List<Vector2>();

        Vector2 prevPoint = subject[subject.Length - 1];
        bool prevPointInside = !RightOf(clipOrigin, clipDirection, prevPoint);
        for (int i = 0; i < subject.Length; i++)
        {
            Vector2 currPoint = subject[i];
            bool currPointInside = !RightOf(clipOrigin, clipDirection, currPoint);

            if (!prevPointInside && currPointInside)
            {
                //stepping into allowed space
                Vector2 intersection = LineIntersectionPoint(clipOrigin, clipOrigin + clipDirection, prevPoint, currPoint);
                output.Add(intersection);
                output.Add(currPoint);
            }
            else if (prevPointInside && !currPointInside)
            {
                //stepping into not allowed space
                Vector2 intersection = LineIntersectionPoint(clipOrigin, clipOrigin + clipDirection, prevPoint, currPoint);
                output.Add(prevPoint);
                output.Add(intersection);
            }
            else if (currPointInside)
            {
                output.Add(currPoint);
            }

            prevPoint = currPoint;
            prevPointInside = currPointInside;
        }

        return output.ToArray();
    }

    float Cross2D(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    private float PolygonArea(Vector2[] points)
    {
        if (points.Length < 3)
        {
            return 0.0f;
        }

        float output = 0.0f;

        Vector2 a = points[0];

        Vector2 prevEdgeVector = points[1] - a;
        for (int i = 2; i < points.Length; i++)
        {
            Vector2 currEdgeVector = points[i] - a;

            output += Cross2D(prevEdgeVector, currEdgeVector);

            prevEdgeVector = currEdgeVector;
        }

        return output;
    }

    private Mesh GenerateMeshForPolygon(Vector2[] polygon)
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

    private void SplitEnvironmentPolygon(Ray2D ray, GameObject go)
    {
        Ray2D minusRay = new Ray2D(ray.origin, -ray.direction);

        PolygonCollider2D collider = go.GetComponent<PolygonCollider2D>();

        Vector2[] points = collider.points;
        Vector2[] worldPoints = new Vector2[points.Length];
        Matrix4x4 pointTransform = collider.transform.localToWorldMatrix;
        Matrix4x4 invPointTransform = collider.transform.worldToLocalMatrix;
        for (int j = 0; j < points.Length; j++)
        {
            worldPoints[j] = pointTransform.MultiplyPoint(points[j]);
        }

        Vector3 spawnPosition = collider.transform.position;
        Quaternion spawnRotation = collider.transform.rotation;
        Material material = go.GetComponent<MeshRenderer>().material;

        DestroyImmediate(collider.gameObject);

        Vector2[] leftPoints = ClipPolygon(ray.origin, ray.direction, worldPoints);
        for (int j = 0; j < leftPoints.Length; j++)
        {
            leftPoints[j] = invPointTransform.MultiplyPoint(leftPoints[j]);
        }
        float leftArea = PolygonArea(leftPoints);
        Vector2[] rightPoints = ClipPolygon(minusRay.origin, minusRay.direction, worldPoints);
        for (int j = 0; j < rightPoints.Length; j++)
        {
            rightPoints[j] = invPointTransform.MultiplyPoint(rightPoints[j]);
        }
        float rightArea = PolygonArea(rightPoints);

        float totalArea = rightArea + leftArea;

        float leftMass = leftArea / totalArea;
        float rightMass = rightArea / totalArea;

        if (leftPoints.Length > 2 && (leftArea > minPolygonArea))
        {
            GameObject leftGO = new GameObject();
            leftGO.transform.position = spawnPosition;
            leftGO.transform.rotation = spawnRotation;
            PolygonCollider2D leftPC = leftGO.AddComponent<PolygonCollider2D>();
            leftPC.points = leftPoints;
            leftGO.AddComponent<Rigidbody2D>();
            leftGO.layer = environmentPolygonLayer;
            leftGO.AddComponent<MeshFilter>().mesh = GenerateMeshForPolygon(leftPoints);
            leftGO.AddComponent<MeshRenderer>().material = material;
        }

        if (rightPoints.Length > 2 && (rightArea > minPolygonArea))
        {
            GameObject rightGO = new GameObject();
            rightGO.transform.position = spawnPosition;
            rightGO.transform.rotation = spawnRotation;
            PolygonCollider2D rightPC = rightGO.AddComponent<PolygonCollider2D>();
            rightPC.points = rightPoints;
            rightGO.AddComponent<Rigidbody2D>();
            rightGO.layer = environmentPolygonLayer;
            rightGO.AddComponent<MeshFilter>().mesh = GenerateMeshForPolygon(rightPoints);
            rightGO.AddComponent<MeshRenderer>().material = material;
        }
    }

    private void CutThroughSpace(Vector2 start, Vector2 end)
    {
        RaycastHit2D[] hits = Physics2D.LinecastAll(start, end, 1 << environmentPolygonLayer);

        Ray2D ray = new Ray2D(start, end - start);

        for (int i = 0; i < hits.Length; i++)
        {
            PolygonCollider2D collider = (PolygonCollider2D)hits[i].collider;

            SplitEnvironmentPolygon(ray, collider.gameObject);
        }
    }
}
