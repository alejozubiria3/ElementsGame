using UnityEngine;
using TMPro;

public class ElementAbilityIcons : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ElementSwitcher elementSwitcher;

    [Header("UI Fire")]
    [SerializeField] private GameObject fireIcon; 
    [SerializeField] private TextMeshProUGUI fireKeyText;

    [Header("UI Water")]
    [SerializeField] private GameObject waterIcon; 
    [SerializeField] private TextMeshProUGUI waterKeyText;

    [Header("Etiqueta de tecla")]
    [SerializeField] private string abilityKey = "Q"; 

    void Awake()
    {
        if (!elementSwitcher) elementSwitcher = FindObjectOfType<ElementSwitcher>();
    }

    void Start()
    {
        if (!elementSwitcher)
        {
            Debug.LogWarning("[ElementAbilityIcons] Falta ElementSwitcher.");
            enabled = false;
            return;
        }

        if (fireKeyText) fireKeyText.text = abilityKey;
        if (waterKeyText) waterKeyText.text = abilityKey;

        Refresh();
    }

    void Update() => Refresh();

    void Refresh()
    {
        bool isFire = elementSwitcher.current == Element.Fire;

        if (fireIcon) fireIcon.SetActive(isFire);
        if (waterIcon) waterIcon.SetActive(!isFire);

        if (fireKeyText) fireKeyText.gameObject.SetActive(isFire);
        if (waterKeyText) waterKeyText.gameObject.SetActive(!isFire);
    }
}   