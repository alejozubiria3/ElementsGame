using UnityEngine;
using System.Collections;

public class BurnEffect : MonoBehaviour
{
    public float tickDamage = 5f;
    public float tickInterval = 1f;
    public float duration = 3f;

    EnemyHealth _enemy;
    Coroutine _routine;

    void Awake()
    {
        _enemy = GetComponent<EnemyHealth>();
    }

    public void StartBurn()
    {
        if (_enemy == null || _enemy.isDead) { Destroy(this); return; }
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        _enemy.ApplyBurnVisual(true);
        float t = 0f;
        while (t < duration && _enemy != null && !_enemy.isDead)
        {
            _enemy.TakeDamage(tickDamage, true);
            yield return new WaitForSeconds(tickInterval);
            t += tickInterval;
        }
        if (_enemy != null && !_enemy.isDead)
            _enemy.ApplyBurnVisual(false);
        Destroy(this);
    }
}

