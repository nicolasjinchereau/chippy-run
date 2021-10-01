using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlarePosition : MonoBehaviour {

    public Transform target;

    void Start() {
        Update();
	}
	
	void Update() {
        transform.position = new Vector3(30, target.position.y + 100, 30);
	}
}
