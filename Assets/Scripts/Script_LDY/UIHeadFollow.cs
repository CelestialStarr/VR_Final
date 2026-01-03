using UnityEngine;

public class UIHeadFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public float distance = 1.0f;
    public float smoothTime = 0.3f;

    [Header("Height Adjustment")]
    public bool lockYAxis = false;
    public float heightOffset = 0.0f;

    private Transform cameraTransform;
    private Vector3 currentVelocity;

    private void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("Main Camera not found! Please ensure your XR Origin camera has the 'MainCamera' Tag.");
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 targetPosition = cameraTransform.position + (cameraTransform.forward * distance);

        if (lockYAxis)
        {
            targetPosition.y = cameraTransform.position.y + heightOffset;
        }
        else
        {
            targetPosition.y += heightOffset;
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

        Quaternion targetRotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / smoothTime);
    }
}