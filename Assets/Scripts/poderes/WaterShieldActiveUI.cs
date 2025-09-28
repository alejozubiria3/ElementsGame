using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaterShieldActiveUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private WaterShield waterShield;
    [SerializeField] private Image overlay;  
    [SerializeField] private TextMeshProUGUI counterText;  

    void Reset()
    {
        if (!waterShield) waterShield = FindObjectOfType<WaterShield>();
    }

    void Update()
    {
        if (!waterShield || overlay == null) return;

        if (waterShield.IsActive)
        {
          
            overlay.gameObject.SetActive(true);

            float t = waterShield.ActiveRemaining;
            float total = Mathf.Max(0.0001f, waterShield.ActiveTotal);
            overlay.fillAmount = t / total; 

            if (counterText)
            {
                counterText.gameObject.SetActive(true);
                counterText.text = Mathf.CeilToInt(t).ToString();
            }
        }
        else
        {
            overlay.gameObject.SetActive(false);
            if (counterText) counterText.gameObject.SetActive(false);
        }
    }
}   