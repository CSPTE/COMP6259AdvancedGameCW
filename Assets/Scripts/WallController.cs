using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallController : MonoBehaviour
{
    public Sprite[] bloodSplatterSprites0;
    public Sprite[] bloodSplatterSprites1;
    public Sprite[] bloodSplatterSprites2;
    public Sprite[] bloodSplatterSprites3;
    public Sprite[] bloodSplatterSprites4;
    public SpriteRenderer spriteRenderer;
    private int currentLevel = 0;

    private int roomID = -1;

    public bool isFinishLine = false;
    public Sprite finishLineSprite;
    public Sprite defaultSprite;

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Monster"))
        {
            MonsterController monsterController = other.gameObject.GetComponent<MonsterController>();
            
            // Check if the monster's health is zero
            if (monsterController.GetHealth() <= 0)
            {
                // Get the appropriate blood splatter sprites array based on currentLevel
                Sprite[] bloodSplatterSprites = null;
                switch (currentLevel)
                {
                    case 0:
                        bloodSplatterSprites = bloodSplatterSprites0;
                        break;
                    case 1:
                        bloodSplatterSprites = bloodSplatterSprites1;
                        break;
                    case 2:
                        bloodSplatterSprites = bloodSplatterSprites2;
                        break;
                    case 3:
                        bloodSplatterSprites = bloodSplatterSprites3;
                        break;
                    case 4:
                        bloodSplatterSprites = bloodSplatterSprites4;
                        break;
                    default:
                        return;
                }

                // Choose a random blood splatter sprite from the selected array
                Sprite randomBloodSplatter = bloodSplatterSprites[Random.Range(0, bloodSplatterSprites.Length)];
                spriteRenderer.sprite = randomBloodSplatter;
                currentLevel = currentLevel + 1;
            }
        }
    }

    public void SetRoomID(int id){
        roomID = id;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && other is BoxCollider2D) 
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) {
                spriteRenderer.color = new Color(1f,1f,1f,0.7f); // is about 50% transparent
            }
            GameManager.instance.SendRoomID(roomID, other.gameObject);
        } else if (other.gameObject.CompareTag("Monster")){
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) {
                spriteRenderer.color = new Color(1f,1f,1f,0.7f); // is about 50% transparent
            }
        }
    }

    public void SetAsFinishLine()
    {
        isFinishLine = true;
        // Change sprite to indicate it's a transition to the next floor
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && finishLineSprite != null)
        {
            spriteRenderer.sprite = finishLineSprite;
            float widthScale = 2f / spriteRenderer.sprite.bounds.size.x;
            float heightScale = 7f / spriteRenderer.sprite.bounds.size.y;
            spriteRenderer.transform.localScale = new Vector3(3f, 7f, 1);
        }
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            // Adjusting collider size
            collider.size = new Vector2(0.33f, 0.31f);
        }
    }
}
