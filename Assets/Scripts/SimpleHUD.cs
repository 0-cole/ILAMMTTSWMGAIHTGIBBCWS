using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SimpleHUD : MonoBehaviour
{
    [Header("References")]
    public WeaponController weaponController;
    public TextMeshProUGUI manaTextTMP;
    public Text manaTextLegacy;

    [Header("Smooth Counter Settings")]
    [SerializeField] private float lerpSpeed = 10f;
    [SerializeField] private bool useSmoothing = true;

    [Header("Visual Polish")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color lowManaColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private float lowManaThreshold = 0.25f;
    [SerializeField] private bool pulseWhenLow = true;
    [SerializeField] private float pulseSpeed = 4f;

    private float displayedMana;
    private float targetMana;

    void Start()
    {
        if (weaponController != null)
        {
            displayedMana = weaponController.currentMana;
            targetMana = weaponController.currentMana;
        }
    }

    void Update()
    {
        if (weaponController == null) return;

        targetMana = weaponController.currentMana;

        if (useSmoothing)
        {
            displayedMana = Mathf.Lerp(displayedMana, targetMana, lerpSpeed * Time.deltaTime);
        }
        else
        {
            displayedMana = targetMana;
        }

        int displayValue = Mathf.RoundToInt(displayedMana);
        int maxValue = Mathf.RoundToInt(weaponController.maxMana);
        
        // Reverted <monospace> tag as it was rendering as raw text.
        // Using "000" padding to keep width consistent (e.g. "095 / 100")
        string manaString = $"<b>MANA</b>  {displayValue:000} / {maxValue}";

        float manaRatio = weaponController.currentMana / weaponController.maxMana;
        Color currentColor = normalColor;

        if (manaRatio <= lowManaThreshold)
        {
            currentColor = lowManaColor;
            
            if (pulseWhenLow)
            {
                float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
                currentColor = Color.Lerp(lowManaColor, normalColor, pulse * 0.3f);
            }
        }

        if (manaTextTMP != null)
        {
            manaTextTMP.text = manaString;
            manaTextTMP.color = currentColor;
        }
        else if (manaTextLegacy != null)
        {
            manaTextLegacy.text = $"FIRE MANA: {displayValue:000} / {maxValue}";
            manaTextLegacy.color = currentColor;
        }
    }
}
