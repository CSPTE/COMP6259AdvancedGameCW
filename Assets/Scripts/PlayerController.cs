using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    private int moveSpeed = 5;
    private int maxHealth = 10;
    private int health = 10;
    private float lastDamageTime = 0f;
    private float damageCooldown = 0.5f; // 0.5 seconds cooldown between damage instances
    private int attack1Damage = 1;
    //private int attack2Damage = 2;
    //private float ability1Knockback = 1000.0f;
    private int attack3Damage = 2;
    private int finisherDamage = 1;

    private bool ability2 = false;
    public float dashSpeed = 10f;
    public float dashDuration = 0.3f; // Duration of the dash

    private bool facingRight = true;
    private bool attacking = false;
    private bool attack1 = false;
    private bool attack2 = false;
    private bool ability1 = false;
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
    //private bool gotHurt = false; -> Used for testinga

    public string horizontalInputs = "Horizontal";
    public string verticalInputs = "Vertical";
    public string attack1Inputs = "Attack1";
    public string attack2Inputs = "Attack2";
    public string ability1Inputs = "Ability1";
    public string finisherInputs = "Finisher"; 

    public Rigidbody2D rb;
    public Animator animator;
    public BoxCollider2D playerCollider;
    public CapsuleCollider2D weaponCollider;
    public CapsuleCollider2D weaponColliderUp;
    public CapsuleCollider2D weaponColliderDown;
    public CapsuleCollider2D weaponColliderBack;
    public GameObject projectilePrefab;
    public TextMeshProUGUI healthText;
    public Image attack1CooldownImage;
    public Image attack2CooldownImage;
    public Image ability1CooldownImage;
    public Image finisherCooldownImage;

    
    public AudioSource attack1Audio;
    public AudioSource enemyHitAudio;
    public AudioSource attack2Audio;
    public AudioSource attack2LaunchAudio;
    public AudioSource ability1Audio;
    public AudioSource finisherAudio;
    public AudioSource takeDamageAudio;
    public AudioSource illusoryWallAudio;
    public AudioSource footStepAudio;

    private float illusoryWallAudioCooldown = 0.5f; // Cooldown in seconds
    private float lastIllusoryWallAudioTime = -0.5f; 
    private float footStepAudioCooldown = 0.2f; // Cooldown in seconds
    private float lastFootStepAudioTime = -0.2f; 

    private bool finisherTutorialTriggered = false;

    void Start()
    {
        //Disable roatation
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        //Enable weapon trigger only when player attacks
        weaponCollider.enabled = false;
        weaponColliderUp.enabled = false;
        weaponColliderDown.enabled = false;
    }

    void Update()
    {
        //Set health
        healthText.text = health.ToString();

        if ((health > 0) && (!attacking)){
            //Check last move direction for spell
            float moveX = Input.GetAxisRaw(horizontalInputs);
            float moveY = Input.GetAxisRaw(verticalInputs);

            if (moveX != 0 || moveY != 0)
            {
                lastMoveDirection = new Vector2(moveX, moveY).normalized;
            }

            Movement();

            if (Input.GetButtonDown(attack1Inputs) && (Time.time >= lastAttack1Time + attack1Cooldown)) {
                Attack("Attack1");
                attack1Audio.Play();
                if (attack1CooldownImage != null) {
                    attack1CooldownImage.GetComponent<CooldownController>().StartCooldown();
                }
            } else if (Input.GetButtonDown(finisherInputs) && (Time.time >= lastFinisherTime + finisherCooldown)) {
                Attack("Finisher");
                if (attack2Inputs.Equals("Attack2")){
                    attack2Audio.Play();
                } else if (attack2Inputs.Equals("Attack3")){
                    finisherAudio.Play();
                }
                if (finisherCooldownImage != null) {
                    finisherCooldownImage.GetComponent<CooldownController>().StartCooldown();
                }
            } else if (Input.GetButtonDown(attack2Inputs) && (Time.time >= lastAttack2Time + attack2Cooldown)) {
                if (attack2Inputs.Equals("Attack2")) {
                    Attack("Attack2");
                    attack2Audio.Play();
                    if (attack2CooldownImage != null) {
                        attack2CooldownImage.GetComponent<CooldownController>().StartCooldown();
                    }
                } else if (attack2Inputs.Equals("Attack3")){
                    Attack("Attack3");
                    attack2Audio.Play();
                    if (attack2CooldownImage != null) {
                        attack2CooldownImage.GetComponent<CooldownController>().StartCooldown();
                    }
                }
            } else if (Input.GetButtonDown(ability1Inputs) && (Time.time >= lastAbility1Time + ability1Cooldown)) {
                if (ability1Inputs.Equals("Ability1")) {
                    Attack("Ability1");
                    ability1Audio.Play();
                    if (ability1CooldownImage != null) {
                        ability1CooldownImage.GetComponent<CooldownController>().StartCooldown();
                    }
                } else if (ability1Inputs.Equals("Ability2")) {
                    StartCoroutine(PerformDash());
                    lastAbility1Time = Time.time;
                    if (ability1CooldownImage != null) {
                        ability1CooldownImage.GetComponent<CooldownController>().StartCooldown();
                    }
                }
            }
        } else if (!dead) {
            //Character slides when dead. This stops it.
            Vector2 moveDirection = new Vector2(0, 0);
            rb.velocity = moveDirection;
        }
    }

    IEnumerator PerformDash()
    {
        ability1Audio.Play();
        float startTime = Time.time;
        animator.SetBool("Ability2", true);
        ability2 = true;

        if (lastMoveDirection != Vector2.zero)
        {
            while (Time.time < startTime + dashDuration)
            {
                //Debug.Log("Move direction exists");
                rb.velocity = lastMoveDirection * dashSpeed;
                yield return null; // Wait for the next frame
            }
        }

        /*
        while (Time.time < startTime + dashDuration)
        {
            rb.velocity = new Vector2(transform.right.x, transform.right.y) * dashSpeed;
            yield return null; // Wait for the next frame
        }
        */

        rb.velocity = Vector2.zero; // Reset the velocity after dashing
    }

    private void Attack(string attackType)
    {
        if (attackType.Equals("Attack1")) {
            attacking = true;
            attack1 = true;
            animator.SetBool("Attacking", true);
            weaponCollider.enabled = true;
            weaponColliderUp.enabled = true;
            weaponColliderDown.enabled = true;
            lastAttack1Time = Time.time;
        } else if (attackType.Equals("Finisher")) {
            attacking = true;
            finisher = true;
            animator.SetBool("Finisher", true);
            weaponCollider.enabled = true;
            weaponColliderUp.enabled = true;
            weaponColliderDown.enabled = true;
            lastFinisherTime = Time.time;
        } else if (attackType.Equals("Attack2")) {
            attacking = true;
            attack2 = true;
            animator.SetBool("Attack2", true);
            //weaponCollider.enabled = true;
            //weaponColliderUp.enabled = true;
            //weaponColliderDown.enabled = true;
            lastAttack2Time = Time.time;
        } else if (attackType.Equals("Ability1")) {
            attacking = true;
            ability1 = true;
            animator.SetBool("Ability1", true);
            weaponCollider.enabled = true;
            weaponColliderUp.enabled = true;
            weaponColliderDown.enabled = true;
            lastAbility1Time = Time.time;
        } else if (attackType.Equals("Attack3")) {
            attacking = true;
            attack3 = true;
            animator.SetBool("Attack3", true);
            weaponCollider.enabled = true;
            weaponColliderUp.enabled = true;
            weaponColliderDown.enabled = true;
            weaponColliderBack.enabled = true;
            lastAttack2Time = Time.time;
        }
        //attackAudio.Play();
    }

    private void Movement()
	{
		float horizontalInput = Input.GetAxis(horizontalInputs);
        float verticalInput = Input.GetAxis(verticalInputs);
        Vector2 moveDirection = new Vector2(horizontalInput, verticalInput).normalized;
        
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
        rb.velocity = moveDirection * moveSpeed;
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
                        //Notify Monsters to stop moving
                        //GameManager.instance.KillPlayer(gameObject);
                    }
                }
            }
        } else if (other.gameObject.CompareTag("Wall")){
            WallController wallController = other.gameObject.GetComponent<WallController>();
            if (wallController.isFinishLine){
                //wallController.SetBackDefaultSprite();
                IncreaseHealth(1);
                lastDamageTime = Time.time;
                GameManager.instance.AdvanceFloor();
            } else if (Time.time >= lastDamageTime + damageCooldown) {
                
                lastDamageTime = Time.time; // Update the last damage time{
                
                Vector2 knockbackDirection = (transform.position - other.transform.position).normalized;  // Calculate knockback direction
                float knockback = 1.5f;  // Set the strength of the knockback
                //rb.AddForce(knockbackDirection * knockback, ForceMode2D.Impulse);
                rb.MovePosition(rb.position + knockbackDirection * knockback); 
                //Debug.Log("Knockback"); 
                
                DecreaseHealth(1);
                if (health > 0){
                    animator.SetBool("GetHit", true);
                    //Debug.Log("Player New Health: " + health);
                } else {
                    animator.SetBool("Dead", true);
                    //Debug.Log("Player Died: " + health);
                    //Notify Monsters to stop moving
                    //GameManager.instance.KillPlayer(gameObject);
                }
                
            }
        } else if (other.gameObject.CompareTag("WeaponLock")){
            if (InventoryController.instance != null){
                InventoryController.instance.WeaponNotUnlocked();
            }
        } else if (other.gameObject.CompareTag("AbilityLock")){
            if (InventoryController.instance != null){
                InventoryController.instance.AbilityNotUnlocked();
            }
        } else if (other.gameObject.CompareTag("LoadSample")){
            if (InventoryController.instance != null){
                InventoryController.instance.LoadSample();
            }
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
        if ((other.gameObject.CompareTag("Monster")) && (attacking == true))
        {
            MonsterController monsterController = other.gameObject.GetComponent<MonsterController>();
            if (monsterController != null)
            {
                if (attack1) {
                    monsterController.DecreaseHealth(attack1Damage, "Attack1");
                    attack1Audio.Stop();
                    enemyHitAudio.Play();
                    if ((monsterController.GetHealth() <= finisherDamage) && (!finisherTutorialTriggered) && (monsterController.GetHealth() > 0)) {
                        finisherTutorialTriggered = true;
                        GameManager.instance.FirstFinisher();
                    }
                } else if (finisher) {
                    monsterController.DecreaseHealth(finisherDamage, "Finisher");
                    enemyHitAudio.Play();
                    if ((monsterController.GetHealth() <= finisherDamage) && (!finisherTutorialTriggered) && (monsterController.GetHealth() > 0)) {
                        finisherTutorialTriggered = true;
                        GameManager.instance.FirstFinisher();
                    }
                } else if (ability1) {
                    monsterController.PlayerKnockback();
                    //Debug.Log("Ability 1 trigger");
                } else if (attack3){
                    monsterController.DecreaseHealth(attack3Damage, "Attack3");
                    enemyHitAudio.Play();
                    if ((monsterController.GetHealth() <= finisherDamage) && (!finisherTutorialTriggered) && (monsterController.GetHealth() > 0)) {
                        finisherTutorialTriggered = true;
                        GameManager.instance.FirstFinisher();
                    }
                }
            }
        } else if (other.gameObject.CompareTag("Wall")){
            if (Time.time >= lastIllusoryWallAudioTime + illusoryWallAudioCooldown) {
                lastIllusoryWallAudioTime = Time.time;
                illusoryWallAudio.Stop();
                illusoryWallAudio.Play();
            }
        }
    }

    private void OnAttackFinish() {
        
        attacking = false;
        //Debug.Log("On Attack Finished");
        if (attack1) {
            attack1 = false;
            animator.SetBool("Attacking", false);
            weaponCollider.enabled = false;
            weaponColliderUp.enabled = false;
            weaponColliderDown.enabled = false;
        } else if (finisher) {
            finisher = false;
            animator.SetBool("Finisher", false);
            weaponCollider.enabled = false;
            weaponColliderUp.enabled = false;
            weaponColliderDown.enabled = false;
        } else if (attack2) {
            //Debug.Log("Attack2 Disable");
            attack2 = false;
            animator.SetBool("Attack2", false);
            //GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            //projectile.transform.right = transform.right; // Ensures the projectile faces the player's facing direction
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            projectile.GetComponent<ProjectileController>().SetDirection(lastMoveDirection); // Set the direction of the projectile
            //weaponCollider.enabled = true;
            //weaponColliderUp.enabled = true;
            //weaponColliderDown.enabled = true;
            attack2LaunchAudio.Play();
        } else if (ability1) {
            ability1 = false;
            animator.SetBool("Ability1", false);
            weaponCollider.enabled = false;
            weaponColliderUp.enabled = false;
            weaponColliderDown.enabled = false;
        } else if (attack3) {
            attack3 = false;
            animator.SetBool("Attack3", false);
            weaponCollider.enabled = false;
            weaponColliderUp.enabled = false;
            weaponColliderDown.enabled = false;
            weaponColliderBack.enabled = false;
        } else if (ability2){
            animator.SetBool("Ability2", false);
        }
        //attackCollider.enabled = false;
    }

    private void OnDeathFinished(){
        animator.SetBool("GameEnd", true);
        dead = true;
        if (this.enabled) {
            GameManager.instance.KillPlayer(gameObject);
        }
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
}