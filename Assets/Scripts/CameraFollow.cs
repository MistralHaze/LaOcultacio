using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent (typeof(Camera))]
public class CameraFollow : MonoBehaviour {

    public Transform objective;
    public float easingSpeed = 3f;

    float originalZ;

    private void Awake()
    {
        originalZ = transform.position.z;
    }

    void Update () {
        transform.position = Vector3.Lerp(transform.position, objective.position, easingSpeed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, transform.position.y, originalZ);
	}
}
