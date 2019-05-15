using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SteeringBehaviour : MonoBehaviour
{
    /*Bugueasiones:
         */


    //Podría ser reformulado como no Monobehavior? -> clase accesoria a EnemyShooting parece (no necesita ni start ni update ni referencias externas que EnemyShooting no le pueda dar)

    Vector3 nextPosition; //Siguiente nodo a viajar
    Vector2 direction; //Vector dirección hacia el objetivo
    Vector2 desiredSpeed = Vector2.zero; //Velocidad hacia el nuevo objetivo
    Vector2 steering; //Velocidad del giro hacia el nuevo objetivo

    //public Transform[] goPoints;
    public float maxVelocity = 0.03f; //0.1
    public int mass = 20;   //20
    public float slowingRadius = 3f;
    public float radiusPath;
    float distance;

    public bool showRaycastGizmos;
    public int numRaycasts; //Number of raycasts. Min 6 raycasts.
    public float maxAngle;  //Max angle for the lateral raycasts. Between 1 and 90.
    public float powerProportion;

    private void Awake()
    {
        maxAngle *= Mathf.Deg2Rad;
    }

    public int calculatePosition(List<Transform> goPoints, EnemyAI enemy, int nodeIndex)
    {
        nextPosition = (Vector2)goPoints[nodeIndex].position;
        Vector2 speed = enemy.speed;

        //Calcula la dirección
        direction = (nextPosition - enemy.transform.position);
        //Calcula la magnitud
        if (speed.x == 0 && speed.y == 0) //Si ya tiene una velocidad no hace falta calcularla
            speed = direction.normalized * maxVelocity;

        //Cálculo de la deceleración
        distance = Vector2.Distance(enemy.transform.position, nextPosition);
        // Calcula si el objeto está cerca del radio de deceleración
        if (distance < slowingRadius)
        {
            // Dentro del área
            desiredSpeed = desiredSpeed.normalized * maxVelocity * (distance / slowingRadius);
        }
        else
        {
            // Fuera del área
            desiredSpeed = direction.normalized * maxVelocity;
        }

        //Calculo del giro

        steering = desiredSpeed - speed;
        steering += raycastCorrector();
        steering /= mass;
        speed += steering;


        if (distance < 0.5f)
        {
            nodeIndex++;
            if (nodeIndex == goPoints.Count)
                nodeIndex = 0;
        }

        enemy.transform.position += (Vector3)speed;

        //enemy.transform.right = speed;//Look forward

        transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector3.right, speed));
                                      /* float rotation = Vector3.Angle(Vector3.right, speed);

                                       if (speed.y < 0 && speed.x<0) rotation += Mathf.PI * Mathf.Rad2Deg;

                                       transform.rotation =  Quaternion.Euler(0, 0, rotation);*/


        enemy.speed = speed;
        return nodeIndex;

    }


    public void steeringAttacking(Vector3 nextPosition, EnemyAI enemy)
    {
        Vector2 speed = enemy.speed;

        //Calcula la dirección
        direction = (nextPosition - enemy.transform.position);
        //Calcula la magnitud
        if (speed.x == 0 && speed.y == 0) //Si ya tiene una velocidad no hace falta calcularla
            speed = direction.normalized * maxVelocity;

        //Cálculo de la deceleración
        distance = Vector2.Distance(enemy.transform.position, nextPosition);
        // Calcula si el objeto está cerca del radio de deceleración
        if (distance < slowingRadius)
        {
            // Dentro del área
            desiredSpeed = desiredSpeed.normalized * maxVelocity * (distance / slowingRadius);
        }
        else
        {
            // Fuera del área
            desiredSpeed = direction.normalized * maxVelocity;
        }


        //print(desiredVelocity);
        //Calculo del giro

        steering = desiredSpeed - speed;
        steering += raycastCorrector();
        steering /= mass;
        speed += steering;


        //Aplicar velocidad
        enemy.transform.position += (Vector3)speed;
        enemy.transform.right = speed;//Look forward
        enemy.speed = speed;

    }

    //Problemas que se me ocurren: 
    // - Que en una ruta dada, los dos puntos de las normales halladas estén a la misma distancia. 
    // - O que ningún punto de la normal esté dentro de su segmento (al inicio del recorrido)
    // - O que se escoja un punto anterior y el enemigo se de la vuelta.

    //COMPROBAR SI EL PRIMER PUNTO DEL PATH COINCIDE CON EL PUNTO DE LA NORMAL EN LA PRIMERA ITERACIÓN

    public void FollowPath(List<Vector2> path, EnemyAI enemy)
    {
        if (path == null || path.Count == 0) { return; }

        if (path.Count == 1)
        {
            steeringAttacking(path[0], enemy);
        }
        else
        {
            Vector2 predict = enemy.speed.normalized;
            //No sé por cuánto multiplicar el vector para la predicción
            predict *= 1.4f;
            //Hallamos la futura posición del objeto respecto a su velocidad actual
            predict = new Vector2(enemy.transform.position.x, enemy.transform.position.y) + predict;
            //Debug.DrawLine(enemy.transform.position, predict, Color.blue);

            //Una vez predicha la posición, hallamos el siguiente punto del path más cercano al que dirigirnos
            //Nos guardamos la i del punto que será el target para poder acceder rápidamente al punto anterior y crear el segmento
            int iWinner = 0;
            Vector2 followingPathTarget = Vector2.zero;
            float distanceToClosestPathPoint = Mathf.Infinity;
            for (int i = 0; i < path.Count - 1; i++)
            {
                //Para ello, hacemos la normal desde la posición predicha a los segmentos del path
                Vector2 aux_start = path[i];
                Vector2 aux_end = path[i + 1];
                //Comprobamos que el punto de la normal que hemos hallado está dentro del segmento aux_start-aux_end
                Vector2 auxNormalPoint = getNormalPoint(predict, aux_start, aux_end);

                //Dibujamos la normal y el punto
                //Debug.DrawLine(aux_start, aux_end);
                //Debug.DrawLine(predict, auxNormalPoint, Color.green);

                if (auxNormalPoint.x < Mathf.Min(aux_start.x, aux_end.x) || auxNormalPoint.x > Mathf.Max(aux_start.x, aux_end.x) ||
                    auxNormalPoint.y < Mathf.Min(aux_start.y, aux_end.y) || auxNormalPoint.y > Mathf.Max(aux_start.y, aux_end.y))
                {
                    //Si está fuera del segmento, asignamos aux_end como posible target en vez del propio punto de la normal
                    auxNormalPoint = aux_end;
                }
                //Para todos los puntos hallados, comprobaremos cuál está más cerca y ése será el target escogido            
                float auxDistance = Vector2.Distance(predict, auxNormalPoint);
                if (auxDistance < distanceToClosestPathPoint)
                {
                    distanceToClosestPathPoint = auxDistance;
                    followingPathTarget = auxNormalPoint;
                    iWinner = i;
                }
            }
            //Una vez elegido el punto destino, aplicamos el steering behavior
            Vector2 currentPathStart = path[iWinner];
            Vector2 currentPathEnd = followingPathTarget;

            Vector2 normalPoint = getNormalPoint(predict, currentPathStart, currentPathEnd);
            Vector2 start_end = (currentPathEnd - currentPathStart).normalized;
            //En la versión de una sola recta como path, el punto de la normal hallada se mueve un poco hacia delante en la ruta y ese
            //es el target escogido, en este caso voy a probar a usar siempre como target el siguiente punto de la ruta.
            float distanceToPath = Vector2.Distance(normalPoint, predict);
            if (distanceToPath > radiusPath)
            {
                steeringAttacking(followingPathTarget, enemy);
            }
            //print(iWinner + 1);
            //Debug.DrawLine(predict, followingPathTarget, Color.red);
            //EditorApplication.isPaused = true;
        }


    }


    Vector2 raycastCorrector()
    {
        //Si se quiere evitar este algoritmo de Raycasting en alguno de los estados, se arregla fácilmente poniendo 
        //una condición de que no se realice si el enemigo está en ese estado.

        //if (numRaycasts % 2 == 0) numRaycasts += 1; //Si el número es par sumamos uno para que tenga un rayo delante y los demás separados equitativamente en los lados.

        //Estas 2 líneas antes del for se pueden poner en el Start, pero de momento está aquí para poder debuggear más rápido.
        float step = maxAngle / (numRaycasts - 1),
            originalAngle = transform.eulerAngles.z;         //Compute step angle between raycasts.
        //float angle = -maxAngle - step;                       //Compute initial angle.

        Vector2 offset = Vector2.zero;

        for (int i = 0; i < numRaycasts; i++)
        {
            //if (i != 0) angle += step;

            //Compute vector of the raycast.
            float currentAngle = -maxAngle * 0.5f + i * step;
            Vector2 raycVector = rotateVector(transform.right, currentAngle);
            if (showRaycastGizmos) Debug.DrawRay(transform.position, transform.position + (Vector3)raycVector * 500, Color.yellow);
            //Point where it collides
            RaycastHit2D hit = Physics2D.Raycast(transform.position, raycVector, 1000, 1 << LayerMask.NameToLayer("Unwalkable"));


            if (hit.collider != null)
            {
                //Show raycast line in editor.
                Vector2 distance = hit.point - (Vector2)transform.position,
                    dir = distance.normalized;


                //Compute distance from player to hit point.
                float dist = distance.magnitude;

                //float proportion = powerProportion / distance;

                /*if (Mathf.Abs(dist) < 1.5f) //Si la distancia es menor que 1.5 (para que no entorpezcan otros raycasts)
                {*/
                //Vector3 auxOffset = -dir * (1.5f - dist) * powerProportion;   //Cuanto menor sea la distancia a la pared, mayor será el offset.
                //Vector3 auxOffset = hit.normal.normalized * powerProportion / dist;
                Vector3 auxOffset = powerProportion * -transform.up * currentAngle / (maxAngle * dist);
                //this.transform.position += auxOffset;                               //Sumamos el offset al enemigo.
                offset += (Vector2)auxOffset;

                //}

            }

        }

        return offset;
    }

    Vector2 rotateVector(Vector2 original, float radians)
    {
        return new Vector2(
            original.x * Mathf.Cos(radians) - original.y * Mathf.Sin(radians),
            original.x * Mathf.Sin(radians) + original.y * Mathf.Cos(radians)
            );
    }
    /*
    Vector2 speedCorrector(Vector2 speed, Vector2 raycastOffset)
    {

        Vector2 newSpeed = Vector2.zero;
        newSpeed = speed + raycastOffset;

        float magnitude = speed.magnitude;
        float magnitude2 = newSpeed.magnitude;

        //print("maginitude 1 = " + magnitude + ",  maginitude2 = " + magnitude2);

        float newMagnitude = magnitude - magnitude2;
        //print(newMagnitude);
        newSpeed += (newSpeed * newMagnitude * 5);

        if ((newSpeed.x >= -speed.x + 0.2f && newSpeed.x <= -speed.x - 0.2f)
            || (newSpeed.y >= -speed.y + 0.2f && newSpeed.y <= -speed.y - 0.2f))
            return speed;
        // print(speed.x +  "     " + speed.y);

        return newSpeed;
    }
    */

    Vector2 getNormalPoint(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 start_point = point - start;
        Vector2 start_end = end - start;

        start_end.Normalize();
        start_end *= (Vector2.Dot(start_point, start_end));

        Vector2 normalPoint = start + start_end;
        return normalPoint;
    }

}
