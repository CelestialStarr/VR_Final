using UnityEngine;

[RequireComponent(typeof(Outline))]
public class OutlinePulse : MonoBehaviour
{
    private Outline outline;

    [Header("Pulse Settings")]
    [Tooltip("Speed of the pulse animation")]
    public float pulseSpeed = 2.0f;

    [Tooltip("Minimum width of the outline")]
    public float minWidth = 0.0f;

    [Tooltip("Maximum width of the outline")]
    public float maxWidth = 6.0f;

    void Awake()
    {
        outline = GetComponent<Outline>();
    }

    void Update()
    {
        if (outline.enabled)
        {
            float wave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            outline.OutlineWidth = Mathf.Lerp(minWidth, maxWidth, wave);
        }
    }
}