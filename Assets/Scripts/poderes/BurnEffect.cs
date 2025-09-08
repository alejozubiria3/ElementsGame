using UnityEngine;
using System.Collections;

public class BurnEffect : MonoBehaviour
{
    public float tickDamage = 5f;
    public float tickInterval = 1f;
    public float duration = 3f;

    private DummyHealth health;

    void Awake()
    {
        health = GetComponent<DummyHealth>();
    }

    public void StartBurn()
    {
        StartCoroutine(BurnCoroutine());
    }

    private IEnumerator BurnCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (health != null)
            {
                health.TakeDamage(tickDamage, true); 
            }
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }
        Destroy(this);
    }
}