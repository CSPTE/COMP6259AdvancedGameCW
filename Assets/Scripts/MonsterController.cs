using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class MonsterController : MonoBehaviour
{
    private int moveSpeed = 1;
    private int maxHealth = 3;
    private int health = 3;
    private int damage = 1;
    private float collisionCooldown = 1f;
    private float knockback = 1f;
    private float ability1Knockback = 3000f;
    private float finisherKnockback = 20000f;

    private bool facingRight = true;
    private bool spawned = false;
    private bool canMove = true; 
    private float collisionTimer = 0f;

    private float lastDamageTime = 0f;
    private float damageCooldown = 0.5f; // 0.5 seconds cooldown between damage instances

    public Animator animator;
    public Rigidbody2D rb;
    private Transform targetPlayer;
    public EffectController effectScript;
    public event Action OnMonsterDeath;
    public Image healthBarFill;

    private int roomID = 1;

    //public AudioSource growlAudio;
    //public AudioSource attackAudio;

    void Start()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        //targetPlayer = GameObject.FindGameObjectWithTag("Player").transform;
        health = maxHealth;
        GameManager.instance.OnPlayerDeath.AddListener(StopMoving);
        UpdateTarget(); // Initial call to set the target
        UpdateHealthBar();
    }

    void Update()
    {  
        if (GameManager.instance.isPlayerAlive){
            UpdateTarget(); // Update the target each frame to adapt to player movements
            Movement();
        }
    }

    private void UpdateHealthBar() {
        float healthPercentage = (float)health / maxHealth; // Cast health to float
        healthBarFill.fillAmount = healthPercentage;
        healthBarFill.color = Color.Lerp(Color.red, Color.green, healthPercentage);
    }

    private void UpdateTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float minDistance = float.MaxValue;

        foreach (GameObject player in players)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                targetPlayer = player.transform;
            }
        }
    }

    private void Movement()
    {
        if ((targetPlayer != null) && (spawned = true) && (canMove == true))
        {
            Vector2 direction = (targetPlayer.position - transform.position).normalized;
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
            if ((direction.x > 0) && (facingRight == true)){
                Flip();
                facingRight = false;
            } else if ((direction.x < 0) && (facingRight == false)) {
                Flip();
                facingRight = true;
            }
        } else {
            collisionTimer += Time.deltaTime;
            if (collisionTimer >= collisionCooldown)
            {
                collisionTimer = 0f;
                canMove = true;
            }
        }
    }

    private void Flip()
	{
		facingRight = !facingRight;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            canMove = false;
            Vector2 collisionDirection = (transform.position - other.transform.position).normalized;
            rb.velocity = Vector2.zero; 
            rb.MovePosition(rb.position + collisionDirection * knockback);     
            //Debug.Log("Ability1 knockback: " + ability1Knockback + " - knockback: " + knockback + " - finisherKnockback: " + finisherKnockback);
        } else if ((other.gameObject.CompareTag("Wall")) && (health <= 0)) {
            OnDespawnFinished();
        }
    }

    void OnTriggerEnter2D(Collider2D other){
        if ((other.gameObject.CompareTag("Wall")) && (health != 0)) {
            OnDespawnFinished();
        } 
        /*
        else if (other.gameObject.CompareTag("Player")) {
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                if ((playerController.GetAttacking()) && (playerController.GetAbility1())){
                    Vector2 knockbackDirection = (transform.position - player.transform.position).normalized;
                    rb.AddForce(knockbackDirection * ability1Knockback, ForceMode2D.Impulse);
                }
            }
        }
        */
    }

    public void PlayerKnockback(){
        canMove = false;
        Vector2 knockbackDirection = (transform.position - targetPlayer.transform.position).normalized;
        rb.velocity = Vector2.zero; 
        rb.AddForce(knockbackDirection * ability1Knockback, ForceMode2D.Impulse);
        //Debug.Log("Ability1 knockback: " + ability1Knockback + " - knockback: " + knockback + " - finisherKnockback: " + finisherKnockback);
    }

    public void DecreaseHealth(int damage, string attackType){
        if (Time.time >= lastDamageTime + damageCooldown) {
            lastDamageTime = Time.time; // Update the last damage time{
            health = health - damage;
            UpdateHealthBar();
            //Debug.Log("Monster New Health: " + health);
            //Stop moving when damage taken
            canMove = false;
            if (health <= 0){
                if (attackType.Equals("Finisher")) {
                    //Smash into wall
                    Vector2 knockbackDirection = (transform.position - targetPlayer.transform.position).normalized;
                    rb.velocity = Vector2.zero; 
                    rb.AddForce(knockbackDirection * finisherKnockback, ForceMode2D.Impulse);
                } else {
                    //Disintegrate
                    animator.SetBool("Killed", true);
                }
            } else {
                effectScript.TriggerBloodEffect();
            }
        }
    }

    public int GetDamage()
    {
        return damage;
    }

    public int GetHealth(){
        return health;
    }

    private void StopMoving()
    {   
        canMove = false;
    }

    private void OnSpawnFinish() {
        spawned = true;
        animator.SetBool("Spawned", true);
    }

    public void OnDespawnFinished(){
        OnMonsterDeath?.Invoke();  // Notify listeners
        Destroy(gameObject);
    }

    public void SetRoomID(int id){
        Debug.Log("Monster ID: " + id);
        roomID = id;
    }

    public int GetRoomID(){
        return roomID;
    }
}
