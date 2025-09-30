using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TargetSelector))]
public class BasicAutoAttack : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject basicProjectilePrefab;
    [SerializeField] private Transform muzzle; 

    [Header("Ataque básico")]
    [SerializeField] private float intervalSeconds = 3f;
    [SerializeField] private float damage = 6f;
    [SerializeField] private float projSpeed = 22f;
    [SerializeField] private float projLifetime = 4f;
    [SerializeField] private float maxRange = 30f;

    private TargetSelector _selector;
    private Coroutine _loopCo;

    void Awake()
    {
        _selector = GetComponent<TargetSelector>();
        if (!muzzle) muzzle = transform;
    }

    void Update()
    {
       
        if (Input.GetMouseButtonDown(0))
        {
            if (_selector.CurrentTarget != null)
            {
                if (_loopCo == null) _loopCo = StartCoroutine(AutoAttackLoop());
            }
            else
            {
                Debug.Log("[Basic] No hay objetivo seleccionado.");
            }
        }

       
        if (Input.GetKeyDown(KeyCode.Escape))
            StopAuto();
    }

    IEnumerator AutoAttackLoop()
    {
        while (true)
        {
            var t = _selector.CurrentTarget;
            if (t == null) { StopAuto(); yield break; }

            
            if (t.TryGetComponent<EnemyHealth>(out var eh) && eh.isDead)
            {
                StopAuto();
                yield break;
            }

           
            float dist = Vector3.Distance(transform.position, t.transform.position);
            if (dist > maxRange)
            {
               
                StopAuto();
                yield break;
            }

            ShootAt(t.transform);

            
            yield return new WaitForSeconds(intervalSeconds);
        }
    }

    void ShootAt(Transform target)
    {
        Vector3 origin = (muzzle ? muzzle.position : transform.position + Vector3.up * 1.6f);
        Vector3 dir = (target.position - origin);
        dir.y = 0f;
        dir.Normalize();

        var go = Instantiate(basicProjectilePrefab, origin + dir * 0.6f, Quaternion.LookRotation(dir));
        if (go.TryGetComponent<BasicProjectile>(out var proj))
            proj.Launch(dir, projSpeed, projLifetime, damage, this.gameObject);
    }

    public void StopAuto()
    {
        if (_loopCo != null)
        {
            StopCoroutine(_loopCo);
            _loopCo = null;
        }
    }
}     