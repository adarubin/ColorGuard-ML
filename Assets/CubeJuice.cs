using UnityEngine;
using System.Collections;

public class CubeJuice : MonoBehaviour
{
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void Pulse()
    {
        StopAllCoroutines();
        StartCoroutine(DoPulse());
    }

    IEnumerator DoPulse()
    {
        // הגדלה אגרסיבית יותר (פי 1.5)
        transform.localScale = originalScale * 1.5f; 
        
        float t = 0;
        // חזרה מעט יותר איטית לגודל המקורי (בערך 0.2 שניות במקום 0.1)
        while (t < 1f)
        {
            t += Time.deltaTime * 5f; 
            transform.localScale = Vector3.Lerp(originalScale * 1.5f, originalScale, t);
            yield return null;
        }
        transform.localScale = originalScale;
    }
}