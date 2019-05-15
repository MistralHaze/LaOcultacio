/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseDetector : MonoBehaviour {

    public GameObject Player;
    public AlertSystem alertSystem;
    public float Range;

	// Update is called once per frame
	void Update () {
		if (Vector3.Distance(Player.transform.position, this.transform.position) <= Range && Input.GetKey(KeyCode.LeftShift))
        {
            alertSystem.PlayerDetected(this.gameObject);
        }
	}
}
*/