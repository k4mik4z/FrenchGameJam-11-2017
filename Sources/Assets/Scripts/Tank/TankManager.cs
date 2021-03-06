﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TankManager : MonoBehaviour {

    public GameManager gm;

    [Header("Score")]
    private GameObject score;
    public GameObject Score {
        get { return score; }
        set {
            Text[] objs = value.GetComponentsInChildren<Text>();
            foreach (var obj in objs) {
                switch(obj.name) {
                    case "Player":
                        obj.text = "Player " + playerNumber;
                        break;
                    case "Lives":
                        lives = obj;
                        break;
                    case "Kills":
                        kills = obj;
                        break;
                }
            }
        }
    }
    public Text lives { get; set; }
    public Text kills { get; set; }
    public Text death { get; set; }
    public int health = 50;
    public int maxHealth = 50;

    public int Health {
        get { return health; }
        set {
            health = value;
            var ratio = health / ((float) maxHealth);
            healthBar.GetComponent<UnityEngine.UI.Image>().fillAmount = ratio;
        }
    }

    private int playerNumber;
    public int PlayerNumber {
        get { return playerNumber; }
        set {
            playerNumber = value;
            playerNameLabel.GetComponent<UnityEngine.UI.Text>().text = "P" + value;
        }
    }

    public GameObject playerNameLabel;
    public GameObject healthBar;

    [Header("Shoot Handling")]
    private bool shoot = false;
    private bool isLoading = false;
    public float forceFactorBySecond = 10f;
    public float shotSpeed = 0.5f; //Number of shots per second
    private float timeLastShot;
    private float startLoading;
    public GameObject projectile;
    public Transform shootDirection;

    [Header("Rotation Handling")]
    public GameObject canon;
    public float rotationSpeed; //Degrees by second
    public int direction = 1;
    public float maxAngle;
    public float minAngle;

    [Header("Sounds")]
    public AudioSource audioShotSource;
    public List<AudioClip> shotSounds;
    public AudioSource audioDeathSource;
    public List<AudioClip> deathSounds;
    public List<AudioClip> victorySounds;

    void Update() {
        if (!isLoading) {
            float z = canon.transform.eulerAngles.z;
            z += direction * rotationSpeed * Time.deltaTime;
            if (z > maxAngle) {
                z = maxAngle;
                direction *= -1;
            }
            if (z < minAngle) {
                z = minAngle;
                direction *= -1;
            }
            canon.transform.rotation = Quaternion.AngleAxis(z, Vector3.forward);
        }

        if (shoot && (timeLastShot + 1f / shotSpeed) < Time.time) {
            GameObject p = GameObject.Instantiate(projectile, shootDirection.position, shootDirection.rotation);

            var projManager = p.GetComponent<ProjectileManager>();
            projManager.Emitter = gameObject;
            projManager.gameManager = gm;

            p.GetComponent<Projectile>().setForce((Time.time - startLoading) * forceFactorBySecond);

            canon.SetActive(false);
            Invoke("reenableArm", 1f / shotSpeed);

            audioShotSource.clip = shotSounds[Random.Range(0, shotSounds.Count)];
            audioShotSource.Play();

            timeLastShot = Time.time;
        }
        shoot = false;
    }

    private void reenableArm() {
        canon.SetActive(true);
        startLoading = Time.time;
        var canonAngle = Random.Range(minAngle, maxAngle);
        canon.transform.rotation = Quaternion.AngleAxis(canonAngle, Vector3.forward);
    }

    public void setGameManager(GameManager gm) {
        this.gm = gm;
    }

    public void setIsLoading(bool loading) {
        if (isLoading && !loading) {
            this.shoot = true;
        }
        if (!isLoading) {
            startLoading = Time.time;
        }
        isLoading = loading;
    }

    public void OnTriggerEnter2D(Collider2D coll) {
        var proj = coll.gameObject.GetComponent<ProjectileManager>();
        if (proj != null) {
            // Ignore self-collision when if happens soon after the shot
            var ignoreCollision = gameObject == proj.Emitter && Time.time - proj.creationTime < ProjectileManager.TOLERANCE_DURATION;
            if (!ignoreCollision) {
                var instantDamage = proj.InstantDamage();
                var attackerTankManager = proj.Emitter.GetComponent<TankManager>();
                if (instantDamage > Health) {
                    instantDamage = Health;
                }
                Health -= instantDamage;
                
                gm.registerPlayerDamage(attackerTankManager.PlayerNumber, instantDamage);
                if (Health <= 0) {
                    gm.OnPlayerKill(attackerTankManager.PlayerNumber, PlayerNumber);

                    audioDeathSource.clip = deathSounds[Random.Range(0, deathSounds.Count)];
                    audioDeathSource.Play();
                }
            }

        }
    }
}
