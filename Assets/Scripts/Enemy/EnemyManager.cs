using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour {

    /*PREGUNTAS DESDE CHEMISTÁN:
     * Spawneamos enemigos o son los que están en cada nivel? De uan forma u otra habrá que añadirlos a su lista
     * 
     * */

    public List<EnemyAI> Enemies;
    public float Range;

    public void AlertReceived(GameObject enemyID, Vector3 lastAlertPosition)
    {
        //print(Enemies.Count);

        for (int i = 0; i < Enemies.Count; i++)
        {
            if (enemyID != Enemies[i].gameObject && Vector3.Distance(enemyID.transform.position, Enemies[i].transform.position) < Range)
            {
                print("There's an enemy that can help");
                if (Enemies[i].state != EnemyAI.EnemState.ATTACKING)
                {
                    print("Enemy sent to alert");
                    Enemies[i].ReceiveAlert(lastAlertPosition, false);
                }
            }
        }

    }

    public void playerDetected(EnemyAI enemyID)
    {
        for (int i = 0; i < Enemies.Count; i++)
        {
            if (enemyID != Enemies[i])
            {
                Enemies[i].ReceiveAlert(enemyID.player.transform.position, false);
            }
        }
    }
}
