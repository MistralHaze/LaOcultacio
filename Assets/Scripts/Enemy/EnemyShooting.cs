using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooting : MonoBehaviour {
    //Laser
    public LineRenderer laserLineRenderer;
    public float laserWidth = 0.1f;
    public float laserMaxLength = 15f;

    public Transform gunTransform;
    public Rigidbody2D bullet;
    SteeringBehaviour steeringBehaviour;

    public EnemyAI myEnemy;

    public float attackRadius = 6f;     //Radio en el que atacar

    const float rotationVelAiming = 3f;     //Velocidad de rotacion cuando esta apuntando
    const float rotationVel = 2f;     //Velocidad de rotacion cuando esta persiguiendo
    const float bulletVelocity = 5f;  //Velocidad de la bala

    bool isBullet;              //Indica si hay una bala en el cargador
    bool isReloading;           //Indica si esta recargando

    AudioSource soundManagerEnemy;

    public AudioClip soundShootShotgun;
    public AudioClip soundReloadShotgun;

    void Start()
    {
        Vector3[] initLaserPositions = new Vector3[2] { Vector3.zero, Vector3.zero };
        laserLineRenderer.SetPositions(initLaserPositions);
        isBullet = true;
        isReloading = false;
        steeringBehaviour = this.GetComponent<SteeringBehaviour>();
        soundManagerEnemy = GetComponent<AudioSource>();
    }

    public void AttackBehavior(GameObject player)
    {
        float distance = CalculateDistance(this.transform.position, player.transform.position);     //Calcular distancia al jugador

        if (distance < attackRadius)        //Si es menor que el radio de ataque apuntamos
        {
            Aim(player);
        }
        else
        {
            steeringBehaviour.steeringAttacking(player.transform.position, myEnemy);
            transform.rotation = RotationTo(player.transform.position, gunTransform.position, gunTransform.rotation, rotationVel);   
        }
    }

    private void Aim(GameObject player )
    {
        if (!isReloading)       //Si no esta recargando
        {
            ShootLaserFromTargetPosition(gunTransform.localPosition, Vector3.right, laserMaxLength);    
            laserLineRenderer.enabled = true;

            Quaternion rotationToApply = RotationTo(player.transform.position, gunTransform.position, gunTransform.rotation, rotationVelAiming);     //Calculamos rotacion
            if (rotationToApply == transform.rotation && isBullet)      //Si ya ha apuntado y la bala esta cargada
            {
                isBullet = false;
                isReloading = true;
                Invoke("Shoot", 0.75f);      //Disparamos en 0.5s
            }
            transform.rotation = rotationToApply;       //Aplicamos rotacion
        }
    }

    private Quaternion RotationTo(Vector3 to, Vector3 from, Quaternion rotation, float rotationVel)     //Rotacion a un vector de posicion desde otro vector de posicion a determinada velocidad.
    {
        Vector3 vectorToTarget = to - from;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        Quaternion rotationToApply = Quaternion.Slerp(rotation, q, Time.deltaTime * rotationVel);
        return rotationToApply;
    }

    private void Shoot() 
    {
        soundManagerEnemy.PlayOneShot(soundShootShotgun);
        Rigidbody2D bulletInstance = Instantiate(bullet, gunTransform.position, gunTransform.rotation) as Rigidbody2D;

        bulletInstance.velocity = bulletVelocity * gunTransform.right;
        Invoke("Reload", 0.2f);
    }

    private void Reload()
    {
        soundManagerEnemy.PlayOneShot(soundReloadShotgun);
        isBullet = true;
        isReloading = false;
    }

    private float CalculateDistance(Vector3 fromPosition, Vector3 toPosition)
    {
        Vector2 distanceVector = new Vector2();
        distanceVector = toPosition - fromPosition;
        float distance = distanceVector.magnitude;
        return distance;
    }

    void ShootLaserFromTargetPosition(Vector3 targetPosition, Vector3 direction, float length)
    {
        Ray ray = new Ray(targetPosition, direction);
        RaycastHit raycastHit;
        Vector3 endPosition = targetPosition + (length * direction);
        if (Physics.Raycast(ray, out raycastHit, length))
        {
            endPosition = raycastHit.point;
        }

        laserLineRenderer.SetPosition(0, targetPosition);
        laserLineRenderer.SetPosition(1, endPosition);
    }
}