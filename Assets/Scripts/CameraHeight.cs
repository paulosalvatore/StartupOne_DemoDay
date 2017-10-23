using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHeight : MonoBehaviour {

	float speed = 6f;
	float height = 0.1f;

	Vector3 target;

	void FixedUpdate () {
		if(Input.GetKey(KeyCode.KeypadPlus)){
			target = new Vector3(transform.position.x, transform.position.y + height, transform.position.z);
			transform.position = Vector3.Lerp (transform.position, target, speed * Time.deltaTime);
		}

		if(Input.GetKey(KeyCode.KeypadMinus)){
			target = new Vector3(transform.position.x, transform.position.y - height, transform.position.z);
			transform.position = Vector3.Lerp (transform.position, target, speed * Time.deltaTime);
		}
	}
}
