using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class ManaBarUI : MonoBehaviour
{
    [Header("Refs")]
    public ManaSystem mana; 
    public Image fill; 
    public TMP_Text percentText;  

    [Header("Animación")]
    public bool smoothFill = true;
    public float fillLerpSpeed = 10f;

    float targetFill01 = 1f;

    void Awake()
    {
        if (!mana) mana = Object.FindFirstObjectByType<ManaSystem>();
    }

    void OnEnable()
    {
        if (mana != null) mana.OnManaChanged += HandleMana;
    }

    void OnDisable()
    {
        if (mana != null) mana.OnManaChanged -= HandleMana;
    }

    void Start()
    {
        if (mana != null) HandleMana(mana.currentMana, mana.maxMana);
    }

    void Update()
    {
        if (smoothFill && fill)
            fill.fillAmount = Mathf.MoveTowards(fill.fillAmount, targetFill01, fillLerpSpeed * Time.deltaTime);
    }

    void HandleMana(float current, float max)
    {
        targetFill01 = (max > 0f) ? current / max : 0f;

        if (!smoothFill && fill)
            fill.fillAmount = targetFill01;

        if (percentText)
            percentText.text = $"{Mathf.RoundToInt(targetFill01 * 100f)}%";
    }
}  