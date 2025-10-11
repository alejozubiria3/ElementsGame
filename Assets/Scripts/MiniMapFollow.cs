using UnityEngine;

public class MiniMapFollow : MonoBehaviour
{
    public Transform player;
    public float height = 20f;      
    public Vector2 offsetXZ = Vector2.zero; 

    void LateUpdate()
    {
        if (!player) return;

        
        Vector3 pos = player.position;
        transform.position = new Vector3(
            pos.x + offsetXZ.x,
            height,
            pos.z + offsetXZ.y
        );

        
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
