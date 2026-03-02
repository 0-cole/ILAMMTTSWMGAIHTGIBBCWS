using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [SerializeField] private float bobHeight = 0.25f;
    [SerializeField] private float bobSpeed = 2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPos + Vector3.up * offset;
    }
}
