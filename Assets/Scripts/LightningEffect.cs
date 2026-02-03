using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightningEffect : MonoBehaviour
{
    private LineRenderer lineRenderer;
    [SerializeField] private float fadeSpeed = 5f;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void Setup(Vector3 start, Vector3 end)
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    void Update()
    {
        if (lineRenderer.startColor.a > 0)
        {
            Color color = lineRenderer.startColor;
            color.a -= fadeSpeed * Time.deltaTime;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
