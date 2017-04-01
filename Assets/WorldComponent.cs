using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldComponent : MonoBehaviour {

    public GameObject floorPrefab;
    public GameObject wallPrefab;

    public PlayerControllerComponent player;

	// Use this for initialization
	void Start () {

        const int BLOCK_COUNT = 10;

        for (int i = -BLOCK_COUNT; i < BLOCK_COUNT; i++)
        {
            for (int j = -BLOCK_COUNT; j < BLOCK_COUNT; j++)
            {
                Vector3 position = new Vector3(j * 10.0f, i * 10.0f, 0.0f);
                GameObject.Instantiate(floorPrefab, position, Quaternion.identity);
                GameObject.Instantiate(wallPrefab, position, Quaternion.identity);
            }
        }

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
