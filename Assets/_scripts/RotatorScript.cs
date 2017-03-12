using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatorScript : MonoBehaviour {

    [Range(0,50)]
    public int rotationSpeed;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float translation = Time.deltaTime * rotationSpeed;
        transform.Rotate(new Vector3(0, translation, 0));
	}
}
