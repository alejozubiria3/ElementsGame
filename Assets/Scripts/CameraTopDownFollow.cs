using UnityEngine;

public class CameraTopDownFollow : MonoBehaviour
{
    public Transform player;   
    private Vector3 offset;    

    void Start()
    {
        
        offset = transform.position - player.position;
    }

    void LateUpdate()
    {
        if (player != null)
        {
            
            transform.position = player.position + offset;
        }
    }
}