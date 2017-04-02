using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxControllerComponent : MonoBehaviour {

    public Transform background0;
    public Transform background1;
    public Transform background2;

    public float scale = 0.01f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        background1.localPosition = new Vector3(scale * -transform.position.x, 0.0f, 10.0f);
        background2.localPosition = new Vector3(scale * scale * -transform.position.x, 0.0f, 10.0f);
    }
}
