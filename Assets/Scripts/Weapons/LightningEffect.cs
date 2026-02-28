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

    public void Setup(System.Collections.Generic.List<Vector3> points)
    {
        if (points == null || points.Count < 2) return;

        lineRenderer.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++)
        {
            lineRenderer.SetPosition(i, points[i]);
        }
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
