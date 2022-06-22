using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class TankController : MonoBehaviour
{
    private int gameMode;
    public Transform shotSpawn;
    public GameObject shot;
    public GameObject missile;
    public GameObject mine;
    public GameObject explosion;
    public GameObject ricochet;
    public GameObject husk;
    public float speed;
    private int tankSelector;

    public Text ammoText;
    public int HP;
    private int enemyCount;
    public int livesRemaining;
    private bool isDead;

    [SerializeField]
    Texture Box;

    public float fireRate;
    public float originalFireRate;
    private bool canFire;

    public AudioSource audio;
    public AudioClip shotSound;
    public AudioClip missileSound;
    public AudioClip alarm;
    public AudioClip ammoSound;
    public AudioClip healthSound;

    public float moveSpeed = 1.0f;
    public float turnSpeed = 25.0f;
    private bool canMove;

    private Rigidbody2D rb2D;
    private float nextFire;
    private float spread;
    private int playAlarm;
    private float initialHP;
    public int ammoType;
    private int ammoCountdown;

    private float lts;
    private float rts;

    private Animator muzzleAnimator;

    private void Awake()
    {
        if (!PlayerPrefs.HasKey("gameMode"))
        {
            PlayerPrefs.SetInt("gameMode", 3);
        }
        else
        {
            gameMode = PlayerPrefs.GetInt("gameMode");
        }
        if (!PlayerPrefs.HasKey("tankModel"))
        {
            PlayerPrefs.SetInt("tankModel", 0);
        }
        else
        {
            tankSelector = PlayerPrefs.GetInt("tankModel");
        }
        //tankSelector = Random.Range(0, 8);
        this.transform.GetChild(0).GetChild(tankSelector).gameObject.SetActive(true);
        this.transform.GetChild(1).GetChild(0).GetChild(tankSelector).gameObject.SetActive(true);
    }

    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        this.transform.position = GameController.spawnPoint;
        enemyCount = GameController.enemyCount;
        if (!PlayerPrefs.HasKey("tankInitialHP"))
        {
            PlayerPrefs.SetInt("tankInitialHP", 10);
        }
        else
        {
            initialHP = PlayerPrefs.GetInt("tankInitialHP");
        }
        if (!PlayerPrefs.HasKey("tankHP"))
        {
            PlayerPrefs.SetInt("tankHP", 10);
        }
        else
        {
            if (SceneManager.GetActiveScene().buildIndex == 1) HP = (int)initialHP;
            else HP = PlayerPrefs.GetInt("tankHP");
        }
        SetCountText();
        audio = GetComponent<AudioSource>();
        playAlarm = 1;
        ammoType = 0;
        ammoCountdown = 0;
        GameController.allyCount++;
        muzzleAnimator = this.transform.GetChild(1).GetChild(2).GetChild(0).GetComponent<Animator>();
        originalFireRate = fireRate;
        canFire = true;
        canMove = true;
        if (!PlayerPrefs.HasKey("livesLeft"))
        {
            PlayerPrefs.SetInt("livesLeft", 4);
        }
        else
        {
            livesRemaining = PlayerPrefs.GetInt("livesLeft");
        }
        isDead = false;
    }

    private void Update()
    {
        spread = Random.Range(-0.5f + (float)HP / (initialHP * 2), 0.5f - (float)HP / (initialHP * 2));
        shotSpawn.localRotation = Quaternion.Euler(0, 0, spread * 1.25f);

        if (ammoCountdown <= 0) ammoType = 0;

        if ((Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2")) && Time.time > nextFire && canFire)
        {
            switch(ammoType)
            {
                default:
                case 2:
                    shoot(shotSound, shot);
                    break;
                case 1:
                    shoot(missileSound, missile);
                    break;
                case 4:
                    dropMine(mine);
                    break;
                case 5:
                    this.transform.GetChild(2).gameObject.SetActive(true);
                    StartCoroutine(DeactivateShield());
                    ammoCountdown = 0;
                    break;
            }
            muzzleAnimator.SetTrigger("Fire");
        }
        /*
        if (HP <= (initialHP / 4))
        {
            if (playAlarm > 0)
            {
                audio.PlayOneShot(alarm, 1.0f);
                playAlarm = 0;
            }
        }
        */
        if (HP <= 0 && !isDead)
        {
            isDead = true;
            GameObject exp = Instantiate(explosion, gameObject.transform, false) as GameObject;
            exp.transform.parent = null;
            GameObject remains = Instantiate(husk, transform.position, transform.rotation) as GameObject;
            remains.transform.parent = null;

            if (livesRemaining > 0)
            {
                Debug.Log("You have lives left");
                livesRemaining--;
                PlayerPrefs.SetInt("livesLeft", livesRemaining);
                gameObject.transform.localScale = new Vector3(0, 0, 0);
                canFire = false;
                canMove = false;
                StartCoroutine(Respawn());
            }
            else
            {
                Debug.Log("No lives left");
                GameController.allyCount--;
                gameObject.SetActive(false);
                audio.Stop();
            }
        }

        SetCountText();
    }

    private void FixedUpdate()
    {
        
        lts = Input.GetAxis("Vertical");
        rts = Input.GetAxis("Vertical2");

        float turnLeft = turnSpeed * lts;
        float turnRight = turnSpeed * rts;

        if (rts > 0f && lts > 0f && canMove) { transform.Translate(Vector3.up * moveSpeed * Time.deltaTime); } //Forward movement if both triggers depressed
        if (rts < 0f && lts < 0f && canMove) { transform.Translate(Vector3.up * -moveSpeed * Time.deltaTime); } //Backward movement if both triggers depressed
        if (lts > 0f && canMove) { transform.Rotate(Vector3.forward, -turnLeft * Time.deltaTime); } //Track left
        if (rts < 0f && canMove) { transform.Rotate(Vector3.forward, turnRight * Time.deltaTime); } //Track left
        if (lts > 0f && rts < 0f && canMove) { transform.Rotate(Vector3.forward, -turnLeft * Time.deltaTime); } //Track left
        if (rts > 0f && canMove) { transform.Rotate(Vector3.forward, turnRight * Time.deltaTime); } //Track right
        if (lts < 0f && canMove) { transform.Rotate(Vector3.forward, -turnLeft * Time.deltaTime); } //Track right
        if (rts > 0f && lts < 0f && canMove) { transform.Rotate(Vector3.forward, turnRight * Time.deltaTime); } //Track right
    }

    void SetCountText()
    {
        switch(ammoType)
        {
            default:
                ammoText.text = "";
                break;
            case 1:
                ammoText.text = "Rocket";
                break;
            case 2:
                ammoText.text = "Triple shot";
                break;
            case 4:
                ammoText.text = "Mine";
                break;
            case 5:
                ammoText.text = "Shield";
                break;
        }
    }

    void shoot(AudioClip sound, GameObject ammo)
    {
        audio.PlayOneShot(sound, 1.0f);
        nextFire = Time.time + fireRate;
        GameObject shell = Instantiate(ammo, shotSpawn.transform, false) as GameObject;
        shell.transform.parent = null;

        if (ammoType == 2)
        {
            GameObject shell2 = Instantiate(ammo, shotSpawn.transform, false) as GameObject;
            GameObject shell3 = Instantiate(ammo, shotSpawn.transform, false) as GameObject;
            shell2.transform.localRotation = Quaternion.Euler(0f, 0f, 10f);
            shell3.transform.localRotation = Quaternion.Euler(0f, 0f, -10f);
            shell2.transform.parent = null;
            shell3.transform.parent = null;
        }

        if (ammoCountdown > 0) ammoCountdown--;
        else fireRate = originalFireRate;
    }

    void dropMine(GameObject ammo)
    {
        GameObject laidMine = Instantiate(ammo, this.transform, false) as GameObject;
        laidMine.transform.parent = null;
        if (ammoCountdown > 0) ammoCountdown--;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Pickup"))
        {
            Pickup pickup = collision.GetComponent<Pickup>();
            switch(pickup.ammo)
            {
                default:
                    break;
                case 1:
                    audio.PlayOneShot(ammoSound, 1.0f);
                    ammoType = pickup.ammo;
                    ammoCountdown = 5;
                    fireRate = fireRate / 2.0f;
                    break;
                case 2:
                    audio.PlayOneShot(ammoSound, 1.0f);
                    ammoType = pickup.ammo;
                    ammoCountdown = 5;
                    break;
                case 3:
                    audio.PlayOneShot(healthSound, 1.0f);
                    HP += 5;
                    HP = Mathf.Clamp(HP, 0, (int)initialHP);
                    playAlarm = 1;
                    break;
                case 4:
                    audio.PlayOneShot(ammoSound, 1.0f);
                    ammoType = pickup.ammo;
                    ammoCountdown = 3;
                    break;
                case 5:
                    audio.PlayOneShot(ammoSound, 1.0f);
                    ammoType = pickup.ammo;
                    ammoCountdown = 1;
                    break;
            }
            //Destroy(collision.gameObject, 0.25f);
            pickup.DestroyPickup();
        }

        if (this.gameObject.CompareTag("Tank") && (collision.gameObject.CompareTag("EnemyBullet") || collision.gameObject.CompareTag("Mine")))
        {
            int overfly = Random.Range(0, 9);
            if (tankSelector == 6 && overfly >= 5) ;
            else
            {
                GameObject ric = Instantiate(ricochet, collision.transform, false) as GameObject;
                ric.transform.parent = null;
                Destroy(collision.gameObject);
                CameraShake.Shake(0.1f, 0.1f);
                HP -= 1;
                PlayerPrefs.SetInt("tankHP", HP);
            }
            
        }
    }

    IEnumerator DeactivateShield()
    {
        yield return new WaitForSeconds(10.0f);
        this.transform.GetChild(2).gameObject.SetActive(false);
    }

    void OnGUI()
    {
        Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);

        // draw the name with a shadow (colored for buf)	
        GUI.color = Color.black;
        //GUI.Label(new Rect(pos.x - 20, Screen.height - pos.y - 30, 100, 30), PlayerName.Value.Value);

        GUI.color = Color.white;
        if (ammoType == 1 && ammoCountdown > 0) { GUI.color = Buff.GetColor(Buff.BuffType.Rocket); }

        if (ammoType == 2 && ammoCountdown > 0) { GUI.color = Buff.GetColor(Buff.BuffType.Triple); }

        if (ammoType == 4 && ammoCountdown > 0) { GUI.color = Buff.GetColor(Buff.BuffType.Mine); }

        if (ammoType == 5) { GUI.color = Buff.GetColor(Buff.BuffType.Shield); }

        //GUI.Label(new Rect(pos.x - 21, Screen.height - pos.y - 31, 100, 30), PlayerName.Value.Value);

        // draw health bar background
        GUI.color = Color.grey;
        GUI.DrawTexture(new Rect(pos.x - 26, Screen.height - pos.y + 20, 52, 7), Box);

        // draw health bar amount
        if (HP > (initialHP / 2)) GUI.color = Color.green;
        if (HP <= (initialHP / 2)) GUI.color = Color.yellow;
        if (HP <= (initialHP / 4)) GUI.color = Color.red;
        GUI.DrawTexture(new Rect(pos.x - 25, Screen.height - pos.y + 21, (float)HP / initialHP * (float)50f, 5), Box);
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(2.0f);

        HP = (int)initialHP;
        transform.position = GameController.spawnPoint;
        gameObject.transform.localScale = new Vector3(1, 1, 1);
        rb2D.velocity = Vector3.zero;
        rb2D.angularVelocity = 0;
        canFire = true;
        canMove = true;
        isDead = false;
    }
}

