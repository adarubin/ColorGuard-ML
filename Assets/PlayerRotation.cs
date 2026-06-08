using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    [Header("Player Settings")]
    public bool isLeftPlayer = true; 

    void Update()
    {
        if (isLeftPlayer)
        {
            if (Input.GetKeyDown(KeyCode.A)) transform.Rotate(0, 0, 90f);
            if (Input.GetKeyDown(KeyCode.D)) transform.Rotate(0, 0, -90f);
        }
        else 
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) transform.Rotate(0, 0, 90f);
            if (Input.GetKeyDown(KeyCode.RightArrow)) transform.Rotate(0, 0, -90f);
        }
    }
}