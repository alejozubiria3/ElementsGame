using UnityEngine;

public class TargetSelector : MonoBehaviour
{
    [Header("Selección")]
    public KeyCode selectKey = KeyCode.Mouse1; 
    public LayerMask enemyMask = ~0;  
    public float rayMaxDistance = 200f;

    [Header("Rotación al seleccionar")]
    public bool rotateOnSelect = true;   
    public float rotateSpeed = 12f;      

    public Targetable CurrentTarget { get; private set; }

    Camera _cam;

    void Awake()
    {
        _cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(selectKey))
            TryPickTarget();

        
        if (rotateOnSelect && CurrentTarget != null)
        {
            Vector3 toTarget = CurrentTarget.transform.position - transform.position;
            toTarget.y = 0f; 

            if (toTarget.sqrMagnitude > 0.001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(toTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotateSpeed * Time.deltaTime);
            }
        }
    }

    void TryPickTarget()
    {
        if (_cam == null) return;

        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, rayMaxDistance, enemyMask, QueryTriggerInteraction.Ignore))
        {
            var t = hit.collider.GetComponentInParent<Targetable>();
            if (t != null)
            {
                SetTarget(t);
                return;
            }
        }

        ClearTarget();
    }

    void SetTarget(Targetable t)
    {
        if (CurrentTarget) CurrentTarget.SetSelected(false);

        CurrentTarget = t;
        CurrentTarget.SetSelected(true);

        
        if (rotateOnSelect && CurrentTarget != null)
        {
            Vector3 toTarget = CurrentTarget.transform.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(toTarget);
            }
        }
    }

    public void ClearTarget()
    {
        if (CurrentTarget) CurrentTarget.SetSelected(false);
        CurrentTarget = null;
    }
}
