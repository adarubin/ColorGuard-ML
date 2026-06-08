using UnityEngine;

public class BallMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;               
    public Vector3 targetPosition;         

    [Header("Color Shift Settings")]
    public bool canShiftColor = false;     
    public float safeShiftDistance = 3f;   
    
    private bool hasShifted = false;       
    private SpriteRenderer spriteRenderer;
    private Color[] colors = new Color[4] { Color.red, Color.blue, Color.green, Color.yellow };

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // תנועת הכדור לכיוון הקוביה
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // לוגיקת החלפת צבע פשוטה ובטוחה ממרחק
        if (canShiftColor && !hasShifted)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

            if (distanceToTarget > safeShiftDistance)
            {
                if (Random.value < 0.05f * Time.deltaTime * speed) 
                {
                    ShiftColor();
                }
            }
            else if (distanceToTarget <= safeShiftDistance)
            {
                 hasShifted = true; 
            }
        }
    }

    private void ShiftColor()
    {
        if (spriteRenderer == null) return;

        Color currentColor = spriteRenderer.color;
        Color newColor;
        
        // בחירת צבע שונה מהנוכחי
        int attempts = 0;
        do
        {
            newColor = colors[Random.Range(0, colors.Length)];
            attempts++;
        } while (newColor == currentColor && attempts < 5);

        spriteRenderer.color = newColor;
        hasShifted = true; 
    }

    // התנגשות בקוביות
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerCube")) 
        {
            SpriteRenderer cubeRenderer = other.GetComponent<SpriteRenderer>();
            
            // אם הצבע של הכדור תואם לצבע של הקוביה
            if (cubeRenderer != null && spriteRenderer.color == cubeRenderer.color)
            {
                // מקבלים נקודות
                GameManager.Instance.AddScore(10);
                
                // --- הפעלת אפקט ההתנפחות (Pulse) של הקוביה ---
                other.GetComponent<CubeJuice>()?.Pulse();
            }
            else
            {
                // צבע לא תואם - מאבדים חיים
                GameManager.Instance.LoseLife();
            }

            // השמדת הכדור מיד לאחר ההתנגשות
            Destroy(gameObject); 
        }
    }
}