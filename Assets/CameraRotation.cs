using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour {

	private float rot_;

	void Start()
	{
		rot_ = 0f;
	}

	void Update()
	{
		var pos = Quaternion.Euler(-1f - Mathf.PerlinNoise(Time.time*0.25f, 0f) * 30f,
								   rot_,
								   0f) * Vector3.forward * (2f + Mathf.PerlinNoise(Time.time*0.2f, 0)*5f);
		transform.position = pos;
		transform.LookAt(Vector3.up * 0.5f);

		rot_ += 20f * Time.deltaTime;
		rot_ = Mathf.Repeat(rot_, 360f);
	}
}
