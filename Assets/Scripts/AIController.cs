using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AIController : MonoBehaviour
{
    private int moveSpeed = 3;
    private int maxHealth = 10;
    private int health = 10;
    private float lastDamageTime = 0f;
    private float damageCooldown = 0.5f; // 0.5 seconds cooldown between damage instances
    private int attack1Damage = 1;
    private int attack3Damage = 2;
    private int finisherDamage = 1;

    private bool ability2 = false;
    private float dashSpeed = 10f;
    private float dashDuration = 0.3f; // Duration of the dash

    private bool facingRight = true;
    private bool attacking = false;
    private bool attack1 = false;
    private bool attack3 = false;
    private Vector2 lastMoveDirection = Vector2.right; // Default facing right
    private bool finisher = false;
    private float attack1Cooldown = 0.5f; // Cooldown in seconds
    private float lastAttack1Time = -0.5f; // Initialize with negative cooldown to allow immediate use
    private float finisherCooldown = 3.0f; // Cooldown in seconds
    private float lastFinisherTime = -3.0f; // Initialize with negative cooldown to allow immediate use
    private float attack2Cooldown = 5.0f; // Cooldown in seconds
    private float lastAttack2Time = -5.0f; // Initialize with negative cooldown to allow immediate use
    private float ability1Cooldown = 5.0f; // Cooldown in seconds
    private float lastAbility1Time = -5.0f; // Initialize with negative cooldown to allow immediate use
    private bool dead = false;

    public Rigidbody2D rb;
    public Animator animator;
    public BoxCollider2D playerCollider;
    public CapsuleCollider2D weaponCollider;
    public CapsuleCollider2D weaponColliderUp;
    public CapsuleCollider2D weaponColliderDown;
    public CapsuleCollider2D weaponColliderBack;
    public TextMeshProUGUI healthText;
    public Image attack1CooldownImage;
    public Image attack2CooldownImage;
    public Image ability1CooldownImage;
    public Image finisherCooldownImage;

    
    public AudioSource attack1Audio;
    public AudioSource enemyHitAudio;
    public AudioSource attack2Audio;
    public AudioSource ability1Audio;
    public AudioSource finisherAudio;
    public AudioSource takeDamageAudio;
    public AudioSource footStepAudio;

    private float footStepAudioCooldown = 0.2f; // Cooldown in seconds
    private float lastFootStepAudioTime = -0.2f; 

    private int roomID = -1;

    private bool finisherTutorialTriggered = false;

    public enum State
    {
        Search,
        Attack,
        Flee
    }
    public State currentState;
    private Transform targetMonster;

    void Start()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        weaponCollider.enabled = true;
        weaponColliderUp.enabled = true;
        weaponColliderDown.enabled = true;
        weaponColliderBack.enabled = false;
        currentState = State.Search;
    }

    void Update()
    {
        //Set health
        healthText.text = health.ToString();

        if ((health > 0) && (!attacking)){
            if (currentState == State.Search) {
                SearchForMonsters();
            } else if (currentState == State.Flee) {
                if ((Time.time >= lastAbility1Time + ability1Cooldown) && (!attack1) && (!attack3) && (!finisher) && (!ability2)){
                    StartCoroutine(PerformDash());
                    lastAbility1Time = Time.time;
                    if (ability1CooldownImage != null) {
                        ability1CooldownImage.GetComponent<CooldownController>().StartCooldown();
                    }
                } else if ((!attack1) && (!attack3) && (!finisher) && (!ability2)){
                    currentState = State.Search;
                }
            }
        } else if (!dead) {
            //Character slides when dead. This stops it.
            Vector2 moveDirection = new Vector2(0, 0);
            rb.velocity = moveDirection;
        } else if ((currentState == State.Attack) && (!attack1) && (!attack3) && (!finisher) && (!ability2)) {
            attacking = false;
            currentState = State.Search;
        } 
    }
    
    void SearchForMonsters()
    {
        // Find the nearest monster
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        Transform nearestMonster = null;
        float minDistance = float.MaxValue;

        foreach (var monster in monsters)
        {
            if (monster.GetComponent<MonsterController>() != null){
                if (roomID != -1){
                    if (monster.GetComponent<MonsterController>().GetRoomID() == roomID){
                        float distance = Vector2.Distance(transform.position, monster.transform.position);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nearestMonster = monster.transform;
                        }
                    }
                } else {
                    float distance = Vector2.Distance(transform.position, monster.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestMonster = monster.transform;
                    }
                }
            } else {
                float distance = Vector2.Distance(transform.position, monster.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestMonster = monster.transform;
                }
            }
        }

        if (nearestMonster != null && minDistance < 18f) // Ensure monster is not too far
        {
            targetMonster = nearestMonster;
            MoveTowardsTarget(nearestMonster.position);
        }
    }

    void MoveTowardsTarget(Vector2 targetPosition)
    {
        Vector2 moveDirection = (targetPosition - (Vector2)transform.position).normalized;
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);

        if (moveDirection.x != 0 || moveDirection.y != 0)
        {
            lastMoveDirection = new Vector2(moveDirection.x, moveDirection.y).normalized;
        }

        if ((moveDirection.x != 0) || (moveDirection.y != 0)){
            if (Time.time >= lastFootStepAudioTime + footStepAudioCooldown) {
                lastFootStepAudioTime = Time.time;
                footStepAudio.Stop();
                footStepAudio.Play();
            };
        }

        if ((moveDirection.x < 0) && (facingRight == true)){
            Flip();
            facingRight = false;
        } else if ((moveDirection.x > 0) && (facingRight == false)) {
            Flip();
            facingRight = true;
        }

        animator.SetFloat("Speed", Mathf.Abs(moveDirection.x) + Mathf.Abs(moveDirection.y));
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
        if (other.gameObject.CompareTag("Monster"))
        {
            MonsterController monsterController = other.gameObject.GetComponent<MonsterController>();
            int monsterDamage = monsterController.GetDamage();
            if (monsterController != null)
            {
                if (Time.time >= lastDamageTime + damageCooldown) {
                    lastDamageTime = Time.time; // Update the last damage time{
                    DecreaseHealth(monsterDamage);
                    if (health > 0){
                        animator.SetBool("GetHit", true);
                        //Debug.Log("Player New Health: " + health);
                    } else {
                        animator.SetBool("Dead", true);
                        //Debug.Log("Player Died: " + health);
                        //gameObject.SetActive(false);
                    }
                }
            }
        } else if (other.gameObject.CompareTag("Wall")){
            WallController wallController = other.gameObject.GetComponent<WallController>();
            if (wallController.isFinishLine){
                IncreaseHealth(1);
                lastDamageTime = Time.time;
            } else if (Time.time >= lastDamageTime + damageCooldown) {
                
                lastDamageTime = Time.time; // Update the last damage time{
                
                Vector2 knockbackDirection = (transform.position - other.transform.position).normalized;  // Calculate knockback direction
                float knockback = 1.5f;  // Set the strength of the knockback
                rb.MovePosition(rb.position + knockbackDirection * knockback); 
                
                DecreaseHealth(1);
                if (health > 0){
                    //animator.SetBool("GetHit", true);
                    //Debug.Log("Player New Health: " + health);
                } else {
                    animator.SetBool("Dead", true);
                }
                
            }
        } else if (other.gameObject.CompareTag("Pillar")) {
            Debug.Log("Collided with pillar");
            //TODO: Pillar Collision
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        //Exit "GetHit" state
        if (health > 0){
            animator.SetBool("GetHit", false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!attacking){
            if (other.gameObject.CompareTag("Monster"))
            {
                currentState = State.Attack;
                MonsterController monsterController = other.gameObject.GetComponent<MonsterController>();
                if (monsterController != null)
                {
                    attacking = true;
                    if ((Time.time >= lastFinisherTime + finisherCooldown) && (monsterController.GetHealth() <= finisherDamage) && (!attack1) && (!attack3) && (!finisher) && (!ability2)) {
                        //Finisher
                        if (finisherCooldownImage != null) {
                            finisherCooldownImage.GetComponent<CooldownController>().StartCooldown();
                        }
                        finisher = true;
                        animator.SetBool("Finisher", true);
                        lastFinisherTime = Time.time;
                        monsterController.DecreaseHealth(finisherDamage, "Finisher");
                        finisherAudio.Play();
                        enemyHitAudio.Play();
                        if ((monsterController.GetHealth() <= finisherDamage) && (!finisherTutorialTriggered) && (monsterController.GetHealth() > 0)) {
                            finisherTutorialTriggered = true;
                            GameManager.instance.FirstFinisher();
                        }
                    } else if ((Time.time >= lastAttack2Time + attack2Cooldown) && (!attack1) && (!attack3) && (!finisher) && (!ability2)) {
                        //Attack2
                        if (attack2CooldownImage != null) {
                            attack2CooldownImage.GetComponent<CooldownController>().StartCooldown();
                        }
                        attack2Audio.Play();
                        attack3 = true;
                        animator.SetBool("Attack3", true);
                        weaponColliderBack.enabled = true;
                        lastAttack2Time = Time.time;
                        monsterController.DecreaseHealth(attack3Damage, "Attack3");
                        enemyHitAudio.Play();
                        if ((monsterController.GetHealth() <= finisherDamage) && (!finisherTutorialTriggered) && (monsterController.GetHealth() > 0)) {
                            finisherTutorialTriggered = true;
                            GameManager.instance.FirstFinisher();
                        }
                    } else if ((Time.time >= lastAttack1Time + attack1Cooldown) && (!attack1) && (!attack3) && (!finisher) && (!ability2)){
                        //Attack1
                        attack1Audio.Play();
                        if (attack1CooldownImage != null) {
                            attack1CooldownImage.GetComponent<CooldownController>().StartCooldown();
                        }
                        attack1Audio.Play();
                        attack1 = true;
                        animator.SetBool("Attacking", true);
                        lastAttack1Time = Time.time;
                        monsterController.DecreaseHealth(attack1Damage, "Attack1");
                        enemyHitAudio.Play();
                        if ((monsterController.GetHealth() <= finisherDamage) && (!finisherTutorialTriggered) && (monsterController.GetHealth() > 0)) {
                            finisherTutorialTriggered = true;
                            GameManager.instance.FirstFinisher();
                        }
                    }
                }
            }
        }
    }

    private void OnAttackFinish() {
        if (attack1) {
            attack1 = false;
            animator.SetBool("Attacking", false);
            currentState = State.Flee;
        } else if (finisher) {
            finisher = false;
            animator.SetBool("Finisher", false);
            currentState = State.Search;
        } else if (attack3) {
            attack3 = false;
            animator.SetBool("Attack3", false);
            weaponColliderBack.enabled = false;
            currentState = State.Search;
        } else if (ability2){
            animator.SetBool("Ability2", false);
            ability2 = false;
            currentState = State.Search;
        }
        attacking = false;
    }

    IEnumerator PerformDash()
    {
        ability1Audio.Play();
        float startTime = Time.time;
        animator.SetBool("Ability2", true);
        ability2 = true;

        if (lastMoveDirection != Vector2.zero)
        {
            // Negate the direction to dash in the opposite direction
            Vector2 dashDirection = -lastMoveDirection;

            while (Time.time < startTime + dashDuration)
            {
                rb.velocity = dashDirection * dashSpeed; // Use lastMoveDirection here
                yield return null; // Wait for the next frame
            }
        }

        rb.velocity = Vector2.zero; // Reset the velocity after dashing
        currentState = State.Search;
    }

    private void OnDeathFinished(){
        animator.SetBool("GameEnd", true);
        dead = true;
        gameObject.SetActive(false);
    }

    public void DecreaseHealth(int damage){
        health = health - damage;
        takeDamageAudio.Play();
    }

    public void IncreaseHealth(int damage){
        if (health + damage < maxHealth) {
            health = health + damage;
        } else {
            health = maxHealth;
        }
    }

    public void SetRoomID(int id){
        //Debug.Log("AI new Room ID set: " + id);
        roomID = id;
    }
}
