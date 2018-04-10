using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteConstraints : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    // Update is called once per frame
    void Update()
    {
        Vector3 myRotation = transform.rotation.eulerAngles;
        myRotation.x = 0;
        myRotation.y = 180;
        transform.rotation = Quaternion.Euler(myRotation);
    }
}


