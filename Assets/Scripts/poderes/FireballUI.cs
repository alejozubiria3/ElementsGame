using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FireballUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private FireballShooter shooter; 
    [SerializeField] private Image overlay;  
    [SerializeField] private TextMeshProUGUI counterText; 

    void Reset()
    {
        if (!shooter) shooter = FindObjectOfType<FireballShooter>();
    }

    void Update()
    {
        if (!shooter || !overlay) return;

        float remaining = shooter.CooldownRemaining;
        if (remaining > 0f)
        {
            overlay.gameObject.SetActive(true);

            overlay.fillAmount = shooter.Cooldown01; 
            if (counterText)
            {
                counterText.gameObject.SetActive(true);
                counterText.text = Mathf.CeilToInt(remaining).ToString();
            }
        }
        else
        {
            overlay.gameObject.SetActive(false);
            if (counterText) counterText.gameObject.SetActive(false);
        }
    }
}
   