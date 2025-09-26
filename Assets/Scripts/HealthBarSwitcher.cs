using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class HealthBarSwitcher : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerSimpleHealth playerHealth;   
    public ElementSwitcher elementSwitcher;   

    [Header("Barra Fuego")]
    public GameObject fireBarRoot;            
    public Image fireFill;                   

    [Header("Barra Agua")]
    public GameObject waterBarRoot;           
    public Image waterFill;                   

    [Header("Texto")]
    public TMP_Text percentText;              
    public bool showAsFraction = false;       

    [Header("Animación")]
    public bool smoothFill = true;
    public float fillLerpSpeed = 8f;

    private float targetFill01 = 1f;

    void Awake()
    {
        
        if (!playerHealth) playerHealth = Object.FindFirstObjectByType<PlayerSimpleHealth>();
        if (!elementSwitcher) elementSwitcher = Object.FindFirstObjectByType<ElementSwitcher>();
    }

    void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged += HandleHealthChanged;
    }

    void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= HandleHealthChanged;
    }

    void Start()
    {
        if (playerHealth != null)
            HandleHealthChanged(playerHealth.currentHealth, playerHealth.maxHealth);

        ApplyElementVisibility();
    }

    void Update()
    {
        
        if (smoothFill)
        {
            if (fireFill) fireFill.fillAmount = Mathf.MoveTowards(fireFill.fillAmount, targetFill01, Time.deltaTime * fillLerpSpeed);
            if (waterFill) waterFill.fillAmount = Mathf.MoveTowards(waterFill.fillAmount, targetFill01, Time.deltaTime * fillLerpSpeed);
        }

        
        ApplyElementVisibility();
    }

    public void HandleHealthChanged(int current, int max)
    {
        targetFill01 = (max > 0) ? (float)current / max : 0f;

        if (!smoothFill)
        {
            if (fireFill) fireFill.fillAmount = targetFill01;
            if (waterFill) waterFill.fillAmount = targetFill01;
        }

        
        if (percentText)
        {
            if (showAsFraction)
                percentText.text = $"{current}/{max}";
            else
                percentText.text = $"{Mathf.RoundToInt(targetFill01 * 100f)}%";
        }
    }

    void ApplyElementVisibility()
    {
        if (!elementSwitcher) return;

        bool isFire = elementSwitcher.current == Element.Fire;

        if (fireBarRoot) fireBarRoot.SetActive(isFire);
        if (waterBarRoot) waterBarRoot.SetActive(!isFire);
    }
}  