using UnityEngine;

public class WeaponPreviewManager : MonoBehaviour
{
    [Header("Setup")]
    public Transform previewStage; // Assign a transform (e.g., an Empty GameObject far away)
    public float spinSpeed = 30f;
    public float modelScale = 5f; // New Scale Control
    public Vector3 rotationAxis = new Vector3(1f, 1f, 1f); // Default to tumble on all axes

    private GameObject currentModel;
    private Quaternion lastRotation = Quaternion.identity;

    // Call this when weapon changes
    public void UpdateModel(GameObject modelPrefab)
    {
        // 1. Cleanup old model
        if (currentModel != null) 
        {
            lastRotation = currentModel.transform.localRotation; // Save rotation
            Destroy(currentModel);
        }
        
        // 2. Spawn new model
        if (modelPrefab != null)
        {
            currentModel = Instantiate(modelPrefab, previewStage);
            
            // Reset transforms relative to stage
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = lastRotation; // Apply saved rotation
            currentModel.transform.localScale = Vector3.one * modelScale; 
            
            // Optional: Ensure layer is correct (e.g., "UI" or "Preview")
            // SetLayerRecursively(currentModel, previewStage.gameObject.layer);
        }
    }

    void Update()
    {
        if (currentModel != null)
        {
            // Smoothly rotate the object
            // Tumble on all axes (using Space.Self to ensure it tumbles regardless of parent)
            currentModel.transform.Rotate(rotationAxis.normalized * spinSpeed * Time.deltaTime, Space.Self);
        }
    }
}
