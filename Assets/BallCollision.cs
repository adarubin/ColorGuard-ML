using UnityEngine;

public class BallCollision : MonoBehaviour
{
    private SpriteRenderer mySprite;

    void Start()
    {
        mySprite = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. התעלמות מאובייקטים שלא קשורים לפגיעה (אש ידידותית)
        if (other.CompareTag("Distraction") || other.CompareTag("Ball"))
        {
            return; 
        }

        SpriteRenderer hitSprite = other.GetComponent<SpriteRenderer>();
        
        if (hitSprite != null)
        {
            // 2. השוואת צבעים גמישה שמתעלמת מהבדלי Floating Point קטנים
            float colorDifference = Vector4.Distance((Vector4)mySprite.color, (Vector4)hitSprite.color);

            // אם ההבדל כמעט אפסי, מדובר באותו צבע!
            if (colorDifference < 0.1f) 
            {
                GameManager.Instance.AddScore(10);
            }
            else 
            {
                GameManager.Instance.LoseLife();
            }
            
            // הכדור מושמד רק כשהוא פוגע בשחקן עצמו
            Destroy(gameObject);
        }
    }
}