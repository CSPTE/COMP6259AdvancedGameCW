using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private float speed = 5f;
    private int damage = 2;

    private Vector2 direction;

    private bool finisherTutorialTriggered = false;

    void Update()
    {
        // Move the projectile forward
        //transform.Translate(Vector2.right * speed * Time.deltaTime);
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection;
        UpdateRotation(); // Update rotation based on the direction
    }

    private void UpdateRotation()
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            MonsterController monsterController = other.gameObject.GetComponent<MonsterController>();
            if (monsterController != null)
            {
                monsterController.DecreaseHealth(damage, "Attack2");
            }
            if ((monsterController.GetHealth() <= damage) && (!finisherTutorialTriggered) && (monsterController.GetHealth() > 0)) {
                finisherTutorialTriggered = true;
                GameManager.instance.FirstFinisher();
            }
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
