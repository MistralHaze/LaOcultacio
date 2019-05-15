/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertSystem : MonoBehaviour {

    //ALERTAS POR SONIDO

    public GameObject Player;
    public List<GameObject> Enemies;
    public int EnemyCallRange; //Distancia máxima a la que se puede avisar a un enemigo.

    private Vector3 lastPlayerPosition;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PlayerDetected(GameObject enemyID)
    {
        lastPlayerPosition = Player.transform.position;

        for (int i = 0; i < Enemies.Count; i++)
        {
            if (enemyID != Enemies[i])
            {
                if (Vector3.Distance(enemyID.transform.position, Enemies[i].transform.position) <= EnemyCallRange)
                {
                    //Llamar al pathfinding de Enemies[n]
                }
            }
        }

        //Llamar al PF de enemyID
        //CUESTIÓN: Si varios enemigos llegan al mismo punto, ¿colisionan? ¿generamos puntos diferentes? ¿el último en llegar se da la vuelta...?
         //  L  PROPUESTA CARLES: Crear un area redonda pequeña y dar un punto al azar de ese circulo
        //PROPUESTA 1: GENERAR PUNTOS DE INTERÉS A LOS QUE EL ENEMIGO SE ACERQUE.

    }
}
*/