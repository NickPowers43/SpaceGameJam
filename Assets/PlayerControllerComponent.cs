using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

public class PlayerControllerComponent : MonoBehaviour {

    public float debrisDensity = 1.0f;
    public float minPolygonArea = 0.025f;
    public float moveForce = 1.0f;
    public float jumpForce = 800.0f;
    private Rigidbody2D rb;
    public Camera camera;
    public Animator spriteAnimator;
    public SpriteRenderer spriteRenderer;
    public Transform reticalTransform;

    private bool falling = false;
    private bool flipSprite = false;

    private LayerMask environmentLayer;
    private LayerMask environmentPolygonLayer;
    private LayerMask debrisLayer;

    private Vector2 cutStart;

	// Use this for initialization
	void Start () {
        rb = gameObject.GetComponent<Rigidbody2D>();
        environmentLayer = LayerMask.NameToLayer("environment");
        environmentPolygonLayer = LayerMask.NameToLayer("environmentPolygon");
        debrisLayer = LayerMask.NameToLayer("debris");
    }
	
	// Update is called once per frame
	void Update () {

        Vector2 force = Vector2.zero;

        if(Input.GetKey(KeyCode.D))
        {
            if (!Input.GetKey(KeyCode.A))
            {
                force += Vector2.right;
                spriteRenderer.flipX = false;
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.A))
            {
                force += Vector2.left;
                spriteRenderer.flipX = true;
            }
        }

        force = force.normalized;
        force *= moveForce;

        bool onFloor = Physics2D.OverlapCircle((Vector2)transform.position + (Vector2.down * 0.2f), 0.7f, (1 << environmentLayer) | (1 << environmentPolygonLayer)) != null;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            spriteAnimator.SetTrigger("Jump");
            if(onFloor)
            {
                falling = true;
                force += Vector2.up * jumpForce;
            }
        }

        if (falling && onFloor)
        {
            spriteAnimator.SetTrigger("Landed");
            falling = false;
        }


        rb.AddForce(force);

        Vector3 mousePositionWorld = camera.ScreenToWorldPoint(Input.mousePosition);

        Vector2 vector = mousePositionWorld - transform.position;
        float vectorMag = vector.magnitude;
        float angle = Mathf.Acos(Vector2.Dot(vector * (1.0f / vectorMag), Vector2.right));
        if (vector.y < 0.0f)
            angle *= -1.0f;

        reticalTransform.rotation = Quaternion.Euler(0.0f, 0.0f, angle * Mathf.Rad2Deg);
        reticalTransform.position = transform.position;

        if (Input.GetMouseButtonDown(0))
        {
            cutStart = camera.ScreenToWorldPoint(Input.mousePosition);
            CutThroughSpace(transform.position, mousePositionWorld, 2f);
        }

        camera.transform.position = new Vector3(transform.position.x, transform.position.y, -10.0f);
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

    private Vector2 Right(Vector2 vector)
    {
        return new Vector2(vector.y, -vector.x);
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

    private void SplitEnvironmentPolygon(Ray2D ray, float radius, GameObject go)
    {
        Vector2 right = Right(ray.direction);
        
        Ray2D leftRay = new Ray2D(ray.origin - (right * radius), ray.direction);
        Ray2D rightRay = new Ray2D(ray.origin + (right * radius), -ray.direction);

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
        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
        float originalMass = 1.0f;
        if (rb != null)
        {
            originalMass = go.GetComponent<Rigidbody2D>().mass;
        }

        DestroyImmediate(collider.gameObject);

        Vector2[] leftPoints = ClipPolygon(leftRay.origin, leftRay.direction, worldPoints);
        for (int j = 0; j < leftPoints.Length; j++)
        {
            leftPoints[j] = invPointTransform.MultiplyPoint(leftPoints[j]);
        }
        float leftArea = PolygonArea(leftPoints);
        Vector2[] rightPoints = ClipPolygon(rightRay.origin, rightRay.direction, worldPoints);
        for (int j = 0; j < rightPoints.Length; j++)
        {
            rightPoints[j] = invPointTransform.MultiplyPoint(rightPoints[j]);
        }
        float rightArea = PolygonArea(rightPoints);

        float totalArea = rightArea + leftArea;

        float leftMass = originalMass * leftArea / totalArea;
        float rightMass = originalMass * rightArea / totalArea;

        if (leftPoints.Length > 2 && (leftArea > minPolygonArea))
        {
            GameObject leftGO = new GameObject();
            leftGO.transform.position = spawnPosition;
            leftGO.transform.rotation = spawnRotation;
            PolygonCollider2D leftPC = leftGO.AddComponent<PolygonCollider2D>();
            leftPC.points = leftPoints;
            leftGO.layer = environmentPolygonLayer;
            leftGO.AddComponent<MeshFilter>().mesh = Utility.GenerateMeshForPolygon(leftPoints);
            leftGO.AddComponent<MeshRenderer>().material = material;
            if(rb != null)
            {
                Rigidbody2D leftRB = leftGO.AddComponent<Rigidbody2D>();
                leftRB.mass = leftMass;
            }
        }

        if (rightPoints.Length > 2 && (rightArea > minPolygonArea))
        {
            GameObject rightGO = new GameObject();
            rightGO.transform.position = spawnPosition;
            rightGO.transform.rotation = spawnRotation;
            PolygonCollider2D rightPC = rightGO.AddComponent<PolygonCollider2D>();
            rightPC.points = rightPoints;
            rightGO.layer = environmentPolygonLayer;
            rightGO.AddComponent<MeshFilter>().mesh = Utility.GenerateMeshForPolygon(rightPoints);
            rightGO.AddComponent<MeshRenderer>().material = material;
            if (rb != null)
            {
                Rigidbody2D rightRB = rightGO.AddComponent<Rigidbody2D>();
                rightRB.mass = rightMass;
            }
        }
    }

    private void CutThroughSpace(Vector2 start, Vector2 end, float radius)
    {
        Ray2D ray = new Ray2D(start, end - start);

        //Vector2 vector = end - start;
        //Vector2 middle = (start + end) * 0.5f;
        //float vectorMag = vector.magnitude;
        //float angle = Mathf.Acos(Vector2.Dot(vector * (1.0f / vectorMag), Vector2.right));

        //if (vector.y < 0.0f)
        //    angle *= -1.0f;


        //Collider2D[] colliders = Physics2D.OverlapCapsuleAll(middle, new Vector2(radius * 2.0f, vectorMag), CapsuleDirection2D.Vertical, angle, 1 << environmentPolygonLayer);
        
        //for (int i = 0; i < colliders.Length; i++)
        //{
        //    PolygonCollider2D collider = (PolygonCollider2D)colliders[i];
        //    SplitEnvironmentPolygon(ray, radius, collider.gameObject);
        //}

        RaycastHit2D hit = Physics2D.Linecast(start, end, 1 << environmentPolygonLayer);
        if (hit != null)
        {
            try
            {
                PolygonCollider2D collider = (PolygonCollider2D)hit.collider;
                SplitEnvironmentPolygon(ray, radius, collider.gameObject);
            }
            catch (System.Exception)
            {

                throw;
            }
        }
    }

    
}
