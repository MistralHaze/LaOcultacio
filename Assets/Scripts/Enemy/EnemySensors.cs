using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Visibility2D;

public class EnemySensors : MonoBehaviour {

    //Scripts de Raúl^2 y Andoni 🌲

    public Transform player;
    public EnemyAI aI;
    public ShadowCasting2D lightCone;
    public float hearingRange = 3f;

    PolygonCollider2D lightCollider;

    // Use this for initialization
    void Start()
    {
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Ignore Raycast"), LayerMask.NameToLayer("Rooms"), true);
        lightCollider = gameObject.AddComponent<PolygonCollider2D>();
        lightCollider.isTrigger = true;
    }

    // Update is called once per frame
    void Update()
    {
        //Add the light mesh points to the Polygon Collider
        lightCollider.points = lightCone.getLightPoints();

        //Debug.Log(Vector3.Distance(player.position, transform.position) <= hearingRange && !Input.GetKey(KeyCode.LeftShift) && player.gameObject.GetComponent<PlayerMovement>().IsMoving);

        //Noise
        if (Vector3.Distance(player.position, transform.position) <= hearingRange && !Input.GetKey(KeyCode.LeftShift) && player.gameObject.GetComponent<PlayerMovement>().IsMoving) //Si el jugador está en rango Y corriendo
        {
            aI.ReceiveAlert(player.position);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {        
        if (other.tag == "Player" && !other.GetComponent<PlayerMovement>().isHidden())
        {
            //print("Light - Player collision");
            //alertSystem.PlayerDetected(this.transform.parent.gameObject);
            aI.playerInsideVision = true;
            aI.EnemyInSight();  
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player" && !other.GetComponent<PlayerMovement>().isHidden())
        {
            aI.playerInsideVision = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, hearingRange);
    }


}
