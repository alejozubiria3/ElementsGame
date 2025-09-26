using UnityEngine;
using System;

[DisallowMultipleComponent]
public class ManaSystem : MonoBehaviour
{
    [Header("Mana")]
    public float maxMana = 100f;
    [Tooltip("Solo lectura en runtime")]
    public float currentMana;

    [Header("Regeneración")]
    public float regenPerSecond = 8f;

    public event Action<float, float> OnManaChanged; 

    void Awake()
    {
        currentMana = maxMana;
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    void Update()
    {
        if (regenPerSecond > 0f && currentMana < maxMana)
        {
            currentMana = Mathf.Min(maxMana, currentMana + regenPerSecond * Time.deltaTime);
            OnManaChanged?.Invoke(currentMana, maxMana);
        }
    }

    
    public bool TrySpend(float amount)
    {
        if (amount <= 0f) return true;
        if (currentMana < amount) return false;

        currentMana -= amount;
        OnManaChanged?.Invoke(currentMana, maxMana);
        return true;
    }

    public void Add(float amount)
    {
        if (amount <= 0f) return;
        currentMana = Mathf.Min(maxMana, currentMana + amount);
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    public void SetMax(float newMax, bool fill = true)
    {
        maxMana = Mathf.Max(1f, newMax);
        if (fill) currentMana = maxMana;
        currentMana = Mathf.Min(currentMana, maxMana);
        OnManaChanged?.Invoke(currentMana, maxMana);
    }
}  