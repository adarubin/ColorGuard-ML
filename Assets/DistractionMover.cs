using UnityEngine;

public class DistractionMover : MonoBehaviour
{
    private Vector3 moveDirection;

    void Start()
    {
        // הגרלת כיוון תנועה אקראי לחלוטין (360 מעלות)
        float randomAngle = Random.Range(0f, 360f);
        moveDirection = new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0).normalized;
        
        // הריסת האובייקט אחרי 8 שניות כדי לפנות זיכרון (Performance)
        Destroy(gameObject, 8f);
    }

    void Update()
    {
        // תנועה חלקה בכיוון שנבחר, תוך שימוש במהירות שהוגדרה ב-GameManager
        if (GameManager.Instance != null)
        {
            transform.position += moveDirection * GameManager.Instance.ml_DistractionSpeed * Time.deltaTime;
        }
    }
}