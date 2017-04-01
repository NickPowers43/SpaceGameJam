using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshFromColliderComponent : MonoBehaviour {

    public enum PolygonType : int
    {
        wall = 0,
        floor = 1
    }

    private static Mesh[] MESHES = new Mesh[2];

    public PolygonType type;

	// Use this for initialization
	void Start () {
        
        if (MESHES[(int)type] == null)
        {
            PolygonCollider2D collider = GetComponent<PolygonCollider2D>();
            MESHES[(int)type] = Utility.GenerateMeshForPolygon(collider.points);
        }

        MeshFilter mf = GetComponent<MeshFilter>();
        mf.mesh = MESHES[(int)type];
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
