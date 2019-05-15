using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RaycastCorrector : MonoBehaviour {

    //JM: En vez de variar directamente la posición, debería variar la velocidad como una subrutina, probablemente de EnemySensors, pedir permiso

    public bool showRaycastGizmos;
    public int numRaycasts; //Number of raycasts. Min 6 raycasts.
    public float maxAngle;  //Max angle for the lateral raycasts. Between 1 and 90.
    public float powerProportion;

	void Awake () {

        if (numRaycasts % 2 == 0) numRaycasts += 1; //Si el número es par sumamos uno para que tenga un rayo delante y los demás separados equitativamente en los lados.

	}
	
	void FixedUpdate () {

        //Si se quiere evitar este algoritmo de Raycasting en alguno de los estados, se arregla fácilmente poniendo 
        //una condición de que no se realice si el enemigo está en ese estado.

        //Estas 2 líneas antes del for se pueden poner en el Start, pero de momento está aquí para poder debuggear más rápido.
        float step = maxAngle / (numRaycasts / 2 - 1);         //Compute step angle between raycasts.
        float angle = -maxAngle - step;                       //Compute initial angle.
        for (int i = 0; i < numRaycasts; i++)
        {
            if (i != 0) angle += step; 

            //Compute vector of the raycast.
            Vector2 forward = this.transform.TransformDirection(Vector2.right);
            Vector2 vector = Quaternion.AngleAxis(angle, Vector3.forward) * forward;
      
            //Point where it collides
            RaycastHit2D hit = Physics2D.Raycast(this.transform.position, vector);


            if (hit.collider != null)
            {
                //Show raycast line in editor.
                Vector2 dir = (hit.point - (Vector2)this.transform.position).normalized;
                if(showRaycastGizmos) Debug.DrawRay(hit.point, (Vector2)this.transform.position - hit.point, Color.yellow);

                //Compute distance from player to hit point.
                float distance = Vector2.Distance(hit.point, this.transform.position);

                //float proportion = powerProportion / distance;

                if(Mathf.Abs(distance) < 1.5f) //Si la distancia es menor que 1.5 (para que no entorpezcan otros raycasts)
                {
                        
                    Vector2 reflex = new Vector2(-dir.x, -dir.y).normalized;            //Obtenemos el vector reflejo respecto a la perpendicular.
                    Vector3 auxOffset = reflex * (1.5f - distance) * powerProportion;   //Cuanto menor sea la distancia a la pared, mayor será el offset.
                    this.transform.position += auxOffset;                               //Sumamos el offset al enemigo.

                }
                
            }

        }

    }
}
