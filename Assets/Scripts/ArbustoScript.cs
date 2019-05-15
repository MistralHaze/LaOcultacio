using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArbustoScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
           // print("ENTRANDO");
            other.gameObject.GetComponent<PlayerMovement>().playerHidden();
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            //print("SALIENDO");
            other.gameObject.GetComponent<PlayerMovement>().playerNotHidden();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        //Gizmos.DrawWireSphere(transform.position,1f);
    }
}
