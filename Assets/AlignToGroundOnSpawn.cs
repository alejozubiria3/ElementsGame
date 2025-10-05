using UnityEngine;

public class AlignToGroundOnSpawn : MonoBehaviour
{
    [Header("Raycast al suelo")]
    public LayerMask groundMask = ~0;  
    public float snapUp = 50f;
    public float snapDown = 200f;
    public float extraY = 0.05f;
    public float maxSlope = 55f;        

    void Start()
    {
        Vector3 start = transform.position + Vector3.up * snapUp;
        float maxDist = snapUp + snapDown;

        if (Physics.Raycast(start, Vector3.down, out var hit, maxDist, groundMask, QueryTriggerInteraction.Ignore))
        {
            if (Vector3.Angle(hit.normal, Vector3.up) <= maxSlope)
                transform.position = hit.point + Vector3.up * extraY;
        }
    }
}
