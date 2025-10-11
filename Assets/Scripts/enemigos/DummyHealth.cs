using UnityEngine;

public class DummyHealth : MonoBehaviour
{
    [Header("Vida")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Colores de feedback")]
    public Color hitColor = Color.red;                     // Daño instantáneo
    public Color dotColor = new Color(1f, 0.5f, 0f);       // Daño por quemadura

    private Renderer rend;
    private Color originalColor;

    void Awake()
    {
        currentHealth = maxHealth;

        rend = GetComponentInChildren<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;
    }

    public void TakeDamage(float amount, bool isDot = false)
    {
        if (currentHealth <= 0f) return; // ya está muerto

        currentHealth -= amount;

        if (rend != null)
        {
            rend.material.color = isDot ? dotColor : hitColor;
            CancelInvoke(nameof(RestoreColor));
            Invoke(nameof(RestoreColor), 0.2f);
        }

        Debug.Log($"{name} recibió {amount} daño. Vida: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
            Die();
    }

    private void RestoreColor()
    {
        if (rend != null)
            rend.material.color = originalColor;
    }

    private void Die()
    {
        Debug.Log($"{name} murió!");
        Destroy(gameObject);
    }
}
