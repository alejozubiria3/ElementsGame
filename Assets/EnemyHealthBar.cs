using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class EnemyHealthBar : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] EnemyHealth enemy;   // si lo dejás vacío, lo busca en el padre
    [SerializeField] Image fill;          // arrastrá la Image "Fill"

    [Header("Colocación (solo una vez)")]
    [SerializeField] Vector3 localOffset = new Vector3(0f, 2f, 0f); // altura/offset fijo

    // cache de pose inicial para dejarla estática
    Vector3 _initialLocalPos;
    Quaternion _initialLocalRot;
    Vector3 _initialLocalScale;

    void Awake()
    {
        if (!enemy) enemy = GetComponentInParent<EnemyHealth>();

        // fijar posición/rotación/escala UNA VEZ
        transform.localPosition = localOffset;
        if (transform.localScale.x > 0.1f) transform.localScale = Vector3.one * 0.015f;

        _initialLocalPos   = transform.localPosition;
        _initialLocalRot   = transform.localRotation;
        _initialLocalScale = transform.localScale;

        gameObject.SetActive(true);
    }

    void LateUpdate()
    {
        if (!enemy || !fill) return;

        // actualizar barra (0..1)
        float t = Mathf.InverseLerp(0f, enemy.maxHealth, enemy.currentHealth);
        fill.fillAmount = t;

        // mantenerla 100% ESTÁTICA (sin billboarding, sin reposicionar)
        if (transform.localPosition != _initialLocalPos)   transform.localPosition = _initialLocalPos;
        if (transform.localRotation != _initialLocalRot)   transform.localRotation = _initialLocalRot;
        if (transform.localScale   != _initialLocalScale)  transform.localScale   = _initialLocalScale;

        // ocultar solo si el enemigo murió
        gameObject.SetActive(!enemy.isDead);
    }
}
