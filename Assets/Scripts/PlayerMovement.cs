using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public bool faceMoveDirection = true;

    [Header("Suelo / Pendientes")]
    public LayerMask groundMask = ~0;  
    public float maxSlopeAngle = 55f;
    public float groundCheckHeight = 0.6f;
    public float groundCheckDist = 0.8f; 

    [Header("Extra")]
    public float extraGravity = 20f;
    Rigidbody rb;
    Vector3 moveInput; 
    Vector3 groundNormal = Vector3.up;
    bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void Update()
    {
        
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(x, 0f, z).normalized;

       
        if (faceMoveDirection && moveInput.sqrMagnitude > 0.0001f)
        {
            Vector3 look = Vector3.ProjectOnPlane(moveInput, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), 15f * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        UpdateGroundInfo();

        
        Vector3 wishDir = moveInput;

       
        if (isGrounded)
            wishDir = Vector3.ProjectOnPlane(wishDir, groundNormal).normalized;

       
        float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
        if (slopeAngle > maxSlopeAngle)
            wishDir = Vector3.zero;

       
        Vector3 delta = wishDir * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + delta);

        
        if (isGrounded && extraGravity > 0f)
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
    }

    void UpdateGroundInfo()
    {
        
        Vector3 origin = transform.position + Vector3.up * groundCheckHeight;
        float rayLen = groundCheckHeight + groundCheckDist;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayLen, groundMask, QueryTriggerInteraction.Ignore))
        {
            groundNormal = hit.normal;
            isGrounded = true;
        }
        else
        {
            groundNormal = Vector3.up;
            isGrounded = false;
        }

        
        Debug.DrawLine(origin, origin + Vector3.down * rayLen, isGrounded ? Color.green : Color.red, 0f, false);
    }
}