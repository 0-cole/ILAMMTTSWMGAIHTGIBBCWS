using UnityEngine;
using TMPro; // TextMesh Pro is recommended, but we'll support Legacy Text too just in case
using UnityEngine.UI;

public class SimpleHUD : MonoBehaviour
{
    [Header("References")]
    public WeaponController weaponController;
    public TextMeshProUGUI manaTextTMP;
    public Text manaTextLegacy;

    void Update()
    {
        if (weaponController == null) return;

        string manaString = $"FIRE MANA: {Mathf.FloorToInt(weaponController.currentMana)} / {weaponController.maxMana}";

        if (manaTextTMP != null)
        {
            manaTextTMP.text = manaString;
        }
        else if (manaTextLegacy != null)
        {
            manaTextLegacy.text = manaString;
        }
    }
}
