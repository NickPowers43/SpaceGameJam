using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormControllerComponent : MonoBehaviour {

    public GameObject headPrefab;
    public GameObject middlePrefab;
    public PlayerControllerComponent player;
    public float speed = 0.75f;
    public float bodySpacing = 0.5f;

    private GameObject head;
    private GameObject[] bodySegments = new GameObject[100];
    

	// Use this for initialization
	void Start () {
        head = Instantiate(headPrefab, transform);

        for (int i = 0; i < bodySegments.Length; i++)
        {
            bodySegments[i] = Instantiate(middlePrefab, head.transform.position + (Vector3.left * i), head.transform.rotation);
        }
	}
	
	// Update is called once per frame
	void Update () {
        
        for (int i = bodySegments.Length - 1; i > 0; i--)
        {
            Vector3 diff = bodySegments[i].transform.position - bodySegments[i - 1].transform.position;
            float distance = diff.magnitude;
            if (distance > 0.0f)
            {
                diff *= bodySpacing / distance;
                bodySegments[i].transform.position = bodySegments[i - 1].transform.position + diff;
                float angle = Mathf.Atan2(diff.y, diff.x);
                bodySegments[i].transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle);
            }
        }

        {
            Vector3 diff = bodySegments[0].transform.position - head.transform.position;
            float distance = diff.magnitude;
            if (distance > 0.0f)
            {
                diff *= bodySpacing / distance;
                bodySegments[0].transform.position = head.transform.position + diff;
                bodySegments[0].transform.LookAt(head.transform);
            }
        }
        
        {
            Vector3 towardsTarget = player.transform.position - head.transform.position;
            Vector3 facingVector = head.transform.TransformVector(Vector3.right);
            Vector3 headUp = Vector3.Cross(facingVector, Vector3.back);
            float dot = Vector3.Dot(towardsTarget, facingVector);
            //float angleDiff = Vector3.Dot(facingVector, )
            head.transform.rotation = Quaternion.Euler(head.transform.rotation.eulerAngles + new Vector3());

            //move head forward
            Vector3 heading = head.transform.rotation.eulerAngles;
            Vector3 diff = new Vector3(Mathf.Cos(heading.z), Mathf.Sin(heading.z), 0.0f);
            diff *= speed * Time.deltaTime;

            head.transform.position += diff; 
        }
    }
}
