using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private float magnitude = 0.2f;

    private Vector3 originalPos;
    private float shakeTimer;

    void OnEnable()
    {
        originalPos = transform.localPosition;
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            transform.localPosition = originalPos + Random.insideUnitSphere * magnitude;
            shakeTimer -= Time.deltaTime;
        }
        else
        {
            shakeTimer = 0f;
            transform.localPosition = originalPos;
        }
    }

    public void Shake(float customDuration = -1f, float customMagnitude = -1f)
    {
        shakeTimer = (customDuration > 0) ? customDuration : duration;
        float mag = (customMagnitude > 0) ? customMagnitude : magnitude;
        
        // Apply immediate offset for instant feedback
        transform.localPosition = originalPos + Random.insideUnitSphere * mag;
    }
}
