using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    /*COSAS DEL FALTAR
     * 
     */
    public enum EnemState { WANDERING, ALERTED, INVESTIGATING, ATTACKING }

    //public Color wanderColor, alertColor, attackColor;

    public GameObject gameManager;
    public EnemyManager enemyManager;
    public GameObject player;
    public SteeringBehaviour wanderingBeh;
    public EnemyShooting attackBeh;
    public PathFinding pathFinder;
    public Visibility2D.ShadowCasting2D vision;
    public EnemState state = EnemState.WANDERING;
    public float alertRange = 3f,
        investigationTime = 3f,
        nodeArrivalDist = 1f,//RECOMENDACIÓN: que sea siempre mayor que la distancia de frenado del steering behaviour de wandering
        lostAttackTime = 1.5f,
        combatDetectionDistanceMultiplier = 1.3f
        ;
    public List<Transform> defaultRouteNodes;
    public Vector2 speed;
    public bool playerInsideVision = false;

    //Investigating stuff
    public List<ListWrapper> rooms;

    public Sprite wanderingSprite, alertedSprite, investSprite, attackSprite;

    Vector2 lastAlertPosition = Vector2.zero/*, nextRouteNode = Vector2.zero*/;
    List<Vector2> alertPath;
    int nodeIndex = 0;
    SpriteRenderer spriteRend;
    //Transform nextNode;

    Vector3 currentSpeed = Vector3.zero;//TODO: Tenemos la velocidad mezclada entre scripts, hay que unificar
    float timer = 0f;
    float firstTimer = 0f;
    bool returning = false;
    public string lastRoom = "";
    public string currentRoom = "";
    private bool pointChosen = false;
    private bool sameRoomTrigger = false; //Por si entramos en un trigger de la misma habitación

    // Use this for initialization
    void Awake()
    {
        alertPath = new List<Vector2>();
        spriteRend = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.GetComponent<GameManager>().gameRunning)
            return;

        switch (state)
        {
            case EnemState.WANDERING:
                //Hace wandering por los puntos de su RUTA PREDEFINIDA, cambiando nextNode paulatinamente
                //ESTADO POR DEFECTO
                nodeIndex = wanderingBeh.calculatePosition(defaultRouteNodes, this, nodeIndex);

                break;

            case EnemState.ALERTED:
                /* Sigue el camino con steering behaviour de seguir ruta que evitan obstáculos
                 * Si estamos cerca del final del camino cambia de modo a Investigating
                 */
                wanderingBeh.FollowPath(alertPath, this);

                if (alertPath == null)
                {

                    //Calcular camino a jugador UNA SOLA VEZ en alertPath. Si hay una nueva alerta el camino se recalculará con una nueva llamada a esta función
                    alertPath = pathFinder.FindPathToBlocked(transform.position, lastAlertPosition, this.transform, player.transform);

                    lastAlertPosition = alertPath[alertPath.Count - 1];
                }

                if (Vector2.Distance(lastAlertPosition, transform.position) < nodeArrivalDist)
                {
                    spriteRend.sprite = investSprite;
                    state = EnemState.INVESTIGATING;
                }
                break;//Fin ALERTED

            case EnemState.INVESTIGATING:

                //Wander alrededor del punto de interés durante cierto tiempo

                //AQUÍ HAY QUE IR A LOS PUNTOS DE INTERÉS
                firstTimer += Time.deltaTime;
                if (!pointChosen && firstTimer > 1)
                {
                    pointChosen = true;
                    chooseInvesigatingPoint();
                }
                else
                {
                    if (alertPath == null || alertPath.Count == 0 || Vector3.Distance(transform.position, alertPath[alertPath.Count - 1]) < 0.5f)
                    {
                        timer += Time.deltaTime;
                    }
                    else
                    {
                        wanderingBeh.FollowPath(alertPath, this);
                    }
                }

                if (!returning && timer > investigationTime)
                {
                    //Si ha tenido suficiente investigación debemos hacer el camino inverso con su siguiente nodo de ruta como destino
                    ReturnToWandering();
                }
                else if (returning)
                {
                    /* Sigue el camino con steering behaviour de seguir ruta que evitan obstáculos
                    * Si estamos cerca del nodo destino cambia de modo a WANDERING
                    */

                    wanderingBeh.FollowPath(alertPath, this);
                    if (alertPath == null || Vector2.Distance(lastAlertPosition, transform.position) < nodeArrivalDist)
                    {
                        timer = 0f;
                        firstTimer = 0f;
                        pointChosen = false;
                        returning = false;
                        spriteRend.sprite = wanderingSprite;
                        state = EnemState.WANDERING;
                    }
                }
                break;//Fin INVESTIGATING

            case EnemState.ATTACKING:

                //Ataca si puede al jugador. Si le pierde de vista, vuelve a ALERTED

                attackBeh.AttackBehavior(player);

                Vector2 distance = player.transform.position - transform.position;

                RaycastHit2D rayHit;
                rayHit = Physics2D.Raycast(transform.position, distance.normalized, vision.visionDistance * combatDetectionDistanceMultiplier);

                //Debug.Log((distance.magnitude > vision.visionDistance * combatDetectionDistanceMultiplier) + " y " + (rayHit.collider==null ) + " y " +(rayHit.collider.tag != "Player"));

                if (/*distance.magnitude > vision.visionDistance * combatDetectionDistanceMultiplier*/ /*|| rayHit.collider == null|| rayHit.collider.tag != "Player" */ !playerInsideVision)
                {
                    timer += Time.deltaTime;
                    if (timer > lostAttackTime)
                    {
                        returning = false;
                        timer = 0f;
                        spriteRend.sprite = alertedSprite;
                        state = EnemState.ALERTED;
                        //Calcular ruta nueva
                        lastAlertPosition = player.transform.position;
                        alertPath = pathFinder.FindPathToBlocked(transform.position, player.transform.position, this.transform, player.transform);
                        Debug.Log("Salimos del combate");
                    }
                }
                else
                {
                    timer = 0f;
                }

                break;

            default://Jamás debería llegar a este estado por lógica pero porsiaca
                print("Hemos llegado al default, algo esta mal");
                spriteRend.sprite = wanderingSprite;
                state = EnemState.WANDERING;
                break;
        }
    }

    public void EnemyInSight(bool callTheCops = true)
    {
        timer = 0;        
        spriteRend.sprite = attackSprite;
        state = EnemState.ATTACKING;
        if (callTheCops)
        {
            enemyManager.playerDetected(this);
        }
    }

    public void ReceiveAlert(Vector3 point, bool callTheCops = true)
    {
        if (state == EnemState.WANDERING || state == EnemState.INVESTIGATING || state == EnemState.ALERTED)
        {


            if (player.GetComponent<PlayerMovement>().isHidden())
            {

                lastAlertPosition = point;
                //print("vamos a buscar la ruta");

                //Calcular camino a jugador UNA SOLA VEZ en alertPath. Si hay una nueva alerta el camino se recalculará con una nueva llamada a esta función
                alertPath = pathFinder.FindPathToBlocked(transform.position, point, this.transform, player.transform);
                print("hemos cambiado el alertpath");
                spriteRend.sprite = alertedSprite;
                state = EnemState.ALERTED;
                //print("receivealert ha terminado bien");
            }
            else
            {
                //if (Vector2.Distance(point, transform.position) > alertRange) return;

                lastAlertPosition = point;

                //Calcular camino a jugador UNA SOLA VEZ en alertPath. Si hay una nueva alerta el camino se recalculará con una nueva llamada a esta función
                alertPath = pathFinder.FindPathToBlocked(transform.position, point, this.transform, player.transform);

                spriteRend.sprite = alertedSprite;
                state = EnemState.ALERTED;
            }

        }

        if (callTheCops)
        {
            enemyManager.AlertReceived(this.gameObject, lastAlertPosition);
        }

        /*
        lastAlertPosition = point;

        //Calcular camino a jugador UNA SOLA VEZ en alertPath. Si hay una nueva alerta el camino se recalculará con una nueva llamada a esta función
        alertPath = pathFinder.FindPath(transform.position, point, this.transform, player.transform);

        spriteRend.sprite = alertedSprite;
        state = EnemState.ALERTED;*/
    }

    void ReturnToWandering()
    {
        //Calcular camino a siguiente nodo UNA SOLA VEZ en alertPath
        alertPath = pathFinder.FindPathToBlocked(transform.position, defaultRouteNodes[nodeIndex].position, this.transform, player.transform);
        lastAlertPosition = defaultRouteNodes[nodeIndex].position;
        returning = true;
    }

    void chooseInvesigatingPoint()
    {
        Vector3 puntoMasCercano = Vector3.zero;
        float distanciaAPunto = Mathf.Infinity;
        //Primer for: buscar en qué habitación estamos
        for (int i = 0; i < rooms.Count; i++)
        {
            if (currentRoom == rooms[i].nestedList[0].name)
            {
                //Buscar puntos de interés en habitaciones contiguas
                for (int j = 1; j < rooms[i].nestedList.Count; j++)
                {
                    //Solo elegimos el punto de la hab cont por la que no hemos venido
                    if (lastRoom != rooms[i].nestedList[j].tag)
                    {
                        if (Vector3.Distance(transform.position, rooms[i].nestedList[j].transform.position) < distanciaAPunto)
                        {
                            puntoMasCercano = rooms[i].nestedList[j].transform.position;
                            distanciaAPunto = Vector3.Distance(transform.position, rooms[i].nestedList[j].transform.position);
                        }
                    }
                }
                break;
            }
        }

        lastAlertPosition = puntoMasCercano;
        alertPath = pathFinder.FindPathToBlocked(this.transform.position, lastAlertPosition, this.transform, player.transform);

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (currentRoom != other.name && other.tag == "Room")
        {            
            lastRoom = currentRoom;
            currentRoom = other.name;
        }
        else if (currentRoom == other.name && other.tag == "Room")
        {
            print("saliendo de la misma habitación");
            sameRoomTrigger = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Room")
        {
            if (currentRoom == collision.name && !sameRoomTrigger)
            {
                //print("exit: " + collision.name);
                currentRoom = lastRoom;
                lastRoom = collision.name;
            }
            else
            {
                //print("poniendo a false");
                sameRoomTrigger = false;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, vision.visionDistance * combatDetectionDistanceMultiplier);
    }
}
