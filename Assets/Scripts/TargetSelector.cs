using UnityEngine;

public class TargetSelector : MonoBehaviour
{
    [Header("Selección")]
    public KeyCode selectKey = KeyCode.Mouse1; 
    public LayerMask enemyMask = ~0;  
    public float rayMaxDistance = 200f;

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
    }

    public void ClearTarget()
    {
        if (CurrentTarget) CurrentTarget.SetSelected(false);
        CurrentTarget = null;
    }
} 