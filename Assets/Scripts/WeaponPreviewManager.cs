using UnityEngine;

public class WeaponPreviewManager : MonoBehaviour
{
    [Header("Setup")]
    public Transform previewStage; // Assign a transform (e.g., an Empty GameObject far away)
    public float spinSpeed = 30f;
    public Vector3 rotationAxis = Vector3.up;

    private GameObject currentModel;

    // Call this when weapon changes
    public void UpdateModel(GameObject modelPrefab)
    {
        // 1. Cleanup old model
        if (currentModel != null) 
        {
            Destroy(currentModel);
        }
        
        // 2. Spawn new model
        if (modelPrefab != null)
        {
            currentModel = Instantiate(modelPrefab, previewStage);
            
            // Reset transforms relative to stage
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;
            
            // Optional: Ensure layer is correct (e.g., "UI" or "Preview")
            // SetLayerRecursively(currentModel, previewStage.gameObject.layer);
        }
    }

    void Update()
    {
        if (currentModel != null)
        {
            // Smoothly rotate the object
            // "Randomly spinning smoothly" -> We can tumble it a bit
            currentModel.transform.Rotate(rotationAxis * spinSpeed * Time.deltaTime, Space.World);
            
            // Optional: Add a slight secondary wobble
            currentModel.transform.Rotate(Vector3.right * (spinSpeed * 0.3f) * Time.deltaTime, Space.World);
        }
    }
}
