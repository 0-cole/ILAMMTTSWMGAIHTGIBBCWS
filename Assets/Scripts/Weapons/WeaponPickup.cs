using UnityEngine;
using TMPro;

public class WeaponPickup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string weaponName = "Lightning";
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Sprite weaponIcon; // Optional icon for UI

    [Header("References")]
    [SerializeField] private GameObject pickupPrompt; // The "Press E" UI world text
    [SerializeField] private WeaponController weaponController;

    private Transform playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            if (weaponController == null)
                weaponController = player.GetComponent<WeaponController>();
        }

        // Hide prompt initially
        if (pickupPrompt != null) pickupPrompt.SetActive(false);
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool inRange = distance <= interactionDistance;

        // Show/Hide Prompt
        if (pickupPrompt != null)
            pickupPrompt.SetActive(inRange);

        // Interaction
        if (inRange && Input.GetKeyDown(KeyCode.E))
        {
            Pickup();
        }
    }

    void Pickup()
    {
        if (weaponController != null)
        {
            // Check if already unlocked
            bool isAlreadyUnlocked = weaponController.IsWeaponUnlocked(weaponName);

            // Find model
            GameObject modelPrefab = null;
            foreach (var w in weaponController.weapons)
            {
                if (w.name == weaponName)
                {
                    modelPrefab = w.modelPrefab;
                    break;
                }
            }

            if (isAlreadyUnlocked)
            {
                // DUPLICATE: Trigger Mana Boost
                weaponController.ActivateManaBoost();
                if (UnlockNotification.Instance != null)
                {
                    UnlockNotification.Instance.ShowUnlock("MANA BOOSTED!", weaponIcon, modelPrefab, "Regen Doubled for 20s!");
                }
            }
            else
            {
                // NEW: Unlock Weapon
                weaponController.UnlockWeapon(weaponName);
                if (UnlockNotification.Instance != null)
                {
                    UnlockNotification.Instance.ShowUnlock(weaponName, weaponIcon, modelPrefab, "Press Q to swap weapons!");
                }
            }
        }
        else
        {
            // Fallback
            if (UnlockNotification.Instance != null)
                UnlockNotification.Instance.ShowUnlock(weaponName, weaponIcon, null);
        }

        // Destroy object
        Destroy(gameObject);
    }
}
