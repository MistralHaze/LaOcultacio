using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public bool IsMoving = false;

    public float speed = 3f;        //Velocidad del jugador
    float speedMinus = 2f;      //Velocidad a restar cuando se pulsa shift
    int rotateLock = 0;     //Seguro para evitar que al movernos diagonalmente y parar rote equivocamnt
    static int ROTATEUNLOCK = 7;    //Tiene que superar este seguro

    Vector2 movement;   //Vector direccion del jugador
    Rigidbody2D playerRigidbody;
	Animator anim;

    AudioSource soundManagerPlayer;
    
    public bool hidden = false;
    public GameObject gameManager;
    public GameObject particlesWave;

    public AudioClip soundDeathByHit;
    public AudioClip soundTokenCollected;
    public AudioClip soundEndGameReached;

    void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
		anim = GetComponent <Animator> ();
        soundManagerPlayer = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameManager.GetComponent<GameManager>().pauseGame();
        }
    }

	void FixedUpdate () 
    {
        //Almacenamos el input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        float shift = Input.GetAxisRaw("Fire3");

        if (h != 0 || v != 0)
        {
            IsMoving = true;
        }
        else
        {
            IsMoving = false;
        }

        movement = new Vector2(h, v);   //Creamos vector con direccion actual
        movement = movement.normalized; //Lo normalizamos

		//Animamos el jugador
		Animate(movement, shift);

        // Movemos jugador 
        Move(movement, shift);

        //Giramos el jugador
        Turn(movement, v);
	}


	private void Animate(Vector2 movement, float shift){

		if (movement != Vector2.zero && shift == 0) {
			anim.SetBool ("IdleToRun", true);
            particlesWave.transform.localScale = new Vector3(10, 0, 10);
		} else {
			anim.SetBool ("IdleToRun", false);
            particlesWave.transform.localScale = new Vector3(3, 0, 3);
		}

	}

    private void Turn(Vector2 movement, float v)
    {
        
        float rotation = Vector2.Angle(Vector2.right, movement);    //Calculamos el angulo
        if (v < 0) rotation = -rotation;    //Negativo para mirar hacia abajo

        //Si no queremos que se quede en diagonal bastaria con eliminar todo lo del lock
        if (movement != Vector2.zero)   //Si nos hemos movido
        {
            rotateLock++;
            if (rotateLock > ROTATEUNLOCK)     //Y no ha sido porque estábamos parando en diagonal
            {
                playerRigidbody.MoveRotation(rotation);     //Rotamos
                rotateLock = 0;
            }
        }
    }

    void Move(Vector2 movement, float shift)
    {

        Vector2 position = transform.position;

        //Esta comprobacion es solo para que no de problemas si alguien toca lo que no hay que tocar
        if (speedMinus < speed) playerRigidbody.MovePosition(position + movement * (speed - (speedMinus * shift)) * Time.deltaTime);    //Y lo multiplicamos finalmente por la velocidad, Restamos velocidad si shift esta pulsado
        else playerRigidbody.MovePosition(position + movement * speed * Time.deltaTime);

    }
    public void playerHidden()
    {
        hidden = true;
    }
    public void playerNotHidden()
    {
        hidden = false; 
    }

    public bool isHidden()
    {
        return hidden;
    }

    void ResetAudioSourceValues()
    {
        soundManagerPlayer.pitch = 1;
        soundManagerPlayer.volume = 1;
    }

    void OnTriggerEnter2D (Collider2D other)
    {

        if (other.gameObject.name == "EndGame")
        {
            soundManagerPlayer.PlayOneShot(soundEndGameReached);
            gameManager.GetComponent<GameManager>().winGame();
        }

        if (other.gameObject.name == "BulletDiego(Clone)")
        {
            gameManager.GetComponent<GameManager>().playerDead();
            //Al ser imposible destruir la instancia del player, deberiamos hacer otro sprite o una animación de muerte
        }
        if (other.gameObject.tag == "Token")
        {
            soundManagerPlayer.clip = soundTokenCollected;
            soundManagerPlayer.pitch = 2.7f;
            soundManagerPlayer.volume = 0.5f;
            soundManagerPlayer.Play();
            Invoke("ResetAudioSourceValues", 1);
            gameManager.GetComponent<GameManager>().tokenCollected();
            Destroy(other.gameObject);



        }
        if (other.gameObject.tag == "Enemy")
        {
            soundManagerPlayer.PlayOneShot(soundDeathByHit);
            gameManager.GetComponent<GameManager>().playerDead();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            soundManagerPlayer.PlayOneShot(soundDeathByHit);
            gameManager.GetComponent<GameManager>().playerDead();
        }
    }
}
